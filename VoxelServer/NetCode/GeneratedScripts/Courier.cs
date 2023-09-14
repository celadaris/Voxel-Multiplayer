using ENet;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Courier : MonoBehaviour
{
    [SerializeField] LogicScript logicScript;

    public void SendAllPlayerStatePacket(PacketConfig packetConfig, AllPlayerStatePacket allPlayerStatePacket, [Optional] Peer[] excludedPeers)
    {
        PacketConfig addedExcludedPeers;

        if (excludedPeers != null)
        {
            addedExcludedPeers = PeersArrayConverter.ConvertPeersArrayToPtr(packetConfig, excludedPeers);
        }
        else
        {
            addedExcludedPeers = PeersArrayConverter.ConvertPeersArrayToPtr(packetConfig);
        }

        IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(addedExcludedPeers));
        Marshal.StructureToPtr(addedExcludedPeers, configIntPtr, false);

        AllPlayerStatePacketContainer allPlayerStatePacketContainer = new AllPlayerStatePacketContainer
        {
                packetConfig = configIntPtr,
                allPlayerStatePacket = allPlayerStatePacket,
        };

        logicScript.logicSendAllPlayerStatePacketBuffer.Enqueue(allPlayerStatePacketContainer);
    }

    public void SendAllPlayerInputPacket(PacketConfig packetConfig, AllPlayerInputPacket allPlayerInputPacket, [Optional] Peer[] excludedPeers)
    {
        PacketConfig addedExcludedPeers;

        if (excludedPeers != null)
        {
            addedExcludedPeers = PeersArrayConverter.ConvertPeersArrayToPtr(packetConfig, excludedPeers);
        }
        else
        {
            addedExcludedPeers = PeersArrayConverter.ConvertPeersArrayToPtr(packetConfig);
        }

        IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(addedExcludedPeers));
        Marshal.StructureToPtr(addedExcludedPeers, configIntPtr, false);

        AllPlayerInputPacketContainer allPlayerInputPacketContainer = new AllPlayerInputPacketContainer
        {
                packetConfig = configIntPtr,
                allPlayerInputPacket = allPlayerInputPacket,
        };

        logicScript.logicSendAllPlayerInputPacketBuffer.Enqueue(allPlayerInputPacketContainer);
    }

    public void SendPlayerInfoPacket(PacketConfig packetConfig, PlayerInfoPacket playerInfoPacket, [Optional] Peer[] excludedPeers)
    {
        PacketConfig addedExcludedPeers;

        if (excludedPeers != null)
        {
            addedExcludedPeers = PeersArrayConverter.ConvertPeersArrayToPtr(packetConfig, excludedPeers);
        }
        else
        {
            addedExcludedPeers = PeersArrayConverter.ConvertPeersArrayToPtr(packetConfig);
        }

        IntPtr configIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf(addedExcludedPeers));
        Marshal.StructureToPtr(addedExcludedPeers, configIntPtr, false);

        PlayerInfoPacketContainer playerInfoPacketContainer = new PlayerInfoPacketContainer
        {
                packetConfig = configIntPtr,
                playerInfoPacket = playerInfoPacket,
        };

        logicScript.logicSendPlayerInfoPacketBuffer.Enqueue(playerInfoPacketContainer);
    }

}