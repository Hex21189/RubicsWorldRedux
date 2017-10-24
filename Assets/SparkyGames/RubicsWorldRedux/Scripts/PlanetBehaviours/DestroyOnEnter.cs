using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class DestroyOnEnter : MonoBehaviour
    {
        #region Fields

        private PlanetStats myPlanet;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Finds the attached planet stats. Planet Stats are not required for use but is highly recommended and may 
        /// become a future requirement.
        /// </summary>
        protected void Awake()
        {
            myPlanet = GetComponentInParent<PlanetStats>();

            if (!myPlanet)
            {
                Debug.LogWarning("Warning: Destroyer script not associated with a planet.");
            }
        }

        /// <summary>
        /// Destories any planet that enters this ones trigger if and only if it is either destroyable and is not this planet itself.
        /// </summary>
        /// <param name="other"></param>
        protected void OnTriggerEnter(Collider other)
        {
            PlanetStats otherPlanet = other.GetComponentInParent<PlanetStats>();

            if (otherPlanet && otherPlanet.isDestroyable && (!myPlanet || myPlanet != otherPlanet))
            {
                PlayerStats[] players = otherPlanet.GetComponentsInChildren<PlayerStats>();

                foreach (PlayerStats player in players)
                {
                    player.transform.parent = null;
                }

                otherPlanet.gameObject.SetActive(false); // TODO: Disable and spawn destroyed mesh.
            }
        }

        #endregion
    }
}