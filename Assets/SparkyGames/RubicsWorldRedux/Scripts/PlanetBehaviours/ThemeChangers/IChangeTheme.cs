using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public interface IChangeTheme
    {
        /// <summary>
        /// Instantly changes the theme of this component. Useful for initialization. The theme changed is executed no matter the current theme.
        /// </summary>
        /// <param name="targetTheme">Target Theme to change to.</param>
        void ChangeThemeInstant(Theme targetTheme);

        /// <summary>
        /// Changes the current theme of the component to the target theme. The theme is considered changed instantly but the visual effects 
        /// are delayed for asthetics.
        /// </summary>
        /// <param name="targetTheme">Target Theme to change to.</param>
        /// <param name="force">True if we should change Theme even if the current Theme is the same as the target.</param>
        /// <returns>Delay while change is taking place.</returns>
        IEnumerator ChangeThemeRoutine(Theme targetTheme, bool force = false);
    }
}