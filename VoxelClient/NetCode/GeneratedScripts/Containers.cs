
using System;

public struct AllPlayerStatePacketContainer
{
    public IntPtr packetConfig { get; set; }
    public AllPlayerStatePacket allPlayerStatePacket { get; set; }
}
public struct AllPlayerInputPacketContainer
{
    public IntPtr packetConfig { get; set; }
    public AllPlayerInputPacket allPlayerInputPacket { get; set; }
}
public struct PlayerInfoPacketContainer
{
    public IntPtr packetConfig { get; set; }
    public PlayerInfoPacket playerInfoPacket { get; set; }
}
public struct PlayerStatePacketContainer
{
    public IntPtr packetConfig { get; set; }
    public PlayerStatePacket playerStatePacket { get; set; }
}
public struct PlayerInputPacketContainer
{
    public IntPtr packetConfig { get; set; }
    public PlayerInputPacket playerInputPacket { get; set; }
}