using UnityEngine;

namespace Speed.CameraSystem
{
    public class FixedCameraState : CameraState
    {
        public FixedCameraState(CameraManager manager) : base(manager) { }

        public override void Enter()
        {
            // Lock into a fixed position/rotation based on the current room data
        }

        public override void LateTick()
        {
            // Track the player rotation-wise, but do not move position.
            if (cameraManager.target == null) return;

            // Basic tracking stub
            Vector3 followPosition = cameraManager.target.position + cameraManager.targetOffset;
            cameraManager.mainCamera.transform.LookAt(followPosition);
        }
    }
}
