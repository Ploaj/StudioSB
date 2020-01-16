using System;
using System.Collections.Generic;
using StudioSB.Scenes;
using System.Windows.Forms;
using StudioSB.GUI.Attachments;
using StudioSB.Rendering.Bounding;
using OpenTK;
using StudioSB.Tools;

namespace StudioSB.GUI
{
    public class SBBoneTree : GroupBox, IAttachment
    {
        private SBBoneEditor BoneEditor { get; set; }

        private SBTreeView BoneList { get; set; }

        private SBSkeleton SelectedSkeleton { get; set; }
        
        private SBButton LoadJOBJNames;

        private SBScene Scene { get; set; }

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


            LoadJOBJNames = new SBButton("Load Bone Labels from File");
            LoadJOBJNames.Dock = DockStyle.Top;
            LoadJOBJNames.Click += (sender, args) =>
            {
                string f = "";
                if (FileTools.TryOpenFile(out f, "Bone Name INI(*.ini)|*.ini"))
                {
                    var skel = SelectedSkeleton;
                    Dictionary<string, string> newNames = new Dictionary<string, string>();
                    foreach (var line in System.IO.File.ReadAllLines(f))
                    {
                        var name = line.Split('=');
                        if (name.Length > 1)
                        {
                            var bname = name[0].Trim();
                            var newName = name[1].Trim();
                            var bone = skel[bname];
                            if (bone != null)
                            {
                                bone.Name = newName;
                            }
                            newNames.Add(bname, newName);
                        }
                    }
                    foreach (var v in Scene.GetMeshObjects())
                        if (v.ParentBone != null && newNames.ContainsKey(v.ParentBone))
                            v.ParentBone = newNames[v.ParentBone];
                    RefreshBoneList();
                }
            };

            Dock = DockStyle.Fill;
            Controls.Add(BoneEditor);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(BoneList);
            Controls.Add(LoadJOBJNames);
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
            if (SelectedSkeleton != null)
            {
                SelectedSkeleton.ClearSelection();
            }

            if (BoneList.SelectedNode == null)
            {
                BoneEditor.Visible = false;
                return;
            }

            ((SBBone)BoneList.SelectedNode.Tag).Selected = true;
            BoneEditor.BindBone((SBBone)BoneList.SelectedNode.Tag);
            BoneEditor.Visible = true;
        }

        /// <summary>
        /// loads the bone nodes from a scene
        /// </summary>
        /// <param name="Scene"></param>
        private void LoadFromScene(SBScene Scene)
        {
            this.Scene = Scene;
            SelectedSkeleton = Scene.Skeleton as SBSkeleton;
            RefreshBoneList();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshBoneList()
        {
            BoneList.Nodes.Clear();
            Dictionary<SBBone, SBTreeNode> boneToNode = new Dictionary<SBBone, SBTreeNode>();
            if (SelectedSkeleton != null)
            {
                SelectedSkeleton = SelectedSkeleton as SBSkeleton;

                foreach (var bone in SelectedSkeleton.Bones)
                {
                    var node = new SBTreeNode(bone.Name) { Tag = bone };
                    boneToNode.Add(bone, node);
                    if (bone.Parent == null)
                        BoneList.Nodes.Add(node);
                    else if (boneToNode.ContainsKey(bone.Parent))
                        boneToNode[bone.Parent].Nodes.Add(node);
                }
            }
            if (BoneList.Nodes.Count > 0)
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
            SelectedSkeleton = null;
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
