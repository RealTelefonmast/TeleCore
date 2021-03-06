using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public static class TMath
    {
        //Basic
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            return a == b ? 0 : (value - a) / (b - a);
        }

        public static float NormalizeInRange(float value, float start, float end)
        {
            float width = end - start;
            float offsetValue = value - start;
            return (offsetValue - ((offsetValue / width) * width)) + start;
        }

        public static float AngleWrapped(this float angle)
        {
            return NormalizeInRange(angle, 0, 360);
        }

        public static float Negate(this float value)
        {
            return value <= 0 ? value : -value;
        }

        //Special
        public static int Mobius(int n)
        {
            if (n == 1) return 1;
            var p = 0;
            for (var i = 2; i <= n; i++)
            {
                if (n % i != 0 || !IsPrime(i)) continue;
                if (n % (i * i) == 0)
                    return 0;
                p++;
            }
            return (p % 2 != 0) ? -1 : 1;
        }

        //Comparing Math
        public static int Compare(this IntVec3 a, IntVec3 b, int gridWidth)
        {
            var aValue = a.x + (a.z * gridWidth);
            var bValue = b.x + (b.z * gridWidth);
            return aValue - bValue;
        }

        public static bool IsPrime(int n)
        {
            if (n <= 1) return false;
            if (n == 2) return true;
            if (n % 2 == 0) return false;

            var boundary = (int)Math.Floor(Math.Sqrt(n));
            for (int i = 3; i <= boundary; i += 2)
                if (n % i == 0)
                    return false;
            return true;
        }

        //Trigonometry
        public static float OscillateBetween(float minVal, float maxVal, float duration, int currentTick)
        {
            float sineVal = Mathf.Sin((currentTick + duration / 2f) / duration * Mathf.PI) / 2 + 0.5f;
            return Mathf.Lerp(minVal, maxVal, sineVal);
        }

        public static float Cosine(float yMin = 0f, float yMax = 1f, float freq = 1f, float curX = 0f, float period = 2f)
        {
            float mltp = (yMax - yMin) / 2f;
            float height = mltp + yMin;
            float ang = (period * Mathf.PI) * curX * freq;
            return (mltp * Mathf.Cos(ang)) + height;
        }

        public static float Cosine2(float yMin = 0f, float yMax = 1f, float xMax = 1f, float xOff = 0f, float curX = 0f)
        {
            float mltp = (yMax - yMin) / 2f;
            float height = mltp + yMin;
            float ang = (1f / (xMax * 2f)) * Mathf.PI * (curX - xOff);
            return (mltp * Mathf.Cos(ang)) + height;
        }

        //Vectors
        public static Vector2 Clamp(this Vector2 vec, Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Clamp(vec.x, min.x, max.x), Mathf.Clamp(vec.y, min.x, max.x));
        }

        public static Vector2 Abs(this Vector2 v2)
        {
            return new Vector2(Mathf.Abs(v2.x), Mathf.Abs(v2.y));
        }

        public static float InverseLerp(this Vector3 value, Vector3 a, Vector3 b)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }

        //Rect
        public static Rect Lerp(this Rect rect1, Rect rect2, float val)
        {
            return new Rect(Vector2.Lerp(rect1.position, rect2.position, val), Vector2.Lerp(rect1.size, rect2.size, val));
        }
    }
}
