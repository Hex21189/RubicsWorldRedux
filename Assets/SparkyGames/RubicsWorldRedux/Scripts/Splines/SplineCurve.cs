using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    [System.Serializable]
    public class SplineCurve
    {
        #region Fields

        public static int DISTANCE_COUNT = 3; // increase for a more accurate constant speed
        public static int SUBLINE_COUNT = 20; // increase for a more accurate smoothing of the curves into lines

        public bool constantSpeed = true;
        public float distance = 0.0f;
        public bool orientToPath;
        public bool orientToPath2d;

        public Vector3[] points;
        public Vector3[] adjustedPoints;
        public int adjustedPointsLength;
        private int numberOfSections;
        private int currentPointId;

        #endregion

        #region Constructors

        public SplineCurve(Vector3[] newPoints)
        {
            Initialize(newPoints, true);
        }

        public SplineCurve(Vector3[] newPoints, bool constantSpeed)
        {
            this.constantSpeed = constantSpeed;
            Initialize(newPoints, constantSpeed);
        }

        #endregion

        #region Public Methods
                
        public Vector3 Map(float u)
        {
            if (u >= 1f)
            {
                return points[points.Length - 2];
            }
            
            float t = u * (adjustedPointsLength - 1);
            int first = (int)Mathf.Floor(t);
            int next = (int)Mathf.Ceil(t);

            if (first < 0)
            {
                first = 0;
            }

            Vector3 currentPoint = adjustedPoints[first];
            Vector3 nextPoint = adjustedPoints[next];
            float diff = t - first;

            currentPoint = currentPoint + (nextPoint - currentPoint) * diff;

            return currentPoint;
        }

        public Vector3 Interpolate(float t)
        {
            currentPointId = Mathf.Min(Mathf.FloorToInt(t * numberOfSections), numberOfSections - 1);
            float u = t * numberOfSections - currentPointId;

            Vector3 a = points[currentPointId];
            Vector3 b = points[currentPointId + 1];
            Vector3 c = points[currentPointId + 2];
            Vector3 d = points[currentPointId + 3];

            Vector3 val = (.5f * (
                (-a + 3f * b - 3f * c + d) * (u * u * u)
                + (2f * a - 5f * b + 4f * c - d) * (u * u)
                + (-a + c) * u
                + 2f * b));

            return val;
        }

        public float RatioAtPoint(Vector3 pt)
        {
            float minDistance = float.MaxValue;
            int minPointIndex = 0;

            for (int i = 0; i < adjustedPointsLength; i++)
            {
                float distance = Vector3.Distance(pt, adjustedPoints[i]);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minPointIndex = i;
                }
            }

            return minPointIndex / (adjustedPointsLength - 1.0f);
        }

        public Vector3 Point(float ratio)
        {
            float t = ratio > 1.0f ? 1.0f : ratio;
            return constantSpeed ? Map(t) : Interpolate(t);
        }

        public void Place2d(Transform transform, float ratio)
        {
            transform.position = Point(ratio);
            ratio += 0.001f;

            if (ratio <= 1.0f)
            {
                Vector3 direction = Point(ratio) - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }

        public void PlaceLocal2d(Transform transform, float ratio)
        {
            Transform parent = transform.parent;
            if (parent == null) // this has no parent, just do a regular transform
            {
                Place2d(transform, ratio);
                return;
            }

            transform.localPosition = Point(ratio);
            ratio += 0.001f;

            if (ratio <= 1.0f)
            {
                Vector3 nextPoint = Point(ratio);//trans.TransformPoint(  );
                Vector3 direction = nextPoint - transform.localPosition;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.localEulerAngles = new Vector3(0, 0, angle);
            }
        }

        public void Place(Transform transform, float ratio)
        {
            Place(transform, ratio, Vector3.up);
        }

        public void Place(Transform transform, float ratio, Vector3 worldUp)
        {
            transform.position = Point(ratio);
            ratio += 0.001f;

            if (ratio <= 1.0f)
            {
                transform.LookAt(Point(ratio), worldUp);
            }

        }

        public void PlaceLocal(Transform transform, float ratio)
        {
            PlaceLocal(transform, ratio, Vector3.up);
        }

        public void PlaceLocal(Transform transform, float ratio, Vector3 worldUp)
        {
            transform.localPosition = Point(ratio);
            ratio += 0.001f;

            if (ratio <= 1.0f)
            {
                transform.LookAt(transform.parent.TransformPoint(Point(ratio)), worldUp);
            }
        }

        public void DrawGizmos(float t = -1.0f)
        {
            if (adjustedPoints == null || adjustedPoints.Length <= 0)
            {
                return;
            }

            Vector3 lastPoint = adjustedPoints[0];

            for (int i = 0; i < adjustedPointsLength; i++)
            {
                Vector3 currentPoint = adjustedPoints[i];
                Gizmos.DrawLine(lastPoint, currentPoint);
                lastPoint = currentPoint;
            }
        }

        public void DrawGizmos(Color color)
        {
            if (adjustedPointsLength >= 4)
            {
                Vector3 lastPoint = adjustedPoints[0];

                Color colorBefore = Gizmos.color;
                Gizmos.color = color;

                for (int i = 0; i < adjustedPointsLength; i++)
                {
                    Vector3 currentPoint = adjustedPoints[i];
                    Gizmos.DrawLine(lastPoint, currentPoint);
                    lastPoint = currentPoint;
                }

                Gizmos.color = colorBefore;
            }
        }

        public static void DrawGizmos(Transform[] transformPoints, Color color)
        {
            if (transformPoints.Length >= 4)
            {
                Vector3[] points = new Vector3[transformPoints.Length];

                for (int i = 0; i < transformPoints.Length; i++)
                {
                    points[i] = transformPoints[i].position;
                }

                SplineCurve spline = new SplineCurve(points);
                Vector3 lastPoint = spline.adjustedPoints[0];

                Color colorBefore = Gizmos.color;
                Gizmos.color = color;

                for (int i = 0; i < spline.adjustedPointsLength; i++)
                {
                    Vector3 currentPoint = spline.adjustedPoints[i];
                    Gizmos.DrawLine(lastPoint, currentPoint);
                    lastPoint = currentPoint;
                }

                Gizmos.color = colorBefore;
            }
        }

        public void DrawLinesGlLines(Material outlineMaterial, Color color, float width)
        {
            GL.PushMatrix();
            outlineMaterial.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);

            if (constantSpeed && adjustedPointsLength >= 4)
            {
                Vector3 lastPoint = adjustedPoints[0];

                for (int i = 0; i < adjustedPointsLength; i++)
                {
                    Vector3 currentPoint = adjustedPoints[i];
                    GL.Vertex(lastPoint);
                    GL.Vertex(currentPoint);

                    lastPoint = currentPoint;
                }
            }
            else if (points.Length >= 4)
            {
                Vector3 lastPoint = points[0];

                float split = 1f / (points.Length * 10f);

                float interpolation = 0f;
                while (interpolation < 1f)
                {
                    float at = interpolation / 1f;
                    Vector3 currentPoint = Interpolate(at);

                    GL.Vertex(lastPoint);
                    GL.Vertex(currentPoint);

                    lastPoint = currentPoint;

                    interpolation += split;
                }
            }

            GL.End();
            GL.PopMatrix();
        }

        public Vector3[] GenerateVectors()
        {
            if (points.Length >= 4)
            {
                List<Vector3> meshPoints = new List<Vector3>();
                Vector3 prevPt = points[0];
                meshPoints.Add(prevPt);

                float split = 1.0f / (points.Length * 10.0f);

                float iteration = 0.0f;
                while (iteration < 1.0f)
                {
                    float at = iteration / 1.0f;
                    Vector3 nextPoint = Interpolate(at);
                    meshPoints.Add(nextPoint);
                    iteration += split;
                }

                meshPoints.ToArray();
            }
            return null;
        }

        #endregion

        #region Private Methods

        private void Initialize(Vector3[] newPoints, bool constantSpeed)
        {
            if (newPoints.Length < 4)
            {
                Debug.LogError("When passing values for a spline path, you must pass four or more values!");
                return;
            }

            points = new Vector3[newPoints.Length];
            System.Array.Copy(newPoints, points, newPoints.Length);

            numberOfSections = points.Length - 3;

            float minSegment = float.MaxValue;
            Vector3 earlierPoint = points[1];
            float totalDistance = 0f;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float pointDistance = Vector3.Distance(points[i], earlierPoint);

                if (pointDistance < minSegment)
                {
                    minSegment = pointDistance;
                }

                totalDistance += pointDistance;
            }

            if (constantSpeed)
            {
                minSegment = totalDistance / (numberOfSections * SUBLINE_COUNT);

                float minPrecision = minSegment / SUBLINE_COUNT; // number of subdivisions in each segment
                int precision = (int)Mathf.Ceil(totalDistance / minPrecision) * DISTANCE_COUNT;

                if (precision <= 1) // precision has to be greater than one
                    precision = 2;

                adjustedPoints = new Vector3[precision];
                earlierPoint = Interpolate(0.0f);
                adjustedPointsLength = 1;
                adjustedPoints[0] = earlierPoint;
                distance = 0.0f;

                for (int i = 0; i < precision + 1; i++)
                {
                    float segmentRatio = ((float)(i)) / precision;
                    Vector3 point = Interpolate(segmentRatio);
                    float segmentDistance = Vector3.Distance(point, earlierPoint);

                    if (segmentDistance >= minPrecision || segmentRatio >= 1.0f)
                    {
                        adjustedPoints[adjustedPointsLength] = point;
                        distance += segmentDistance; // only add it to the total distance once we know we are adding it as an adjusted point

                        earlierPoint = point;
                        adjustedPointsLength++;
                    }
                }
                // make sure there is a point at the very end
            }
        }

        #endregion
    }
}