using Cinemachine;
using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class HandleConnections : MonoBehaviour, IPlayerInfoPacket
{
    [SerializeField] CinemachineVirtualCamera m_Camera;

    [SerializeField] GameObject player1;
    [SerializeField] PlayerController player1Controller;
    [SerializeField] PlayerSync player1Sync;

    [SerializeField] GameObject player2;
    [SerializeField] PlayerController player2Controller;
    [SerializeField] PlayerSync player2Sync;

    [SerializeField] GameObject player3;
    [SerializeField] PlayerController player3Controller;
    [SerializeField] PlayerSync player3Sync;

    [SerializeField] GameObject player4;
    [SerializeField] PlayerController player4Controller;
    [SerializeField] PlayerSync player4Sync;


    public List<Peer> connectedPeers { get; set; }
    public List<int> allPlayerIDs { get; set; }
    public int myPlayerID { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        connectedPeers = new List<Peer>();
        allPlayerIDs = new List<int>();
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
                    Debug.Log("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Port: " + netEvent.Peer.Port);
                    break;

                case ENet.EventType.Disconnect:
                    Debug.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                    break;

                case ENet.EventType.Timeout:
                    Debug.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                    break;
            }
        }
    }

    public void PlayerInfoPacketRecieved(ENet.Event netEvent, PlayerInfoPacket playerInfoPacket)
    {
        ENet.EventType playerStatus = (ENet.EventType)playerInfoPacket.playerStatus;

        if (playerInfoPacket.iAmThisPlayer)
        {
            if (playerStatus == ENet.EventType.Connect)
            {
                connectedPeers.Add(netEvent.Peer);
                myPlayerID = playerInfoPacket.playerID;

                switch (myPlayerID)
                {
                    case 0:
                        player1.SetActive(true);
                        player1Controller.enabled = true;
                        player1Sync.enabled = true;
                        m_Camera.Follow = player1.transform;
                        break;
                    case 1:
                        player2.SetActive(true);
                        player2Controller.enabled = true;
                        player2Sync.enabled = true;
                        m_Camera.Follow = player2.transform;
                        break;
                    case 2:
                        player3.SetActive(true);
                        player3Controller.enabled = true;
                        player3Sync.enabled = true;
                        m_Camera.Follow = player3.transform;
                        break;
                    case 3:
                        player4.SetActive(true);
                        player4Controller.enabled = true;
                        player4Sync.enabled = true;
                        m_Camera.Follow = player4.transform;
                        break;
                }
            }
        }
        else
        {
            if (playerStatus == ENet.EventType.Connect)
            {
                connectedPeers.Add(netEvent.Peer);
                allPlayerIDs.Add(playerInfoPacket.playerID);

                switch (playerInfoPacket.playerID)
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

            else if (playerStatus == ENet.EventType.Disconnect)
            {
                Peer disconnectedPeer = connectedPeers.Where(x => x.ID == netEvent.Peer.ID).FirstOrDefault();
                connectedPeers.Remove(disconnectedPeer);

                allPlayerIDs.Remove(playerInfoPacket.playerID);

                switch (playerInfoPacket.playerID)
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

            else if (playerStatus == ENet.EventType.Timeout)
            {
                Peer timeoutPeer = connectedPeers.Where(x => x.ID == netEvent.Peer.ID).FirstOrDefault();
                connectedPeers.Remove(timeoutPeer);

                allPlayerIDs.Remove(playerInfoPacket.playerID);

                switch (playerInfoPacket.playerID)
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
    }
}
