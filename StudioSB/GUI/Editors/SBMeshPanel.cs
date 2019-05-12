using System.Collections.Generic;
using System.Windows.Forms;
using StudioSB.Scenes;

namespace StudioSB.GUI.Editors
{
    public class SBMeshPanel : Panel
    {
        private Label meshName;
        //private Label indexCount;
        //private Label vertexCount;

        private GenericBindingComboBox<ISBMaterial> materialSelector;
        // proxy in order to update all selected meshes instead of just one
        public ISBMaterial Material
        {
            get
            {
                return _material;
            }
            set
            {
                _material = value;
                if (SelectedMeshes != null)
                {
                    foreach (var mesh in SelectedMeshes)
                        mesh.Material = _material;
                }
            }
        }
        private ISBMaterial _material;

        private GenericBindingComboBox<string> parentBoneSelector;

        // proxy in order to update all selected meshes instead of just one
        public string ParentBone
        {
            get
            {
                return _parentBone;
            }
            set
            {
                _parentBone = value;
                if(SelectedMeshes != null)
                {
                    foreach (var mesh in SelectedMeshes)
                        mesh.ParentBone = _parentBone;
                }
            }
        }
        private string _parentBone;


        private ISBMesh[] SelectedMeshes = null;

        public SBMeshPanel()
        {
            materialSelector = new GenericBindingComboBox<ISBMaterial>("");
            materialSelector.Dock = DockStyle.Top;
            materialSelector.Bind(this, "Material");
            //materialSelector.MaximumSize = new System.Drawing.Size(200, 32);

            parentBoneSelector = new GenericBindingComboBox<string>("Parent Bone");
            parentBoneSelector.Dock = DockStyle.Top;
            parentBoneSelector.Bind(this, "ParentBone");
            //parentBoneSelector.MaximumSize = new System.Drawing.Size(200, 32);

            meshName = new Label();
            meshName.Dock = DockStyle.Top;
            meshName.TextAlign = System.Drawing.ContentAlignment.BottomCenter;

            Controls.Add(parentBoneSelector);
            Controls.Add(new Label() { Text = "Parent Bone:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.BottomLeft });
            Controls.Add(materialSelector);
            Controls.Add(new Label() { Text = "Material:", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.BottomLeft });
            Controls.Add(meshName);
        }

        /// <summary>
        /// Adds the selected mesh from the Scene to the editor
        /// </summary>
        /// <param name="scene"></param>
        public void SetSelectedMeshFromScene(SBScene scene)
        {
            // clear currently selected meshes
            SelectedMeshes = null;

            // loads the materials from the scene
            materialSelector.Items.Clear();
            foreach (var material in scene.GetMaterials())
            {
                materialSelector.Items.Add(material);
            }

            // loads the bone names from the scene
            parentBoneSelector.Items.Clear();
            foreach(var bone in scene.Skeleton.Bones)
            {
                parentBoneSelector.Items.Add(bone.Name);
            }

            // get selected meshes in scene
            List<ISBMesh> selected = new List<ISBMesh>();
            foreach(var mesh in scene.GetMeshObjects())
            {
                if(mesh.Selected)
                {
                    selected.Add(mesh);
                }
            }

            // for now just get the first selected
            if(selected.Count > 0)
            {
                meshName.Text = selected[0].Name + (selected.Count > 1 ? $" + {selected.Count - 1} Others" : "");
                materialSelector.SelectedItem = selected[0].Material;
                parentBoneSelector.SelectedItem = selected[0].ParentBone;
            }

            //
            SelectedMeshes = selected.ToArray();
        }
    }
}
