using System;
using System.Collections.Generic;

using Common.Core.Mathematics;

namespace Common.Core.LinearAlgebra
{
    public static class MatrixMxN
    {

        public static double[] MultiplyVector(double[,] matrix, double[] vector)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            if (vector.Length != matrix.GetLength(1))
                throw new ArgumentException("Matrix must have same number of columns as vectors length.");

            double[] multiplied = new double[M];

            for (int i = 0; i < M; i++)
            {
                double sum = 0.0;
                for (int j = 0; j < N; j++)
                {
                    sum += vector[j] * matrix[i, j];
                }

                multiplied[i] = sum;
            }

            return multiplied;
        }

        public static double[] MultiplyMatrix(double[] matrix1, double[,] matrix2)
        {
            int M = matrix1.Length;
            int N = matrix2.GetLength(1);

            if (matrix1.Length != matrix2.GetLength(0))
                throw new ArgumentException("Matrix2 must have same number of rows as matrix1 has columns.");

            double[] multiplied = new double[N];

            for (int j = 0; j < N; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < M; i++)
                {
                    sum += matrix1[i] * matrix2[i, j];
                }

                multiplied[j] = sum;
            }

            return multiplied;
        }

        public static double[,] MultiplyMatrix(double[,] matrix1, double[,] matrix2)
        {
            int M = matrix1.GetLength(0);
            int N = matrix2.GetLength(1);

            if (matrix1.GetLength(1) != matrix2.GetLength(0))
                throw new ArgumentException("Matrix2 must have same number of rows as matrix1 has columns.");

            double[,] multiplied = new double[M, N];

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < matrix1.GetLength(1); k++)
                    {
                        sum += matrix1[i, k] * matrix2[k, j];
                    }

                    multiplied[i, j] = sum;
                }
            }

            return multiplied;
        }

        public static double[,] MultiplyScalar(double[,] matrix, double v)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            double[,] multiplied = new double[M, N];

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                    multiplied[i, j] = matrix[i, j] * v;
            }

            return multiplied;
        }

        public static double[,] Add(double[,] matrix1, double[,] matrix2)
        {
            int M = matrix1.GetLength(0);
            int N = matrix1.GetLength(1);

            if (M != matrix2.GetLength(0) || N != matrix2.GetLength(1))
                throw new ArgumentException("Two matrices should be the same dimension to add.");

            double[,] sum = new double[M, N];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                    sum[i, j] = matrix1[i, j] + matrix2[i, j];

            }

            return sum;
        }

        public static double[,] Subtract(double[,] matrix1, double[,] matrix2)
        {
            int M = matrix1.GetLength(0);
            int N = matrix1.GetLength(1);

            if (M != matrix2.GetLength(0) || N != matrix2.GetLength(1))
                throw new ArgumentException("Two matrices should be the same dimension.");

            double[,] sum = new double[M, N];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                    sum[i, j] = matrix1[i, j] - matrix2[i, j];

            }

            return sum;
        }

        public static double[,] Transpose(double[,] matrix)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            double[,] transposed = new double[N, M];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    transposed[j, i] = matrix[i, j];
                }
            }
            return transposed;
        }

        public static double[,] Inverse(double[,] matrix)
        {
		    return MultiplyScalar(Transpose(Cofactor(matrix)), DMath.SafeInv(Determinant(matrix)));
        }

        public static double Determinant(double[,] matrix)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            if (N != M)
                throw new ArgumentException("Matrix need to be square to find determinant.");

            if (M == 1)
                return matrix[0, 0];

            if (M == 2)
                return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];

            double sum = 0.0;
            for (int i = 0; i < N; i++)
                sum += ChangeSign(i) * matrix[0, i] * Determinant(SubMatrix(matrix, 0, i));
            
            return sum;
        }

        public static double[,] Cofactor(double[,] matrix)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            double[,] mat = new double[M, N];

            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    mat[i, j] = ChangeSign(i) * ChangeSign(j) * Determinant(SubMatrix(matrix, i, j));
                }
            }

            return mat;
        }

        public static double[,] SubMatrix(double[,] matrix, int excluding_row, int excluding_col)
        {
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            double[,] mat = new double[M - 1, N - 1];
            int r = -1;
            for (int i = 0; i < M; i++)
            {
                if (i == excluding_row)
                    continue;
                r++;
                int c = -1;
                for (int j = 0; j < N; j++)
                {
                    if (j == excluding_col)
                        continue;
                    mat[r, ++c] = matrix[i, j];
                }
            }

            return mat;
        }

        private static int ChangeSign(int i)
        {
            if (i % 2 == 0)
                return 1;
            return -1;
        }

    }
}
