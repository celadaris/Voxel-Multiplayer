using UnityEngine;

public class PlayerSync : MonoBehaviour
{
    [SerializeField] HandleConnections handleConnections;
    [SerializeField] Courier courier;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerController playerController;

    float stateTimer = 0.016f;
    float inputTimer = 0.016f;

    void Update()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer < 0)
        {
            SendPlayerState();
            stateTimer = 0.016f;
        }

        inputTimer -= Time.deltaTime;
        if (inputTimer < 0)
        {
            SendPlayerInput();
            inputTimer = 0.016f;
        }
    }

    private void SendPlayerInput()
    {
        PacketConfig packetConfig = new PacketConfig()
        {
            packetFlag = ENet.PacketFlags.None
        };

        PlayerInputPacket playerInputPacket = new PlayerInputPacket()
        {
            packetID = 3,
            xInput = playerController.controlInput.x,
            yInput = playerController.controlInput.y,
            yRot = transform.localEulerAngles.y
        };

        courier.SendPlayerInputPacket(packetConfig, playerInputPacket);
    }

    void SendPlayerState()
    {
        PacketConfig packetConfig = new PacketConfig()
        {
            packetFlag = ENet.PacketFlags.None
        };

        PlayerStatePacket playerStatePacket = new PlayerStatePacket()
        {
            packetID = 1,
            playerPos = new Vector2(transform.position.x, transform.position.z),
        };
        courier.SendPlayerStatePacket(packetConfig, playerStatePacket);
    }
}
