using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class ChangeColorTheme : MonoBehaviour, IChangeTheme
    {
        #region Fields

        public Theme currentTheme = Theme.None;

        public Color[] themeColors = new Color[(int)Theme.COUNT];
        public Renderer themeRenderer;
        public float fadeRate = 0.5f;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Loads the default theme.
        /// </summary>
        protected void Start()
        {
            ChangeThemeInstant(currentTheme);
        }

        /// <summary>
        /// Prevents users from resizing the Theme array directly. This should only be resized by updating the Theme enumerator.
        /// </summary>
        protected void OnValidate()
        {
            if (themeColors.Length != (int)Theme.COUNT)
            {
                Debug.LogWarning("Theme array must be resized by updating the Theme enumerator class.");
                Array.Resize(ref themeColors, (int)Theme.COUNT);
            }
        }

        #endregion

        #region IChangeTheme Methods

        /// <summary>
        /// Instantly changes the theme of this component. Useful for initialization. The theme changed is executed no matter the current theme.
        /// </summary>
        /// <param name="targetTheme">Target Theme to change to.</param>
        public void ChangeThemeInstant(Theme targetTheme)
        {
            currentTheme = targetTheme;
            themeRenderer.material.color = themeColors[(int)targetTheme];
        }

        /// <summary>
        /// Changes the current theme of the component to the target theme. The theme is considered changed instantly but the visual effects 
        /// are delayed for asthetics.
        /// </summary>
        /// <param name="targetTheme">Target Theme to change to.</param>
        /// <param name="force">True if we should change Theme even if the current Theme is the same as the target.</param>
        /// <returns>Single frame delay while fading.</returns>
        public IEnumerator ChangeThemeRoutine(Theme targetTheme, bool force = false)
        {
            if (!force && currentTheme == targetTheme)
                yield break;

            currentTheme = targetTheme;

            Color targetColor = themeColors[(int)targetTheme];
            float fadeTimer = 1.0f / fadeRate;
            float blend = 0.0f;

            while (fadeTimer > 0.0f && blend < 1.0f && currentTheme == targetTheme)
            {
                fadeTimer -= Time.deltaTime;
                blend = Mathf.Min(blend + fadeRate * Time.deltaTime, 1.0f);
                themeRenderer.material.color = Color.Lerp(themeRenderer.material.color, targetColor, blend);
                yield return null;
            }
        }

        #endregion
    }
}