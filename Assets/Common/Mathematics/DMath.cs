using System;

namespace Common.Core.Mathematics
{
    public class DMath
    {

        public static readonly double EPS = 1e-18;

        public static readonly double PI = Math.PI;

        public static readonly double Rad2Deg = 180.0 / PI;

        public static readonly double Deg2Rad = PI / 180.0;

        public static double SafeAcos(double r)
        {
            return Math.Acos(Math.Min(1.0, Math.Max(-1.0, r)));
        }

        public static double SafeAsin(double r)
        {
            return Math.Asin(Math.Min(1.0, Math.Max(-1.0, r)));
        }

        public static double SafeSqrt(double v)
        {
            if (v <= 0.0) return 0.0;
            return Math.Sqrt(v);
        }

        public static double SafeInvSqrt(double n, double d)
        {
            if (d <= 0.0) return 0.0;
            d = Math.Sqrt(d);
            if (d < EPS) return 0.0;
            return n / d;
        }

        public static double SafeInv(double v)
        {
            if (Math.Abs(v) < EPS) return 0.0;
            return 1.0 / v;
        }

        public static double SafeDiv(double n, double d)
        {
            if (Math.Abs(d) < EPS) return 0.0;
            return n / d;
        }

        public static bool IsZero(double v)
        {
            return Math.Abs(v) < EPS;
        }

        public static bool IsFinite(double f)
        {
            return !(double.IsInfinity(f) || double.IsNaN(f));
        }

        public static double Clamp(double v, double min, double max)
        {
            if (v < min) v = min;
            if (v > max) v = max;
            return v;
        }

        public static double Clamp01(double v)
        {
            if (v < 0.0) v = 0.0;
            if (v > 1.0) v = 1.0;
            return v;
        }

        public static double SmoothStep(double edge0, double edge1, double x)
        {
            double t = Clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
            return t * t * (3.0 - 2.0 * t);
        }

        public static double Frac(double x)
        {
            return x - Math.Floor(x);
        }

        public static double Lerp(double v0, double v1, double a)
        {
            return v0 * (1.0 - a) + v1 * a;
        }
    }
}




















