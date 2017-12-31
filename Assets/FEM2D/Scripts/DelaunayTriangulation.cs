
using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public class DelaunayTriangle
    {
        public int i0, i1, i2;

        public Vector2f CircumCenter;

        public float CircumRadius;

        public DelaunayTriangle(int i, int j, int k)
        {
            i0 = i;
            i1 = j;
            i2 = k;
        }

        public void CalculateCircumcircle(IList<Vector2f> vertices)
	    {
            Vector2f p = vertices[i0];
            Vector2f q = vertices[i1];
            Vector2f r = vertices[i2];

            // calculate the intersection of two perpendicular bisectors
            Vector2f pq = q - p;
            Vector2f qr = r - q;

            // check winding
            if(Vector2f.Cross(pq, qr) < 0.0f)
                throw new InvalidOperationException("Triangle winding order incorrect");

            // mid-points of  edges 
            Vector2f a = 0.5f * (p + q);
            Vector2f b = 0.5f * (q + r);
            Vector2f u = pq.PerpendicularCCW;

            float d = Vector2f.Dot(u, qr);
            float t = Vector2f.Dot(b - a, qr) / d;

            CircumCenter = a + t* u;
            CircumRadius = (CircumCenter-p).Magnitude;
	    }

        public void MakeCCW(IList<Vector2f> vertices)
        {
            if(TriArea(vertices) < 0.0f)
            {
                int tmp = i0;
                i0 = i2;
                i2 = tmp;
            }
        }

        public float TriArea(IList<Vector2f> vertices)
        {

            Vector2f a = vertices[i0];
            Vector2f b = vertices[i1];
            Vector2f c = vertices[i2];

            return 0.5f * (Vector2f.Cross(b - a, c - a));
        }

        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return i0;
                    case 1: return i1;
                    case 2: return i2;
                    default: throw new IndexOutOfRangeException("Index out of range: " + i);
                }
            }
            set
            {
                switch (i)
                {
                    case 0: i0 = value; break;
                    case 1: i1 = value; break;
                    case 2: i2 = value; break;
                    default: throw new IndexOutOfRangeException("Index out of range: " + i);
                }
            }
        }
    }

    public class Edge
    {
        public int i0, i1;

        public Edge(int i, int j)
        {
            i0 = i;
            i1 = j;
        }
	};

    public class DelaunayTriangulation
    {

        public List<Vector2f> Vertices;

        public List<DelaunayTriangle> Triangles;

        public DelaunayTriangulation()
        {
            Vertices = new List<Vector2f>();

            Triangles = new List<DelaunayTriangle>();
        }

        public void Triangulate(IList<Vector2f> points)
        {

            Vertices.Clear();
            Triangles.Clear();

            AddBoundingTriangle(points);

            int numPoints = points.Count;
            if (numPoints < 3) return;

            for (int i = 0; i < numPoints; ++i)
                Insert(points[i]);

            //Remove bounding triangle;
            Vertices.RemoveRange(0, 3);

            for (int i = 0; i < Triangles.Count;)
            {
                var t =Triangles[i];

                // throw away tris connected to the initial bounding box 
                if (t.i0 < 3 || t.i1 < 3 || t.i2 < 3)
                {
                    Triangles.Remove(t);
                }
                else
                {
                    t.i0 -= 3;
                    t.i1 -= 3;
                    t.i2 -= 3;
                    i++;
                }
            }

        }

        private void AddBoundingTriangle(IList<Vector2f> points)
        {
            Vector2f lower = Vector2f.PositiveInfinity;
            Vector2f upper = Vector2f.NegativeInfinity;

            int numPoints = points.Count;

            // find bounding box
            for (int i = 0; i < numPoints; ++i)
            {
                if (points[i].x < lower.x) lower.x = points[i].x;
                if (points[i].y < lower.y) lower.y = points[i].y;

                if (points[i].x > upper.x) upper.x = points[i].x;
                if (points[i].y > upper.y) upper.y = points[i].y;
            }

            Vector2f margin = upper - lower;
            lower -= margin;
            upper += margin;

            Vector2f extents = upper - lower;

            // initialize triangulation with a bounding triangle 
            Vertices.Add(lower);
            Vertices.Add(lower + 2.0f * new Vector2f(extents.x, 0.0f));
            Vertices.Add(lower + 2.0f * new Vector2f(0.0f, extents.y));

            var tri = new DelaunayTriangle(0, 1, 2);

            tri.MakeCCW(Vertices);
            tri.CalculateCircumcircle(Vertices);

            Triangles.Add(tri);
        }

        private int ContainsEdge(List<Edge> edges, Edge e)
        {

            for(int i = 0; i < edges.Count; i++)
            {
                if (edges[i].i0 == e.i0 && edges[i].i1 == e.i1) return i;
                if (edges[i].i0 == e.i1 && edges[i].i1 == e.i0) return i;
            }

            return -1;
        }

        private void Insert(Vector2f p)
        {
            List<Edge> edges = new List<Edge>();

            int i = Vertices.Count;
            Vertices.Add(p);

            // find all triangles for which inserting this point would
            // violate the Delaunay condition, that is, which triangles
            // circumcircles does this point lie inside
            for (int j = 0; j < Triangles.Count;)
            {
                var t = Triangles[j];

                Vector2f a = Vertices[t.i0];
                Vector2f b = Vertices[t.i1];
                Vector2f c = Vertices[t.i2];

                if ((t.CircumCenter - p).Magnitude < t.CircumRadius)
                {
                    for (int e = 0; e < 3; ++e)
                    {
                        Edge edge = new Edge(t[e], t[(e + 1) % 3]);

                        // if edge doesn't already exist add it
                        int index = ContainsEdge(edges, edge);

                        if (index == -1)
                            edges.Add(edge);
                        else
                            edges.RemoveAt(index);
                    }

                    // remove triangle
                    Triangles.RemoveAt(j);
				}
				else
				{
                    // next triangle
                    ++j;
				}
			}

			// re-triangulate point to the enclosing set of edges
			for (int e = 0; e < edges.Count; ++e)
			{
				var t = new DelaunayTriangle(edges[e].i0, edges[e].i1, i);

                t.MakeCCW(Vertices);
                t.CalculateCircumcircle(Vertices);

                Triangles.Add(t);
			}

		}

    }

}
