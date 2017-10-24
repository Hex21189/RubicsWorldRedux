using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public interface IOnHitBehaviour
    {
        /// <summary>
        /// Triggers on hit behaviours of component script. It is possible that no effect at all will be triggered if
        /// the child script chooses to ignore this call.
        /// </summary>
        /// <param name="user">The character that caused the hit.</param>
        /// <param name="hitPlanet">The planet that was hit.</param>
        /// <param name="hitPoint">The world coordinates of the hit.</param>
        /// <param name="hitNormal">The world normal of the hit.</param>
        void OnHit(PlayerStats user, PlanetStats hitPlanet, Vector3 hitPoint, Vector3 hitNormal);
    }
}