using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Camera _camera;
    [SerializeField] Rigidbody rb;
    [SerializeField] float movementSpeed;
    [SerializeField] Animator animator;

    public Vector2 controlInput { get; private set; }
    public Vector3 pointToLook { get; private set; }

    Vector3 camForward;
    Vector3 move;
    Vector3 moveInput;

    float forwardAmount;
    float turnAmount;

    Controls controls;

    // Start is called before the first frame update
    void Awake()
    {
        _camera = FindObjectOfType<Camera>();
        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void MovePlayer()
    {
        controlInput = controls.Player.Move.ReadValue<Vector2>();
        AnimatePlayer(controlInput.y, controlInput.x);
        rb.velocity = new Vector3(controlInput.x * movementSpeed, 0, controlInput.y * movementSpeed);
    }

    private void Update()
    {
        MovePlayer();
        RotatePlayer();
    }

    void AnimatePlayer(float x, float y)
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

        if(move.magnitude > 1)
        {
            move.Normalize();
        }

        Move(move);
    }

    private void Move(Vector3 move)
    {
        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        moveInput = move;

        ConvertMoveInput();
        UpdateAnimator();
    }

    void RotatePlayer()
    {
        Ray cameraRay = _camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            pointToLook = cameraRay.GetPoint(rayLength);

            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }

    private void ConvertMoveInput()
    {
        Vector3 localMove = transform.InverseTransformDirection(moveInput);
        turnAmount = localMove.x;

        forwardAmount = localMove.z;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
        animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
    }
}