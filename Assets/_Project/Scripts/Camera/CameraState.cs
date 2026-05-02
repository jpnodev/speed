using UnityEngine;

namespace Speed.CameraSystem
{
    public abstract class CameraState
    {
        protected CameraManager cameraManager;

        public CameraState(CameraManager manager)
        {
            this.cameraManager = manager;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }

        // LateUpdate is generally used for cameras to ensure player has already moved
        public virtual void LateTick() { }
    }
}
