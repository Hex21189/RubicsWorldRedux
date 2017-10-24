using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    [RequireComponent(typeof(Animator))]
    public class ChangeFlipdownTheme : MonoBehaviour, IChangeTheme
    {
        #region Fields

        public Theme currentTheme = Theme.None;

        public Texture[] themeTextures = new Texture[(int)Theme.COUNT];
        public Renderer themeRenderer;
        public AnimationClip flipClip;
        private Animator flipdownAnimator;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Finds the required animator.
        /// </summary>
        protected void Awake()
        {
            flipdownAnimator = GetComponent<Animator>();
        }

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
            if (themeTextures.Length != (int)Theme.COUNT)
            {
                Debug.LogWarning("Theme array must be resized by updating the Theme enumerator class.");
                Array.Resize(ref themeTextures, (int)Theme.COUNT);
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
            Texture targetTexture = themeTextures[(int)targetTheme];
            themeRenderer.material.SetTexture("_MainTex", targetTexture);
            currentTheme = targetTheme;
        }

        /// <summary>
        /// Changes the current theme of the component to the target theme. Theme is changed after the animation clip has finished playing once.
        /// </summary>
        /// <param name="targetTheme">Target Theme to change to.</param>
        /// <param name="force">True if we should change Theme even if the current Theme is the same as the target.</param>
        /// <returns>Timed delay while animating. Time is determined by the assigned animation clip.</returns>
        public IEnumerator ChangeThemeRoutine(Theme targetTheme, bool force = false)
        {
            if (!force && currentTheme == targetTheme)
                yield break;

            currentTheme = targetTheme; // TODO: check for race condition if two players are fighting over this
            Texture targetTexture = themeTextures[(int)targetTheme];
            flipdownAnimator.SetTrigger("Toggle");

            yield return new WaitForSeconds(flipClip.length);

            themeRenderer.material.SetTexture("_MainTex", targetTexture);

            flipdownAnimator.SetTrigger("Toggle");
            
        }

        #endregion
    }
}