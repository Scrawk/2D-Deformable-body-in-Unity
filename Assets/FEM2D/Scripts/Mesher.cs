using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public class Mesher
    {

        /// <summary>
        /// incremental insert Delaunay triangulation based on Bowyer/Watson's algorithm
        /// </summary>
        public static void TriangulateDelaunay(List<Vector2f> points, out List<Vector2f> outPoints, out List<int> outTris)
        {

            var mesh = new DelaunayTriangulation();
            mesh.Triangulate(points);

            int vertCount = mesh.Vertices.Count;
            int triCount = mesh.Triangles.Count;

            outPoints = new List<Vector2f>(vertCount);
            outTris = new List<int>(triCount * 3);

            int offset = 0;

            for (int i = offset; i < vertCount; ++i)
            {
                outPoints.Add(mesh.Vertices[i]);
            }

            for (int i = 0; i < triCount; ++i)
            {
                var t = mesh.Triangles[i];

                // throw away tris connected to the initial bounding box 
                if (t.i0 < offset || t.i1 < offset || t.i2 < offset)
                    continue;

                outTris.Add(t.i0 - offset);
                outTris.Add(t.i1 - offset);
                outTris.Add(t.i2 - offset);
            }

        }

        public static void CreateTorus(List<Vector2f> points, List<int> indices, float inner, float outer, int segments)
        {

            int b;

            for (int i = 0; i < segments; ++i)
            {
                float theta = (float)i / segments * Mathf.PI * 2.0f;

                float x = Mathf.Sin(theta);
                float y = Mathf.Cos(theta);

                points.Add(new Vector2f(x, y) * outer);
                points.Add(new Vector2f(x, y) * inner);

                if (i > 0)
                {
                    b = (i - 1) * 2;

                    indices.Add(b + 0);
                    indices.Add(b + 1);
                    indices.Add(b + 2);

                    indices.Add(b + 2);
                    indices.Add(b + 1);
                    indices.Add(b + 3);
                }
            }

            b = points.Count - 2;

            indices.Add(b + 0);
            indices.Add(b + 1);
            indices.Add(0);

            indices.Add(0);
            indices.Add(b + 1);
            indices.Add(1);
        }

        /// <summary>
        /// iterative optimisation algoirthm based on Variational Tetrahedral Meshing
        /// </summary>
        public static void TriangulateVariational(List<Vector2f> inPoints, List<Vector2f> bPoints, int iterations, out List<Vector2f> outPoints, out List<int> outTris)
        {

            Vector2f[] points = new Vector2f[inPoints.Count];
            float[] weights = new float[inPoints.Count];

            for (int i = 0; i < points.Length; ++i)
                points[i] = inPoints[i];

            var mesh = new DelaunayTriangulation();

            for (int k = 0; k < iterations; ++k)
	        {
                mesh.Triangulate(points);

                Array.Clear(points, 0, points.Length);
                Array.Clear(weights, 0, weights.Length);

                // optimize boundary points
                for (int i = 0; i < bPoints.Count; ++i)
		        {
			        int closest = 0;
                    float closestDistSq = float.PositiveInfinity;

                    Vector2f b = bPoints[i];

			        // find closest point (todo: use spatial hash)
			        for (int j = 0; j < mesh.Vertices.Count; ++j)
			        {
				        float dSq = (mesh.Vertices[j] - b).SqrMagnitude;

				        if (dSq < closestDistSq)
				        {
					        closest = j;
					        closestDistSq = dSq;
				        }
                    }

                    points[closest] -= b;
			        weights[closest] -= 1.0f;
		        }

                // optimize interior points by moving them to the centroid of their 1-ring
                for (int i = 0; i < mesh.Triangles.Count; ++i)
		        {
                    var t = mesh.Triangles[i];
		
                    float w = t.TriArea(mesh.Vertices);

			        for (int v = 0; v < 3; ++v)	
			        {
				        int s = t[v];

				        if (weights[s] >= 0.0f)
				        {
					        points[s] += w * t.CircumCenter;
                            weights[s] += w;
				        }
			        }
		        }

                for (int i = 0; i < points.Length; ++i)
                {
                    points[i] /= weights[i];
                }

            }

            mesh.Triangulate(points);

            /*
            points.resize(0);
	        points.assign(mesh.vertices.begin()+3, mesh.vertices.end());

	        // remove any sliver tris on the boundary
	        for (uint32_t i = 0; i<mesh.triangles.size();)
	        {
		        real q = mesh.TriangleQuality(i);

		        if (q > 3.0f)
			        mesh.triangles.erase(mesh.triangles.begin() + i);
		        else
			        ++i;
	        }	
            */

            outPoints = new List<Vector2f>(mesh.Vertices);
            outTris = new List<int>(mesh.Triangles.Count * 3);

	        for (int i = 0; i< mesh.Triangles.Count; ++i)
	        {
		        var t = mesh.Triangles[i];

                outTris.Add(t.i0);
		        outTris.Add(t.i1);
		        outTris.Add(t.i2);
	        }

        }
    }

}
