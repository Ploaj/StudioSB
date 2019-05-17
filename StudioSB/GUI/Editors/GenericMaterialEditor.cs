using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using StudioSB.Scenes;
using OpenTK;
using SFGraphics.Controls;
using OpenTK.Graphics.OpenGL;
using StudioSB.Rendering;
using StudioSB.GUI.Menus;

namespace StudioSB.GUI.Editors
{
    /// <summary>
    /// An editor for editing ISBMaterial objects
    /// Currently Incomplete
    /// </summary>
    public class GenericMaterialEditor : Form
    {
        //private GLControl _viewport;
        private SBScene scene;

        private SBMenuBar menu;
        private SBPopoutPanel vectorSection;
        private SBPopoutPanel floatSection;
        private SBPopoutPanel boolSection;
        private Panel mainPanel;
        private Label typeLabel;
        private ToolTip toolTips;
        private GLViewport viewport;
        private SBListView materialList;
        private GenericBindingTextBox<string> materialLabel;
        private GenericBindingTextBox<string> materialName;

        private ISBMaterial CurrentMaterial;

        public GenericMaterialEditor() : base()
        {
            Text = "Material Editor";

            Size = new Size(300, 600);

            menu = new SBMenuBar();
            {
                var file = new SBToolStripMenuItem("Material");
                {
                    var delete = new SBToolStripMenuItem("Delete Selected");
                    delete.Click += DeleteMaterial;
                    file.DropDownItems.Add(delete);
                    var import = new SBToolStripMenuItem("Import Material");
                    import.Click += ImportMaterial;
                    file.DropDownItems.Add(import);
                    var export = new SBToolStripMenuItem("Export Material");
                    export.Click += ExportMaterial;
                    file.DropDownItems.Add(export);
                }
                menu.Items.Add(file);
            }

            //_viewport = new GLControl();
            //_viewport.Size = new Size(400, 400);
            //_viewport.Dock = DockStyle.Top;

            MinimumSize = new Size(240, 240);

            ApplicationSettings.SkinControl(this);
            //Controls.Add(_viewport);

            viewport = new GLViewport();
            viewport.OnRenderFrame += Render;
            viewport.Dock = DockStyle.Top;
            viewport.MinimumSize = new Size(240, 240);

            materialList = new SBListView();
            materialList.Dock = DockStyle.Top;
            materialList.MinimumSize = new Size(240, 180);
            materialList.MultiSelect = false;
            materialList.View = View.LargeIcon;
            materialList.HideSelection = false;
            materialList.SelectedIndexChanged += SelectedMaterialChanged;

            toolTips = new ToolTip();

            TopMost = true;

            mainPanel = new Panel();
            mainPanel.AutoSize = true;
            mainPanel.AutoScroll = true;
            mainPanel.Dock = DockStyle.Fill;

            vectorSection = new SBPopoutPanel(PopoutSide.Bottom, "Vectors", "Vectors");
            vectorSection.Dock = DockStyle.Top;

            floatSection = new SBPopoutPanel(PopoutSide.Bottom, "Floats", "Floats");
            floatSection.Dock = DockStyle.Top;

            boolSection = new SBPopoutPanel(PopoutSide.Bottom, "Bools", "Bools");
            boolSection.Dock = DockStyle.Top;

            mainPanel.Controls.Add(vectorSection);
            mainPanel.Controls.Add(floatSection);
            mainPanel.Controls.Add(boolSection);

            typeLabel = new Label();
            typeLabel.Dock = DockStyle.Top;

            materialLabel = new GenericBindingTextBox<string>();
            materialLabel.Dock = DockStyle.Top;

            materialName = new GenericBindingTextBox<string>();
            materialName.Dock = DockStyle.Top;

            Controls.Add(mainPanel);
            //Controls.Add(viewport);
            Controls.Add(materialName);
            Controls.Add(materialLabel);
            Controls.Add(materialList);
            Controls.Add(typeLabel);
            Controls.Add(menu);

            ResizeEnd += onResize;
            FormClosing += Editor_FormClosing;
        }

        /// <summary>
        /// Hides this form instead of closing it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        /// <summary>
        /// sets and binds the material to an editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="material"></param>
        public void SetMaterialsFromScene(SBScene scene)
        {
            ISBMaterial[] material = scene.GetMaterials();

            if (material == null || material.Length == 0)
            {
                ClearMaterial();
                return;
            }
            typeLabel.Text = "Material Type: " + material[0].GetType().Name;

            SetMaterial(material[0]);

            materialList.Clear();
            foreach (var mat in material)
            {
                var listitem = new ListViewItem();
                listitem.Tag = mat;
                listitem.Text = mat.Label;
                materialList.Items.Add(listitem);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        private void SetMaterial(ISBMaterial material)
        {
            ClearMaterial();

            CurrentMaterial = material;
            //applyToMesh.Enabled = true;

            materialLabel.Bind(material, "Label");
            materialName.Bind(material, "Name");

            foreach (var prop in material.GetType().GetProperties())
            {
                CreateControl(prop, material);
            }
        }

        /// <summary>
        /// Clears the currently loaded material
        /// </summary>
        private void ClearMaterial()
        {
            //applyToMesh.Enabled = false;
            CurrentMaterial = null;
            boolSection.Contents.Clear();
            floatSection.Contents.Clear();
            vectorSection.Contents.Clear();
            toolTips.RemoveAll();
        }
        
        /// <summary>
        /// Exports material to file
        /// </summary>
        /// <param name="FileName">The output file path</param>
        /// <param name="material">The material to export</param>
        private void ExportMaterial(string FileName, ISBMaterial material)
        {
            material.ExportMaterial(FileName);
        }

        /// <summary>
        /// Imports material from file
        /// </summary>
        /// <param name="FileName"></param>
        private void ImportMaterial(string FileName)
        {

        }

        /// <summary>
        /// Exports selected material to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ExportMaterial(object sender, EventArgs args)
        {
            if (CurrentMaterial == null) return;
            string FileName;
            if(StudioSB.Tools.FileTools.TrySaveFile(out FileName))
            {
                ExportMaterial(FileName, CurrentMaterial);
            }
        }

        /// <summary>
        /// Imports material from file into the scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ImportMaterial(object sender, EventArgs args)
        {
            //TODO:
        }

        /// <summary>
        /// Deletes selected material
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DeleteMaterial(object sender, EventArgs args)
        {
            //TODO:
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SelectedMaterialChanged(object sender, EventArgs args)
        {
            if(materialList.SelectedItems.Count != 0)
            {
                SetMaterial((ISBMaterial)materialList.SelectedItems[0].Tag);
            }
        }

        /// <summary>
        /// Creates a specified control based on the SBMatAttrb in the Material File
        /// </summary>
        /// <param name="property"></param>
        /// <param name="Object"></param>
        /// <returns></returns>
        private Control CreateControl(PropertyInfo property, object Object)
        {
            if(property.PropertyType == typeof(SBMatAttrib<Vector4>))
            {
                SBMatAttrib<Vector4> matinfo = (SBMatAttrib<Vector4>)property.GetValue(Object);

                GenericBindingVector4Editor editor = new GenericBindingVector4Editor(matinfo.Name, matinfo.IsColor);
                editor.Dock = DockStyle.Top;
                editor.Bind(matinfo, "Value");
                toolTips.SetToolTip(editor.Controls[0], matinfo.Description);

                vectorSection.Contents.Add(editor);
            }
            if (property.PropertyType == typeof(SBMatAttrib<bool>))
            {
                SBMatAttrib<bool> matinfo = (SBMatAttrib<bool>)property.GetValue(Object);

                GenericBindingCheckBox editor = new GenericBindingCheckBox(matinfo.Name);
                editor.Dock = DockStyle.Top;
                editor.Bind(matinfo, "Value");

                toolTips.SetToolTip(editor, matinfo.Description);
                boolSection.Contents.Add(editor);
            }
            if (property.PropertyType == typeof(SBMatAttrib<float>))
            {
                SBMatAttrib<float> matinfo = (SBMatAttrib<float>)property.GetValue(Object);

                GenericBindingTextBox<float> editor = new GenericBindingTextBox<float>();
                editor.Bind(matinfo, "Value");

                SBHBox hungrybox = new SBHBox();
                hungrybox.AddControl(new Label() { Text = matinfo.Name });
                hungrybox.AddControl(editor);
                hungrybox.Dock = DockStyle.Top;

                toolTips.SetToolTip(hungrybox, matinfo.Description);
                floatSection.Contents.Add(hungrybox);
            }

            return null;
        }

        private void onResize(object sender, EventArgs args)
        {
            viewport.RenderFrame();
        }

        public void Render(object sender, EventArgs args)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.Enable(EnableCap.Texture2D);

            StudioSB.Rendering.Shapes.ScreenTriangle.RenderTexture(DefaultTextures.Instance.defaultPrm);
        }
    }
}
