using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class GameManager
    {
        #region Fields

        private static int nextPhysicsGroupId = 1; // default value 0 is considered undefined

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Returns a unique physics group ID every time it is called. This is used to make sure galaxies or planets don't interfere 
        /// with each other unless they are designed to.
        /// </summary>
        /// <returns>
        /// Unique physics group ID. A planets/galaxies physics group ID should be verified before being used by a galaxy modificaiton 
        /// script such as a cube rotator.
        /// </returns>
        public static int GenerateNewPhysicsGroupId()
        {
            return nextPhysicsGroupId++;
        }

        #endregion
    }
}