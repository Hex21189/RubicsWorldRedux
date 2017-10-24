using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class PlayerWrangler : MonoBehaviour
    {
        #region Fields

        public List<GalaxyStats> galaxies;
        public List<Gravity> playerGravityControllers;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Checks every frame if all the gravity controlled objects (players usually) are within range of any of the galaxies. If not
        /// then the out of range object will have gravity forcibly applied until it is back in range.
        /// </summary>
        protected void Update()
        {
            foreach (Gravity gravityController in playerGravityControllers)
            {
                bool inRange = false;

                foreach (GalaxyStats galaxy in galaxies)
                {
                    if (galaxy.maxPlayerDistance > Vector3.Distance(gravityController.transform.position, galaxy.transform.position))
                    {
                        inRange = true;
                    }
                }

                if (!inRange)
                {
                    gravityController.ForceApplyGravity = true;

                    CharacterMovement movement = gravityController.GetComponent<CharacterMovement>();
                    if (movement)
                    {
                        movement.OverrideGravity = false;
                    }
                }
                else if (gravityController.ForceApplyGravity)
                {
                    gravityController.ForceApplyGravity = false;
                }
            }
        }

        #endregion
    }
}