using UnityEngine;

public class RotateNetPlayers : MonoBehaviour
{
    [SerializeField] JitterBufferScript jitterBufferScript;
    [SerializeField] HandleConnections handleConnections;
    [SerializeField] float desiredTime;
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    [SerializeField] GameObject player3;
    [SerializeField] GameObject player4;

    Vector3[] player1LocalRot;
    Vector3[] player2LocalRot;
    Vector3[] player3LocalRot;
    Vector3[] player4LocalRot;

    bool player1IsInterpolating;
    bool player2IsInterpolating;
    bool player3IsInterpolating;
    bool player4IsInterpolating;

    float player1ElapsedTime;
    float player2ElapsedTime;
    float player3ElapsedTime;
    float player4ElapsedTime;

    private void Awake()
    {
        jitterBufferScript.PacketReceived += JitterBufferScript_PacketReceived;
        player1LocalRot = new Vector3[2];
        player2LocalRot = new Vector3[2];
        player3LocalRot = new Vector3[2];
        player4LocalRot = new Vector3[2];
    }

    private void JitterBufferScript_PacketReceived()
    {
        PrepareToInterpolate();
    }

    private void Update()
    {
        InterpolateRotation();
    }

    void PrepareToInterpolate()
    {
        if (player1.activeSelf && handleConnections.myPlayerID != 0)
        {
            if (!player1IsInterpolating)
            {
                player1LocalRot[0] = new Vector3(0, player1.transform.localEulerAngles.y, 0);
                player1LocalRot[1] = new Vector3(0, jitterBufferScript.player1yRot, 0);

                player1IsInterpolating = true;
            }
        }

        if (player2.activeSelf && handleConnections.myPlayerID != 1)
        {
            if (!player2IsInterpolating)
            {
                player2LocalRot[0] = new Vector3(0, player2.transform.localEulerAngles.y, 0);
                player2LocalRot[1] = new Vector3(0, jitterBufferScript.player2yRot, 0);

                player2IsInterpolating = true;
            }
        }

        if (player3.activeSelf && handleConnections.myPlayerID != 2)
        {
            if (!player3IsInterpolating)
            {
                player3LocalRot[0] = new Vector3(0, player3.transform.localEulerAngles.y, 0);
                player3LocalRot[1] = new Vector3(0, jitterBufferScript.player3yRot, 0);

                player3IsInterpolating = true;
            }
        }

        if (player4.activeSelf && handleConnections.myPlayerID != 3)
        {
            if (!player4IsInterpolating)
            {
                player4LocalRot[0] = new Vector3(0, player4.transform.localEulerAngles.y, 0);
                player4LocalRot[1] = new Vector3(0, jitterBufferScript.player4yRot, 0);

                player4IsInterpolating = true;
            }
        }
    }

    void InterpolateRotation()
    {
        if (player1IsInterpolating)
        {
            if (player1.activeSelf && handleConnections.myPlayerID != 0)
            {
                if (player1ElapsedTime < desiredTime)
                {
                    player1ElapsedTime += Time.deltaTime;
                    float player1Percentage = player1ElapsedTime / desiredTime;

                    player1.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(player1LocalRot[0]), Quaternion.Euler(player1LocalRot[1]), player1Percentage);
                }
                else
                {
                    player1ElapsedTime = 0.0f;
                    player1.transform.localRotation = Quaternion.Euler(player1LocalRot[1]);
                    player1IsInterpolating = false;
                }
            }
        }

        if (player2IsInterpolating)
        {
            if (player2.activeSelf && handleConnections.myPlayerID != 1)
            {
                if (player2ElapsedTime < desiredTime)
                {
                    player2ElapsedTime += Time.deltaTime;
                    float player2Percentage = player2ElapsedTime / desiredTime;

                    player2.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(player2LocalRot[0]), Quaternion.Euler(player2LocalRot[1]), player2Percentage);
                }
                else
                {
                    player2ElapsedTime = 0.0f;
                    player2.transform.localRotation = Quaternion.Euler(player2LocalRot[1]);
                    player2IsInterpolating = false;
                }
            }
        }

        if (player3IsInterpolating)
        {
            if (player3.activeSelf && handleConnections.myPlayerID != 2)
            {
                if (player3ElapsedTime < desiredTime)
                {
                    player3ElapsedTime += Time.deltaTime;
                    float player3Percentage = player3ElapsedTime / desiredTime;

                    player3.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(player3LocalRot[0]), Quaternion.Euler(player3LocalRot[1]), player3Percentage);
                }
                else
                {
                    player3ElapsedTime = 0.0f;
                    player3.transform.localRotation = Quaternion.Euler(player3LocalRot[1]);
                    player3IsInterpolating = false;
                }
            }
        }

        if (player4IsInterpolating)
        {
            if (player4.activeSelf && handleConnections.myPlayerID != 3)
            {
                if (player4ElapsedTime < desiredTime)
                {
                    player4ElapsedTime += Time.deltaTime;
                    float player4Percentage = player4ElapsedTime / desiredTime;

                    player4.transform.localRotation = Quaternion.Slerp(Quaternion.Euler(player4LocalRot[0]), Quaternion.Euler(player4LocalRot[1]), player4Percentage);
                }
                else
                {
                    player4ElapsedTime = 0.0f;
                    player4.transform.localRotation = Quaternion.Euler(player4LocalRot[1]);
                    player4IsInterpolating = false;
                }
            }
        }
    }

    private void OnDestroy()
    {
        jitterBufferScript.PacketReceived -= JitterBufferScript_PacketReceived;
    }
}
