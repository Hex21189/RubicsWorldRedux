using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    /// <summary>
    /// TODO: make input interface or remove references to the input class from this class.
    /// </summary>
    public class CameraMovement : MonoBehaviour
    {
        #region Fields

        [Header("Character Following")]
        public Transform target;
        public float followSpeed = 5.0f;
        public float turnSpeed = 10.0f;

        [Header("Mouse Rotation")]
        public Transform mouseRotationTransform;
        public float mouseRotationSensitivityY = 1.0f;
        public float mouseRotationSensitivityX = 1.0f;
        public float maxPitch = 60.0f;
        public float minPitch = -5.0f;
        private float pitch;
        private float yaw;

        [Header("Object Avoidance")]
        public LayerMask avoidanceMask;
        public float correctionSpeed = 20.0f;

        [Header("Camera Zoom")]
        public Transform zoomTransform;
        public float minZoom = 5.0f;
        public float maxZoom = 15.0f;
        public float zoomStep = 1.0f;
        public float zoomSpeed = 10.0f;
        private float targetZoom;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Initializes local variables.
        /// </summary>
        protected void Awake()
        {
            targetZoom = zoomTransform.localPosition.z;
        }

        /// <summary>
        /// Updates camera position, rotation, and zoom based on input.
        /// </summary>
        private void FixedUpdate()
        {
            AdjustPosition();
            AdjustRotationByTarget();
            AdjustRotationByMouse();
            AdjustZoom();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Moves towards the target position.
        /// </summary>
        private void AdjustPosition()
        {
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSpeed);
        }

        /// <summary>
        /// Uses the mouse input axies to adjust the camera pitch and yaw. This is be applied to the camera arm to
        /// avoid messing up the player orientation rotation.
        /// </summary>
        private void AdjustRotationByMouse()
        {
            pitch = Mathf.Clamp(pitch - mouseRotationSensitivityX * Input.GetAxis("Mouse Y"), minPitch, maxPitch);
            yaw += mouseRotationSensitivityY * Input.GetAxis("Mouse X");
            mouseRotationTransform.localRotation = Quaternion.Euler(pitch, yaw, 0.0f);
        }

        /// <summary>
        /// Rotates towards the targets rotation. All axies are updated so that the cameras rotation matches the targets perfectly.
        /// </summary>
        private void AdjustRotationByTarget()
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * turnSpeed);
        }

        /// <summary>
        /// Moves the zoom transform in or out depending on the delta mouse scroll value. This is applied to the zoom transform to 
        /// make zooming simpler by excluding rotation data.
        /// </summary>
        private void AdjustZoom()
        {
            targetZoom = Mathf.Clamp(targetZoom + Input.mouseScrollDelta.y * zoomStep, -maxZoom, -minZoom);

            // TODO: Update raycast to check entire camera plane
            RaycastHit obsucrityHit;
            if (Physics.Raycast(target.position, -zoomTransform.forward, out obsucrityHit, -targetZoom, avoidanceMask, QueryTriggerInteraction.Ignore))
            {
                float zoom = Mathf.MoveTowards(zoomTransform.localPosition.z, -obsucrityHit.distance, Time.deltaTime * correctionSpeed);
                zoomTransform.localPosition = new Vector3(0.0f, 0.0f, zoom);
            }
            else
            {
                float zoom = Mathf.MoveTowards(zoomTransform.localPosition.z, targetZoom, Time.deltaTime * zoomSpeed);
                zoomTransform.localPosition = new Vector3(0.0f, 0.0f, zoom);
            }
        }

        #endregion
    }
}