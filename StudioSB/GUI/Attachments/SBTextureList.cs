using System;
using System.Collections.Generic;
using StudioSB.Rendering.Bounding;
using System.Windows.Forms;
using SFGraphics.GLObjects.Textures;
using System.Drawing;
using StudioSB.IO.Formats;
using StudioSB.Scenes;
using StudioSB.Rendering;

namespace StudioSB.GUI.Attachments
{
    public class SBTextureList : GroupBox, IAttachment
    {
        /// <summary>
        /// Returns true if this panel the current attachment
        /// </summary>
        private bool IsActive
        {
            get
            {
                if (Parent != null && Parent.Parent is SBTabPanel tabPanel)
                {
                    if (tabPanel.SelectedTab == Parent)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private ListBox TextureList;
        private PropertyGrid PropertyGrid;
        private GroupBox DisplayBox;
        private SBButton R;
        private SBButton G;
        private SBButton B;
        private SBButton A;
        private TrackBar MipLevel;

        private Texture ScreenTexture { get; set; }

        private SBSurface Surface { get; set; }

        public SBTextureList()
        {
            Text = "Textures";
            Dock = DockStyle.Fill;
            ApplicationSettings.SkinControl(this);

            TextureList = new ListBox();
            TextureList.Dock = DockStyle.Top;
            TextureList.SelectedValueChanged += (sender, args) =>
            {
                PropertyGrid.SelectedObject = TextureList.SelectedValue;
            };
            
            PropertyGrid = new PropertyGrid();
            PropertyGrid.Dock = DockStyle.Top;
            PropertyGrid.Size = new Size(200, 500);

            DisplayBox = new GroupBox();
            DisplayBox.Text = "Display Options";
            DisplayBox.Dock = DockStyle.Top;
            ApplicationSettings.SkinControl(DisplayBox);

            MipLevel = new TrackBar();
            MipLevel.Maximum = 20;
            MipLevel.Dock = DockStyle.Top;
            //DisplayBox.Controls.Add(MipLevel);

            SBHBox hungryBox = new SBHBox();
            hungryBox.Dock = DockStyle.Top;
            DisplayBox.Controls.Add(hungryBox);

            R = new SBButton("R");
            R.BackColor = Color.Red;
            R.Width = 32;
            R.Click += (sender, args) =>
            {
                if (R.BackColor == Color.Gray)
                    R.BackColor = Color.Red;
                else
                    R.BackColor = Color.Gray;
            };
            hungryBox.AddControl(R);

            G = new SBButton("G");
            G.BackColor = Color.Green;
            G.Width = 32;
            G.Click += (sender, args) =>
            {
                if (G.BackColor == Color.Gray)
                    G.BackColor = Color.Green;
                else
                    G.BackColor = Color.Gray;
            };
            hungryBox.AddControl(G);

            B = new SBButton("B");
            B.BackColor = Color.Blue;
            B.Width = 32;
            B.Click += (sender, args) =>
            {
                if (B.BackColor == Color.Gray)
                    B.BackColor = Color.Blue;
                else
                    B.BackColor = Color.Gray;
            };
            hungryBox.AddControl(B);

            A = new SBButton("A");
            A.BackColor = Color.Black;
            A.Width = 32;
            A.Click += (sender, args) =>
            {
                if (A.BackColor == Color.Gray)
                    A.BackColor = Color.Black;
                else
                    A.BackColor = Color.Gray;
            };
            hungryBox.AddControl(A);

            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(PropertyGrid);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(DisplayBox);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(TextureList);
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public bool OverlayScene()
        {
            return false;
        }
        
        public string[] Extension()
        {
            return new string[] { ".nutexb" };
        }

        public void Open(string FilePath)
        {
            TextureList.Items.Clear();
            Surface = IO_NUTEXB.Open(FilePath);
            TextureList.Items.Add(Surface);
            PropertyGrid.SelectedObject = Surface;
        }

        public void Save(string FilePath)
        {

        }

        public void Pick(Ray ray)
        {

        }

        public void OnAttach(SBViewportPanel viewportPanel)
        {
            viewportPanel.TabPanel.AddTab("Textures", this);
        }

        public void OnRemove(SBViewportPanel viewportPanel)
        {

        }

        public void Render(SBViewport viewport, float frame = 0)
        {
            if (IsActive && PropertyGrid.SelectedObject != null && PropertyGrid.SelectedObject is SBSurface surface)
            {
                OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);

                Rendering.Shapes.ScreenTriangle.RenderTexture(DefaultTextures.Instance.defaultWhite);

                Rendering.Shapes.ScreenTriangle.RenderTexture(surface.CreateRenderTexture(),
                    R.BackColor != Color.Gray, G.BackColor != Color.Gray, B.BackColor != Color.Gray, A.BackColor != Color.Gray,
                    MipLevel.Value, surface.IsSRGB);
            }
        }

        public void Step(SBViewport viewport)
        {

        }

        public void Update(SBViewport viewport)
        {

        }
    }
}
