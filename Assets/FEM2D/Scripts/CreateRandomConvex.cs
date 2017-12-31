using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public static class CreateRandomConvex
    {

        public static FEMScene Create(Vector2f origin, float scale, int numPoints)
        {
            FEMScene Scene = new FEMScene();

            Scene.Substeps = 40;
            Scene.Drag = 1.0f;
            Scene.LameLambda = 10000.0f;
            Scene.LameMu = 10000.0f;
            Scene.Damping = 80.0f;
            Scene.Friction = 0.95f;
            Scene.Toughness = 20000.0f;

            List<Vector2f> points = new List<Vector2f>();

            //Random.InitState(0);

            for (int i = 0; i < numPoints; ++i)
            {
                float rx = Random.Range(-scale, scale) * 0.5f;
                float ry = Random.Range(-scale, scale) * 0.5f;

                points.Add(origin + new Vector2f(rx, ry));
            }

            List<Vector2f> verts;
            List<int> tris;
            Mesher.TriangulateDelaunay(points, out verts, out tris);

            // generate elements
            for (int i = 0; i < verts.Count; ++i)
                Scene.Particles.Add(new FEMParticle(verts[i], 1.0f));

            for (int i = 0; i < tris.Count / 3; ++i)
                Scene.Triangles.Add(new Triangle(tris[i * 3], tris[i * 3 + 1], tris[i * 3 + 2]));

            // assign index to particles
            for (int i = 0; i < Scene.Particles.Count; ++i)
                Scene.Particles[i].index = i;

            Scene.CreateElements();

            return Scene;
        }

    }

}
