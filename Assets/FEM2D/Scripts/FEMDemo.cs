using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.Core.LinearAlgebra;

namespace FEM2D
{

    public enum FEM_SCENE { BEAM, RANDOM_CONVEX, TORUS, ARMADILLO, BUNNY, DONUT };

    public class FEMDemo : MonoBehaviour
    {

        public FEM_SCENE option = FEM_SCENE.ARMADILLO;

        public bool drawWireframe = false;

        [HideInInspector]
        public Texture2D armadillo, bunny, donut;

        [HideInInspector]
        public Material textureMaterial;

        private Texture2D texture;

        private const float TimeStep = 1.0f / 60.0f;

        private FEMScene Scene;

        private Vector2f MousePos;

        private int MouseIndex;

        private bool drawTexture;

        private float mouseStrength = 1000;

        private Material m_colorMaterial;
        private Material ColorMaterial
        {
            get
            {
                if (m_colorMaterial == null)
                    m_colorMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                return m_colorMaterial;
            }
        }

        void Start()
        {


            switch(option)
            {
                case FEM_SCENE.BEAM:
                    drawWireframe = true;
                    drawTexture = false;
                    mouseStrength = 200;
                    Scene = CreateCantileverBeam.Create(new Vector2f(-0.5f, 0.5f), 1.0f, 6, true);
                    break;

                case FEM_SCENE.RANDOM_CONVEX:
                    drawWireframe = true;
                    drawTexture = false;
                    mouseStrength = 200;
                    Scene = CreateRandomConvex.Create(new Vector2f(0.0f, 0.5f), 1.0f, 10);
                    break;

                case FEM_SCENE.TORUS:
                    drawWireframe = true;
                    drawTexture = false;
                    mouseStrength = 200;
                    Scene = CreateTorus.Create(new Vector2f(0.0f, 0.5f), 0.2f, 0.5f, 12);
                    break;

                case FEM_SCENE.ARMADILLO:
                    drawTexture = true;
                    texture = armadillo;
                    mouseStrength = 2000;
                    Scene = CreateFromImage.Create(texture);
                    break;

                case FEM_SCENE.BUNNY:
                    drawTexture = true;
                    texture = bunny;
                    mouseStrength = 2000;
                    Scene = CreateFromImage.Create(texture);
                    break;

                case FEM_SCENE.DONUT:
                    drawTexture = true;
                    texture = donut;
                    mouseStrength = 2000;
                    Scene = CreateFromImage.Create(texture);
                    break;
            }

            Scene.Planes.Add(new Vector3f(0.0f, 1.0f, 1.8f));
            Scene.Planes.Add(new Vector3f(1.0f, 0.0f, 2.8f));
            Scene.Planes.Add(new Vector3f(-1.0f, 0.0f, 2.8f));

            if (textureMaterial != null)
                textureMaterial.SetTexture("_MainTex", texture);
        }

        void Update()
        {

            if (Scene == null) return;

            float dt = TimeStep;
            int steps = Scene.Substeps;

            dt /= steps;

            Vector3 p = Input.mousePosition;
            p = Camera.main.ScreenToWorldPoint(p);

            MousePos = new Vector2f(p.x, p.y);

            MouseIndex = -1;
            if (Input.GetMouseButton(0))
                MouseIndex = FindClosestParticle();

            for (int i = 0; i < steps; ++i)
            {
                MoveByMouseDrag(dt);
                Scene.Update(dt);
            }

        }

        int FindClosestParticle()
        {
            float minDistSq = float.PositiveInfinity;
            int minIndex = -1;

            for (int i = 0; i < Scene.Particles.Count; ++i)
            {
                float d = (MousePos - Scene.Particles[i].p).Magnitude;

                if (d < minDistSq)
                {
                    minDistSq = d;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        void MoveByMouseDrag(float dt)
        {

            float dampStrength = 10;

            if (MouseIndex != -1)
            {
                Vector2f pq = MousePos - Scene.Particles[MouseIndex].p;

                Vector2f damp = -dampStrength * Vector2f.Dot(pq.Normalized, Scene.Particles[MouseIndex].v) * pq.Normalized;
                Vector2f stretch = mouseStrength * pq;

                Scene.Particles[MouseIndex].f += stretch + damp;
            }
        }

        private void OnPostRender()
        {
            Camera camera = Camera.current;
            if (camera == null) return;
            if (Scene == null) return;

            GL.PushMatrix();

            GL.LoadIdentity();
            GL.MultMatrix(camera.worldToCameraMatrix);
            GL.LoadProjectionMatrix(camera.projectionMatrix);

            DrawTexturedTriangles();
            DrawLineTriangles();

            DrawMouseLine();

            DrawScenePlanes();
            //DrawFracturePlanes();

            DrawMouseVert();

            DrawModelVerts();

            GL.PopMatrix();
        }

        private void DrawPoint(Vector2f p, float s)
        {
            GL.Vertex3(p.x + s, p.y + s, 0.0f);
            GL.Vertex3(p.x + s, p.y - s, 0.0f);
            GL.Vertex3(p.x - s, p.y - s, 0.0f);
            GL.Vertex3(p.x - s, p.y + s, 0.0f);
        }

        private void DrawPoint(Vector2 p, float s)
        {
            GL.Vertex3(p.x + s, p.y + s, 0.0f);
            GL.Vertex3(p.x + s, p.y - s, 0.0f);
            GL.Vertex3(p.x - s, p.y - s, 0.0f);
            GL.Vertex3(p.x - s, p.y + s, 0.0f);
        }

        private void DrawMouseLine()
        {
            ColorMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            if (MouseIndex != -1)
            {
                Vector2f v0 = Scene.Particles[MouseIndex].p;
                Vector2f v1 = MousePos;

                GL.Vertex3(v0.x, v0.y, 0.0f);
                GL.Vertex3(v1.x, v1.y, 0.0f);
            }

            GL.End();
        }

        private void DrawScenePlanes()
        {
            ColorMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            for (int i = 0; i < Scene.Planes.Count; ++i)
            {
                Vector3f plane = Scene.Planes[i];
                Vector2f n = plane.xy;
                Vector2f c = -plane.z * n;

                Vector2f v0 = c + n.PerpendicularCCW * 100.0f;
                Vector2f v1 = c - n.PerpendicularCCW * 100.0f;

                GL.Vertex3(v0.x, v0.y, 0.0f);
                GL.Vertex3(v1.x, v1.y, 0.0f);
            }

            GL.End();
        }

        private void DrawFracturePlanes()
        {
            ColorMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.green);

            for (int i = 0; i < Scene.Fractures.Count; ++i)
            {
                Vector3f plane = Scene.Fractures[i].Plane;
                Vector2f n = plane.xy;
                Vector2f c = -plane.z * n;

                Vector2f v0 = c + n.PerpendicularCCW * 100.0f;
                Vector2f v1 = c - n.PerpendicularCCW * 100.0f;

                GL.Vertex3(v0.x, v0.y, 0.0f);
                GL.Vertex3(v1.x, v1.y, 0.0f);
            }

            GL.End();
        }

        private void DrawTexturedTriangles()
        {

            if (!drawTexture) return;
            if (textureMaterial == null) return;
            if (texture == null) return;

            textureMaterial.SetPass(0);
            textureMaterial.SetTexture("_MainTex", texture);

            GL.Begin(GL.TRIANGLES);

            for (int i = 0; i < Scene.Triangles.Count; i++)
            {
                int i0 = Scene.Triangles[i].i;
                int i1 = Scene.Triangles[i].j;
                int i2 = Scene.Triangles[i].k;

                Vector2f v0 = Scene.Particles[i0].p;
                Vector2f v1 = Scene.Particles[i1].p;
                Vector2f v2 = Scene.Particles[i2].p;

                Vector2f uv0 = Scene.Particles[i0].uv;
                Vector2f uv1 = Scene.Particles[i1].uv;
                Vector2f uv2 = Scene.Particles[i2].uv;

                //triangles are ccw but should be cw for drawing so flip them

                GL.TexCoord2(uv2.x, uv2.y);
                GL.Vertex3(v2.x, v2.y, 0.0f);

                GL.TexCoord2(uv1.x, uv1.y);
                GL.Vertex3(v1.x, v1.y, 0.0f);

                GL.TexCoord2(uv0.x, uv0.y);
                GL.Vertex3(v0.x, v0.y, 0.0f);
                
            }

            GL.End();

        }

        private void DrawLineTriangles()
        {
            if (!drawWireframe) return;

            ColorMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);

            for (int i = 0; i < Scene.Triangles.Count; i++)
            {
                int i0 = Scene.Triangles[i].i;
                int i1 = Scene.Triangles[i].j;
                int i2 = Scene.Triangles[i].k;

                Vector2f v0 = Scene.Particles[i0].p;
                Vector2f v1 = Scene.Particles[i1].p;
                Vector2f v2 = Scene.Particles[i2].p;

                GL.Vertex3(v0.x, v0.y, 0.0f);
                GL.Vertex3(v1.x, v1.y, 0.0f);

                GL.Vertex3(v0.x, v0.y, 0.0f);
                GL.Vertex3(v2.x, v2.y, 0.0f);

                GL.Vertex3(v2.x, v2.y, 0.0f);
                GL.Vertex3(v1.x, v1.y, 0.0f);
            }

            GL.End();
        }

        private void DrawMouseVert()
        {
            ColorMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(Color.yellow);

            if (MouseIndex != -1)
            {
                DrawPoint(MousePos, 0.005f);
            }

            GL.End();
        }

        private void DrawModelVerts()
        {
            if (!drawWireframe) return;

            ColorMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color(Color.yellow);

            for (int i = 0; i < Scene.Particles.Count; i++)
            {
                DrawPoint(Scene.Particles[i].p, 0.005f);
            }

            GL.End();
        }

    }


}
