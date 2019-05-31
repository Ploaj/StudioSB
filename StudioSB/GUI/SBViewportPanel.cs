using System;
using System.Collections.Generic;
using System.Windows.Forms;
using StudioSB.Scenes;
using StudioSB.GUI.Attachments;
using System.Diagnostics;

namespace StudioSB.GUI
{
    /// <summary>
    /// Panel control for viewport that contains
    /// scene information and attachment panels
    /// </summary>
    public class SBViewportPanel : Panel
    {
        public float CurrentFrame
        {
            get
            {
                return animationBar.Frame;
            }
        }
        public int FrameCount
        {
            set
            {
                animationBar.FrameCount = value;
            }
        }
        public bool EnableAnimationBar
        {
            set
            {
                if (value)
                    Controls.Add(animationBar);
                else
                    Controls.Remove(animationBar);
            }
        }
        private SBAnimationBar animationBar;

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

        private Stopwatch stopWatch;
        public int timing = 0;

        public SBViewportPanel()
        {
            ApplicationSettings.SkinControl(this);
            BackColor = ApplicationSettings.MiddleColor;

            //Application.Idle += new EventHandler(TriggerViewportRender);
            stopWatch = new Stopwatch();
            stopWatch.Start();
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
            
            animationBar = new SBAnimationBar();
            animationBar.Dock = DockStyle.Bottom;

            animationBar.FrameCount = 0;

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
            timing += (int)stopWatch.ElapsedMilliseconds;
            stopWatch.Reset();
            stopWatch.Start();

            if (!Viewport.IsDisposed && Viewport.IsIdle && timing > 16)
            {
                timing = timing % 16;

                Viewport.Updated = true;

                animationBar.Process();

                Viewport.Frame = CurrentFrame;
                Viewport.RenderFrame();
            }
        }
        
        /// <summary>
        /// Clears all information in scene
        /// </summary>
        public void Clear()
        {
            foreach (var att in Viewport.Attachments)
                att.RemoveFromPanel(this);

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
        /// Adds an attachment to this panel and viewport
        /// </summary>
        public void AddAttachment(IAttachment attachment)
        {
            if (!attachment.AllowMultiple())
            {
                List<IAttachment> ToRemove = new List<IAttachment>();
                foreach (var att in Viewport.Attachments)
                {
                    if (!attachment.AllowMultiple() && att.GetType() == attachment.GetType())
                    {
                        ToRemove.Add(att);
                        continue;
                    }
                }
                foreach(var att in ToRemove)
                {
                    RemoveAttachment(att);
                }
            }

            if (LoadedScene != null)
                if(LoadedScene.HasBones)
                    LoadedScene.Skeleton.Reset();

            attachment.AttachToPanel(this);
            Viewport.Attachments.Add(attachment);
        }

        /// <summary>
        /// Removes attachment from panel and viewport
        /// </summary>
        /// <param name="attachment"></param>
        public void RemoveAttachment(IAttachment attachment)
        {
            if (!Viewport.Attachments.Contains(attachment))
                return;
            attachment.RemoveFromPanel(this);
            Viewport.Attachments.Remove(attachment);
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
            Clear();
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
