using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class CharacterAnimator : MonoBehaviour
    {
        #region Fields

        private Animator animator;

        #endregion

        #region Monobehaviour Methods

        /// <summary>
        /// Verifies that an animator component is found somewhere on the character. If none is present then this class iksn't needed.
        /// </summary>
        protected void Start()
        {
            animator = GetComponentInChildren<Animator>();

            if (!animator)
            {
                Debug.LogError("Character animation controller requires animator in children to animate.");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Set to true if a jump animation should play. This should only happen once at the start of a jump but chained
        /// calls can be cleared by setting the value to false.
        /// </summary>
        public bool Jump
        {
            set
            {
                if (value)
                {
                    animator.SetTrigger("Jump");
                }
                else
                {
                    animator.ResetTrigger("Jump");
                }
            }
        }

        /// <summary>
        /// Set to ture if the ground pound animation should play. This should only happen once during a jump but
        /// chained calls can be canceled by setting the value to false.
        /// </summary>
        public bool GroundPound
        {
            set
            {
                if (value)
                {
                    animator.SetTrigger("GroundPound");
                }
                else
                {
                    animator.ResetTrigger("GroundPound");

                }
            }
        }

        /// <summary>
        /// Set to current player movement speed.
        /// </summary>
        public float Speed { set { animator.SetFloat("Speed", value); } }

        /// <summary>
        /// Set to true if a grounded animation should be played.
        /// </summary>
        public bool IsGrounded { set { animator.SetBool("IsGrounded", value); } }

        #endregion
    }
}