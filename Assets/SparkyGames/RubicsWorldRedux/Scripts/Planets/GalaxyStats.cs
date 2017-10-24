using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class GalaxyStats : MonoBehaviour
    {
        #region Fields

        public float maxPlayerDistance;

        #endregion

        #region Properties

        /// <summary>
        /// The physics group id is used to determine if it should or should not be affected by custom physics algorithms such as rotation.
        /// </summary>
        public int PhysicsGroupId { get; set; }

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Draws a bounding sphere around the galaxy. The player should be kept within this area if possible.
        /// </summary>
        protected void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxPlayerDistance);
        }

        #endregion
    }
}