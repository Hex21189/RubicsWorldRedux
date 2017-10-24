using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class GravityOverrideOnEnter : MonoBehaviour
    {
        #region Fields

        public float gravityMultiplier = 2.0f;

        private List<Gravity> affectedGravityControllers = new List<Gravity>();
        private List<float> originalGravities = new List<float>();

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Adds the gravity object we have collieded with to the list of affected colliders and forces gravity 
        /// to be applied. The amount of gravity applied can be increased by setting a gravity multiplier.
        /// </summary>
        /// <param name="other">The collider that entered this trigger.</param>
        protected void OnTriggerEnter(Collider other)
        {
            Gravity gravityControl = other.GetComponent<Gravity>();

            if (gravityControl && !affectedGravityControllers.Contains(gravityControl))
            {
                affectedGravityControllers.Add(gravityControl);
                originalGravities.Add(gravityControl.gravityAcceleration);
                gravityControl.gravityAcceleration *= gravityMultiplier;
                gravityControl.ForceApplyGravity = true;
            }
        }

        /// <summary>
        /// Removes the gravity object we have collieded with to the list of affected colliders and stops forcing 
        /// gravity to be applied. If multiple scripts are forcing gravity to be applied this could cause bugs.
        /// TODO: change ForceApplyGravity from a bool to a list of components forcing gravity.
        /// </summary>
        /// <param name="other">The collider that left this trigger.</param>
        protected void OnTriggerExit(Collider other)
        {
            Gravity gravityControl = other.GetComponent<Gravity>();

            if (gravityControl && affectedGravityControllers.Contains(gravityControl))
            {
                int index = affectedGravityControllers.IndexOf(gravityControl);
                gravityControl.gravityAcceleration = originalGravities[index];
                gravityControl.ForceApplyGravity = false;
                originalGravities.RemoveAt(index);
                affectedGravityControllers.RemoveAt(index);
            }
        }

        #endregion
    }
}