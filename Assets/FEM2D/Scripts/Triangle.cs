using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{
    public class Triangle
    {
        public int i, j, k;

        public Triangle(int i, int j, int k)
        {
            this.i = i;
            this.j = j;
            this.k = k;
        }
    }
}
