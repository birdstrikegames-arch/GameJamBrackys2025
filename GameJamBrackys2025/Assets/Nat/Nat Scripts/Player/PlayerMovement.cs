using UnityEngine;

namespace Nat
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Transform orientation;

        [Header("Look")]
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Footsteps")]
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float stepInterval = 0.5f;

        [Header("Jumping")]
        [Tooltip("Impulse applied upward when jumping.")]
        [SerializeField] private float jumpForce = 7.5f;

        [Tooltip("Layers considered ground.")]
        [SerializeField] private LayerMask groundMask = ~0;

        [Tooltip("Sphere ground check radius.")]
        [SerializeField] private float groundCheckRadius = 0.3f;

        [Tooltip("Offset from the player position for ground check (usually slightly below feet).")]
        [SerializeField] private Vector3 groundCheckOffset = new Vector3(0f, -0.6f, 0f);

        private Rigidbody rb;
        private float pitch = 0f;
        private float stepTimer = 0f;
        private int footstepIndex = 0;
        private bool isFrozen = false;
        private float freezeTimer = 0f;

        // Jump state
        private bool jumpRequested = false;
        private bool isGrounded = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;
            if (isFrozen) return;

            // Queue jump when Space is pressed; executed in FixedUpdate for physics consistency
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpRequested = true;
            }
        }

        void FixedUpdate()
        {
            if (isFrozen)
            {
                freezeTimer -= Time.deltaTime;
                if (freezeTimer <= 0f)
                    isFrozen = false;
                return;
            }

            if (Time.timeScale == 0f) return;

            // Ground check
            isGrounded = Physics.CheckSphere(transform.position + groundCheckOffset, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 moveDir = (orientation.forward * v + orientation.right * h).normalized;
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

            HandleFootsteps(moveDir);

            // Apply jump if requested and grounded
            if (jumpRequested)
            {
                jumpRequested = false;
                if (isGrounded)
                {
                    // Reset any downward velocity to get a consistent jump height
                    Vector3 vel = rb.linearVelocity;
                    if (vel.y < 0f) vel.y = 0f;
                    rb.linearVelocity = vel;

                    rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                }
            }

            // Hard reset rotation just in case
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }

        void LateUpdate()
        {
            if (Time.timeScale == 0f) return;
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        void HandleFootsteps(Vector3 moveDir)
        {
            if (moveDir.magnitude > 0.1f && isGrounded)
            {
                stepTimer -= Time.fixedDeltaTime;
                if (stepTimer <= 0f && footstepClips.Length > 0)
                {
                    footstepSource.PlayOneShot(footstepClips[footstepIndex]);
                    footstepIndex = (footstepIndex + 1) % footstepClips.Length;
                    stepTimer = stepInterval;
                }
            }
            else
            {
                stepTimer = 0f;
            }
        }

        public void FreezeMovement(float duration)
        {
            isFrozen = true;
            freezeTimer = duration;
        }

        // Visualize the ground check in the editor
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
        }
    }
}
