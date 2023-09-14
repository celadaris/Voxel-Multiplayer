using ENet;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Courier : MonoBehaviour
{
    [SerializeField] LogicScript logicScript;
public void SendPlayerStatePacket(PacketConfig packetConfig, PlayerStatePacket playerStatePacket)
    {
        IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(packetConfig));
        Marshal.StructureToPtr(packetConfig, configIntPtr, false);

        PlayerStatePacketContainer playerStatePacketContainer = new PlayerStatePacketContainer
        {
                packetConfig = configIntPtr,
                playerStatePacket = playerStatePacket,
        };

        logicScript.logicSendPlayerStatePacketBuffer.Enqueue(playerStatePacketContainer);
    }
public void SendPlayerInputPacket(PacketConfig packetConfig, PlayerInputPacket playerInputPacket)
    {
        IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(packetConfig));
        Marshal.StructureToPtr(packetConfig, configIntPtr, false);

        PlayerInputPacketContainer playerInputPacketContainer = new PlayerInputPacketContainer
        {
                packetConfig = configIntPtr,
                playerInputPacket = playerInputPacket,
        };

        logicScript.logicSendPlayerInputPacketBuffer.Enqueue(playerInputPacketContainer);
    }

}