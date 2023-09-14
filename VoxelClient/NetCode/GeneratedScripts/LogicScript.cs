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

    
    public RingBuffer<PlayerStatePacketContainer> logicSendPlayerStatePacketBuffer { get; set; }
    public RingBuffer<PlayerInputPacketContainer> logicSendPlayerInputPacketBuffer { get; set; }
    public RingBuffer<IntPtr> recvNetToLogic { get; set; }

    void Awake()
    {
        tokenSource = new CancellationTokenSource();
        ct = tokenSource.Token;

                logicSendPlayerStatePacketBuffer = new RingBuffer<PlayerStatePacketContainer>(100);

                logicSendPlayerInputPacketBuffer = new RingBuffer<PlayerInputPacketContainer>(100);

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
                        case 2:
                            //deserialize bitbuffer to a packet struct
                            AllPlayerStatePacket allPlayerStatePacket = new AllPlayerStatePacket();
                            allPlayerStatePacket.packetID = packetID;
                            allPlayerStatePacket.arraySize = data.ReadUShort();

                            List<ushort> playerIDList = new List<ushort>();
                            for (ushort i = 0; i < allPlayerStatePacket.arraySize; i++)
                            {
                                playerIDList.Add(data.ReadUShort());
                            }
                            
                            //add array to struct
                            allPlayerStatePacket.playerID = playerIDList.ToArray();


                            BoundedRange[] playerPosBounds = new BoundedRange[2];
                            playerPosBounds[0] = new BoundedRange(-38f, 47f, 0.0000009f);
                            playerPosBounds[1] = new BoundedRange(-22f, 28f, 0.0000009f);

                            List<Vector2> playerPosList = new List<Vector2>();

                            for (ushort i = 0; i < allPlayerStatePacket.arraySize; i++)
                            {
                                QuantizedVector2 playerPosVector2 = new QuantizedVector2(data.ReadUInt(), data.ReadUInt());
                                playerPosList.Add(BoundedRange.Dequantize(playerPosVector2, playerPosBounds));
                            }
                            
                            //add array to struct
                            allPlayerStatePacket.playerPos = playerPosList.ToArray();



                            //clear bitbuffer
                            data.Clear();

                            AllPlayerStatePacketContainer allPlayerStatePacketContainer = new AllPlayerStatePacketContainer()
                            {
                                packetConfig = recvEventRePackage,
                                allPlayerStatePacket = allPlayerStatePacket
                            };

                            //send to main thread
                            MainThreadNetcode.recvAllPlayerStatePacketToGame.Enqueue(allPlayerStatePacketContainer);
                            break;

                        case 4:
                            //deserialize bitbuffer to a packet struct
                            AllPlayerInputPacket allPlayerInputPacket = new AllPlayerInputPacket();
                            allPlayerInputPacket.packetID = packetID;
                            allPlayerInputPacket.arraySize = data.ReadUShort();

                            List<ushort> playerIDList1 = new List<ushort>();
                            for (ushort i = 0; i < allPlayerInputPacket.arraySize; i++)
                            {
                                playerIDList1.Add(data.ReadUShort());
                            }
                            
                            //add array to struct
                            allPlayerInputPacket.playerID = playerIDList1.ToArray();


                            List<float> yRotList = new List<float>();
                            for (ushort i = 0; i < allPlayerInputPacket.arraySize; i++)
                            {
                                yRotList.Add(HalfPrecision.Dequantize(data.ReadUShort()));
                            }
                            
                            //add array to struct
                            allPlayerInputPacket.yRot = yRotList.ToArray();


                            List<float> xInputList = new List<float>();
                            for (ushort i = 0; i < allPlayerInputPacket.arraySize; i++)
                            {
                                xInputList.Add(HalfPrecision.Dequantize(data.ReadUShort()));
                            }
                            
                            //add array to struct
                            allPlayerInputPacket.xInput = xInputList.ToArray();


                            List<float> yInputList = new List<float>();
                            for (ushort i = 0; i < allPlayerInputPacket.arraySize; i++)
                            {
                                yInputList.Add(HalfPrecision.Dequantize(data.ReadUShort()));
                            }
                            
                            //add array to struct
                            allPlayerInputPacket.yInput = yInputList.ToArray();


                            //clear bitbuffer
                            data.Clear();

                            AllPlayerInputPacketContainer allPlayerInputPacketContainer = new AllPlayerInputPacketContainer()
                            {
                                packetConfig = recvEventRePackage,
                                allPlayerInputPacket = allPlayerInputPacket
                            };

                            //send to main thread
                            MainThreadNetcode.recvAllPlayerInputPacketToGame.Enqueue(allPlayerInputPacketContainer);
                            break;

                        case 5:
                            //deserialize bitbuffer to a packet struct
                            PlayerInfoPacket playerInfoPacket = new PlayerInfoPacket();
                            playerInfoPacket.packetID = packetID;
                            playerInfoPacket.iAmThisPlayer = data.ReadBool();
                            playerInfoPacket.playerStatus = data.ReadUShort();
                            playerInfoPacket.playerID = data.ReadUShort();

                            //clear bitbuffer
                            data.Clear();

                            PlayerInfoPacketContainer playerInfoPacketContainer = new PlayerInfoPacketContainer()
                            {
                                packetConfig = recvEventRePackage,
                                playerInfoPacket = playerInfoPacket
                            };

                            //send to main thread
                            MainThreadNetcode.recvPlayerInfoPacketToGame.Enqueue(playerInfoPacketContainer);
                            break;

                    }
                }

                while (logicSendPlayerStatePacketBuffer.Count > 0)
                {
                    //get contents from main thread
                    PlayerStatePacketContainer playerStatePacketContainer = logicSendPlayerStatePacketBuffer.Dequeue();

                    //convert IntPtr's to structs
                    PacketConfig packetConfig = (PacketConfig)Marshal.PtrToStructure(playerStatePacketContainer.packetConfig, typeof(PacketConfig));

                    //free the IntPtr memory
                    Marshal.FreeHGlobal(playerStatePacketContainer.packetConfig);

                    //compress playerStatePacket
                    BitBuffer playerStatePacketBitBuffer = new BitBuffer();

                    playerStatePacketBitBuffer.AddUShort(playerStatePacketContainer.playerStatePacket.packetID);
                    BoundedRange[] playerPosBounds = new BoundedRange[2];
                    playerPosBounds[0] = new BoundedRange(-38f, 47f, 0.0000009f);
                    playerPosBounds[1] = new BoundedRange(-22f, 28f, 0.0000009f);

                    // Quantize position data ready for compact bit-packing 
                    QuantizedVector2 playerPosQuantized = BoundedRange.Quantize(playerStatePacketContainer.playerStatePacket.playerPos, playerPosBounds);

                    playerStatePacketBitBuffer.AddUInt(playerPosQuantized.x);
                    playerStatePacketBitBuffer.AddUInt(playerPosQuantized.y);

                    //convert to byte array
                    byte[] playerStatePacketBufferToSend = new byte[playerStatePacketBitBuffer.Length];
                    playerStatePacketBitBuffer.ToArray(playerStatePacketBufferToSend);

                    //get size of our byte array
                    packetConfig.packetSize = playerStatePacketBufferToSend.Length;

                    //convert packetConfig to IntPtr
                    IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(packetConfig));
                    Marshal.StructureToPtr(packetConfig, configIntPtr, false);

                    //convert byte array to IntPtr
                    IntPtr playerStatePacketBufferIntPtr = Marshal.AllocHGlobal(playerStatePacketBufferToSend.Length);
                    Marshal.Copy(playerStatePacketBufferToSend, 0, playerStatePacketBufferIntPtr, playerStatePacketBufferToSend.Length);

                    //clear playerStatePacketBitBuffer
                    playerStatePacketBitBuffer.Clear();

                    IntPtr[] completePackage = new IntPtr[2]
                    {
                        configIntPtr,
                        playerStatePacketBufferIntPtr
                    };

                    //add byte[] and packeConfig to ringbuffer
                    NetworkScript.sendLogicToNet.Enqueue(completePackage);
                }
                
                while (logicSendPlayerInputPacketBuffer.Count > 0)
                {
                    //get contents from main thread
                    PlayerInputPacketContainer playerInputPacketContainer = logicSendPlayerInputPacketBuffer.Dequeue();

                    //convert IntPtr's to structs
                    PacketConfig packetConfig = (PacketConfig)Marshal.PtrToStructure(playerInputPacketContainer.packetConfig, typeof(PacketConfig));

                    //free the IntPtr memory
                    Marshal.FreeHGlobal(playerInputPacketContainer.packetConfig);

                    //compress playerInputPacket
                    BitBuffer playerInputPacketBitBuffer = new BitBuffer();

                    playerInputPacketBitBuffer.AddUShort(playerInputPacketContainer.playerInputPacket.packetID);
                    ushort convertedyRot = HalfPrecision.Quantize(playerInputPacketContainer.playerInputPacket.yRot);
                    playerInputPacketBitBuffer.AddUShort(convertedyRot);

                    ushort convertedxInput = HalfPrecision.Quantize(playerInputPacketContainer.playerInputPacket.xInput);
                    playerInputPacketBitBuffer.AddUShort(convertedxInput);

                    ushort convertedyInput = HalfPrecision.Quantize(playerInputPacketContainer.playerInputPacket.yInput);
                    playerInputPacketBitBuffer.AddUShort(convertedyInput);

                    //convert to byte array
                    byte[] playerInputPacketBufferToSend = new byte[playerInputPacketBitBuffer.Length];
                    playerInputPacketBitBuffer.ToArray(playerInputPacketBufferToSend);

                    //get size of our byte array
                    packetConfig.packetSize = playerInputPacketBufferToSend.Length;

                    //convert packetConfig to IntPtr
                    IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(packetConfig));
                    Marshal.StructureToPtr(packetConfig, configIntPtr, false);

                    //convert byte array to IntPtr
                    IntPtr playerInputPacketBufferIntPtr = Marshal.AllocHGlobal(playerInputPacketBufferToSend.Length);
                    Marshal.Copy(playerInputPacketBufferToSend, 0, playerInputPacketBufferIntPtr, playerInputPacketBufferToSend.Length);

                    //clear playerInputPacketBitBuffer
                    playerInputPacketBitBuffer.Clear();

                    IntPtr[] completePackage = new IntPtr[2]
                    {
                        configIntPtr,
                        playerInputPacketBufferIntPtr
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
