using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    /// <summary>
    /// DEPRECATED: This class was found to be insuffecient when working with planet and galaxy transforms that did not 
    /// have the identity rotation. As a result unexpected behaviour happens if the galaxy or a planet is rotated to achieve
    /// a more dynamic level.
    /// </summary>
    [RequireComponent(typeof(GalaxyStats))]
    public class OnHitCubeRotatorGroupFurthestFromOrigin : MonoBehaviour, IOnHitBehaviour
    {
        #region Fields
        
        public float maxDistanceFromAxis;
        public LayerMask groundMask;
        public float rotationSpeed = 18.0f;
        public List<GalaxyStats> friendGroups;

        private GalaxyStats myStats;
        private int activeRotations;
        private Vector3 currentRotationAxis;

        #endregion

        #region MonoBehaviour Methods

        /// <summary>
        /// Find required components and generate a unique physics group id.
        /// </summary>
        protected void Awake()
        {
            myStats = GetComponent<GalaxyStats>();
            myStats.PhysicsGroupId = GameManager.GenerateNewPhysicsGroupId();

            foreach (PlanetStats planet in GetComponentsInChildren<PlanetStats>())
            {
                planet.PhysicsGroupId = myStats.PhysicsGroupId;
            }
        }

        #endregion

        #region IOnHitBehaviour Methods

        /// <summary>
        /// Begins the rotation logic.
        /// </summary>
        /// <param name="user">The character that caused the hit.</param>
        /// <param name="hitPlanet">The planet that was hit.</param>
        /// <param name="hitPoint">The world coordinates of the hit.</param>
        /// <param name="hitNormal">The world normal of the hit.</param>
        public void OnHit(PlayerStats stats, PlanetStats hitPlanet, Vector3 hitPoint, Vector3 hitNormal)
        {
            Rotate(stats, hitPlanet, hitPoint, hitNormal);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Computes the target rotation direction based on where the hit planet was hit and from which direction. The target rotation direcition
        /// will be along the axis perpendicular to the hit normal that is furthest from the galaxy origin. The affected blocks will also be calculated
        /// based on the target rotation axis and the maximum check distance from the axis. After a rotation is calculate it is started in a 
        /// coroutine if none of the affected blocks are currently being rotated.
        /// </summary>
        /// <param name="user">The character that caused the hit.</param>
        /// <param name="hitPlanet">The planet that was hit.</param>
        /// <param name="hitPoint">The world coordinates of the hit.</param>
        /// <param name="hitNormal">The world normal of the hit.</param>
        private void Rotate(PlayerStats stats, PlanetStats hitPlanet, Vector3 hitPoint, Vector3 hitNormal)
        {
            Vector3 galaxyLocalHit = transform.InverseTransformPoint(hitPoint);
            Vector3 galaxyLocalNormal = transform.InverseTransformDirection(hitNormal);

            Vector3 columnRowOffset = Vector3.zero;
            Vector3 overLapSize = Vector3.zero;
            bool positiveRotation = true;

            // if X greatest, rotate based on Y and Z
            if (Mathf.Abs(galaxyLocalNormal.x) > Mathf.Abs(galaxyLocalNormal.y) && Mathf.Abs(galaxyLocalNormal.x) > Mathf.Abs(galaxyLocalNormal.z))
            {
                if (Mathf.Abs(galaxyLocalHit.y) > Mathf.Abs(galaxyLocalHit.z)) // rotate on the Z axis
                {
                    overLapSize = new Vector3(maxDistanceFromAxis, maxDistanceFromAxis, 0.1f);
                    columnRowOffset = new Vector3(0.0f, 0.0f, galaxyLocalHit.z);
                }
                else // rotate on the Y axis
                {
                    overLapSize = new Vector3(maxDistanceFromAxis, 0.1f, maxDistanceFromAxis);
                    columnRowOffset = new Vector3(0.0f, galaxyLocalHit.y, 0.0f);
                }
            }
            // if Y greatest, rotate based on X and Z
            else if (Mathf.Abs(galaxyLocalNormal.y) > Mathf.Abs(galaxyLocalNormal.z))
            {
                if (Mathf.Abs(galaxyLocalHit.z) > Mathf.Abs(galaxyLocalHit.x)) // rotate on the X axis
                {
                    overLapSize = new Vector3(0.1f, maxDistanceFromAxis, maxDistanceFromAxis);
                    columnRowOffset = new Vector3(galaxyLocalHit.x, 0.0f, 0.0f);
                }
                else // rotate on the Z axis
                {
                    overLapSize = new Vector3(maxDistanceFromAxis, maxDistanceFromAxis, 0.1f);
                    columnRowOffset = new Vector3(0.0f, 0.0f, galaxyLocalHit.z);
                }
            }
            // if Z greatest, rotate based on X and Y
            else
            {
                if (Mathf.Abs(galaxyLocalHit.x) > Mathf.Abs(galaxyLocalHit.y)) // rotate on the y axis
                {
                    overLapSize = new Vector3(maxDistanceFromAxis, 0.1f, maxDistanceFromAxis);
                    columnRowOffset = new Vector3(0.0f, galaxyLocalHit.y, 0.0f);
                }
                else // rotate on the X axis
                {
                    overLapSize = new Vector3(0.1f, maxDistanceFromAxis, maxDistanceFromAxis);
                    columnRowOffset = new Vector3(galaxyLocalHit.x, 0.0f, 0.0f);
                }
            }

            Vector3 rotationAxis = transform.TransformDirection(columnRowOffset.normalized);
            columnRowOffset = transform.rotation * columnRowOffset;

            Vector3 playerDirection = hitPoint - transform.position;
            positiveRotation = Vector3.Dot(Vector3.Cross(hitNormal, playerDirection), rotationAxis) > 0;

            Vector3 absDifference = new Vector3(Mathf.Abs(currentRotationAxis.x) - Mathf.Abs(rotationAxis.x),
                                                Mathf.Abs(currentRotationAxis.y) - Mathf.Abs(rotationAxis.y),
                                                Mathf.Abs(currentRotationAxis.z) - Mathf.Abs(rotationAxis.z));
            if (activeRotations == 0 || 0.01f > absDifference.magnitude)
            {
                currentRotationAxis = rotationAxis;
                StartCoroutine(RotateCluster(columnRowOffset, overLapSize, positiveRotation));
            }
        }

        /// <summary>
        /// Rotates along the column row offset direction in either the positive or negative direction. The rotation will be canceled 
        /// if any of the affected planets (determined by overlap size and column row offset) are currently in motion.
        /// </summary>
        /// <param name="columnRowOffset">Rotation axis multiplied by the distance on the axis of the hit point.</param>
        /// <param name="overlapSize">The distance used to check for affected planets.</param>
        /// <param name="positiveRotation">True if we should rotate in the positive direction.</param>
        /// <returns>Returns a single frame delay during the rotation effects.</returns>
        private IEnumerator RotateCluster(Vector3 columnRowOffset, Vector3 overlapSize, bool positiveRotation)
        {
            Collider[] affectedPlanetColliders = Physics.OverlapBox(transform.position + columnRowOffset, overlapSize / 2.0f, transform.rotation, groundMask, QueryTriggerInteraction.Ignore);
            List<PlanetStats> affectedPlanets = new List<PlanetStats>();
            List<int> friendGroupIds = new List<int>();
            friendGroupIds.Add(myStats.PhysicsGroupId);
            Vector3 cachedRotationAxis = currentRotationAxis;

            foreach (GalaxyStats friend in friendGroups)
            {
                friendGroupIds.Add(friend.PhysicsGroupId);
            }

            // verify can rotate
            foreach (Collider planetCollider in affectedPlanetColliders)
            {
                PlanetStats planet = planetCollider.GetComponentInParent<PlanetStats>();
                if (planet != null && friendGroupIds.Contains(planet.PhysicsGroupId))
                {
                    if (!affectedPlanets.Contains(planet))
                        affectedPlanets.Add(planet);

                    if (planet.IsInMotion)
                    {
                        yield break;
                    }
                }
            }

            // initialize rotation data
            activeRotations++;
            float remainingRotation = 90.0f;

            foreach (PlanetStats planet in affectedPlanets)
            {
                planet.transform.parent = transform;
                planet.IsInMotion = true;
            }

            // rotate
            while (remainingRotation > 0.0f)
            {
                float rotationAmount = (positiveRotation ? 1 : -1) * Mathf.Min(Time.deltaTime * rotationSpeed, remainingRotation);

                foreach (PlanetStats planet in affectedPlanets)
                {
                    planet.transform.RotateAround(transform.position, cachedRotationAxis, rotationAmount);
                }

                remainingRotation -= Mathf.Abs(rotationAmount);
                yield return new WaitForFixedUpdate();
            }

            // uninitialize rotation data
            foreach (PlanetStats planet in affectedPlanets)
            {
                planet.IsInMotion = false;
            }

            activeRotations--;
        }

        #endregion
    }
}