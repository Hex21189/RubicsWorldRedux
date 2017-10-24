using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class PlayerStats : MonoBehaviour
    {
        #region Fields

        public Theme theme;

        #endregion

        #region Monobehaviour Methods

        /// <summary>
        /// Verify the player configuration.
        /// </summary>
        protected void Awake()
        {
            if (theme >= Theme.None)
            {
                Debug.LogError("Invalid player theme: " + theme);
            }
        }

        #endregion
    }
}