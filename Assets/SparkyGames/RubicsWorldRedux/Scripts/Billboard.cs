using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class Billboard : MonoBehaviour
    {
        #region Fields

        public bool keepUp = true;
        public Transform target;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Rotates this game object to look towards the target transform while optionaly keeping the same up direction or generating a new one.
        /// </summary>
        protected void Update()
        {
            if (keepUp)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(target.position - transform.position, transform.up), transform.up);
            }
            else
            {
                transform.rotation = target.rotation;
            }
        }

        #endregion
    }
}