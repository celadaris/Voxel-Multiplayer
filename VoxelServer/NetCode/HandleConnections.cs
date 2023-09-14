using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class HandleConnections : MonoBehaviour
{
    [SerializeField] Courier courier;
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    [SerializeField] GameObject player3;
    [SerializeField] GameObject player4;
    public List<Peer> connectedPeers { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        connectedPeers = new List<Peer>();
    }

    // Update is called once per frame
    void Update()
    {
        while (NetworkScript.globalEvents.Count > 0)
        {
            IntPtr netEventPtr = NetworkScript.globalEvents.Dequeue();

            //convert IntPtr to struct
            ENet.Event netEvent;
            netEvent = (ENet.Event)Marshal.PtrToStructure(netEventPtr, typeof(ENet.Event));

            //free the IntPtr memory
            Marshal.FreeHGlobal(netEventPtr);

            switch (netEvent.Type)
            {
                case ENet.EventType.Connect:
                    connectedPeers.Add(netEvent.Peer);
                    HandleConnectingClients((ushort)netEvent.Peer.ID);
                    break;

                case ENet.EventType.Disconnect:
                    Debug.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                    Peer disconnectedPeer = connectedPeers.Where(x => x.ID == netEvent.Peer.ID).FirstOrDefault();
                    connectedPeers.Remove(disconnectedPeer);
                    HandleDisconnectingClients((ushort)netEvent.Peer.ID);
                    break;

                case ENet.EventType.Timeout:
                    Debug.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                    Peer timeoutPeer = connectedPeers.Where(x => x.ID == netEvent.Peer.ID).FirstOrDefault();
                    connectedPeers.Remove(timeoutPeer);
                    HandleTimeoutClients((ushort)netEvent.Peer.ID);
                    break;
            }
        }
    }

    void HandleConnectingClients(ushort peerID)
    {
        Debug.Log("Client connected - ID: " + peerID);

        PacketConfig packetConfig = new PacketConfig
        {
            packetFlag = PacketFlags.Reliable
        };

        PlayerInfoPacket playerInfoToConnectedClient = new PlayerInfoPacket
        {
            packetID = 5,
            iAmThisPlayer = true,
            playerStatus = (ushort)ENet.EventType.Connect,
            playerID = peerID,
        };

        //get all client that just connected
        Peer[] connectedClients = connectedPeers.Where(x => (ushort)x.ID == peerID).ToArray();

        //send packet to player that connected
        courier.SendPlayerInfoPacket(packetConfig, playerInfoToConnectedClient, connectedClients);


        //foreach player that is already connected, tell the newly connected player everyone else that is connected
        foreach (Peer connectedPeer in connectedPeers)
        {
            if ((ushort)connectedPeer.ID != peerID)
            {

                PacketConfig OtherPlayerconfig = new PacketConfig { packetFlag = PacketFlags.Reliable };
                PlayerInfoPacket playerInfoPacket = new PlayerInfoPacket
                {
                    packetID = 5,
                    playerID = (ushort)connectedPeer.ID,
                    playerStatus = (ushort)ENet.EventType.Connect,
                    iAmThisPlayer = false,
                };

                courier.SendPlayerInfoPacket(OtherPlayerconfig, playerInfoPacket, connectedClients);
            }
        }


        //alert of the new connection to all other connected players
        if (connectedPeers.Count > 1)
        {
            Peer[] connectingClient = connectedPeers.Where(x => (ushort)x.ID != peerID).ToArray();

            PacketConfig packetConfigForOthers = new PacketConfig
            {
                packetFlag = PacketFlags.Reliable
            };

            PlayerInfoPacket playerInfoToOthers = new PlayerInfoPacket
            {
                packetID = 5,
                iAmThisPlayer = false,
                playerStatus = (ushort)ENet.EventType.Connect,
                playerID = peerID,
            };

            //send packet to other players
            courier.SendPlayerInfoPacket(packetConfigForOthers, playerInfoToOthers, connectingClient);
        }

        switch (peerID)
        {
            case 0:
                player1.SetActive(true);
                break;
            case 1:
                player2.SetActive(true);
                break;
            case 2:
                player3.SetActive(true);
                break;
            case 3:
                player4.SetActive(true);
                break;
        }
    }

    void HandleDisconnectingClients(ushort peerID)
    {
        PacketConfig packetConfig = new PacketConfig
        {
            packetFlag = PacketFlags.Reliable
        };

        PlayerInfoPacket playerInfoToConnectedClient = new PlayerInfoPacket
        {
            packetID = 5,
            iAmThisPlayer = false,
            playerStatus = (ushort)ENet.EventType.Disconnect,
            playerID = peerID,
        };

        courier.SendPlayerInfoPacket(packetConfig, playerInfoToConnectedClient);

        switch (peerID)
        {
            case 0:
                player1.SetActive(false);
                break;
            case 1:
                player2.SetActive(false);
                break;
            case 2:
                player3.SetActive(false);
                break;
            case 3:
                player4.SetActive(false);
                break;
        }
    }

    void HandleTimeoutClients(ushort peerID)
    {
        PacketConfig packetConfig = new PacketConfig
        {
            packetFlag = PacketFlags.Reliable
        };

        PlayerInfoPacket playerInfoToConnectedClient = new PlayerInfoPacket
        {
            packetID = 5,
            iAmThisPlayer = false,
            playerStatus = (ushort)ENet.EventType.Timeout,
            playerID = peerID,
        };

        courier.SendPlayerInfoPacket(packetConfig, playerInfoToConnectedClient);

        switch (peerID)
        {
            case 0:
                player1.SetActive(false);
                break;
            case 1:
                player2.SetActive(false);
                break;
            case 2:
                player3.SetActive(false);
                break;
            case 3:
                player4.SetActive(false);
                break;
        }
    }
}
