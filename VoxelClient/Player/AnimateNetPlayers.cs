using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateNetPlayers : MonoBehaviour
{
    Camera _camera;
    [SerializeField] HandleConnections handleConnections;
    [SerializeField] JitterBufferScript jitterBufferScript;
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2;
    [SerializeField] GameObject player3;
    [SerializeField] GameObject player4;

    [SerializeField] Animator player1Animator;
    [SerializeField] Animator player2Animator;
    [SerializeField] Animator player3Animator;
    [SerializeField] Animator player4Animator;

    Vector3 camForward;

    Vector3 player1Move;
    Vector3 player2Move;
    Vector3 player3Move;
    Vector3 player4Move;

    Vector3 player1MoveInput;
    Vector3 player2MoveInput;
    Vector3 player3MoveInput;
    Vector3 player4MoveInput;

    float player1ForwardAmount;
    float player2ForwardAmount;
    float player3ForwardAmount;
    float player4ForwardAmount;

    float player1TurnAmount;
    float player2TurnAmount;
    float player3TurnAmount;
    float player4TurnAmount;

    private void Awake()
    {
        _camera = FindObjectOfType<Camera>();
    }

    private void Update()
    {
        AnimateAllPlayers();
    }

    void AnimateAllPlayers()
    {
        if (player1.activeSelf && handleConnections.myPlayerID != 0)
        {
            AnimatePlayer(jitterBufferScript.player1Input.y
                , jitterBufferScript.player1Input.x
                , player1
                , ref player1Move
                , ref player1MoveInput
                , ref player1ForwardAmount
                , ref player1TurnAmount
                , player1Animator);
        }

        if (player2.activeSelf && handleConnections.myPlayerID != 1)
        {
            AnimatePlayer(jitterBufferScript.player2Input.y
                , jitterBufferScript.player2Input.x
                , player2
                , ref player2Move
                , ref player2MoveInput
                , ref player2ForwardAmount
                , ref player2TurnAmount
                , player2Animator);
        }

        if (player3.activeSelf && handleConnections.myPlayerID != 2)
        {
            AnimatePlayer(jitterBufferScript.player3Input.y
                , jitterBufferScript.player3Input.x
                , player3
                , ref player3Move
                , ref player3MoveInput
                , ref player3ForwardAmount
                , ref player3TurnAmount
                , player3Animator);
        }

        if (player4.activeSelf && handleConnections.myPlayerID != 3)
        {
            AnimatePlayer(jitterBufferScript.player4Input.y
                , jitterBufferScript.player4Input.x
                , player4
                , ref player4Move
                , ref player4MoveInput
                , ref player4ForwardAmount
                , ref player4TurnAmount
                , player4Animator);
        }
    }

    void AnimatePlayer(float x, float y, GameObject currentGameObj, ref Vector3 move, ref Vector3 moveInput, ref float forwardAmount, ref float turnAmount, Animator animator)
    {
        if (_camera != null)
        {
            camForward = Vector3.Scale(_camera.transform.up, new Vector3(1, 0, 1)).normalized;
            move = x * camForward + y * _camera.transform.right;
        }
        else
        {
            move = x * Vector3.forward + y * Vector3.right;
        }

        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        Move(currentGameObj, ref move, ref moveInput, ref turnAmount, ref forwardAmount, animator);
    }

    private void Move(GameObject currentGameObj, ref Vector3 move, ref Vector3 moveInput, ref float turnAmount, ref float forwardAmount, Animator animator)
    {
        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        moveInput = move;

        ConvertMoveInput(currentGameObj, moveInput, ref turnAmount, ref forwardAmount);
        UpdateAnimator(animator, ref turnAmount, ref forwardAmount);
    }

    private void ConvertMoveInput(GameObject currentGameObj, Vector3 moveInput, ref float turnAmount, ref float forwardAmount)
    {
        Vector3 localMove = currentGameObj.transform.InverseTransformDirection(moveInput);
        turnAmount = localMove.x;

        forwardAmount = localMove.z;
    }

    private void UpdateAnimator(Animator animator, ref float turnAmount, ref float forwardAmount)
    {
        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
    }
}
