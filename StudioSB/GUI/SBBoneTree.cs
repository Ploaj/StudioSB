using System;
using System.Collections.Generic;
using StudioSB.Scenes;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBBoneTree : SBTreeView
    {
        private MainForm ParentForm
        {
            get
            {
                var parent = Parent;
                while (true)
                {
                    if (parent is MainForm form)
                        return form;
                    parent = parent.Parent;
                    if (parent == null)
                        return null;
                }
            }
        }

        public SBBoneTree() : base()
        {
            //CheckBoxes = true;
            //ImageList = new System.Windows.Forms.ImageList();
            //ImageList.ImageSize = new System.Drawing.Size(24, 24);

            Indent = 8;

            LabelEdit = true;

            AfterSelect += SelectBone;

            AfterLabelEdit += treeView_AfterLabelEdit;
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
            if (SelectedNode == null)
            {
                ParentForm.ResetControls();
                return;
            }
            ParentForm.SelectBone((SBBone)SelectedNode.Tag);
        }

        /// <summary>
        /// loads the bone nodes from a scene
        /// </summary>
        /// <param name="Scene"></param>
        public void LoadFromScene(SBScene Scene)
        {
            Nodes.Clear();
            Dictionary<SBBone, SBTreeNode> boneToNode = new Dictionary<SBBone, SBTreeNode>();
            if(Scene.Skeleton != null)
            foreach(var bone in Scene.Skeleton.Bones)
            {
                var node = new SBTreeNode(bone.Name) { Tag = bone };
                boneToNode.Add(bone, node);
                if (bone.Parent == null)
                    Nodes.Add(node);
                else if (boneToNode.ContainsKey(bone.Parent))
                    boneToNode[bone.Parent].Nodes.Add(node);
            }
            if(Nodes.Count > 0)
                Nodes[0].ExpandAll();
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
