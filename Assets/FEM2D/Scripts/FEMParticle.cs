using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public class FEMParticle
    {

        public Vector2f p;
        public Vector2f v;
        public Vector2f f;
        public Vector2f c;
        public Vector2f uv;
        public float invMass;
        public int index;
        public float max;

        public FEMParticle(Vector2f pos, float im)
        {
            p = pos;
            invMass = im;
            index = 0;
        }

        public FEMParticle(Vector2f pos, Vector2f uv, float im)
        {
            p = pos;
            this.uv = uv;
            invMass = im;
            index = 0;
        }

        public FEMParticle Copy()
        {
            FEMParticle particle = new FEMParticle(p, invMass);
            particle.v = v;
            particle.f = f;
            particle.c = c;
            particle.index = index;
            particle.max = max;

            return particle;
        }

    }

}
