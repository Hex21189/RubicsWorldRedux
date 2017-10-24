#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    // TODO: Delete prior to release
    public class RandomizeTransformUtility : MonoBehaviour
    {
        #region Fields

        public bool randomPosition = false;
        public Vector3 minPosition;
        public Vector3 maxPosition;

        public bool randomRotation = false;

        public bool randomScale = false;
        public float maxScale = 1.0f;
        public float minScale = 0.1f;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// If Random Position is selected, this game object is moved to a random local position between min and max position. 
        /// If Random Rotation is selected, this game object is rotated randomly along the local y axis.
        /// If Random Scale is selected, this game object is randomly uniformly scaled between min and max scale.
        /// </summary>
        protected void OnDrawGizmos()
        {
            if (randomPosition)
            {
                randomPosition = false;
                transform.localPosition = new Vector3(Random.Range(minPosition.x, maxPosition.x), Random.Range(minPosition.y, maxPosition.y), Random.Range(minPosition.z, maxPosition.z));
            }

            if (randomRotation)
            {
                randomRotation = false;
                transform.localRotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
            }

            if (randomScale)
            {
                randomScale = false;
                transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            }
        }

        #endregion
    }
}

#endif