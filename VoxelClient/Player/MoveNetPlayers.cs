using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveNetPlayers : MonoBehaviour
{
    [SerializeField] HandleConnections handleConnections;
    [SerializeField] JitterBufferScript jitterBufferScript;
    [SerializeField] float desiredTime;

    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    [SerializeField] GameObject player3;
    [SerializeField] GameObject player4;

    [SerializeField] Rigidbody player1Rb;
    [SerializeField] Rigidbody player2Rb;
    [SerializeField] Rigidbody player3Rb;
    [SerializeField] Rigidbody player4Rb;

    float movementSpeed = 2.5f;

    bool player1IsInterpolating;
    bool player2IsInterpolating;
    bool player3IsInterpolating;
    bool player4IsInterpolating;

    float player1ElapsedTime;
    float player2ElapsedTime;
    float player3ElapsedTime;
    float player4ElapsedTime;

    Vector3[] player1LocalPos;
    Vector3[] player2LocalPos;
    Vector3[] player3LocalPos;
    Vector3[] player4LocalPos;

    private void Awake()
    {
        jitterBufferScript.PacketReceived += JitterBufferScript_PacketReceived;

        player1LocalPos = new Vector3[2];
        player2LocalPos = new Vector3[2];
        player3LocalPos = new Vector3[2];
        player4LocalPos = new Vector3[2];
    }

    private void JitterBufferScript_PacketReceived()
    {
        PrepareToInterpolate();
    }

    private void Update()
    {
        InterpolateNetPlayers();
        ExtrapolateNetPlayers();
    }

    void PrepareToInterpolate()
    {
        if (player1.activeSelf && jitterBufferScript.player1Update && handleConnections.myPlayerID != 0)
        {
            if (!player1IsInterpolating)
            {
                player1LocalPos[0] = player1.transform.position;

                player1LocalPos[1] = jitterBufferScript.player1Pos;

                player1Rb.velocity = Vector3.zero;

                player1IsInterpolating = true;
                jitterBufferScript.player1Update = false;
            }
        }

        if (player2.activeSelf && jitterBufferScript.player2Update && handleConnections.myPlayerID != 1)
        {
            if (!player2IsInterpolating)
            {
                player2LocalPos[0] = player2.transform.position;

                player2LocalPos[1] = jitterBufferScript.player2Pos;

                player2Rb.velocity = Vector3.zero;

                player2IsInterpolating = true;
                jitterBufferScript.player2Update = false;
            }
        }

        if (player3.activeSelf && jitterBufferScript.player3Update && handleConnections.myPlayerID != 2)
        {
            if (!player3IsInterpolating)
            {
                player3LocalPos[0] = player3.transform.position;

                player3LocalPos[1] = jitterBufferScript.player3Pos;

                player3Rb.velocity = Vector3.zero;

                player3IsInterpolating = true;
                jitterBufferScript.player3Update = false;
            }
        }

        if (player4.activeSelf && jitterBufferScript.player4Update && handleConnections.myPlayerID != 3)
        {
            if (!player4IsInterpolating)
            {
                player4LocalPos[0] = player4.transform.position;

                player4LocalPos[1] = jitterBufferScript.player4Pos;

                player4Rb.velocity = Vector3.zero;

                player4IsInterpolating = true;
                jitterBufferScript.player4Update = false;
            }
        }
    }

    void InterpolateNetPlayers()
    {
        if (player1IsInterpolating)
        {
            if (player1.activeSelf && handleConnections.myPlayerID != 0)
            {
                if (player1ElapsedTime < desiredTime)
                {
                    player1ElapsedTime += Time.deltaTime;
                    float player1Percentage = player1ElapsedTime / desiredTime;

                    player1.transform.position = Vector3.Lerp(player1LocalPos[0], player1LocalPos[1], player1Percentage);
                }
                else
                {
                    player1ElapsedTime = 0.0f;
                    player1.transform.position = player1LocalPos[1];
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

                    player2.transform.position = Vector3.Lerp(player2LocalPos[0], player2LocalPos[1], player2Percentage);
                }
                else
                {
                    player2ElapsedTime = 0.0f;
                    player2.transform.position = player2LocalPos[1];
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

                    player3.transform.position = Vector3.Lerp(player3LocalPos[0], player3LocalPos[1], player3Percentage);
                }
                else
                {
                    player3ElapsedTime = 0.0f;
                    player3.transform.position = player3LocalPos[1];
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

                    player4.transform.position = Vector3.Lerp(player4LocalPos[0], player4LocalPos[1], player4Percentage);
                }
                else
                {
                    player4ElapsedTime = 0.0f;
                    player4.transform.position = player4LocalPos[1];
                    player4IsInterpolating = false;
                }
            }
        }
    }

    void ExtrapolateNetPlayers()
    {
        if (player1.activeSelf && handleConnections.myPlayerID != 0)
        {
            if (!player1IsInterpolating)
            {
                player1Rb.velocity = new Vector3(jitterBufferScript.player1Input.x * movementSpeed, 0, jitterBufferScript.player1Input.y * movementSpeed);
            }
        }


        if (player2.activeSelf && handleConnections.myPlayerID != 1)
        {
            if (!player2IsInterpolating)
            {
                player2Rb.velocity = new Vector3(jitterBufferScript.player2Input.x * movementSpeed, 0, jitterBufferScript.player2Input.y * movementSpeed);
            }
        }


        if (player3.activeSelf && handleConnections.myPlayerID != 2)
        {
            if (!player3IsInterpolating)
            {
                player3Rb.velocity = new Vector3(jitterBufferScript.player3Input.x * movementSpeed, 0, jitterBufferScript.player3Input.y * movementSpeed);
            }
        }


        if (player4.activeSelf && handleConnections.myPlayerID != 3)
        {
            if (!player4IsInterpolating)
            {
                player4Rb.velocity = new Vector3(jitterBufferScript.player4Input.x * movementSpeed, 0, jitterBufferScript.player4Input.y * movementSpeed);
            }
        }
    }

    private void OnDestroy()
    {
        jitterBufferScript.PacketReceived -= JitterBufferScript_PacketReceived;
    }
}
