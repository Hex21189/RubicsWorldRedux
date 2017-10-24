using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class OnHitThemeChange : MonoBehaviour, IOnHitBehaviour
    {
        #region Fields

        private IChangeTheme[] themeComponents;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Compiles an array of child IChangeTheme components attached to this object.
        /// </summary>
        protected void Awake()
        {
            themeComponents = GetComponentsInChildren<IChangeTheme>();
        }

        #endregion

        #region IOnHitBehaviour Methods

        /// <summary>
        /// Changes the theme of all child IChangeTheme components that were attached at the start of the game to the players theme.
        /// </summary>
        /// <param name="user">The character that caused the hit.</param>
        /// <param name="hitPlanet">The planet that was hit.</param>
        /// <param name="hitPoint">The world coordinates of the hit.</param>
        /// <param name="hitNormal">The world normal of the hit.</param>
        public void OnHit(PlayerStats stats, PlanetStats hitPlanet, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (stats != null)
            {
                SetTheme(stats.theme);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calls the change theme coroutine on all child IChangeTheme components that were attached at the start of the game.
        /// A Theme of Count will be ignored as this is invalid.
        /// </summary>
        /// <param name="targetTheme">Theme to change the components to, cannot be Count.</param>
        private void SetTheme(Theme targetTheme)
        {
            if (targetTheme < Theme.COUNT)
            {
                foreach (IChangeTheme themeChanger in themeComponents)
                {
                    StartCoroutine(themeChanger.ChangeThemeRoutine(targetTheme));
                }
            }
        }

        #endregion
    }
}