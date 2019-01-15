using OpenTK;
using System;
using StudioSB.Scenes;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.GLObjectManagement;
using StudioSB.Rendering;
using SFGraphics.Controls;
using StudioSB.Scenes.Animation;
using SFGraphics.GLObjects.Textures;

namespace StudioSB.GUI
{
    /// <summary>
    /// This is the main viewport used for rendering in the MainForm
    /// </summary>
    public class SBViewport : GLViewport
    {
        public SBScene Scene { get
            {
                return _scene;
            }
            set
            {
                _scene = value;

                UpdateSceneInformation();

                ScreenTexture = null;

                Updated = true;
            }
        }
        private SBScene _scene;

        public Texture ScreenTexture;

        public float Frame
        {
            set
            {
                if (_animation != null)
                    _animation.UpdateScene(value, Scene);
            }
            get
            {
                return 0;
            }
        }

        public SBAnimation Animation
        {
            get
            {
                return _animation;
            }
            set
            {
                _animation = value;
            }
        }
        private SBAnimation _animation;

        // cache information
        private int PolyCount { get; set; }
        private int VertexCount { get; set; }

        private bool ReadyToRender { get; set; } = false;
        public bool Updated { get; set; } = true;

        #region Camera Controls

        public Camera camera = new Camera() { FarClipPlane = 500000 };
        private Vector2 mousePosition = new Vector2();
        private float mouseScrollWheel = 0;

        #endregion

        public SBViewport()
        {
            ApplicationSettings.SkinControl(this);
        }

        /// <summary>
        /// Setup OpenGL
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ReadyToRender = true;

            Paint += ViewportPaint;

            Rendering.ShaderManager.SetUpShaders();

            OnRenderFrame += Render;

            camera.RenderWidth = Width;
            camera.RenderHeight = Height;
        }

        /// <summary>
        /// Updates cached information about the scene
        /// </summary>
        private void UpdateSceneInformation()
        {
            PolyCount = 0;
            VertexCount = 0;
            if (_scene != null)
            {
                foreach (var mesh in _scene.GetMeshObjects())
                {
                    PolyCount += mesh.PolyCount;
                    VertexCount += mesh.VertexCount;
                }
            }
        }

        private void ViewportPaint(object sender, EventArgs args)
        {
            OnUpdateFrame();

            // Only render when something in the scene has been updated
            if (Updated)
                RenderFrame();
            Updated = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            if (ReadyToRender)
            {
                GL.Viewport(0, 0, Width, Height);

                camera.RenderWidth = Width;
                camera.RenderHeight = Height;

                Updated = true;
            }
        }

        /// <summary>
        /// </summary>
        protected void OnUpdateFrame()
        {
            UpdateCamera();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void Render(object sender, EventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // push and pop attributes so no cleanup is needed
            GL.PushAttrib(AttribMask.AllAttribBits);

            if (ApplicationSettings.RenderBackgroundGradient)
            {
                GL.Disable(EnableCap.DepthTest);
                RenderBackground();
            }

            GL.Enable(EnableCap.DepthTest);

            GL.MatrixMode(MatrixMode.Modelview);
            Matrix4 modelViewMatrix = camera.MvpMatrix;
            GL.LoadMatrix(ref modelViewMatrix);
            
            if(ApplicationSettings.EnableGridDisplay)
                Rendering.Shapes.GridFloor3D.Draw(ApplicationSettings.GridSize, 25, ApplicationSettings.GridLineColor);

            if(Scene != null)
            {
                Scene.Render(camera);
            }

            if (ApplicationSettings.RenderSceneInformation)
            {
                TextRenderer.DrawOrtho(camera, "Polygon Count: " + PolyCount.ToString(), new Vector2(0, 30));
                TextRenderer.DrawOrtho(camera, "Vertex  Count: " + VertexCount.ToString(), new Vector2(0, 46));
            }

            GL.PopAttrib();

            if(ScreenTexture != null)
            StudioSB.Rendering.Shapes.ScreenTriangle.RenderTexture(ScreenTexture, false);

            // Cleanup unused gl objects
            GLObjectManager.DeleteUnusedGLObjects();
        }

        /// <summary>
        /// Rendes the background gradient
        /// TODO: use shader
        /// </summary>
        private void RenderBackground()
        {
            GL.UseProgram(0);
            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(ApplicationSettings.BGColor1);
            GL.Vertex2(-1, -1);
            GL.Color3(ApplicationSettings.BGColor1);
            GL.Vertex2(1, -1);
            GL.Color3(ApplicationSettings.BGColor2);
            GL.Vertex2(1, 1);
            GL.Color3(ApplicationSettings.BGColor2);
            GL.Vertex2(-1, 1);
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Saves the current framebuffer to specified file path
        /// </summary>
        public void SaveRender(string FileName)
        {
            using (var bitmap = SFGraphics.GLObjects.Framebuffers.Framebuffer.ReadDefaultFramebufferImagePixels(Width, Height, true))
            {
                bitmap.Save(FileName);
                SBConsole.WriteLine($"Viewport render saved to: {FileName}");
            }
        }

        /// <summary>
        /// Updates the camera with the given inputs
        /// </summary>
        private void UpdateCamera()
        {
            if (Mouse.GetState() == null) return;

            Vector2 newMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            float newMouseScrollWheel = Mouse.GetState().Wheel;
            if (Focused && ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                if (Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    camera.RotationXRadians += ((newMousePosition.Y - mousePosition.Y) / 300f);
                    camera.RotationYRadians += (newMousePosition.X - mousePosition.X) / 300f;
                    Updated = true;
                }
                if (Mouse.GetState().IsButtonDown(MouseButton.Right))
                {
                    camera.Pan((newMousePosition.X - mousePosition.X), (newMousePosition.Y - mousePosition.Y));
                    Updated = true;
                }
                if (Keyboard.GetState().IsKeyDown(Key.W))
                {
                    camera.Zoom(0.25f);
                    Updated = true;
                }
                if (Keyboard.GetState().IsKeyDown(Key.S))
                {
                    camera.Zoom(-0.25f);
                    Updated = true;
                }

                camera.Zoom((newMouseScrollWheel - mouseScrollWheel) * 0.1f);
                if ((newMouseScrollWheel - mouseScrollWheel) != 0)
                    Updated = true;
            }
            mousePosition = newMousePosition;
            mouseScrollWheel = newMouseScrollWheel;
        }
    }
}
