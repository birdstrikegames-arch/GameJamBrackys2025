using UnityEngine;

public class InputManager : MonoBehaviour
{
    [HideInInspector]
    public InputSystem_Actions InputMap;
    [Header("Player Components")]
    public Rigidbody rb;
    public Transform cameraPivot;
    public GameObject jumpRayPOS;
    public GameObject body;

    [Header("Player settings")]
    public float playerSpeed;
    public float lookSensitivity;
    public float maxPitch = 85f;
    public float crouchHeight;

    [Header("Jump Controls")]
    public float jumpForce;
    public float groundCheckDistance = 0.15f;
    public LayerMask groundMask;


    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool onJump;
    private bool canjump = true;
    private bool onCrouch;
    private float pitch;

   


    void Start()
    {
        InputMap = new InputSystem_Actions();
        InputMap.Player.Enable();

        Cursor.lockState = CursorLockMode.Locked;   //REMEBER TO REMOVE!!!!
        Cursor.visible = false;
    }

    void Update()
    {
        moveInput = InputMap.Player.Move.ReadValue<Vector2>();
        lookInput = InputMap.Player.Look.ReadValue<Vector2>();
        onJump = InputMap.Player.Jump.triggered;
        onCrouch = InputMap.Player.Crouch.IsPressed();

        Looking();



        
        if (onJump && IsGrounded()) // jump 
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        onJump = false;

        if (onCrouch) //crouching
        {
            canjump = false;
            rb.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
            body.transform.localScale = new Vector3(1, crouchHeight, 1);
        }
        else
        {
            canjump = true;
            body.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void FixedUpdate()
    {
        if (!IsGrounded()) return;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 moveDir = (right * moveInput.x + forward * moveInput.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 vel = rb.linearVelocity;
        Vector3 targetXZ = moveDir * playerSpeed;
        vel.x = targetXZ.x;
        vel.z = targetXZ.z;
        rb.linearVelocity = vel;



    }


    public void Looking()
    {
        float yaw = lookInput.x * lookSensitivity;
        transform.Rotate(Vector3.up * yaw);

        // Pitch: rotate the camera locally
        float deltaPitch = -lookInput.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch + deltaPitch, -maxPitch, maxPitch);
        if (cameraPivot)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    bool IsGrounded()
    {

        return Physics.Raycast(jumpRayPOS.transform.position, Vector3.down, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(jumpRayPOS.transform.position, Vector3.down * groundCheckDistance);
    }

}
