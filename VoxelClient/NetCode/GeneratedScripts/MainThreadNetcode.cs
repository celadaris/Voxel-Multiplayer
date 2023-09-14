using DisruptorUnity3d;
using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class MainThreadNetcode : MonoBehaviour
{
    [SerializeField] HandleConnections handleConnections;
    [SerializeField] JitterBufferScript jitterBufferScript;

    public static RingBuffer<AllPlayerStatePacketContainer> recvAllPlayerStatePacketToGame { get; set; }
    public static RingBuffer<AllPlayerInputPacketContainer> recvAllPlayerInputPacketToGame { get; set; }
    public static RingBuffer<PlayerInfoPacketContainer> recvPlayerInfoPacketToGame { get; set; }

    void Awake()
    {
        recvAllPlayerStatePacketToGame = new RingBuffer<AllPlayerStatePacketContainer>(100);
        recvAllPlayerInputPacketToGame = new RingBuffer<AllPlayerInputPacketContainer>(100);
        recvPlayerInfoPacketToGame = new RingBuffer<PlayerInfoPacketContainer>(100);
    }

    // Update is called once per frame
    void Update()
    {
        while (recvAllPlayerStatePacketToGame.Count > 0)
        {
            AllPlayerStatePacketContainer objects = recvAllPlayerStatePacketToGame.Dequeue();

            //convert IntPtr to struct
            ENet.Event recvEvent;
            recvEvent = (ENet.Event)Marshal.PtrToStructure(objects.packetConfig, typeof(ENet.Event));

            //free the IntPtr memory
            Marshal.FreeHGlobal(objects.packetConfig);

            jitterBufferScript.AllPlayerStatePacketRecieved(recvEvent, objects.allPlayerStatePacket);
        }
        while (recvAllPlayerInputPacketToGame.Count > 0)
        {
            AllPlayerInputPacketContainer objects = recvAllPlayerInputPacketToGame.Dequeue();

            //convert IntPtr to struct
            ENet.Event recvEvent;
            recvEvent = (ENet.Event)Marshal.PtrToStructure(objects.packetConfig, typeof(ENet.Event));

            //free the IntPtr memory
            Marshal.FreeHGlobal(objects.packetConfig);

            jitterBufferScript.AllPlayerInputPacketRecieved(recvEvent, objects.allPlayerInputPacket);
        }
        while (recvPlayerInfoPacketToGame.Count > 0)
        {
            PlayerInfoPacketContainer objects = recvPlayerInfoPacketToGame.Dequeue();

            //convert IntPtr to struct
            ENet.Event recvEvent;
            recvEvent = (ENet.Event)Marshal.PtrToStructure(objects.packetConfig, typeof(ENet.Event));

            //free the IntPtr memory
            Marshal.FreeHGlobal(objects.packetConfig);

            handleConnections.PlayerInfoPacketRecieved(recvEvent, objects.playerInfoPacket);
        }
    }
}