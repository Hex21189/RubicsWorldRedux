using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class ChangeModelTheme : MonoBehaviour, IChangeTheme
    {
        #region Fields

        public Theme currentTheme = Theme.None;

        public GameObject[] themeModelPrefabs = new GameObject[(int)Theme.COUNT];
        public GameObject themeModel;
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
            if (themeModelPrefabs.Length != (int)Theme.COUNT)
            {
                Debug.LogWarning("Theme array must be resized by updating the Theme enumerator class.");
                Array.Resize(ref themeModelPrefabs, (int)Theme.COUNT);
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
            GameObject currentThemeModel = themeModel;
            GameObject nextThemePrefab = themeModelPrefabs[(int)targetTheme];

            if (currentThemeModel)
            {
                Destroy(currentThemeModel);
                currentThemeModel = null;
            }

            if (nextThemePrefab)
            {
                themeModel = Instantiate(nextThemePrefab);
                themeModel.transform.parent = transform;
                themeModel.transform.localPosition = Vector3.zero;
                themeModel.transform.localRotation = Quaternion.identity;
            }
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
            GameObject currentThemeModel = themeModel;
            GameObject nextThemePrefab = themeModelPrefabs[(int)targetTheme];

            if (nextThemePrefab)
            {
                themeModel = Instantiate(nextThemePrefab);
                themeModel.transform.parent = transform;
                themeModel.transform.localPosition = Vector3.zero;
                themeModel.transform.localRotation = Quaternion.identity;
            }

            Renderer[] fadeInRenderers = themeModel ? themeModel.GetComponentsInChildren<Renderer>() : new Renderer[0];
            Renderer[] fadeOutRenderers = currentThemeModel ? currentThemeModel.GetComponentsInChildren<Renderer>() : new Renderer[0];
            float fadeTimer = 1.0f / fadeRate;

            foreach (Renderer childRenderer in fadeInRenderers)
            {
                Color currentColor = childRenderer.material.color;
                currentColor.a = 0.0f;
                childRenderer.material.color = currentColor;
            }

            while (fadeTimer > 0.0f && currentTheme == targetTheme)
            {
                fadeTimer -= Time.deltaTime;

                foreach (Renderer childRenderer in fadeOutRenderers)
                {
                    Color currentColor = childRenderer.material.color;
                    currentColor.a -= fadeRate * Time.deltaTime;
                    childRenderer.material.color = currentColor;
                }

                foreach (Renderer childRenderer in fadeInRenderers)
                {
                    Color currentColor = childRenderer.material.color;
                    currentColor.a += fadeRate * Time.deltaTime;
                    childRenderer.material.color = currentColor;
                }

                yield return null;
            }

            if (currentThemeModel)
                Destroy(currentThemeModel);
        }

        #endregion
    }
}