using OpenTK;
using System;
using StudioSB.Scenes;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.GLObjectManagement;
using StudioSB.Rendering;
using SFGraphics.Controls;
using SFGraphics.GLObjects.Textures;
using StudioSB.GUI.Attachments;
using System.Collections.Generic;

namespace StudioSB.GUI
{
    /// <summary>
    /// This is the main viewport used for rendering in the MainForm
    /// </summary>
    public class SBViewport : GLViewport
    {
        /// <summary>
        /// 
        /// </summary>
        public List<IAttachment> Attachments = new List<IAttachment>();

        /// <summary>
        /// Scene to be rendered within the viewport
        /// </summary>
        public SBScene Scene
        {
            get => _scene;
            set
            {
                _scene = value;

                UpdateSceneInformation();

                ScreenTexture = null;

                Updated = true;

                if(_scene != null)
                {
                    float Extent = 0;
                    var io = value.GetIOModel();
                    foreach (var mesh in io.Meshes)
                        foreach (var vert in mesh.Vertices)
                        {
                            Extent = Math.Max(Extent, vert.Position.Y);
                        }
                    if (Extent > 0)
                    {
                        Camera.RotationXDegrees = 0;
                        Camera.RotationYDegrees = 0;
                        Camera.Position = new Vector3(0, Extent / 2, -Extent * 3);
                    }
                }
                
            }
        }
        private SBScene _scene;

        /// <summary>
        /// Displays texture over screen
        /// </summary>
        public Texture ScreenTexture { get; set; }

        // cache information
        private int polyCount;
        private int vertexCount;
        
        //Rendering
        private bool readyToRender = false;

        public bool Updated { get; set; } = true;
        
        public Camera Camera { get; set; } = new Camera() { FarClipPlane = 500000 };

        private Vector2 mousePosition = new Vector2();
        private float mouseScrollWheel = 0;

        public float Frame { get; set; }

        public SBViewport()
        {
            // Do I even need to skin this?
            ApplicationSettings.SkinControl(this);
        }

        /// <summary>
        /// Setup OpenGL
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            readyToRender = true;

            ShaderManager.SetUpShaders();

            OnRenderFrame += RenderViewport;

            Camera.RenderWidth = Width;
            Camera.RenderHeight = Height;
        }

        /// <summary>
        /// Updates cached information about the scene
        /// </summary>
        private void UpdateSceneInformation()
        {
            polyCount = 0;
            vertexCount = 0;
            if (_scene != null)
            {
                foreach (var mesh in _scene.GetMeshObjects())
                {
                    polyCount += mesh.PolyCount;
                    vertexCount += mesh.VertexCount;
                }
            }
        }

        /// <summary>
        /// Renders the viewport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void RenderViewport(object sender, EventArgs args)
        {
            OnUpdateFrame();

            // Only render when something in the scene has been updated
            // TODO: This conditional causes flickering.
            //if (Updated)
            Render();
            Updated = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            if (readyToRender)
            {
                Camera.RenderWidth = Width;
                Camera.RenderHeight = Height;

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
        protected void Render()
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
            Matrix4 modelViewMatrix = Camera.MvpMatrix;
            GL.LoadMatrix(ref modelViewMatrix);
            
            if(ApplicationSettings.EnableGridDisplay)
                Rendering.Shapes.GridFloor3D.Draw(ApplicationSettings.GridSize, 25, ApplicationSettings.GridLineColor);
            
            if(Scene != null)
            {
                Scene.Render(Camera);
            }

            foreach(var attachment in Attachments)
            {
                attachment.Render(this, Frame);
            }

            if (ApplicationSettings.RenderSceneInformation)
            {
                TextRenderer.DrawOrtho(Camera, "Polys: " + (polyCount/3).ToString(), new Vector2(0, 30));
                TextRenderer.DrawOrtho(Camera, "Verts: " + vertexCount.ToString(), new Vector2(0, 46));
            }

            GL.PopAttrib();

            if(ScreenTexture != null)
                StudioSB.Rendering.Shapes.ScreenTriangle.RenderTexture(ScreenTexture, false);

            // Cleanup unused gl objects
            GLObjectManager.DeleteUnusedGLObjects();
        }

        /// <summary>
        /// Renders the background gradient
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
            if (Mouse.GetState() == null)
                return;

            Vector2 newMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            float newMouseScrollWheel = Mouse.GetState().Wheel;
            if (Focused && ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                if (Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    Camera.RotationXRadians += ((newMousePosition.Y - mousePosition.Y) / 300f);
                    Camera.RotationYRadians += (newMousePosition.X - mousePosition.X) / 300f;
                    Updated = true;
                }
                if (Mouse.GetState().IsButtonDown(MouseButton.Right))
                {
                    Camera.Pan((newMousePosition.X - mousePosition.X), (newMousePosition.Y - mousePosition.Y));
                    Updated = true;
                }
                if (Keyboard.GetState().IsKeyDown(Key.W))
                {
                    Camera.Zoom(0.25f);
                    Updated = true;
                }
                if (Keyboard.GetState().IsKeyDown(Key.S))
                {
                    Camera.Zoom(-0.25f);
                    Updated = true;
                }

                Camera.Zoom((newMouseScrollWheel - mouseScrollWheel) * 0.1f);
                if ((newMouseScrollWheel - mouseScrollWheel) != 0)
                    Updated = true;
            }
            mousePosition = newMousePosition;
            mouseScrollWheel = newMouseScrollWheel;
        }
    }
}
