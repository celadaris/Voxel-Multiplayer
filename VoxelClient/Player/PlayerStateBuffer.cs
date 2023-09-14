public struct PlayerStateBuffer
{
    public bool isUseable { get; set; }
    public ENet.Event netEvent { get; set; }
    public AllPlayerStatePacket allPlayerStatePacket { get; set; }
}