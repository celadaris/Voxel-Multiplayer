using UnityEngine;

public class JitterBufferScript : MonoBehaviour, IAllPlayerStatePacket, IAllPlayerInputPacket
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

    // Update is called once per frame
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

    int bufferElement;
    public void AllPlayerStatePacketRecieved(ENet.Event netEvent, AllPlayerStatePacket allPlayerStatePacket)
    {
        PlayerStateBuffer playerStateBuffer = new PlayerStateBuffer
        {
            isUseable = true,
            netEvent = netEvent,
            allPlayerStatePacket = allPlayerStatePacket
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

    void ProcessPacket(PlayerStateBuffer playerStateBuffer)
    {
        for (int i = 0; i < playerStateBuffer.allPlayerStatePacket.arraySize; i++)
        {
            switch (playerStateBuffer.allPlayerStatePacket.playerID[i])
            {
                case 0:
                    player1Pos = new Vector3(playerStateBuffer.allPlayerStatePacket.playerPos[i].x, 0, playerStateBuffer.allPlayerStatePacket.playerPos[i].y);
                    player1Update = true;
                    break;

                case 1:
                    player2Pos = new Vector3(playerStateBuffer.allPlayerStatePacket.playerPos[i].x, 0, playerStateBuffer.allPlayerStatePacket.playerPos[i].y);
                    player2Update = true;
                    break;

                case 2:
                    player3Pos = new Vector3(playerStateBuffer.allPlayerStatePacket.playerPos[i].x, 0, playerStateBuffer.allPlayerStatePacket.playerPos[i].y);
                    player3Update = true;
                    break;

                case 3:
                    player4Pos = new Vector3(playerStateBuffer.allPlayerStatePacket.playerPos[i].x, 0, playerStateBuffer.allPlayerStatePacket.playerPos[i].y);
                    player4Update = true;
                    break;
            }
        }
    }

    public void AllPlayerInputPacketRecieved(ENet.Event netEvent, AllPlayerInputPacket allPlayerInputPacket)
    {
        for (int i = 0; i < allPlayerInputPacket.arraySize; i++)
        {
            switch (allPlayerInputPacket.playerID[i])
            {
                case 0:
                    player1Input = new Vector2(allPlayerInputPacket.xInput[i], allPlayerInputPacket.yInput[i]);
                    player1yRot = allPlayerInputPacket.yRot[i];
                    break;

                case 1:
                    player2Input = new Vector2(allPlayerInputPacket.xInput[i], allPlayerInputPacket.yInput[i]);
                    player2yRot = allPlayerInputPacket.yRot[i];
                    break;

                case 2:
                    player3Input = new Vector2(allPlayerInputPacket.xInput[i], allPlayerInputPacket.yInput[i]);
                    player3yRot = allPlayerInputPacket.yRot[i];
                    break;

                case 3:
                    player4Input = new Vector2(allPlayerInputPacket.xInput[i], allPlayerInputPacket.yInput[i]);
                    player4yRot = allPlayerInputPacket.yRot[i];
                    break;
            }
        }
    }
}
