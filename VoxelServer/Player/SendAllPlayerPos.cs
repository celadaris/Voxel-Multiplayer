using System.Collections.Generic;
using UnityEngine;

public class SendAllPlayerPos : MonoBehaviour
{
    [SerializeField] HandleConnections handleConnections;
    [SerializeField] Courier courier;
    [SerializeField] JitterBufferScript jitterBufferScript;

    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    [SerializeField] GameObject player3;
    [SerializeField] GameObject player4;

    [SerializeField] Rigidbody player1Rb;
    [SerializeField] Rigidbody player2Rb;
    [SerializeField] Rigidbody player3Rb;
    [SerializeField] Rigidbody player4Rb;

    List<ushort> statePlayerIDList;
    List<ushort> inputPlayerIDList;
    List<Vector2> playerPosList;
    List<float> yRotList;
    List<float> xInputList;
    List<float> yInputList;

    private void Awake()
    {
        statePlayerIDList = new List<ushort>();
        inputPlayerIDList = new List<ushort>();
        playerPosList = new List<Vector2>();
        yRotList = new List<float>();
        xInputList = new List<float>();
        yInputList = new List<float>();
    }

    void UpdateAllPlayerStates()
    {
        PacketConfig packetConfig = new PacketConfig()
        {
            packetFlag = ENet.PacketFlags.None
        };

        if (handleConnections.connectedPeers.Count > 0)
        {
            handleConnections.connectedPeers.ForEach(x =>
            {
                statePlayerIDList.Clear();
                playerPosList.Clear();

                handleConnections.connectedPeers.ForEach(y =>
                {
                    if (y.ID != x.ID)
                    {
                        AddStateProperties((ushort)y.ID);
                    }
                });

                AllPlayerStatePacket packet = new AllPlayerStatePacket()
                {
                    packetID = 2,
                    arraySize = (ushort)(handleConnections.connectedPeers.Count - 1),
                    playerID = statePlayerIDList.ToArray(),
                    playerPos = playerPosList.ToArray(),
                };

                ENet.Peer[] peers = new ENet.Peer[1] { x };

                courier.SendAllPlayerStatePacket(packetConfig, packet, peers);
            });
        }
    }

    private void UpdateAllPlayerInputs()
    {
        PacketConfig packetConfig = new PacketConfig()
        {
            packetFlag = ENet.PacketFlags.None
        };

        if (handleConnections.connectedPeers.Count > 0)
        {
            handleConnections.connectedPeers.ForEach(x =>
            {
                inputPlayerIDList.Clear();
                yRotList.Clear();
                xInputList.Clear();
                yInputList.Clear();

                handleConnections.connectedPeers.ForEach(y =>
                {
                    if (y.ID != x.ID)
                    {
                        AddInputProperties((ushort)y.ID);
                    }
                });

                AllPlayerInputPacket packet = new AllPlayerInputPacket()
                {
                    packetID = 4,
                    arraySize = (ushort)(handleConnections.connectedPeers.Count - 1),
                    playerID = inputPlayerIDList.ToArray(),
                    yRot = yRotList.ToArray(),
                    xInput = xInputList.ToArray(),
                    yInput = yInputList.ToArray()
                };

                ENet.Peer[] peers = new ENet.Peer[1] { x };

                courier.SendAllPlayerInputPacket(packetConfig, packet, peers);
            });
        }
    }

    float stateTimer = 0.016f;
    float inputTimer = 0.016f;

    void Update()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer < 0)
        {
            UpdateAllPlayerStates();
            stateTimer = 0.016f;
        }

        inputTimer -= Time.deltaTime;
        if (inputTimer < 0)
        {
            UpdateAllPlayerInputs();
            inputTimer = 0.016f;
        }
    }

    void AddStateProperties(ushort playerID)
    {
        switch (playerID)
        {
            case 0:
                statePlayerIDList.Add(playerID);
                playerPosList.Add(new Vector2(player1.transform.position.x, player1.transform.position.z));
                break;

            case 1:
                statePlayerIDList.Add(playerID);
                playerPosList.Add(new Vector2(player2.transform.position.x, player2.transform.position.z));
                break;

            case 2:
                statePlayerIDList.Add(playerID);
                playerPosList.Add(new Vector2(player3.transform.position.x, player3.transform.position.z));
                break;

            case 3:
                statePlayerIDList.Add(playerID);
                playerPosList.Add(new Vector2(player4.transform.position.x, player4.transform.position.z));
                break;
        }
    }

    void AddInputProperties(ushort playerID)
    {
        switch (playerID)
        {
            case 0:
                inputPlayerIDList.Add(playerID);
                yRotList.Add(player1.transform.localEulerAngles.y);
                xInputList.Add(jitterBufferScript.player1Input.x);
                yInputList.Add(jitterBufferScript.player1Input.y);
                break;

            case 1:
                inputPlayerIDList.Add(playerID);
                yRotList.Add(player2.transform.localEulerAngles.y);
                xInputList.Add(jitterBufferScript.player2Input.x);
                yInputList.Add(jitterBufferScript.player2Input.y);
                break;

            case 2:
                inputPlayerIDList.Add(playerID);
                yRotList.Add(player3.transform.localEulerAngles.y);
                xInputList.Add(jitterBufferScript.player3Input.x);
                yInputList.Add(jitterBufferScript.player3Input.y);
                break;

            case 3:
                inputPlayerIDList.Add(playerID);
                yRotList.Add(player4.transform.localEulerAngles.y);
                xInputList.Add(jitterBufferScript.player4Input.x);
                yInputList.Add(jitterBufferScript.player4Input.y);
                break;
        }
    }
}