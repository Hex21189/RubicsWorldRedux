#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    // TODO: Delete prior to release.
    public class PlanetSwapperUtility : MonoBehaviour
    {
        #region Fields

        public GameObject targetPrefab;
        public bool swap;
        public bool randomRotation;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// If Swap is selected this game object is destroied and replaced with a new instance of the target prefab. The 
        /// local scale, position, and parent are retained and a new swapper untility script instance is added. If Random 
        /// Rotation is selected then this game object is rotated at random right angles.
        /// </summary>
        protected void OnDrawGizmos()
        {
            bool swapped = false;

            if (swap)
            {
                swap = false;
                if (targetPrefab)
                {
                    GameObject planet = PrefabUtility.InstantiatePrefab(targetPrefab) as GameObject;
                    planet.transform.position = transform.position;
                    planet.transform.localScale = transform.localScale;
                    planet.transform.parent = transform.parent;
                    planet.AddComponent<PlanetSwapperUtility>();

                    targetPrefab = null;
                    swapped = true;
                }
            }

            if (randomRotation)
            {
                randomRotation = false;
                transform.localRotation = Quaternion.Euler(Random.Range(0, 4) * 90.0f, Random.Range(0, 4) * 90.0f, Random.Range(0, 4) * 90.0f);
            }

            if (swapped)
            {
                DestroyImmediate(gameObject);
            }
        }

        #endregion
    }
}

#endif