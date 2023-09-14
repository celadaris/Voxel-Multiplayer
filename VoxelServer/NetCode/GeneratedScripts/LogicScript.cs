using DisruptorUnity3d;
using NetStack.Serialization;
using NetStack.Quantization;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class LogicScript : MonoBehaviour
{
    Task logicTask;
    CancellationTokenSource tokenSource;
    CancellationToken ct;

    
    public RingBuffer<AllPlayerStatePacketContainer> logicSendAllPlayerStatePacketBuffer { get; set; }
    public RingBuffer<AllPlayerInputPacketContainer> logicSendAllPlayerInputPacketBuffer { get; set; }
    public RingBuffer<PlayerInfoPacketContainer> logicSendPlayerInfoPacketBuffer { get; set; }
    public RingBuffer<IntPtr> recvNetToLogic { get; set; }

    void Awake()
    {
        tokenSource = new CancellationTokenSource();
        ct = tokenSource.Token;

        logicSendAllPlayerStatePacketBuffer = new RingBuffer<AllPlayerStatePacketContainer>(100);

        logicSendAllPlayerInputPacketBuffer = new RingBuffer<AllPlayerInputPacketContainer>(100);

        logicSendPlayerInfoPacketBuffer = new RingBuffer<PlayerInfoPacketContainer>(100);

        recvNetToLogic = new RingBuffer<IntPtr>(100);
        LogicThread();
    }

    void LogicThread()
    {
        logicTask = Task.Run(() =>
        {
            while (!ct.IsCancellationRequested)
            {
                while (recvNetToLogic.Count > 0)
                {
                    //recv packet and dequeue to another IntPtr
                    byte[] buffer = new byte[1024];
                    IntPtr recvEventPtr = recvNetToLogic.Dequeue();

                    //convert IntPtr to struct
                    ENet.Event recvEvent;
                    recvEvent = (ENet.Event)Marshal.PtrToStructure(recvEventPtr, typeof(ENet.Event));

                    //free the IntPtr memory
                    Marshal.FreeHGlobal(recvEventPtr);

                    //copy packet contents to byte array, dispose of it from Event struct
                    recvEvent.Packet.CopyTo(buffer);
                    recvEvent.Packet.Dispose();

                    //convert Event struct back to IntPtr
                    IntPtr recvEventRePackage = Marshal.AllocHGlobal(Marshal.SizeOf(recvEvent));
                    Marshal.StructureToPtr(recvEvent, recvEventRePackage, false);

                    //convert packet to a bitbuffer
                    BitBuffer data = new BitBuffer(1024);
                    data.FromArray(buffer, buffer.Length);

                    //find what type of packet it is
                    ushort packetID = data.ReadUShort();
                    switch (packetID)
                    {
                        case 1:
                            //deserialize bitbuffer to a packet struct
                            PlayerStatePacket playerStatePacket = new PlayerStatePacket();
                            playerStatePacket.packetID = packetID;
                            BoundedRange[] playerPosBounds = new BoundedRange[2];
                            playerPosBounds[0] = new BoundedRange(-38f, 47f, 0.0000009f);
                            playerPosBounds[1] = new BoundedRange(-22f, 28f, 0.0000009f);

                            QuantizedVector2 playerPosVector2 = new QuantizedVector2(data.ReadUInt(), data.ReadUInt());

                            playerStatePacket.playerPos = BoundedRange.Dequantize(playerPosVector2, playerPosBounds);


                            //clear bitbuffer
                            data.Clear();

                            PlayerStatePacketContainer playerStatePacketContainer = new PlayerStatePacketContainer()
                            {
                                packetConfig = recvEventRePackage,
                                playerStatePacket = playerStatePacket
                            };

                            //send to main thread
                            MainThreadNetcode.recvPlayerStatePacketToGame.Enqueue(playerStatePacketContainer);
                            break;

                        case 3:
                            //deserialize bitbuffer to a packet struct
                            PlayerInputPacket playerInputPacket = new PlayerInputPacket();
                            playerInputPacket.packetID = packetID;
                            playerInputPacket.yRot = HalfPrecision.Dequantize(data.ReadUShort());
                            playerInputPacket.xInput = HalfPrecision.Dequantize(data.ReadUShort());
                            playerInputPacket.yInput = HalfPrecision.Dequantize(data.ReadUShort());

                            //clear bitbuffer
                            data.Clear();

                            PlayerInputPacketContainer playerInputPacketContainer = new PlayerInputPacketContainer()
                            {
                                packetConfig = recvEventRePackage,
                                playerInputPacket = playerInputPacket
                            };

                            //send to main thread
                            MainThreadNetcode.recvPlayerInputPacketToGame.Enqueue(playerInputPacketContainer);
                            break;

                    }
                }

                while (logicSendAllPlayerStatePacketBuffer.Count > 0)
                {
                    //get contents from main thread
                    AllPlayerStatePacketContainer allPlayerStatePacketContainer = logicSendAllPlayerStatePacketBuffer.Dequeue();

                    //convert IntPtr's to structs
                    PacketConfig packetConfig = (PacketConfig)Marshal.PtrToStructure(allPlayerStatePacketContainer.packetConfig, typeof(PacketConfig));

                    //free the IntPtr memory
                    Marshal.FreeHGlobal(allPlayerStatePacketContainer.packetConfig);

                    //compress allPlayerStatePacket
                    BitBuffer allPlayerStatePacketBitBuffer = new BitBuffer();

                    allPlayerStatePacketBitBuffer.AddUShort(allPlayerStatePacketContainer.allPlayerStatePacket.packetID);
                    allPlayerStatePacketBitBuffer.AddUShort(allPlayerStatePacketContainer.allPlayerStatePacket.arraySize);
                    
                    for (ushort i = 0; i < allPlayerStatePacketContainer.allPlayerStatePacket.arraySize; i++)
                    {
                        allPlayerStatePacketBitBuffer.AddUShort(allPlayerStatePacketContainer.allPlayerStatePacket.playerID[i]);
                    }

                    BoundedRange[] playerPosBounds = new BoundedRange[2];
                    playerPosBounds[0] = new BoundedRange(-38f, 47f, 0.0000009f);
                    playerPosBounds[1] = new BoundedRange(-22f, 28f, 0.0000009f);

                    for (ushort i = 0; i < allPlayerStatePacketContainer.allPlayerStatePacket.arraySize; i++)
                    {
                        // Quantize position data ready for compact bit-packing 
                        QuantizedVector2 playerPosQuantized = BoundedRange.Quantize(allPlayerStatePacketContainer.allPlayerStatePacket.playerPos[i], playerPosBounds);

                        allPlayerStatePacketBitBuffer.AddUInt(playerPosQuantized.x);
                        allPlayerStatePacketBitBuffer.AddUInt(playerPosQuantized.y);
                    }


                    //convert to byte array
                    byte[] allPlayerStatePacketBufferToSend = new byte[allPlayerStatePacketBitBuffer.Length];
                    allPlayerStatePacketBitBuffer.ToArray(allPlayerStatePacketBufferToSend);

                    //get size of our byte array
                    packetConfig.packetSize = allPlayerStatePacketBufferToSend.Length;

                    //convert packetConfig to IntPtr
                    IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(packetConfig));
                    Marshal.StructureToPtr(packetConfig, configIntPtr, false);

                    //convert byte array to IntPtr
                    IntPtr allPlayerStatePacketBufferIntPtr = Marshal.AllocHGlobal(allPlayerStatePacketBufferToSend.Length);
                    Marshal.Copy(allPlayerStatePacketBufferToSend, 0, allPlayerStatePacketBufferIntPtr, allPlayerStatePacketBufferToSend.Length);

                    //clear allPlayerStatePacketBitBuffer
                    allPlayerStatePacketBitBuffer.Clear();

                    IntPtr[] completePackage = new IntPtr[2]
                    {
                        configIntPtr,
                        allPlayerStatePacketBufferIntPtr
                    };

                    //add byte[] and packeConfig to ringbuffer
                    NetworkScript.sendLogicToNet.Enqueue(completePackage);
                }
                
                while (logicSendAllPlayerInputPacketBuffer.Count > 0)
                {
                    //get contents from main thread
                    AllPlayerInputPacketContainer allPlayerInputPacketContainer = logicSendAllPlayerInputPacketBuffer.Dequeue();

                    //convert IntPtr's to structs
                    PacketConfig packetConfig = (PacketConfig)Marshal.PtrToStructure(allPlayerInputPacketContainer.packetConfig, typeof(PacketConfig));

                    //free the IntPtr memory
                    Marshal.FreeHGlobal(allPlayerInputPacketContainer.packetConfig);

                    //compress allPlayerInputPacket
                    BitBuffer allPlayerInputPacketBitBuffer = new BitBuffer();

                    allPlayerInputPacketBitBuffer.AddUShort(allPlayerInputPacketContainer.allPlayerInputPacket.packetID);
                    allPlayerInputPacketBitBuffer.AddUShort(allPlayerInputPacketContainer.allPlayerInputPacket.arraySize);
                    
                    for (ushort i = 0; i < allPlayerInputPacketContainer.allPlayerInputPacket.arraySize; i++)
                    {
                        allPlayerInputPacketBitBuffer.AddUShort(allPlayerInputPacketContainer.allPlayerInputPacket.playerID[i]);
                    }
                    
                    for (ushort i = 0; i < allPlayerInputPacketContainer.allPlayerInputPacket.arraySize; i++)
                    {
                        ushort convertedyRot = HalfPrecision.Quantize(allPlayerInputPacketContainer.allPlayerInputPacket.yRot[i]);

                        allPlayerInputPacketBitBuffer.AddUShort(convertedyRot);
                    }
                    
                    for (ushort i = 0; i < allPlayerInputPacketContainer.allPlayerInputPacket.arraySize; i++)
                    {
                        ushort convertedxInput = HalfPrecision.Quantize(allPlayerInputPacketContainer.allPlayerInputPacket.xInput[i]);

                        allPlayerInputPacketBitBuffer.AddUShort(convertedxInput);
                    }
                    
                    for (ushort i = 0; i < allPlayerInputPacketContainer.allPlayerInputPacket.arraySize; i++)
                    {
                        ushort convertedyInput = HalfPrecision.Quantize(allPlayerInputPacketContainer.allPlayerInputPacket.yInput[i]);

                        allPlayerInputPacketBitBuffer.AddUShort(convertedyInput);
                    }
                    //convert to byte array
                    byte[] allPlayerInputPacketBufferToSend = new byte[allPlayerInputPacketBitBuffer.Length];
                    allPlayerInputPacketBitBuffer.ToArray(allPlayerInputPacketBufferToSend);

                    //get size of our byte array
                    packetConfig.packetSize = allPlayerInputPacketBufferToSend.Length;

                    //convert packetConfig to IntPtr
                    IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(packetConfig));
                    Marshal.StructureToPtr(packetConfig, configIntPtr, false);

                    //convert byte array to IntPtr
                    IntPtr allPlayerInputPacketBufferIntPtr = Marshal.AllocHGlobal(allPlayerInputPacketBufferToSend.Length);
                    Marshal.Copy(allPlayerInputPacketBufferToSend, 0, allPlayerInputPacketBufferIntPtr, allPlayerInputPacketBufferToSend.Length);

                    //clear allPlayerInputPacketBitBuffer
                    allPlayerInputPacketBitBuffer.Clear();

                    IntPtr[] completePackage = new IntPtr[2]
                    {
                        configIntPtr,
                        allPlayerInputPacketBufferIntPtr
                    };

                    //add byte[] and packeConfig to ringbuffer
                    NetworkScript.sendLogicToNet.Enqueue(completePackage);
                }
                
                while (logicSendPlayerInfoPacketBuffer.Count > 0)
                {
                    //get contents from main thread
                    PlayerInfoPacketContainer playerInfoPacketContainer = logicSendPlayerInfoPacketBuffer.Dequeue();

                    //convert IntPtr's to structs
                    PacketConfig packetConfig = (PacketConfig)Marshal.PtrToStructure(playerInfoPacketContainer.packetConfig, typeof(PacketConfig));

                    //free the IntPtr memory
                    Marshal.FreeHGlobal(playerInfoPacketContainer.packetConfig);

                    //compress playerInfoPacket
                    BitBuffer playerInfoPacketBitBuffer = new BitBuffer();

                    playerInfoPacketBitBuffer.AddUShort(playerInfoPacketContainer.playerInfoPacket.packetID);
                    playerInfoPacketBitBuffer.AddBool(playerInfoPacketContainer.playerInfoPacket.iAmThisPlayer);
                    playerInfoPacketBitBuffer.AddUShort(playerInfoPacketContainer.playerInfoPacket.playerStatus);
                    playerInfoPacketBitBuffer.AddUShort(playerInfoPacketContainer.playerInfoPacket.playerID);
                    //convert to byte array
                    byte[] playerInfoPacketBufferToSend = new byte[playerInfoPacketBitBuffer.Length];
                    playerInfoPacketBitBuffer.ToArray(playerInfoPacketBufferToSend);

                    //get size of our byte array
                    packetConfig.packetSize = playerInfoPacketBufferToSend.Length;

                    //convert packetConfig to IntPtr
                    IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(packetConfig));
                    Marshal.StructureToPtr(packetConfig, configIntPtr, false);

                    //convert byte array to IntPtr
                    IntPtr playerInfoPacketBufferIntPtr = Marshal.AllocHGlobal(playerInfoPacketBufferToSend.Length);
                    Marshal.Copy(playerInfoPacketBufferToSend, 0, playerInfoPacketBufferIntPtr, playerInfoPacketBufferToSend.Length);

                    //clear playerInfoPacketBitBuffer
                    playerInfoPacketBitBuffer.Clear();

                    IntPtr[] completePackage = new IntPtr[2]
                    {
                        configIntPtr,
                        playerInfoPacketBufferIntPtr
                    };

                    //add byte[] and packeConfig to ringbuffer
                    NetworkScript.sendLogicToNet.Enqueue(completePackage);
                }
                
            }
        }, ct);
        logicTask.ContinueWith(t => { Debug.Log(t.Exception); },
    TaskContinuationOptions.OnlyOnFaulted);
    }

    private void OnApplicationQuit()
    {
        tokenSource.Cancel();
    }
}
