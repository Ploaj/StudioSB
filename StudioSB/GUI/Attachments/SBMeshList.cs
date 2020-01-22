using StudioSB.GUI.Attachments;
using StudioSB.GUI.Editors;
using StudioSB.Scenes;
using System;
using System.Windows.Forms;
using StudioSB.Rendering.Bounding;
using System.Drawing;
using System.Collections.Generic;

namespace StudioSB.GUI
{
    public class SBMeshList : GroupBox, IAttachment
    {
        private SBViewportPanel ParentViewportPanel
        {
            get
            {
                var parent = Parent;
                while (true)
                {
                    if (parent is SBViewportPanel form)
                        return form;
                    parent = parent.Parent;
                    if (parent == null)
                        return null;
                }
            }
        }

        private SBMeshPanel MeshPanel { get; set; }
        
        private SBListView meshObjectList { get; set; }

        private SBButton DeleteButton;
        private SBButton HideVISButton;
        private SBButton MoveUpButton;
        private SBButton MoveDownButton;

        public SBMeshList() : base()
        {
            Text = "Object List";
            Dock = DockStyle.Fill;
            ApplicationSettings.SkinControl(this);

            meshObjectList = new SBListView();
            meshObjectList.CheckBoxes = true;
            meshObjectList.View = View.Details;
            meshObjectList.Scrollable = true;
            meshObjectList.HeaderStyle = ColumnHeaderStyle.None;

            ColumnHeader header = new ColumnHeader();
            header.Text = "";
            header.Name = "Meshes";
            header.Width = 1000;
            meshObjectList.Columns.Add(header);

            meshObjectList.LabelEdit = true;
            meshObjectList.HideSelection = false;

            meshObjectList.AfterLabelEdit += listview_AfterLabelEdit;

            meshObjectList.ItemChecked += CheckChanged;
            meshObjectList.MouseUp += SelectedChanged;

            meshObjectList.Dock = DockStyle.Top;

            meshObjectList.Size = new System.Drawing.Size(400, 200);

            MeshPanel = new SBMeshPanel();
            MeshPanel.Dock = DockStyle.Fill;
            
            DeleteButton = new SBButton("Delete Selected Mesh");
            DeleteButton.Click += (sender, args) =>
            {
                if(MessageBox.Show("Delete Selected Mesh", "This cannot be undone", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    MeshPanel.DeleteSelectedMesh();
                    LoadFromScene(MeshPanel.SelectedScene);
                }
            };
            DeleteButton.Dock = DockStyle.Top;
            
            HideVISButton = new SBButton("Hide VIS Objects");
            HideVISButton.Dock = DockStyle.Top;
            HideVISButton.Click += (sender, args) =>
            {
                HideVISObjects();
            };
            
            MoveUpButton = new SBButton("Move Selected Up");
            MoveUpButton.Dock = DockStyle.Top;
            MoveUpButton.Click += (sender, args) =>
            {
                if (meshObjectList.Items.Count > 0 && !meshObjectList.Items[0].Selected)
                {
                    foreach (ListViewItem lvi in meshObjectList.SelectedItems)
                    {
                        if (lvi.Index > 0)
                        {
                            int index = lvi.Index - 1;
                            meshObjectList.Items.RemoveAt(lvi.Index);
                            meshObjectList.Items.Insert(index, lvi);
                        }
                    }

                    RefreshMeshItemsFromList();
                }
            };

            MoveDownButton = new SBButton("Move Selected Down");
            MoveDownButton.Dock = DockStyle.Top;
            MoveDownButton.Click += (sender, args) =>
            {
                if (meshObjectList.Items.Count > 0 && !meshObjectList.Items[meshObjectList.Items.Count - 1].Selected)
                {
                    foreach (ListViewItem lvi in meshObjectList.SelectedItems)
                    {
                        if (lvi.Index < meshObjectList.Items.Count)
                        {
                            int index = lvi.Index + 1;
                            meshObjectList.Items.RemoveAt(lvi.Index);
                            meshObjectList.Items.Insert(index, lvi);
                        }
                    }
                    RefreshMeshItemsFromList();
                }
            };

            Controls.Add(MeshPanel);
            Controls.Add(MoveDownButton);
            Controls.Add(MoveUpButton);
            Controls.Add(DeleteButton);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(meshObjectList);
            Controls.Add(HideVISButton);
        }

        /// <summary>
        /// Updates the scene to contain the mesh items in this list
        /// </summary>
        private void RefreshMeshItemsFromList()
        {
            var scene = ParentViewportPanel.Viewport.Scene;

            if (scene == null)
                return;

            List<ISBMesh> meshes = new List<ISBMesh>();

            foreach (ListViewItem v in meshObjectList.Items)
                if (v.Tag is ISBMesh mesh)
                    meshes.Add(mesh);

            scene.SetMeshObjects(meshes.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        public void HideVISObjects()
        {
            foreach (ListViewItem v in meshObjectList.Items)
                if (v.Text.Contains("VIS_O"))
                    v.Checked = false;
        }

        public bool OverlayScene()
        {
            return false;
        }
        
        /// <summary>
        /// for renaming a mesh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listview_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (meshObjectList.Items[e.Item].Tag is ISBMesh mesh)
                if(e.Label != null)
                    mesh.Name = e.Label;
        }

        /// <summary>
        /// selecting a mesh object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SelectedChanged(object sender, EventArgs args)
        {
            //deselect all nodes
            foreach (var item in meshObjectList.Items)
                if (((ListViewItem)item).Tag is ISBMesh mesh)
                    mesh.Selected = false;

            if (meshObjectList.SelectedItems == null || meshObjectList.SelectedItems.Count == 0)
            {
                //ParentForm.SelectMesh(null);
                return;
            }

            //select selected nodes
            ISBMesh[] selected = new ISBMesh[meshObjectList.SelectedItems.Count];
            int meshIndex = 0;
            foreach (var item in meshObjectList.SelectedItems)
            {
                if (((ListViewItem)item).Tag is ISBMesh mesh)
                {
                    selected[meshIndex++] = mesh;
                    mesh.Selected = true;
                }
            }
            SelectMesh(selected);
        }

        /// <summary>
        /// Shows the meshpanel and have it load the selected meshes
        /// </summary>
        /// <param name="Mesh"></param>
        public void SelectMesh(ISBMesh[] Mesh)
        {
            if (ParentViewportPanel.Viewport.Scene == null) return;
            MeshPanel.SetSelectedMeshFromScene(ParentViewportPanel.Viewport.Scene);
        }

        /// <summary>
        /// Fired on check changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CheckChanged(object sender, EventArgs args)
        {
            if (args is ItemCheckedEventArgs itemargs)
            {
                if (itemargs.Item.Tag is ISBMesh mesh)
                    mesh.Visible = itemargs.Item.Checked;
            }
            
            if (meshObjectList.SelectedItems == null || meshObjectList.SelectedItems.Count == 0) return;

            foreach(var item in meshObjectList.SelectedItems)
            {
                var tag = ((ListViewItem)item).Tag;
                if (tag is ISBMesh mesh)
                    mesh.Visible = ((ListViewItem)item).Checked;
            }
            
        }

        /// <summary>
        /// loads the bone nodes from a scene
        /// </summary>
        /// <param name="Scene"></param>
        private void LoadFromScene(SBScene Scene)
        {
            meshObjectList.Items.Clear();
            var meshes = Scene.GetMeshObjects();
            if(meshes != null)
            {
                foreach (var mesh in meshes)
                {
                    ListViewItem item = new ListViewItem
                    {
                        Text = mesh.ToString(),
                        Tag = mesh,
                        Checked = mesh.Visible
                    };
                    meshObjectList.Items.Add(item);
                }
            }
        }

        public void OnAttach(SBViewportPanel viewportPanel)
        {
            viewportPanel.TabPanel.AddTab("Objects", this);
        }

        public void OnRemove(SBViewportPanel viewportPanel)
        {
            MeshPanel.Clear();
        }

        public void Update(SBViewport viewport)
        {
            if(viewport.Scene != null)
                LoadFromScene(viewport.Scene);
        }

        public void Step(SBViewport viewport)
        {
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public void Render(SBViewport viewport, float frame)
        {
        }

        public void Pick(Ray ray)
        {
            // bounding sphere for mesh needs to be transformed by parent bone
            /*Vector3 close;
            foreach(ListViewItem item in Items)
            {
                if(item.Tag is ISBMesh mesh)
                {
                    Vector3 spherePosition = mesh.BoundingSphere.Position;
                    if (ray.CheckSphereHit(, mesh.BoundingSphere.Radius, out close))
                    {
                        SBConsole.WriteLine(mesh.Name);
                    }
                }
            }*/
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
}
