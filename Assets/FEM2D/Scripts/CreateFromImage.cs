using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public static class CreateFromImage
    {

        private static float resolution = 0.08f;
        private static float scale = 2.0f;
        private static float density = 140f;
        private static float aspect;

        private static Vector2f ToWorldPos(Vector2f p)
        {
            float x = (p.x - 0.5f) * scale;
            float y = (p.y - 0.5f) * scale * aspect;

            return new Vector2f(x, y);
        }

        public static FEMScene Create(Texture2D img)
        {
            FEMScene Scene = new FEMScene();

            if (img == null) return Scene;

            Scene.Substeps = 80;
            Scene.Drag = 0.1f;
            Scene.LameLambda = 42000.0f;
            Scene.LameMu = 42000.0f;
            Scene.Damping = 250.0f;
            Scene.Friction = 0.8f;
            Scene.Toughness = 40000.0f;

            List<Vector2f> points = new List<Vector2f>();
            List<Vector2f> bpoints = new List<Vector2f>();

            // controls how finely the object is sampled
            aspect = (float)img.height / img.width;

            float fw = img.width;
            float fh = img.height;

            int inc = Mathf.FloorToInt(img.width * resolution);
            int margin = Mathf.Max((inc - 1) / 2, 1);

            // distribute points interior to the object or near it's boundary
            for (int y = 0; y < img.height; y += inc)
            {
                for (int x = 0; x < img.width; x += inc)
                {
                    // if value non-zero then add a point
                    int n = Neighbours(img, x, y, margin);

                    if (n > 0)
                    {
                        Vector2f uv = new Vector2f(x / fw, y / fh);
                        points.Add(uv);
                    }
                }
            }

            // distribute points on the boundary
            for (int y = 0; y < img.height; y++)
            {
                for (int x = 0; x < img.width; x++)
                {
                    if (EdgeDetect(img, x, y))
                    {
                        Vector2f uv = new Vector2f(x / fw, y / fh);
                        bpoints.Add(uv);
                    }
                }
            }

            // triangulate
            int iterations = 7;

            List<Vector2f> verts;
            List<int> tris;
            Mesher.TriangulateVariational(points, bpoints, iterations, out verts, out tris);

            //no longer used
            points = null;
            bpoints = null;

            // discard triangles whose centroid is not inside the shape
            for (int i = 0; i < tris.Count;)
            {
                Vector2f p = verts[tris[i + 0]];
                Vector2f q = verts[tris[i + 1]];
                Vector2f r = verts[tris[i + 2]];

                Vector2f c = (p + q + r) / 3.0f;

                //int x = Mathf.FloorToInt(c.x * fw);
                //int y = Mathf.FloorToInt(c.y * fh);
                //Color col = img.GetPixel(x, y);

                Color col = img.GetPixelBilinear(c.x, c.y);

                if (col.grayscale == 0.0f)
                    tris.RemoveRange(i, 3);
                else
                    i += 3;
            }

            // generate particles
            for (int i = 0; i < verts.Count; ++i)
            {
                Vector2f uv = verts[i];
                Scene.Particles.Add(new FEMParticle(ToWorldPos(uv), uv, 0.0f));
            }

            // generate elements and assign mass based on connected area
            for (int t = 0; t < tris.Count; t += 3)
            {
                int i = tris[t];
                int j = tris[t + 1];
                int k = tris[t + 2];

                // calculate tri area
                Vector2f a = Scene.Particles[i].p;
                Vector2f b = Scene.Particles[j].p;
                Vector2f c = Scene.Particles[k].p;

                float area = 0.5f * Vector2f.Cross(b - a, c - a);
                float mass = density * area / 3.0f;

                Scene.Particles[i].invMass += mass;
                Scene.Particles[j].invMass += mass;
                Scene.Particles[k].invMass += mass;

                Scene.Triangles.Add(new Triangle(i, j, k));
            }

            // convert mass to invmass
            for (int i = 0; i < Scene.Particles.Count; ++i)
            {
                if (Scene.Particles[i].invMass > 0.0f)
                    Scene.Particles[i].invMass = 1.0f / Scene.Particles[i].invMass;
            }

            // assign index to particles
            for (int i = 0; i < Scene.Particles.Count; ++i)
                Scene.Particles[i].index = i;

            Scene.CreateElements();

            return Scene;
        }

        /// <summary>
        /// return true if any pixels in box are non-zero 
        /// </summary>
        private static int Neighbours(Texture2D img, int cx, int cy, int margin)
        {
            int xmin = Mathf.Max(0, cx - margin);
            int xmax = Mathf.Min(cx + margin, img.width - 1);
            int ymin = Mathf.Max(0, cy - margin);
            int ymax = Mathf.Min(cy + margin, img.height - 1);

            int count = 0;
            for (int y = ymin; y <= ymax; ++y)
            {
                for (int x = xmin; x <= xmax; ++x)
                {
                    Color col = img.GetPixel(x, y);

                    if (col.grayscale > 0.0f)
                    {
                        ++count;
                    }
                }
            }

            return count;
        }

        private static bool EdgeDetect(Texture2D img, int x, int y)
        {
            Color colp0 = img.GetPixel(x + 1, y);
            Color coln0 = img.GetPixel(x - 1, y);

            if ((colp0.grayscale > 0.0f) != (coln0.grayscale > 0.0f))
                return true;

            Color col0p = img.GetPixel(x, y + 1);
            Color col0n = img.GetPixel(x, y - 1);

            if ((col0p.grayscale > 0.0f) != (col0n.grayscale > 0.0f))
                return true;

            return false;
        }

    }

}
