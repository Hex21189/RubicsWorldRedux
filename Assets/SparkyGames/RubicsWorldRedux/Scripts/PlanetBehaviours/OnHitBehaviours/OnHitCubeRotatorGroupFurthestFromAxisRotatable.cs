using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    [RequireComponent(typeof(GalaxyStats))]
    public class OnHitCubeRotatorGroupFurthestFromAxisRotatable : MonoBehaviour, IOnHitBehaviour
    {
        #region Fields

        public float maxDistanceFromAxis;
        public LayerMask groundMask;
        public float rotationSpeed;
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
        public void OnHit(PlayerStats user, PlanetStats hitPlanet, Vector3 hitPoint, Vector3 hitNormal)
        {
            Vector3 galaxyHitPoint = hitPoint - transform.position;
            Vector3 planetHitPoint = hitPoint - hitPlanet.transform.position;

            // 1.) Find the normal axis
            float distUp = Vector3.Project(hitNormal, transform.up).magnitude;
            float distForward = Vector3.Project(hitNormal, transform.forward).magnitude;
            float distRight = Vector3.Project(hitNormal, transform.right).magnitude;

            Vector3 normalAxis;
            Vector3 closestRotationAxis;
            Vector3 furthestRotationAxis;
            Vector3 rotationAxis;
            Vector3 unusedRotationAxis; //

            if (distUp > distForward && distUp > distRight)
            {
                normalAxis = transform.up;
                closestRotationAxis = transform.forward; // default, may not be correct
                furthestRotationAxis = transform.right; // default, may not be correct
            }
            else if (distForward > distRight)
            {
                normalAxis = transform.forward;
                closestRotationAxis = transform.up; // default, may not be correct
                furthestRotationAxis = transform.right; // default, may not be correct
            }
            else
            {
                normalAxis = transform.right;
                closestRotationAxis = transform.forward; // default, may not be correct
                furthestRotationAxis = transform.up; // default, may not be correct
            }

            // 2.) Find which axis we are most likely to rotate around (the furthest) and which one we are least likely to rotate around (closest)
            if (Vector3.Project(galaxyHitPoint, furthestRotationAxis).magnitude >
                Vector3.Project(galaxyHitPoint, closestRotationAxis).magnitude) // projecting on to the opposite axis
            {
                Vector3 temp = closestRotationAxis;
                closestRotationAxis = furthestRotationAxis;
                furthestRotationAxis = temp;
            }

            // 3.) Determine the true rotation axis. If there is no planet next to this one in the furthest rotation axis direction (i.e. we are on the edge),
            //     Then we rotate around the closest axis if an only if we are closer to the closest rotation axis than the furthest rotation axis. Otherwise,
            //     we rotate around the furthest axis.
            RaycastHit hitTest;
            bool isPositive = Vector3.Angle(furthestRotationAxis, Vector3.ProjectOnPlane(planetHitPoint, normalAxis)) < 90.0f;

            if (!Physics.Raycast(hitPlanet.transform.position, isPositive ? furthestRotationAxis : -furthestRotationAxis,
                                 out hitTest, 26.0f, groundMask, QueryTriggerInteraction.Ignore) &&
                Vector3.Project(planetHitPoint, closestRotationAxis).magnitude < Vector3.Project(planetHitPoint, furthestRotationAxis).magnitude)
            {
                rotationAxis = closestRotationAxis;
                unusedRotationAxis = furthestRotationAxis;
            }
            else
            {
                rotationAxis = furthestRotationAxis;
                unusedRotationAxis = closestRotationAxis;
            }

            // 4.) Determine if we should rotate in a positive or negative direction.
            isPositive = AngleDirection(rotationAxis, galaxyHitPoint, normalAxis) < 1.0f;

            // 5.) Determine how to find the affected planets.
            Vector3 overlapSize = maxDistanceFromAxis * (Quaternion.Inverse(transform.rotation) * unusedRotationAxis + Quaternion.Inverse(transform.rotation) * normalAxis).normalized;
            Vector3 rotationCenter = transform.position + Vector3.Project(galaxyHitPoint, rotationAxis);

            // 6.) Rotate if we are not currently rotating or if we are rotating on the same axis (positive and negative doesn't matter)
            Vector3 absDifference = new Vector3(Mathf.Abs(currentRotationAxis.x) - Mathf.Abs(rotationAxis.x),
                                                Mathf.Abs(currentRotationAxis.y) - Mathf.Abs(rotationAxis.y),
                                                Mathf.Abs(currentRotationAxis.z) - Mathf.Abs(rotationAxis.z));

            if (activeRotations == 0 || 0.01f > absDifference.magnitude)
            {
                currentRotationAxis = rotationAxis;
                StartCoroutine(RotateCluster(rotationCenter, overlapSize, isPositive));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Left/Right test on the target direction compaired with the forward direction. Result will be 1.0f if the target 
        /// direction is right of the forward direction, -1.0f if the target direcition is left, and 0.0f if they are the same.
        /// </summary>
        /// <param name="forwardDirection">Direction to compair with the target direction.</param>
        /// <param name="targetDirection">Direction to compair with the forward direction.</param>
        /// <param name="up">Direction perpendicular to both the forward and target directions.</param>
        /// <returns></returns>
        private float AngleDirection(Vector3 forwardDirection, Vector3 targetDirection, Vector3 up)
        {
            Vector3 perpendicular = Vector3.Cross(forwardDirection, targetDirection);
            float direction = Vector3.Dot(perpendicular, up);

            if (direction > 0.0)
            {
                return 1.0f;
            }
            else if (direction < 0.0)
            {
                return -1.0f;
            }
            else
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Rotates along the current rotation axis (stored in a field variable so we can check this from seperate rotation requests) 
        /// in either the positive or negative direction. The rotation will be canceled if any of the affected planets (determined by 
        /// overlap size and rotation center) are currently in motion.
        /// </summary>
        /// <param name="rotationCenter">World coordinate point in space we are rotating around.</param>
        /// <param name="overlapSize">The distance used to check for affected planets.</param>
        /// <param name="positiveRotation">True if we should rotate in the positive direction.</param>
        /// <returns>Returns a single frame delay during the rotation effects.</returns>
        private IEnumerator RotateCluster(Vector3 rotationCenter, Vector3 overlapSize, bool positiveRotation)
        {
            Collider[] affectedPlanetColliders = Physics.OverlapBox(rotationCenter, overlapSize / 2.0f, transform.rotation, groundMask, QueryTriggerInteraction.Ignore);
            List<PlanetStats> affectedPlanets = new List<PlanetStats>();
            List<int> friendGroupIds = new List<int>();
            friendGroupIds.Add(myStats.PhysicsGroupId);
            Vector3 cachedRotationAxis = currentRotationAxis;

            foreach (GalaxyStats friend in friendGroups)
            {
                friendGroupIds.Add(friend.PhysicsGroupId);
            }

            // 1.) Verify can rotate.
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

            // 2.) Initialize rotation data.
            activeRotations++;
            float remainingRotation = 90.0f;

            foreach (PlanetStats planet in affectedPlanets)
            {
                planet.transform.parent = transform;
                planet.IsInMotion = true;
                planet.PhysicsGroupId = myStats.PhysicsGroupId; // planet is stolen
            }

            // 3.) Rotate over time.
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

            // 4.) Uninitialize rotation data.
            foreach (PlanetStats planet in affectedPlanets)
            {
                planet.IsInMotion = false;
            }

            activeRotations--;
        }

        #endregion
    }
}