using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public static class CreateCantileverBeam
    {

        public static FEMScene Create(Vector2f origin, float scale, int numPoints, bool weld)
        {
            FEMScene Scene = new FEMScene();

            Scene.Substeps = 40;
            Scene.Drag = 1.0f;
            Scene.LameLambda = 4000.0f;
            Scene.LameMu = 4000.0f;
            Scene.Damping = 200.0f;
            Scene.Friction = 0.5f;
            Scene.Toughness = 8000.0f;

            float u = scale / (numPoints-1.0f);

            for (int i = 0; i < numPoints; ++i)
            {
                Scene.Particles.Add(new FEMParticle(origin + new Vector2f(i * u, 0.0f), 1.0f));
                Scene.Particles.Add(new FEMParticle(origin + new Vector2f(i * u, u), 1.0f));

                if (i != 0)
                {
                    // add quad
                    int start = (i - 1) * 2;

                    Scene.Triangles.Add(new Triangle(start + 0, start + 2, start + 1));
                    Scene.Triangles.Add(new Triangle(start + 1, start + 2, start + 3));
                }
            }

            if (weld)
            {
                Scene.Particles[0].invMass = 0.0f;
                Scene.Particles[1].invMass = 0.0f;
            }

            // assign index to particles
            for (int i = 0; i < Scene.Particles.Count; ++i)
                Scene.Particles[i].index = i;

            Scene.CreateElements();

            return Scene;
        }

    }

}
