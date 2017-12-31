using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{
    public class FEMElement
    {
        public FEMElement(Vector2f[] x)
        {
            Vector2f e1 = x[1] - x[0];
            Vector2f e2 = x[2] - x[0];
            Vector2f e3 = x[2] - x[1];

            Matrix2x2f m = new Matrix2x2f();
            m.SetColumn(0, e1);
            m.SetColumn(1, e2);

            mInvDm = m.Inverse;

            mB = new Vector2f[3];
            mB[0] = e3.PerpendicularCCW;
            mB[1] = e2.PerpendicularCW;
            mB[2] = e1.PerpendicularCCW;
        }

        public Matrix2x2f mInvDm; // inverse rest configuration
        public Matrix2x2f mEp;    // plastic strain

        public Vector2f[] mB;       // area weighted normals in material space
    }
}
