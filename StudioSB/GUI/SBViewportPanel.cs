using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudioSB.Scenes;
using StudioSB.GUI.Editors;
using StudioSB.GUI.Attachments;

namespace StudioSB.GUI
{
    /// <summary>
    /// Panel containing scene information and attachments
    /// </summary>
    public class SBViewportPanel : Panel
    {
        public SBViewport Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
            }
        }
        private SBViewport _viewport;

        public SBScene LoadedScene { get
            {
                return Viewport.Scene;
            } }

        private Timer RenderTimer { get; set; }

        public SBTabPanel TabPanel { get; set; }

        //TODO: attachments
        private System.Drawing.Size RightBarSize = new System.Drawing.Size(500, 400);
        
        private SBPopoutPanel RightPane { get; set; }

        public SBViewportPanel()
        {
            ApplicationSettings.SkinControl(this);
            BackColor = ApplicationSettings.MiddleColor;
            
            RenderTimer = new Timer();
            RenderTimer.Interval = 1000 / 120;
            RenderTimer.Tick += new EventHandler(TriggerViewportRender);
            RenderTimer.Start();

            Viewport = new SBViewport();
            Viewport.Dock = DockStyle.Fill;

            RightPane = new SBPopoutPanel(PopoutSide.Right, "<", ">");
            RightPane.Dock = DockStyle.Right;

            TabPanel = new SBTabPanel();
            TabPanel.Dock = DockStyle.Fill;

            RightPane.Contents.Add(TabPanel);

            Clear();
            Setup();
        }


        /// <summary>
        /// Raises a render frame event for the viewport.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TriggerViewportRender(object sender, EventArgs e)
        {
            if (!Viewport.IsDisposed && Viewport.IsIdle)
            {
                Viewport.Updated = true;
                Viewport.RenderFrame();
            }
        }
        
        /// <summary>
        /// Clears all information in scene
        /// </summary>
        public void Clear()
        {
            Viewport.Attachments.Clear();

            TabPanel.ClearTabs();

            Viewport.Scene = null;
            
            GC.Collect();
        }

        /// <summary>
        /// Sets up controls
        /// </summary>
        public void Setup()
        {
            Controls.Clear();
            Controls.Add(Viewport);
            if (TabPanel.TabPages.Count > 0)
                Controls.Add(RightPane);
        }

        /// <summary>
        /// Adds an attachment to this panel
        /// </summary>
        public void AddAttachment(IAttachment attachment)
        {
            attachment.AttachToPanel(this);
            Viewport.Attachments.Add(attachment);
        }

        /// <summary>
        /// Gets an attachment if it is present in the scene
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAttachment<T>()
        {
            foreach(var att in Viewport.Attachments)
                if(att is T o)
                {
                    return o;
                }
            return default(T);
        }

        /// <summary>
        /// Sets the scene and loads information
        /// </summary>
        /// <param name="scene"></param>
        public void SetScene(SBScene scene)
        {
            Viewport.Scene = scene;

            // basic attachments
            if (scene.HasMesh)
            {
                var boneattachment = new SBMeshList();
                boneattachment.Update(Viewport);
                AddAttachment(boneattachment);
            }
            if (scene.HasBones)
            {
                var boneattachment = new SBBoneTree();
                boneattachment.Update(Viewport);
                AddAttachment(boneattachment);
            }

            Setup();
        }
    }
}
