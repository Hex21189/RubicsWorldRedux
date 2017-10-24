using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class BezierPath
    {
        #region Fields

        public float length;
        public bool orientToPath;
        public bool orientToPath2d;
        public Vector3[] points;

        private BezierCurve[] beziers;
        private int currentBezierId = 0;
        private float[] lengthRatio;
        private int lastBezierId = 0;

        #endregion

        #region Constructors

        public BezierPath() { }

        public BezierPath(Vector3[] newPoints)
        {
            SetPoints(newPoints);
        }

        #endregion

        #region Public Methods

        public void SetPoints(Vector3[] newPoints)
        {
            if (newPoints.Length < 4)
            {
                Debug.LogError("When passing values for a vector path, you must pass four or more values!");
            }

            if (newPoints.Length % 4 != 0)
            {
                Debug.LogError("When passing values for a vector path, they must be in sets of four: controlPoint1, controlPoint2, endPoint2, controlPoint2, controlPoint2...");
            }

            points = newPoints;
            length = 0;
            beziers = new BezierCurve[points.Length / 4];
            lengthRatio = new float[beziers.Length];

            int k = 0;         
            
            for (int i = 0; i < points.Length; i += 4)
            {
                beziers[k] = new BezierCurve(points[i + 0], points[i + 2], points[i + 1], points[i + 3], 0.05f);
                length += beziers[k].length;
                k++;
            }

            for (int i = 0; i < beziers.Length; i++)
            {
                lengthRatio[i] = beziers[i].length / length;
            }
        }

        public float Distance
        {
            get
            {
                return length;
            }
        }

        public Vector3 Point(float ratio)
        {
            float added = 0.0f;
            for (int i = 0; i < lengthRatio.Length; i++)
            {
                added += lengthRatio[i];
                if (added >= ratio)
                    return beziers[i].Point((ratio - (added - lengthRatio[i])) / lengthRatio[i]);
            }
            return beziers[lengthRatio.Length - 1].Point(1.0f);
        }

        public void Place2d(Transform transform, float ratio)
        {
            transform.position = Point(ratio);
            ratio += 0.001f;
            if (ratio <= 1.0f)
            {
                Vector3 v3Dir = Point(ratio) - transform.position;
                float angle = Mathf.Atan2(v3Dir.y, v3Dir.x) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, 0, angle);
            }
        }

        public void PlaceLocal2d(Transform transform, float ratio)
        {
            transform.localPosition = Point(ratio);
            ratio += 0.001f;
            if (ratio <= 1.0f)
            {
                Vector3 v3Dir = Point(ratio) - transform.localPosition;
                float angle = Mathf.Atan2(v3Dir.y, v3Dir.x) * Mathf.Rad2Deg;
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
            ratio = GetRatioInOneRange(ratio);
            transform.localPosition = Point(ratio);
            ratio = GetRatioInOneRange(ratio + 0.001f);

            if (ratio <= 1.0f)
            {
                transform.LookAt(transform.parent.TransformPoint(Point(ratio)), worldUp);
            }
        }

        public float GetRatioInOneRange(float ratio)
        {
            if (ratio >= 0.0f && ratio <= 1.0f)
            {
                return ratio;
            }
            else if (ratio < 0.0f)
            {
                return Mathf.Ceil(ratio) - ratio;   //if -1.4 => it returns 0.4
            }
            else
            {
                return ratio - Mathf.Floor(ratio);  //if 1.4 => it return 0.4
            }
        }

        public void DrawGizmos(float t = -1.0f)
        {
            Vector3 prevPt = Point(0);

            for (int i = 1; i <= 120; i++)
            {
                float pm = i / 120.0f;
                Vector3 currPt2 = Point(pm);
                Gizmos.color = (lastBezierId == currentBezierId) ? Color.magenta : Color.grey;
                Gizmos.DrawLine(currPt2, prevPt);
                prevPt = currPt2;
                lastBezierId = currentBezierId;
            }
        }

        #endregion
    }
}