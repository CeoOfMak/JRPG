using UnityEngine;
using Unity.Cinemachine;

namespace JRPGGame.PlayerController
{
    public class CameraController : MonoBehaviour
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera virtualCamera;

        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private bool invertYAxis = false;

        [Header("Zoom")]
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float currentZoom = 5f;

        [Header("Rotation Limits")]
        [SerializeField] private float minVerticalAngle = -30f;
        [SerializeField] private float maxVerticalAngle = 60f;

        [Header("Controls")]
        [SerializeField] private bool enableCameraControl = true;
        [SerializeField] private KeyCode cameraRotateButton = KeyCode.Mouse1;

        private CinemachineOrbitalFollow orbitalFollow;
        private float currentHorizontalAngle = 0f;
        private float currentVerticalAngle = 20f;

        private void Start()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponent<CinemachineCamera>();
            }

            if (virtualCamera != null)
            {
                orbitalFollow = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (!enableCameraControl) return;

            HandleCameraRotation();
            HandleZoom();
        }

        private void HandleCameraRotation()
        {
            // Only rotate camera when holding right mouse button or middle mouse
            if (Input.GetKey(cameraRotateButton) || Input.GetMouseButton(2))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                if (invertYAxis)
                    mouseY = -mouseY;

                currentHorizontalAngle += mouseX;
                currentVerticalAngle -= mouseY;
                currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

                if (orbitalFollow != null)
                {
                    orbitalFollow.HorizontalAxis.Value = currentHorizontalAngle;
                    orbitalFollow.VerticalAxis.Value = currentVerticalAngle;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void HandleZoom()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                currentZoom -= scrollInput * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

                if (orbitalFollow != null)
                {
                    orbitalFollow.FollowDistance = currentZoom;
                }
            }
        }

        public void SetCameraControlEnabled(bool enabled)
        {
            enableCameraControl = enabled;
        }

        public void SetZoom(float zoom)
        {
            currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);

            if (orbitalFollow != null)
            {
                orbitalFollow.FollowDistance = currentZoom;
            }
        }

        public void ResetCamera()
        {
            currentHorizontalAngle = 0f;
            currentVerticalAngle = 20f;
            currentZoom = 5f;

            if (orbitalFollow != null)
            {
                orbitalFollow.HorizontalAxis.Value = currentHorizontalAngle;
                orbitalFollow.VerticalAxis.Value = currentVerticalAngle;
                orbitalFollow.FollowDistance = currentZoom;
            }
        }

        public void SetTarget(Transform target)
        {
            if (virtualCamera != null)
            {
                virtualCamera.Follow = target;
                virtualCamera.LookAt = target;
            }
        }
    }
}
