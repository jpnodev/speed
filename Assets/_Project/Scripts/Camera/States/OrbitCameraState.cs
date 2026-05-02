using UnityEngine;
using Speed.Inputs;

namespace Speed.CameraSystem
{
    public class OrbitCameraState : CameraState
    {
        private float _yaw;
        private float _pitch;
        private InputManager _inputManager;

        public OrbitCameraState(CameraManager manager) : base(manager) { }

        public override void Enter()
        {
            // Initialize rotation based on current camera rotation
            Vector3 angles = cameraManager.mainCamera.transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;

            // Try to find input manager on target
            if (cameraManager.target != null)
            {
                _inputManager = cameraManager.target.GetComponent<InputManager>();
            }
        }

        public override void LateTick()
        {
            if (cameraManager.target == null || _inputManager == null) return;

            // 1. Gather Input
            Vector2 lookInput = _inputManager.LookInput;
            _yaw += lookInput.x * cameraManager.lookSensitivity;
            _pitch -= lookInput.y * cameraManager.lookSensitivity; // Invert Y axis standard

            _pitch = Mathf.Clamp(_pitch, cameraManager.minVerticalAngle, cameraManager.maxVerticalAngle);

            // 2. Calculate Orbit Rotation
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);

            // 3. Calculate Position based on target, rotation, and distance
            Vector3 followPosition = cameraManager.target.position + cameraManager.targetOffset;
            Vector3 position = followPosition - (rotation * Vector3.forward * cameraManager.distance);

            // 4. (Future) Raycast collision checks would go here to push the camera closer if hitting a wall

            // 5. Apply
            cameraManager.mainCamera.transform.position = position;
            cameraManager.mainCamera.transform.rotation = rotation;
        }
    }
}
