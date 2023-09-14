using UnityEngine;

public class JitterBufferScript : MonoBehaviour, IPlayerStatePacket, IPlayerInputPacket
{
    public delegate void Notify();
    public event Notify PacketReceived;

    PlayerStateBuffer[] jitterBuffer;
    float stateTimer = 0.016f;

    public bool player1Update { get; set; }
    public Vector3 player1Pos { get; private set; }
    public float player1yRot { get; private set; }
    public Vector2 player1Input { get; private set; }

    public bool player2Update { get; set; }
    public Vector3 player2Pos { get; private set; }
    public float player2yRot { get; private set; }
    public Vector2 player2Input { get; private set; }

    public bool player3Update { get; set; }
    public Vector3 player3Pos { get; private set; }
    public float player3yRot { get; private set; }
    public Vector2 player3Input { get; private set; }

    public bool player4Update { get; set; }
    public Vector3 player4Pos { get; private set; }
    public float player4yRot { get; private set; }
    public Vector2 player4Input { get; private set; }


    private void Awake()
    {
        jitterBuffer = new PlayerStateBuffer[4];
        jitterBuffer[0].isUseable = false;
        jitterBuffer[1].isUseable = false;
        jitterBuffer[2].isUseable = false;
        jitterBuffer[3].isUseable = false;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer < 0)
        {
            int currentElement = bufferElement - 1;
            if (currentElement < 0)
            {
                currentElement += jitterBuffer.Length;
            }

            if (jitterBuffer[currentElement].isUseable == true)
            {
                jitterBuffer[currentElement].isUseable = false;
                ProcessPacket(jitterBuffer[currentElement]);
            }

            PacketReceived?.Invoke();
            stateTimer = 0.016f;
        }
    }

    void ProcessPacket(PlayerStateBuffer playerStateBuffer)
    {
        switch (playerStateBuffer.netEvent.Peer.ID)
        {
            case 0:
                player1Update = true;
                player1Pos = new Vector3(playerStateBuffer.playerStatePacket.playerPos.x, 0, playerStateBuffer.playerStatePacket.playerPos.y);
                break;

            case 1:
                player2Update = true;
                player2Pos = new Vector3(playerStateBuffer.playerStatePacket.playerPos.x, 0, playerStateBuffer.playerStatePacket.playerPos.y);
                break;

            case 2:
                player3Update = true;
                player3Pos = new Vector3(playerStateBuffer.playerStatePacket.playerPos.x, 0, playerStateBuffer.playerStatePacket.playerPos.y);
                break;

            case 3:
                player4Update = true;
                player4Pos = new Vector3(playerStateBuffer.playerStatePacket.playerPos.x, 0, playerStateBuffer.playerStatePacket.playerPos.y);
                break;
        }
    }

    int bufferElement;
    public void PlayerStatePacketRecieved(ENet.Event netEvent, PlayerStatePacket playerStatePacket)
    {
        PlayerStateBuffer playerStateBuffer = new PlayerStateBuffer
        {
            isUseable = true,
            netEvent = netEvent,
            playerStatePacket = playerStatePacket
        };

        jitterBuffer[bufferElement] = playerStateBuffer;

        if (bufferElement >= jitterBuffer.Length - 1)
        {
            bufferElement = 0;
        }
        else
        {
            bufferElement++;
        }
    }

    public void PlayerInputPacketRecieved(ENet.Event netEvent, PlayerInputPacket playerInputPacket)
    {
        switch (netEvent.Peer.ID)
        {
            case 0:
                player1Input = new Vector2(playerInputPacket.xInput, playerInputPacket.yInput);
                player1yRot = playerInputPacket.yRot;
                break;

            case 1:
                player2Input = new Vector2(playerInputPacket.xInput, playerInputPacket.yInput);
                player2yRot = playerInputPacket.yRot;
                break;

            case 2:
                player3Input = new Vector2(playerInputPacket.xInput, playerInputPacket.yInput);
                player3yRot = playerInputPacket.yRot;
                break;

            case 3:
                player4Input = new Vector2(playerInputPacket.xInput, playerInputPacket.yInput);
                player4yRot = playerInputPacket.yRot;
                break;
        }
    }
}
