using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    [RequireComponent(typeof(CharacterMovement))]
    public class CameraRelativePlayerMovement : MonoBehaviour
    {
        #region Fields

        public Camera gameCamera;
        private CharacterMovement movement;

        public float delayBeforeReenablingGravity = 0.1f;
        private float reenableGravityTimer = 0.0f;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Finds and verifies references to required scripts.
        /// </summary>
        protected void Awake()
        {
            movement = GetComponent<CharacterMovement>();

            if (!gameCamera)
            {
                Debug.LogError("No game camera found for player character. Required for camera relative movement.");
            }
        }

        /// <summary>
        /// Reads input from the Input controller and passes it to the attached character movement component.
        /// </summary>
        protected void Update()
        {
            // Move
            Vector3 moveDirection = Vector3.zero;

            // TODO: Change to Input.GetAxis
            if (Input.GetKey(KeyCode.W))
            {
                moveDirection.z = 1.0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveDirection.z = -1.0f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                moveDirection.x = 1.0f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                moveDirection.x = -1.0f;
            }

            moveDirection = gameCamera.transform.parent.localRotation * moveDirection;
            moveDirection.y = 0;
            movement.MoveDirection = moveDirection;

            // Jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                movement.Jump = true;
                movement.OverrideGravity = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                movement.Jump = false;
                StartCoroutine(DisableGravityOverride());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a delay before allowing the player to re-enable gravity. This is used to allow the player to jump off a ledge
        /// and plummet straight down without slightly rotating when gravity is re-enabled when the jump button is up and hasn't
        /// been pushed for the second time.
        /// </summary>
        /// <returns>Single frame delays until the gravity re-enable timer has completed.</returns>
        private IEnumerator DisableGravityOverride()
        {
            reenableGravityTimer = delayBeforeReenablingGravity;

            while (reenableGravityTimer > 0.0f)
            {
                reenableGravityTimer -= Time.deltaTime;
                yield return null;
            }

            movement.OverrideGravity = Input.GetKey(KeyCode.Space);
        }

        #endregion
    }
}