using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    /// <summary>
    /// TODO: change speed value to take movement directions of magnitude 0 - 1 into account. Current movement snaps to full speed.
    /// </summary>
    [RequireComponent(typeof(Gravity))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerStats))]
    public class CharacterMovement : MonoBehaviour
    {
        #region Fields

        [Header("Speed")]
        public float groundedMoveForce = 5.0f;
        public float airMoveForce = 0.1f;
        public float maxSpeed;
        public float jumpSpeed;
        public float groundPoundSpeed;
        public float rotationSpeed;
        public float groundPoundRecoveryTime = 0.5f;

        [Header("Grounding")]
        public Transform groundCheck;
        public float groundCheckDistance;
        public LayerMask groundMask;
        public float airDrag = 0.0f;
        public float groundDrag = 5.0f;
        public float minAirTimeForPound = 0.2f;
        public float maxJumpTime = 0.5f;

        [Header("Graphics")]
        public Transform graphics;

        private PlayerStats stats;
        private Gravity gravity;
        private Rigidbody body;

        private bool isGrounded;
        private Vector3 lookDirection;
        private bool isUsingGroundPound;
        private float airTimeTimer;
        private bool overrideGravity = false;
        private float jumpTimer;
        private bool isJumping = false;
        private float speed = 0.0f;
        private float groundPoundRecoveryTimer;

        private CharacterAnimator animator;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Loads all required scripts. Player animator is considered optional.
        /// </summary>
        protected void Awake()
        {
            animator = GetComponent<CharacterAnimator>();
            body = GetComponent<Rigidbody>();
            gravity = GetComponent<Gravity>();
            stats = GetComponent<PlayerStats>();

            lookDirection = Vector3.forward;

            if (!GetComponent<Collider>())
            {
                Debug.LogError("A valid player should have some sort of collider.");
            }
        }

        /// <summary>
        /// Update players current state variables.
        /// </summary>
        protected void Update()
        {
            isGrounded = Physics.CheckBox(groundCheck.position - transform.up * groundCheckDistance / 2.0f,
                                           Vector3.one * groundCheckDistance / 2.0f, groundCheck.rotation, groundMask, QueryTriggerInteraction.Ignore);

            if (isGrounded)
            {
                RaycastHit downHit;

                if (Physics.Raycast(groundCheck.position, gravity.GravityDirection, out downHit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
                {
                    transform.parent = downHit.transform;

                    if (isUsingGroundPound)
                    {
                        PlanetStats hitPlanet = downHit.transform.GetComponentInParent<PlanetStats>();
                        IOnHitBehaviour[] hitBehaviours = downHit.transform.GetComponentsInParent<IOnHitBehaviour>();

                        foreach (IOnHitBehaviour onHitBehaviour in hitBehaviours)
                        {
                            onHitBehaviour.OnHit(stats, hitPlanet, downHit.point, downHit.normal);
                        }

                        groundPoundRecoveryTimer = groundPoundRecoveryTime;
                    }

                    isUsingGroundPound = false; // ground pound or jump has finished
                }

                gravity.CanChangeGravityDirection = !overrideGravity;
            }
            else
            {
                airTimeTimer += Time.deltaTime;
                transform.parent = null;
            }

            if (MoveDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                graphics.localRotation = Quaternion.Slerp(graphics.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            if (groundPoundRecoveryTimer > 0.0f)
            {
                groundPoundRecoveryTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Character movement and physics updates. This includes both jumping and ground pound updates.
        /// </summary>
        protected void FixedUpdate()
        {
            // 1.) Handle jump requests
            if (Jump)
            {
                if (!isJumping)
                {
                    // 1.1) Jump if grounded
                    if (isGrounded)
                    {
                        if (animator)
                            animator.Jump = true;

                        isJumping = true;
                        jumpTimer = maxJumpTime;
                        isGrounded = false;

                        Vector3 jumpVelocity = body.velocity;
                        jumpVelocity += -gravity.GravityDirection * jumpSpeed;
                        body.velocity = jumpVelocity;

                        gravity.CanApplyGravity = false;
                        gravity.CanChangeGravityDirection = false;
                    }
                    // 1.2) If already jumping and not using ground pound, use ground pound.
                    else if (!isUsingGroundPound && airTimeTimer > minAirTimeForPound)
                    {
                        if (animator)
                            animator.GroundPound = true;

                        Vector3 poundVelocity = body.velocity;
                        poundVelocity += gravity.GravityDirection * groundPoundSpeed;
                        body.velocity = poundVelocity;
                        gravity.CanChangeGravityDirection = !overrideGravity;
                        isUsingGroundPound = true;
                        Jump = false;
                    }
                }

                if (isJumping)
                {
                    jumpTimer -= Time.deltaTime;

                    if (jumpTimer <= 0)
                    {
                        Jump = false;
                    }
                }
            }
            else
            {
                gravity.CanApplyGravity = true;
                gravity.CanChangeGravityDirection = !overrideGravity;
                isJumping = false;
            }

            // 2.) Compute movement physics update based on input.
            Vector3 moveForce = Vector3.zero;

            if (MoveDirection.magnitude > 0)
            {
                MoveDirection.Normalize();
                lookDirection = MoveDirection;

                //MoveDirection = transform.rotation * MoveDirection;
                //moveForce = MoveDirection * groundedMoveForce;
                moveForce = transform.rotation * MoveDirection * groundedMoveForce;
            }

            if (!isGrounded)
            {
                moveForce *= airMoveForce;
            }
            else if (animator)
            {
                animator.Jump = false;
                animator.GroundPound = false;
            }
            
            if (groundPoundRecoveryTimer <= 0.0f)
            {
                body.AddForce(moveForce, ForceMode.VelocityChange);
            }

            // 3.) Limit speed to maximum value. Gravity will need to be removed before computing speed and reintroduced in the final speed value to ensure an acceptable drop speed.
            body.drag = isGrounded && MoveDirection.magnitude > 0.1f ? groundDrag : airDrag;
            Vector3 movementVelocity = Vector3.ProjectOnPlane(body.velocity, -gravity.GravityDirection);
            Vector3 dropVelocity = transform.InverseTransformDirection(body.velocity - movementVelocity);
            speed = movementVelocity.magnitude;

            if (dropVelocity.y <= 0.0f)
            {
                gravity.CanChangeGravityDirection = !overrideGravity;
            }

            if (speed > maxSpeed)
            {
                body.velocity = body.velocity - movementVelocity + movementVelocity.normalized * maxSpeed;
                speed = maxSpeed;
            }

            if (animator)
            {
                animator.IsGrounded = IsGrounded;
                animator.Speed = Speed;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Value is true if the player is currently grounded.
        /// </summary>
        public bool IsGrounded { get { return isGrounded; } }

        /// <summary>
        /// Value is true if the player is using their ground pound ability.
        /// </summary>
        public bool IsUsingGroundPound { get { return isUsingGroundPound; } }

        /// <summary>
        /// Value is and can be set to true if the player should jump.
        /// </summary>
        public bool Jump { get; set; }

        /// <summary>
        /// Value is set to the direction a player should move in or face.
        /// </summary>
        public Vector3 MoveDirection { private get; set; }

        /// <summary>
        /// Set to true if the player is currently under the effects of gravity.
        /// </summary>
        public bool OverrideGravity { set { overrideGravity = value; gravity.CanChangeGravityDirection = !value; } }

        /// <summary>
        /// Value is the current movement speed of the player.
        /// </summary>
        public float Speed { get { return speed; } }

        #endregion
    }
}