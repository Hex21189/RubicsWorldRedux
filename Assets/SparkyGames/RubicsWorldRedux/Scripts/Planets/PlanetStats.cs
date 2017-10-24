using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class PlanetStats : MonoBehaviour
    {
        #region Fields

        public Vector3 boundingBoxSize = Vector3.one;
        public bool isDestroyable;

        #endregion

        #region Properties

        /// <summary>
        /// True if someone is currently being affected by custom physics algorithms.
        /// </summary>
        public bool IsInMotion { get; set; }

        /// <summary>
        /// The physics group id is used to determine if it should or should not be affected by custom physics algorithms such as rotation.
        /// </summary>
        public int PhysicsGroupId { get; set; }

        #endregion
    }
}