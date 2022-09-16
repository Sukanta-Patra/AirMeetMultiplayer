using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    [SerializeField] private AudioSource audioSrc;

    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [SerializeField] private LeftJoystick leftJoystick;
    [SerializeField] private RightJoystick rightJoystick;

    [HideInInspector]
    public bool canMove = true;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            Destroy(playerCamera.gameObject);
        }

    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        leftJoystick = FindObjectOfType<GameManager>().leftJoystick;
        rightJoystick = FindObjectOfType<GameManager>().rightJoystick;

    }

    void Update()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;

#if !UNITY_ANDROID
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
#else
        moveDirection = (forward * rightJoystick.GetInputDirection().y * runningSpeed) + (right * rightJoystick.GetInputDirection().x * runningSpeed);
#endif

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.L))
        {
            canMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.K))
        { 
            canMove = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Player and Camera rotation
        if (canMove)
        {
#if !UNITY_ANDROID
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
#else
            rotationX += -leftJoystick.GetInputDirection().y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, leftJoystick.GetInputDirection().x * lookSpeed, 0);
#endif
        }
    }

    private void ActivateSelfCamera()
    {
        playerCamera.gameObject.SetActive(true);
    }
}

