using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public static class CreateTorus
    {

        public static FEMScene Create(Vector2f origin, float inner, float outer, int segments)
        {
            FEMScene Scene = new FEMScene();

            Scene.Substeps = 40;
            Scene.Drag = 1.0f;
            Scene.LameLambda = 10000.0f;
            Scene.LameMu = 10000.0f;
            Scene.Damping = 80.0f;
            Scene.Friction = 0.95f;
            Scene.Toughness = 20000.0f;

            List<Vector2f> verts = new List<Vector2f>();
            List<int> tris = new List<int>();

            Mesher.CreateTorus(verts, tris, inner, outer, segments);

            // generate elements
            for (int i = 0; i < verts.Count; ++i)
                Scene.Particles.Add(new FEMParticle(origin + verts[i], 1.0f));

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
