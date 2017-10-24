using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SparkyGames.RubicsWorldRedux
{
    public class BezierCurve
    {
        #region Fields

        public float length;

        private Vector3 a;
        private Vector3 aa;
        private Vector3 bb;
        private Vector3 cc;
        private float precision;
        private float[] arcLengths;

        #endregion

        #region Constructors

        public BezierCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float precision)
        {
            this.precision = precision;
            this.a = a;
            aa = (-a + 3 * (b - c) + d);
            bb = 3 * (a + c) - 6 * b;
            cc = 3 * (b - a);

            float arcCount = 1.0f / precision;
            arcLengths = new float[(int)arcCount + 1];
            arcLengths[0] = 0;

            Vector3 ov = a;
            Vector3 v;
            float clen = 0.0f;

            for (int i = 1; i < arcLengths.Length; i++)
            {
                v = BezierPoint(i * precision);
                clen += (ov - v).magnitude;
                arcLengths[i] = clen;
                ov = v;
            }

            length = clen;
        }

        #endregion

        #region Public Methods

        public Vector3 Point(float t)
        {
            return BezierPoint(Map(t));
        }

        #endregion

        #region Private Methods

        private Vector3 BezierPoint(float t)
        {
            return ((aa * t + (bb)) * t + cc) * t + a;
        }

        private float Map(float u)
        {
            float targetLength = u * arcLengths[arcLengths.Length - 1];
            int low = 0;
            int high = arcLengths.Length - 1;
            int index = 0;

            while (low < high)
            {
                index = low + ((int)((high - low) / 2.0f) | 0);
                if (arcLengths[index] < targetLength)
                {
                    low = index + 1;
                }
                else
                {
                    high = index;
                }
            }

            if (arcLengths[index] > targetLength)
                index--;
            if (index < 0)
                index = 0;

            return (index + (targetLength - arcLengths[index]) / (arcLengths[index + 1] - arcLengths[index])) / (1.0f / precision);
        }
        
        #endregion
    }
}