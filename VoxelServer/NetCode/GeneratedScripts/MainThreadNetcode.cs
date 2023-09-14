using DisruptorUnity3d;
using System.Runtime.InteropServices;
using UnityEngine;

public class MainThreadNetcode : MonoBehaviour
{
    [SerializeField] JitterBufferScript jitterBufferScript;

    public static RingBuffer<PlayerStatePacketContainer> recvPlayerStatePacketToGame { get; set; }
    public static RingBuffer<PlayerInputPacketContainer> recvPlayerInputPacketToGame { get; set; }

    void Awake()
    {
        recvPlayerStatePacketToGame = new RingBuffer<PlayerStatePacketContainer>(100);
        recvPlayerInputPacketToGame = new RingBuffer<PlayerInputPacketContainer>(100);
    }

    // Update is called once per frame
    void Update()
    {
        while (recvPlayerStatePacketToGame.Count > 0)
        {
            PlayerStatePacketContainer objects = recvPlayerStatePacketToGame.Dequeue();

            //convert IntPtr to struct
            ENet.Event recvEvent;
            recvEvent = (ENet.Event)Marshal.PtrToStructure(objects.packetConfig, typeof(ENet.Event));

            //free the IntPtr memory
            Marshal.FreeHGlobal(objects.packetConfig);

            jitterBufferScript.PlayerStatePacketRecieved(recvEvent, objects.playerStatePacket);
        }

        while (recvPlayerInputPacketToGame.Count > 0)
        {
            PlayerInputPacketContainer objects = recvPlayerInputPacketToGame.Dequeue();

            //convert IntPtr to struct
            ENet.Event recvEvent;
            recvEvent = (ENet.Event)Marshal.PtrToStructure(objects.packetConfig, typeof(ENet.Event));

            //free the IntPtr memory
            Marshal.FreeHGlobal(objects.packetConfig);

            jitterBufferScript.PlayerInputPacketRecieved(recvEvent, objects.playerInputPacket);
        }
    }
}