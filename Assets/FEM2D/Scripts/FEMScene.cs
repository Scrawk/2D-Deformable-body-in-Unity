using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;
using Common.Mathematics.Decomposition;

namespace FEM2D
{

    public class FEMScene
    {

        public Vector2f Gravity = new Vector2f(0, -9.8f);

        public float LameLambda = 1000.0f;

        public float LameMu = 1000.0f;

        public float Yield = 0.5f;

        public float Creep = 25.0f;

        public float Damping = 10.0f;

        public float Drag = 0.0f;

        public float Friction = 0.5f;

        public float Toughness = 500.0f;

        public int Substeps = 1;

        public List<FEMParticle> Particles;

        public List<Triangle> Triangles;

        public List<FEMElement> Elements;

        public List<Vector3f> Planes;

        public List<FEMFractureEvent> Fractures;

        private System.Random Rnd;

        public FEMScene()
        {
            Rnd = new System.Random(0);
            Particles = new List<FEMParticle>();
            Triangles = new List<Triangle>();
            Planes = new List<Vector3f>();
            Fractures = new List<FEMFractureEvent>();
            Elements = new List<FEMElement>();
        }

        public void CreateElements()
        {

            // calculate inverse of the initial configuration
            for (int i = 0; i < Triangles.Count; ++i)
            {
                Triangle t = Triangles[i];

                // read particles into a local array
                Vector2f[] x = new Vector2f[]
                {
                    Particles[t.i].p,
                    Particles[t.j].p,
                    Particles[t.k].p
                };

                Elements.Add(new FEMElement(x));
            }

        }

        public void Update(float dt)
        {

            Fractures.Clear();

            bool performFracture = true;

            UpdateForces(dt, performFracture);

            CollidePlanes();

            IntegrateForces(dt);

            if (performFracture)
	        {
                //Not implemented
            }

        }

        private void UpdateForces(float dt, bool performFracture)
        {
            
            for (int i = 0; i < Particles.Count; ++i)
            {

                Particles[i].max = 0;

                if (Particles[i].invMass > 0.0f)
                    Particles[i].f += Gravity / Particles[i].invMass;
                else
                    Particles[i].f += Vector2f.Zero - Drag * Particles[i].v;
            }

            Vector2f[] x = new Vector2f[3];
            Vector2f[] v = new Vector2f[3];

            for (int i = 0; i < Triangles.Count; ++i)
            {
                Triangle tri = Triangles[i];
                FEMElement elem = Elements[i];

                x[0] = Particles[tri.i].p;
                x[1] = Particles[tri.j].p;
                x[2] = Particles[tri.k].p;

                v[0] = Particles[tri.i].v;
                v[1] = Particles[tri.j].v;
                v[2] = Particles[tri.k].v;

                if (performFracture)
                {
			        Matrix2x2f f = CalcDeformation(x, elem.mInvDm);
                    Matrix2x2f q = Decomposition2x2f.QRDecomposition(f);

                    // strain 
                    Matrix2x2f e = CalcCauchyStrainTensor(q.Transpose * f);
         
                    // update plastic strain
                    float ef = FrobeniusNorm(e);
		
			        if (ef > Yield)
				        elem.mEp +=  e * dt * Creep;

                    const float epmax = 0.6f;	
			        if (ef > epmax)	
				        elem.mEp *= epmax / ef;  

			        // adjust strain
			        e -= elem.mEp;

                    Matrix2x2f s = CalcStressTensor(e, LameLambda, LameMu);

                    // damping forces	
                    Matrix2x2f dfdt = CalcDeformation(v, elem.mInvDm);
                    Matrix2x2f dedt = CalcCauchyStrainTensorDt(q.Transpose * dfdt);
                    Matrix2x2f dsdt = CalcStressTensor(dedt, Damping, Damping);

                    Matrix2x2f p = s + dsdt;

                    float e1, e2;
                    Decomposition2x2f.EigenDecomposition(p, out e1, out e2);

                    float me = Mathf.Max(e1, e2);

			        if (me > Toughness)
			        {
				        // calculate Eigenvector corresponding to max Eigenvalue
				        Vector2f ev = q * (new Vector2f(p.m01, me - p.m00)).Normalized;

                        // pick a random vertex to split on
                        int splitNode = Rnd.Next(0, 2);

				        // don't fracture immovable nodes
				        if (Particles[GetVertex(tri, splitNode)].invMass == 0.0f)
					        break;

                        // fracture plane perpendicular to ev
                        Vector3f plane = new Vector3f(ev.x, ev.y, -Vector2f.Dot(ev, Particles[GetVertex(tri, splitNode)].p));

                        FEMFractureEvent fracture = new FEMFractureEvent();
                        fracture.Tri = i;
                        fracture.Node = splitNode;
                        fracture.Plane = plane;

                        //Fracture not implemented so these fracture planes are not used.
                        Fractures.Add(fracture);
			        }

                    // calculate force on each edge due to stress and distribute to the nodes
                    Vector2f f1 = q * p * elem.mB[0];
                    Vector2f f2 = q * p * elem.mB[1];
                    Vector2f f3 = q * p * elem.mB[2];

                    Particles[tri.i].f -= f1/3.0f;
			        Particles[tri.j].f -= f2/3.0f;
			        Particles[tri.k].f -= f3/3.0f;
                }
                else
                {
                    //This was the code used when fracturing was disabled
                    //in the original. It seems very unstable for me. maybe
                    //a bug or precision issue. Not used atm.

                    Matrix2x2f f = CalcDeformation(x, elem.mInvDm);

                    // elastic forces
                    Matrix2x2f e = CalcGreenStrainTensor(f);
                    Matrix2x2f s = CalcStressTensor(e, LameLambda, LameMu);

                    // damping forces	
                    Matrix2x2f dfdt = CalcDeformation(v, elem.mInvDm);
                    Matrix2x2f dedt = CalcGreenStrainTensorDt(f, dfdt);
                    Matrix2x2f dsdt = CalcStressTensor(dedt, Damping, Damping);

                    Matrix2x2f p = s + dsdt;

                    float e1, e2;
                    Decomposition2x2f.EigenDecomposition(p, out e1, out e2);
                    float me = Mathf.Max(e1, e2);

                    Matrix2x2f finv = f.Transpose.Inverse;

                    Vector2f f1 = p * (finv * elem.mB[0]);
                    Vector2f f2 = p * (finv * elem.mB[1]);
                    Vector2f f3 = p * (finv * elem.mB[2]);

                    Particles[tri.i].f -= f1 / 3.0f;
                    Particles[tri.j].f -= f2 / 3.0f;
                    Particles[tri.k].f -= f3 / 3.0f;

                    Particles[tri.i].max += me / 3.0f;
                    Particles[tri.j].max += me / 3.0f;
                    Particles[tri.k].max += me / 3.0f;
                }
            }

        }

        private void IntegrateForces(float dt)
        {
            // integrate particles forward in time, symplectic Euler step 	
            for (int i = 0; i < Particles.Count; ++i)
            {
                Particles[i].v += Particles[i].f * Particles[i].invMass * dt;
                Particles[i].p += Particles[i].v * dt;
                Particles[i].f = Vector2f.Zero;
            }
        }

        private void CollidePlanes()
        {
	        for (int i = 0; i < Particles.Count; ++i)
	        {
		        for (int p = 0; p < Planes.Count; ++p)
		        {
                    Vector2f n = Planes[p].xy;
                    float d = Vector2f.Dot(Particles[i].p, n) + Planes[p].z; 

			        if (d < 0.0f)
			        {
                        // push out of halfspace
                        Particles[i].p -= d * n;

                        // make relative velocity separating
                        float rv = Vector2f.Dot(Particles[i].v, n);

				        if (rv< 0.0f)
				        {
                            // zero normal velocity, material simulation will take care of restitution
                            Vector2f nv = -rv * n;

                            // friction
                            Vector2f tv = (Particles[i].v + nv) * Friction;

                            // update velocity
                            Particles[i].v = tv;
				        }
                    }		
		        }
	        }
        }

        private int GetVertex(Triangle tri, int i)
        {
            if (i == 0)
                return tri.i;
            else if (i == 1)
                return tri.j;
            else if (i == 2)
                return tri.k;
            else
                throw new IndexOutOfRangeException("Triangle index out of range");
        }

        float FrobeniusNorm(Matrix2x2f m)
        {
            float f = 0.0f;

            for (int i = 0; i < 2; ++i)
                for (int j = 0; j < 2; ++j)
                    f += m[i, j] * m[i, j];

            return (float)Math.Sqrt(f);
        }

        /// <summary>
        /// deformation gradient
        /// </summary>
        private Matrix2x2f CalcDeformation(Vector2f[] x, Matrix2x2f invM)
        {
            Vector2f e1 = x[1] - x[0];
            Vector2f e2 = x[2] - x[0];

            Matrix2x2f m = new Matrix2x2f();
            m.SetColumn(0, e1);
            m.SetColumn(1, e2);

            // mapping from material coordinates to world coordinates	
            Matrix2x2f f = m * invM;
            return f;
        }

        /// <summary>
        /// calculate Green's non-linear strain tensor
        /// </summary>
        private Matrix2x2f CalcGreenStrainTensor(Matrix2x2f f)
        {
            Matrix2x2f e = (f.Transpose * f - Matrix2x2f.Identity) * 0.5f;
            return e;
        }

        /// <summary>
        /// calculate time derivative of Green's strain
        /// </summary>
        private Matrix2x2f CalcGreenStrainTensorDt(Matrix2x2f f, Matrix2x2f dfdt)
        {
            Matrix2x2f e = (f * dfdt.Transpose + dfdt * f.Transpose) * 0.5f;
            return e;
        }

        /// <summary>
        /// calculate Cauchy's linear strain tensor
        /// </summary>
        private Matrix2x2f CalcCauchyStrainTensor(Matrix2x2f f)
        {
            Matrix2x2f e = (f + f.Transpose) * 0.5f - Matrix2x2f.Identity;
            return e;
        }

        /// <summary>
        /// calculate time derivative of Cauchy's strain tensor
        /// </summary>
        private Matrix2x2f CalcCauchyStrainTensorDt(Matrix2x2f dfdt)
        {
            Matrix2x2f e = (dfdt + dfdt.Transpose) * 0.5f;
            return e;
        }

        /// <summary>
        /// calculate isotropic Hookean stress tensor, lambda and mu are the Lame parameters
        /// </summary>
        private Matrix2x2f CalcStressTensor(Matrix2x2f e, float lambda, float mu)
        {
            Matrix2x2f s = Matrix2x2f.Identity * e.Trace * lambda + e * mu * 2.0f;
            return s;
        }

    }

}
