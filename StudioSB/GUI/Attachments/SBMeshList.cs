using StudioSB.GUI.Attachments;
using StudioSB.GUI.Editors;
using StudioSB.Scenes;
using System;
using System.Windows.Forms;

namespace StudioSB.GUI
{
    public class SBMeshList : SBListView, IAttachment
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

        public SBMeshList() : base()
        {
            CheckBoxes = true;
            View = View.Details;
            Scrollable = true;
            HeaderStyle = ColumnHeaderStyle.None;

            ColumnHeader header = new ColumnHeader();
            header.Text = "";
            header.Name = "Meshes";
            header.Width = 1000;
            Columns.Add(header);

            LabelEdit = true;
            HideSelection = false;

            AfterLabelEdit += listview_AfterLabelEdit;

            ItemChecked += CheckChanged;
            MouseUp += SelectedChanged;

            Dock = DockStyle.Top;

            Size = new System.Drawing.Size(400, 400);

            MeshPanel = new SBMeshPanel();
            MeshPanel.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// for renaming a mesh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listview_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (Items[e.Item].Tag is ISBMesh mesh)
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
            foreach (var item in Items)
                if (((ListViewItem)item).Tag is ISBMesh mesh)
                    mesh.Selected = false;

            if (SelectedItems == null || SelectedItems.Count == 0)
            {
                //ParentForm.SelectMesh(null);
                return;
            }

            //select selected nodes
            ISBMesh[] selected = new ISBMesh[SelectedItems.Count];
            int meshIndex = 0;
            foreach (var item in SelectedItems)
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
            
            if (SelectedItems == null || SelectedItems.Count == 0) return;

            foreach(var item in SelectedItems)
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
            Items.Clear();
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
                    Items.Add(item);
                }
            }
        }

        private Panel container;

        public void AttachToPanel(SBViewportPanel viewportPanel)
        {
            if (container != null)
                container.Dispose();
            container = new Panel();
            container.AutoScroll = true;
            container.Dock = DockStyle.Fill;
            container.Controls.Add(MeshPanel);
            container.Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            container.Controls.Add(this);
            viewportPanel.TabPanel.AddTab("Objects", container);
        }

        public void RemoveFromPanel(SBViewportPanel viewportPanel)
        {

        }

        public void Update(SBViewport viewport)
        {
            if(viewport.Scene != null)
                LoadFromScene(viewport.Scene);
        }

        public void Step()
        {
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public void Render(SBViewport viewport, float frame)
        {
        }
    }
}
