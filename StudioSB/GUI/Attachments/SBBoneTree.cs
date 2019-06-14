using System;
using System.Collections.Generic;
using StudioSB.Scenes;
using System.Windows.Forms;
using StudioSB.GUI.Attachments;
using StudioSB.Rendering.Bounding;
using OpenTK;

namespace StudioSB.GUI
{
    public class SBBoneTree : GroupBox, IAttachment
    {
        private SBBoneEditor BoneEditor { get; set; }

        private SBTreeView BoneList { get; set; }

        public SBBoneTree() : base()
        {
            //CheckBoxes = true;
            //ImageList = new System.Windows.Forms.ImageList();
            //ImageList.ImageSize = new System.Drawing.Size(24, 24);
            Text = "Bone List";
            Dock = DockStyle.Fill;
            ApplicationSettings.SkinControl(this);

            BoneList = new SBTreeView();
            BoneList.Indent = 16;

            BoneList.LabelEdit = true;

            BoneList.AfterSelect += SelectBone;

            BoneList.AfterLabelEdit += treeView_AfterLabelEdit;
            
            BoneList.Dock = DockStyle.Top;

            BoneList.Size = new System.Drawing.Size(400, 300);

            BoneEditor = new SBBoneEditor();
            BoneEditor.Dock = DockStyle.Fill;
            
            Dock = DockStyle.Fill;
            Controls.Add(BoneEditor);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(BoneList);
        }

        public bool OverlayScene()
        {
            return false;
        }
        
        private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            this.BeginInvoke(new Action(() => afterAfterEdit(e.Node)));
        }

        private void afterAfterEdit(TreeNode node)
        {
            if (node.Tag is SBBone bone)
                bone.Name = node.Text;
        }

        /// <summary>
        /// Selects a bone and displays information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SelectBone(object sender, EventArgs args)
        {
            if (BoneList.SelectedNode == null)
            {
                BoneEditor.Visible = false;
                return;
            }

            BoneEditor.BindBone((SBBone)BoneList.SelectedNode.Tag);
            BoneEditor.Visible = true;
        }

        /// <summary>
        /// loads the bone nodes from a scene
        /// </summary>
        /// <param name="Scene"></param>
        private void LoadFromScene(SBScene Scene)
        {
            BoneList.Nodes.Clear();
            Dictionary<SBBone, SBTreeNode> boneToNode = new Dictionary<SBBone, SBTreeNode>();
            if(Scene.Skeleton != null)
            foreach(var bone in Scene.Skeleton.Bones)
            {
                var node = new SBTreeNode(bone.Name) { Tag = bone };
                boneToNode.Add(bone, node);
                if (bone.Parent == null)
                        BoneList.Nodes.Add(node);
                else if (boneToNode.ContainsKey(bone.Parent))
                    boneToNode[bone.Parent].Nodes.Add(node);
            }
            if(BoneList.Nodes.Count > 0)
                BoneList.Nodes[0].ExpandAll();
        }

        public void Update(SBViewport viewport)
        {
            if(viewport.Scene != null)
            LoadFromScene(viewport.Scene);
        }

        public void Step(SBViewport viewport)
        {
            // none
        }

        public void Render(SBViewport viewport, float frame)
        {
            // none
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public void OnAttach(SBViewportPanel viewportPanel)
        {
            viewportPanel.TabPanel.AddTab("Bones", this);
        }

        public void OnRemove(SBViewportPanel viewportPanel)
        {
            BoneEditor.Visible = false;
        }

        public void Pick(Ray ray)
        {
            foreach(var v in BoneList.Nodes)
            {
                if(v is SBTreeNode node)
                {
                    Pick(node, ray);
                }
            }
        }

        private void Pick(SBTreeNode treeNode, Ray ray)
        {
            Vector3 close;
            if (treeNode.Tag is SBBone bone)
            {
                if (ray.CheckSphereHit(Vector3.TransformPosition(Vector3.Zero, bone.AnimatedWorldTransform), 0.5f, out close))
                {
                    BoneList.SelectedNode = treeNode;
                    return;
                }
            }

            foreach (var child in treeNode.Nodes)
            {
                Pick((SBTreeNode)child, ray);
            }
        }
        
        public string[] Extension()
        {
            return null;
        }

        public void Open(string FilePath)
        {
        }

        public void Save(string FilePath)
        {
        }
    }

    public class SBTreeNode : System.Windows.Forms.TreeNode
    {
        public SBTreeNode(string Name) : base(Name)
        {
            Checked = true;
        }
    }
}
