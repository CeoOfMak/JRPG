using UnityEngine;
using Unity.Cinemachine;

namespace JRPGGame.PlayerController
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController instance;
        public static PlayerController Instance => instance;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private bool canSprint = true;

        [Header("Controls")]
        [SerializeField] private bool controlsEnabled = true;
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        // Components
        private CharacterController characterController;
        private Vector3 moveDirection;
        private bool isGrounded;
        private bool isSprinting;

        // Animation Hashes
        private int moveSpeedHash;
        private int isMovingHash;
        private int isGroundedHash;

        // Properties
        public bool IsMoving => moveDirection.magnitude > 0.1f;
        public bool IsGrounded => isGrounded;
        public bool ControlsEnabled => controlsEnabled;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            characterController = GetComponent<CharacterController>();

            if (animator != null)
            {
                moveSpeedHash = Animator.StringToHash("MoveSpeed");
                isMovingHash = Animator.StringToHash("IsMoving");
                isGroundedHash = Animator.StringToHash("IsGrounded");
            }
        }

        private void Start()
        {
            SetupCinemachine();
        }

        private void Update()
        {
            if (!controlsEnabled) return;

            CheckGrounded();
            HandleMovement();
            HandleRotation();
            UpdateAnimations();
        }

        private void SetupCinemachine()
        {
            if (cinemachineCamera == null)
            {
                cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (cinemachineCamera != null && cameraTarget != null)
            {
                cinemachineCamera.Follow = cameraTarget;
                cinemachineCamera.LookAt = cameraTarget;
            }
        }

        private void CheckGrounded()
        {
            Vector3 spherePosition = transform.position;
            isGrounded = Physics.CheckSphere(
                spherePosition,
                groundCheckDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );
        }

        private void HandleMovement()
        {
            // Get input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Calculate camera-relative movement direction
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

            if (inputDirection.magnitude >= 0.1f)
            {
                // Get camera forward and right vectors
                Transform cameraTransform = Camera.main != null ? Camera.main.transform : transform;
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;

                // Project on horizontal plane
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();

                // Calculate movement direction relative to camera
                moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

                // Check for sprint
                isSprinting = canSprint && Input.GetKey(sprintKey);
                float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

                // Move the character
                Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
                characterController.Move(movement);
            }
            else
            {
                moveDirection = Vector3.zero;
                isSprinting = false;
            }

            // Apply gravity
            if (!isGrounded)
            {
                Vector3 gravity = Physics.gravity * Time.deltaTime;
                characterController.Move(gravity);
            }
        }

        private void HandleRotation()
        {
            if (moveDirection.magnitude >= 0.1f)
            {
                // Rotate towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            float animationSpeed = moveDirection.magnitude > 0.1f ? currentSpeed / moveSpeed : 0f;

            animator.SetFloat(moveSpeedHash, animationSpeed);
            animator.SetBool(isMovingHash, IsMoving);
            animator.SetBool(isGroundedHash, isGrounded);
        }

        public void SetControlsEnabled(bool enabled)
        {
            controlsEnabled = enabled;

            if (!enabled)
            {
                moveDirection = Vector3.zero;
                isSprinting = false;
            }
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void SetSprintSpeed(float speed)
        {
            sprintSpeed = speed;
        }

        public void Teleport(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw ground check sphere
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
        }
    }
}
