using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    [RequireComponent(typeof(Rigidbody))]
    public class Gravity : MonoBehaviour
    {
        #region Fields

        [Header("Gravity Detection")]
        public LayerMask gravityMask;
        public Transform gravityCastPoint;
        public float gravityCheckDistance = 1000.0f;

        [Header("Gravity Correction")]
        public float gravityAcceleration = 9.8f; // TODO: Maybe move this to it's own script for planets with different gravity strength
        public float rotationCorrectionRate = 10.0f; // this should snap almost instantly
        public float gravityCorrectionRate = 20.0f;
        public float maxGravitySpeed = 20.0f;

        private Transform myTransform;
        private Rigidbody body;
        private bool faceGravity;

        private float gravityDelay = 0.0f;
        private float gravityDelayTimer;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Find required components.
        /// </summary>
        protected void Awake()
        {
            myTransform = transform;

            body = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Initialize the gravity variables.
        /// </summary>
        protected void Start()
        {
            CanApplyGravity = true;
            faceGravity = true;
            CanChangeGravityDirection = true;
            ForceApplyGravity = false;
        }

        /// <summary>
        /// Applies gravity physics and rotation updates. Applying rotation updates outside of fixed update causes graphical glitchiness due 
        /// to the design of the Unity physics engine.
        /// </summary>
        protected void FixedUpdate()
        {
            if (gravityDelayTimer > 0.0f)
            {
                gravityDelay -= Time.deltaTime;
            }

            if (ForceApplyGravity || CanApplyGravity)
            {
                GravityDirection = FindGravityDirection();
                ApplyGravity();
            }

            UpdateRotation();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Value is set to allow or block gravity physics updates. Gravity can be forcibly applied by setting ForceApplyGravity.
        /// </summary>
        public bool CanApplyGravity { get; set; }

        /// <summary>
        /// Value is set to allow or block changes in the gravity direction. Gravity can be forcibly applied by setting ForceApplyGravity.
        /// </summary>
        public bool CanChangeGravityDirection { get; set; }

        /// <summary>
        /// Set to override characters control on gravity.
        /// </summary>
        public bool ForceApplyGravity { get; set; }

        /// <summary>
        /// Value is set to the amount of time to wait before allowing updates to the gravity direction. Until this time has been 
        /// reached no updates will occur unless character control is overridden.
        /// </summary>
        public float GravityDelay
        {
            set { gravityDelay = value; gravityDelayTimer = Mathf.Min(gravityDelay, gravityDelayTimer); }
        }

        /// <summary>
        /// Value is the current direction gravity should be applied in.
        /// </summary>
        public Vector3 GravityDirection { get; private set; }

        #endregion

        #region Private Functions

        /// <summary>
        /// Applies gravity forces in the current gravity direction if allowed. Otherwise no forces or velocity updates are executed.
        /// </summary>
        private void ApplyGravity()
        {
            if (ForceApplyGravity || CanApplyGravity)
            {
                body.AddForce(GravityDirection * gravityAcceleration);

                Vector3 gravityVelocity = Vector3.Project(body.velocity, GravityDirection);
                if (gravityVelocity.magnitude > maxGravitySpeed)
                {
                    body.velocity = body.velocity - gravityVelocity + gravityVelocity.normalized * maxGravitySpeed;
                }
            }
        }

        /// <summary>
        /// Finds the current gravity direction if we are allowed change the current direction of gravity. If we are not then the 
        /// last gravity direction will be returned.
        /// </summary>
        /// <returns>Direction to apply gravity in.</returns>
        private Vector3 FindGravityDirection()
        {
            Vector3 direction = GravityDirection;

            if (ForceApplyGravity || CanChangeGravityDirection)
            {
                Collider[] gravityCollidersInRange = Physics.OverlapSphere(gravityCastPoint.position, gravityCheckDistance, gravityMask, QueryTriggerInteraction.Ignore);
                Vector3 closestPoint = gravityCastPoint.position;
                float minDistance = float.MaxValue;

                foreach (Collider gravityCollider in gravityCollidersInRange)
                {
                    Vector3 colliderClosestPoint = gravityCollider.ClosestPoint(gravityCastPoint.position);
                    float distance = Vector3.Distance(gravityCastPoint.position, colliderClosestPoint);

                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        closestPoint = colliderClosestPoint;
                    }
                }

                direction = Vector3.Normalize(closestPoint - gravityCastPoint.position);

                Vector3 gravityShift = GravityDirection - direction;

                if (gravityShift.magnitude > 0.01f)
                {
                    if (gravityDelayTimer < 0.0f)
                    {
                        gravityDelayTimer = gravityDelay;
                    }
                    else
                    {
                        gravityDelayTimer -= Time.deltaTime;
                        direction = GravityDirection;
                    }
                }
            }

            return direction;
        }

        /// <summary>
        /// Updates the game objects rotation to always keep the bottom facing towards gravity. This will do nothing if the script is 
        /// configured specifically to not face the gravity direction.
        /// </summary>
        private void UpdateRotation()
        {
            if (ForceApplyGravity || faceGravity)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(myTransform.up, -GravityDirection) * myTransform.rotation;
                myTransform.rotation = Quaternion.Slerp(myTransform.rotation, targetRotation, rotationCorrectionRate * Time.deltaTime);
            }
        }

        #endregion
    }
}