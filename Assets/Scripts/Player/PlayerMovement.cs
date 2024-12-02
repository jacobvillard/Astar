using UnityEngine;

/// <summary>
/// Calculates and applies movement to controller and camera.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    //Player Serialized Variables
    [Header("Player Movement Variables"), SerializeField]private float walkSpeed = 7.5f;
    [SerializeField]private float sprintSpeed = 11.5f, jumpSpeed = 8.0f, gravity = 20.0f, lookSpeed = 2.0f, lookXLimit = 45.0f;
    [Header("Player Camera"), SerializeField]private Camera playerCamera;
    [Header("Player Keybindings"), SerializeField]private KeyCode sprintKey = KeyCode.LeftShift, jumpKey = KeyCode.Space;
    
    ////Player Non-Serialized Variables
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX;


    private void Start() {
        characterController = GetComponent<CharacterController>();

        //Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        Movement();
    }
    
    /// <summary>
    /// Controller movement and camera rotation
    /// </summary>
    private void Movement() {
        var forward = transform.TransformDirection(Vector3.forward);
        var right = transform.TransformDirection(Vector3.right);
        var isRunning = Input.GetKey(sprintKey);
        var curSpeedX = (isRunning ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical") ;
        var curSpeedY = (isRunning ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal") ;
        var movementDirectionY = moveDirection.y;
        
        //Set Move Direction
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        
        //Handles jumping
        JumpingMovement(movementDirectionY);

        //Gravity for when player is not grounded with acceleration
        ApplyGravity();

        //Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        //Player and Camera rotation
        PlayerAndCameraRotation();
    }

    /// <summary>
    /// Controls player and camera rotation
    /// </summary>
    private void PlayerAndCameraRotation() {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }

    /// <summary>
    /// Applies gravity to the player when not grounded
    /// </summary>
    private void ApplyGravity() {
        if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;
    }

    /// <summary>
    /// Controls the jumping movement of the player
    /// </summary>
    /// <param name="movementDirectionY"></param>
    private void JumpingMovement(float movementDirectionY) {
        if (Input.GetKey(jumpKey) && characterController.isGrounded) moveDirection.y = jumpSpeed;
        else moveDirection.y = movementDirectionY;
    }
}
