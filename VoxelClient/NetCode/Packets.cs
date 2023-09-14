using UnityEngine;

[SendPacket(packetID = 1)]
public struct PlayerStatePacket
{
    public ushort packetID { get; set; }
    [Vector2Bounds(xBounds = new float[3] { -38.0f, 47.0f, 0.0000009f }, yBounds = new float[3] { -22.0f, 28.0f, 0.0000009f })]
    public Vector2 playerPos { get; set; }
}

[RecvPacket(packetID = 2)]
public struct AllPlayerStatePacket
{
    public ushort packetID { get; set; }
    public ushort arraySize { get; set; }
    public ushort[] playerID { get; set; }
    [Vector2Bounds(xBounds = new float[3] { -38.0f, 47.0f, 0.0000009f }, yBounds = new float[3] { -22.0f, 28.0f, 0.0000009f })]
    public Vector2[] playerPos { get; set; }
}

[SendPacket(packetID = 3)]
public struct PlayerInputPacket
{
    public ushort packetID { get; set; }
    public float yRot { get; set; }
    public float xInput { get; set; }
    public float yInput { get; set; }
}

[RecvPacket(packetID = 4)]
public struct AllPlayerInputPacket
{
    public ushort packetID { get; set; }
    public ushort arraySize { get; set; }
    public ushort[] playerID { get; set; }
    public float[] yRot { get; set; }
    public float[] xInput { get; set; }
    public float[] yInput { get; set; }
}

[RecvPacket(packetID = 5)]
public struct PlayerInfoPacket
{
    public ushort packetID { get; set; }
    public bool iAmThisPlayer { get; set; }
    public ushort playerStatus { get; set; }
    public ushort playerID { get; set; }
}