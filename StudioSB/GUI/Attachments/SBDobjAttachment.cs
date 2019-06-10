using System;
using System.Collections.Generic;
using StudioSB.Rendering.Bounding;
using System.Windows.Forms;
using StudioSB.Scenes.Melee;
using System.Drawing;

namespace StudioSB.GUI.Attachments
{
    public class SBDobjAttachment : GroupBox, IAttachment
    {
        private SBTreeView dobjList;
        private PropertyGrid propertyGrid;

        public SBDobjAttachment()
        {
            Text = "DOBJ List";
            Dock = DockStyle.Fill;
            ApplicationSettings.SkinControl(this);

            dobjList = new SBTreeView();
            dobjList.Dock = DockStyle.Top;
            dobjList.Size = new Size(200, 200);
            dobjList.CheckBoxes = true;
            dobjList.HideSelection = false;
            dobjList.AfterCheck += (sender, args) =>
            {
                if (args.Node != null && args.Node.Tag is SBHsdMesh mesh)
                {
                    mesh.Visible = args.Node.Checked;
                }
            };
            dobjList.AfterSelect += (sender, args) =>
            {
                foreach (TreeNode v in dobjList.Nodes)
                    if (v.Tag is SBHsdMesh mesh)
                        mesh.Selected = false;
                propertyGrid.SelectedObject = null;
                if (dobjList.SelectedNode != null)
                {
                    if (dobjList.SelectedNode.Tag is SBHsdMesh mesh)
                        mesh.Selected = true;
                    propertyGrid.SelectedObject = dobjList.SelectedNode.Tag;
                }
            };

            propertyGrid = new PropertyGrid();
            propertyGrid.Dock = DockStyle.Top;
            propertyGrid.Size = new Size(200, 500);

            Controls.Add(propertyGrid);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(dobjList);
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public string[] Extension()
        {
            return null;
        }

        public void OnAttach(SBViewportPanel viewportPanel)
        {
            viewportPanel.TabPanel.AddTab("DOBJs", this);
        }

        public void OnRemove(SBViewportPanel viewportPanel)
        {
        }

        public void Open(string FilePath)
        {
        }

        public bool OverlayScene()
        {
            return false;
        }

        public void Pick(Ray ray)
        {
        }

        public void Render(SBViewport viewport, float frame = 0)
        {
        }

        public void Save(string FilePath)
        {
        }

        public void Step(SBViewport viewport)
        {
        }

        public void Update(SBViewport viewport)
        {
            if(viewport.Scene is HSDScene scene)
            {
                dobjList.Nodes.Clear();
                int DOBJIndex = 0;
                foreach(SBHsdMesh m in scene.GetMeshObjects())
                {
                    SBTreeNode item = new SBTreeNode("DOBJ_" + DOBJIndex++);
                    item.Tag = m;

                    if(m.DOBJ.MOBJ != null)
                    {
                        SBTreeNode child = new SBTreeNode("MOBJ");
                        child.Tag = m.DOBJ.MOBJ;
                        item.Nodes.Add(child);
                        if(m.DOBJ.MOBJ.MaterialColor != null)
                        {
                            SBTreeNode mc = new SBTreeNode("Material Color");
                            mc.Tag = m.DOBJ.MOBJ.MaterialColor;
                            child.Nodes.Add(mc);
                        }
                        if (m.DOBJ.MOBJ.PixelProcessing != null)
                        {
                            SBTreeNode mc = new SBTreeNode("Pixel Processing");
                            mc.Tag = m.DOBJ.MOBJ.PixelProcessing;
                            child.Nodes.Add(mc);
                        }
                        if (m.DOBJ.MOBJ.Textures != null)
                        {
                            foreach(var tex in m.DOBJ.MOBJ.Textures.List)
                            {
                                SBTreeNode mc = new SBTreeNode(tex.Flags.ToString());
                                mc.Tag = tex;
                                child.Nodes.Add(mc);
                            }
                        }
                    }

                    dobjList.Nodes.Add(item);
                };
            }
        }
    }
}
