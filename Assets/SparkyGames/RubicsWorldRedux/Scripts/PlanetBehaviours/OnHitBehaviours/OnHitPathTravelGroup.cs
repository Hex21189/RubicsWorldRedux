using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class OnHitPathTravelGroup : MonoBehaviour, IOnHitBehaviour
    {
        #region Fields

        public bool debug;

        public Vector3[] path;
        public Vector3 startRotation;
        public Vector3 finishRotation;
        public float speed;
        public bool interpolateRotation = true;
        [Range(0.0f, 1.0f)]
        public float beginFacingFinalRotationAfter = 0.9f;

        private bool move = false;
        private bool forward = false;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Draw debug information on screen. If debug is set to true a custom spline will be generated on each frame
        /// to show the path of movement the galaxy will take.
        /// </summary>
        protected void OnDrawGizmos()
        {
            if (path.Length >= 2)
            {
                Color oldColor = Gizmos.color;
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(path[0], Vector3.one);

                for (int i = 1; i < path.Length - 1; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(path[i], Vector3.one);
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawCube(path[path.Length - 1], Vector3.one);
                Gizmos.color = oldColor;
            }

            if (debug)
            {
                if (path.Length > 3)
                {
                    SplineCurve debugSpline = new SplineCurve(path);
                    debugSpline.DrawGizmos(Color.blue);
                }
            }
        }

        #endregion

        #region IOnHitBehaviour Methods

        /// <summary>
        /// Beings the movement logic. A movement request could be canceled if this group is already moving.
        /// </summary>
        /// <param name="user">The character that caused the hit.</param>
        /// <param name="hitPlanet">The planet that was hit.</param>
        /// <param name="hitPoint">The world coordinates of the hit.</param>
        /// <param name="hitNormal">The world normal of the hit.</param>
        public void OnHit(PlayerStats stats, PlanetStats hitPlanet, Vector3 hitPoint, Vector3 hitNormal)
        {
            StartCoroutine(MoveAlongCurve());
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a custom spline with the current points assigned to this travel ground and interpolates along the spline
        /// according to the movement speed assigned to this group. Move is set to true while the coroutine is in effect and
        /// false upon finishing to protect against multiple calls.
        /// </summary>
        /// <returns>Single frame delays while moving along the spline.</returns>
        private IEnumerator MoveAlongCurve()
        {
            if (!move)
            {
                SplineCurve spline = new SplineCurve(path);

                move = true;
                forward = !forward;

                float progress = 0.0f;
                float rotationProgress = 0.0f;
                Quaternion preInterpRotation = transform.rotation;

                while (progress < 1.0f)
                {
                    Vector3 currentPosition = spline.Point(forward ? progress : 1.0f - progress);
                    progress = Mathf.Min(progress + Time.deltaTime * (speed / spline.distance), 1.0f);
                    Vector3 nextPosition = spline.Point(forward ? progress : 1.0f - progress);

                    Quaternion targetRotation = transform.rotation;
                    float rotationStep = Time.deltaTime * 5.0f;

                    if (progress > beginFacingFinalRotationAfter)
                    {
                        if (rotationProgress == 0)
                        {
                            preInterpRotation = transform.rotation;
                        }

                        rotationProgress = (progress - beginFacingFinalRotationAfter) * 1.0f / (1.0f - beginFacingFinalRotationAfter);
                        rotationStep = rotationProgress;

                        if (forward)
                        {
                            targetRotation = Quaternion.Euler(finishRotation);
                        }
                        else
                        {
                            targetRotation = Quaternion.Euler(startRotation);
                        }
                    }
                    else if (interpolateRotation)
                    {
                        preInterpRotation = transform.rotation;
                        Vector3 direction = nextPosition - currentPosition;

                        if (direction.magnitude > 0)
                        {
                            targetRotation = Quaternion.LookRotation(forward ? direction : -direction);
                        }
                    }
                    else
                    {
                        preInterpRotation = targetRotation;
                    }

                    transform.rotation = Quaternion.Slerp(preInterpRotation, targetRotation, rotationStep);
                    transform.position = nextPosition;
                    yield return new WaitForFixedUpdate();
                }

                move = false;
            }
        }

        #endregion
    }
}