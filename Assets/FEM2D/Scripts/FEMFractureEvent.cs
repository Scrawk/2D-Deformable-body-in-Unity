using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{
    public struct FEMFractureEvent
    {
        public int Tri;
        public int Node;
        public Vector3f Plane;
    }
}
