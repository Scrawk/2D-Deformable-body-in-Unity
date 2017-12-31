using System;

namespace Common.Core.Mathematics
{
	public class IMath 
	{

        public static int Clamp(int v, int min, int max)
        {
            if (v < min) v = min;
            if (v > max) v = max;
            return v;
        }

        public static bool IsPow2(int num)
        {
            int power = (int)(Math.Log(num) / Math.Log(2.0));

            double result = Math.Pow(2.0, power);

            return (result == num);
        }

		public static int NearestPow2(int num)
		{
			int n = num > 0 ? num - 1 : 0;
			
			n |= n >> 1;
			n |= n >> 2;
			n |= n >> 4;
			n |= n >> 8;
			n |= n >> 16;
			n++;
			
			return n;
		}

		public static int LowerPow2(int num)
		{
			int n = num > 0 ? num - 1 : 0;

			n |= n >> 1;
			n |= n >> 2;
			n |= n >> 4;
			n |= n >> 8;
			n |= n >> 16;
			n++;

			return n - (n >> 1);
		}

    }
}




















