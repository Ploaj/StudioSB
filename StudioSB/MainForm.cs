using System;
using System.Windows.Forms;
using StudioSB.Scenes;
using StudioSB.GUI;
using StudioSB.GUI.Menus;
using System.Linq;
using System.Collections.Generic;
using StudioSB.GUI.Editors;
using StudioSB.IO;
using StudioSB.GUI.Projects;

namespace StudioSB
{
    //TODO: move scene specific stuff elsewhere?
    /// <summary>
    /// 
    /// </summary>
    public partial class MainForm : Form
    {
        public SBViewport Viewport {
            get
            {
                return _viewport;
            }
            internal set
            {
                _viewport = value;
            }
        }
        private SBViewport _viewport;
        private SBMenuBar MenuBar { get; set; }
        private Timer RenderTimer { get; set; }

        // constand panels

        private SBProjectTree projectTree { get; set; }

        private SBPopoutPanel LeftPane { get; set; }

        private SBPopoutPanel BottomPane { get; set; }

        private SBPopoutPanel RightPane { get; set; }

        // Model Viewport Stuff
        private SBAnimationBar animationBar { get; set; }

        private System.Drawing.Size RightBarSize = new System.Drawing.Size(500, 400);

        private SBBoneTree BoneTree { get; set; }
        private SBMeshList MeshList { get; set; }

        private SBBoneEditor BoneEditor { get; set; }
        private SBMeshPanel MeshPanel { get; set; }
        
        // Application stuff
        private SBRenderSettingsEditor ApplicationSettingsEditor { get; set; }
        private GenericMaterialEditor MaterialEditor { get; set; }

        // File loading stuff
        public static List<string> OpenableExtensions = new List<string>();

        private static Type[] SceneTypes =
    (from assemblyType in AppDomain.CurrentDomain.GetAssemblies()
    from type in assemblyType.GetTypes()
    where typeof(SBScene).IsAssignableFrom(type)
    select type).ToArray();

        private List<IImportableModelType> ModelImporters = new List<IImportableModelType>();
        private List<IExportableModelType> ModelExporters = new List<IExportableModelType>();

        private List<IImportableAnimation> AnimationImporters = new List<IImportableAnimation>();
        private List<IExportableAnimation> AnimationExporters = new List<IExportableAnimation>();

        public MainForm()
        {
            InitializeComponent();

            ApplicationSettings.Init();

            ApplicationSettings.SkinControl(this);

            // Editors that are forms
            ApplicationSettingsEditor = new SBRenderSettingsEditor();
            MaterialEditor = new GenericMaterialEditor();

            projectTree = new SBProjectTree();
            projectTree.Dock = DockStyle.Fill;

            LeftPane = new SBPopoutPanel(PopoutSide.Left, ">", "<");
            LeftPane.Dock = DockStyle.Left;
            LeftPane.Contents.Add(projectTree);
            
            BottomPane = new SBPopoutPanel(PopoutSide.Bottom, "Open Console", "Close Console");
            BottomPane.Dock = DockStyle.Bottom;
            BottomPane.Contents.Add(SBConsole.Console);
            
            MenuBar = new SBMenuBar();
            {
                var ts = new SBToolStripMenuItem("File");
                {
                    var open = new SBToolStripMenuItem("Open");
                    {
                        var folder = new SBToolStripMenuItem("Folder");
                        folder.Click += OpenFolder;
                        folder.ShortcutKeys = Keys.O | Keys.Control | Keys.Shift | Keys.Alt;
                        open.DropDownItems.Add(folder);

                        var scene = new SBToolStripMenuItem("Scene");
                        scene.Click += OpenFile;
                        scene.ShortcutKeys = Keys.O | Keys.Control;
                        open.DropDownItems.Add(scene);
                    }
                    ts.DropDownItems.Add(open);
                }
                {
                    var open = new SBToolStripMenuItem("Import");
                    {
                        var scene = new SBToolStripMenuItem("Model Into Scene");
                        scene.Click += ImportToScene;
                        open.DropDownItems.Add(scene);

                        var animimport = new SBToolStripMenuItem("Animation Into Scene");
                        animimport.Click += ImportAnimationToScene;
                        open.DropDownItems.Add(animimport);
                    }
                    ts.DropDownItems.Add(open);
                }
                {
                    var open = new SBToolStripMenuItem("Export");
                    {
                        var folder = new SBToolStripMenuItem("Scene to File(s)");
                        folder.Click += SaveScene;
                        open.DropDownItems.Add(folder);

                        var animexport = new SBToolStripMenuItem("Animation to File");
                        animexport.Click += ExportAnimationToFile;
                        open.DropDownItems.Add(animexport);
                    }
                    ts.DropDownItems.Add(open);
                }
                {
                    var closeWkspc = new SBToolStripMenuItem("Clear Workspace");
                    closeWkspc.Click += CloseWorkspace;
                    ts.DropDownItems.Add(closeWkspc);
                }
                MenuBar.Items.Add(ts);

                var view = new SBToolStripMenuItem("View");
                {
                    var rsettings = new SBToolStripMenuItem("Render Settings");
                    rsettings.Click += OpenRenderSettings;
                    view.DropDownItems.Add(rsettings);

                    var meditor = new SBToolStripMenuItem("Material Editor");
                    meditor.Click += OpenMaterialEditor;
                    view.DropDownItems.Add(meditor);
                }
                MenuBar.Items.Add(view);

                var viewport = new SBToolStripMenuItem("Camera");
                {
                    var resetCamera = new SBToolStripMenuItem("Reset Camera Position");
                    resetCamera.Click += Viewport_ResetCameraPosition;
                    viewport.DropDownItems.Add(resetCamera);

                    var rsettings = new SBToolStripMenuItem("Render Viewport to File");
                    rsettings.Click += ExportRenderToFile;
                    viewport.DropDownItems.Add(rsettings);
                }
                MenuBar.Items.Add(viewport);

            }
            MenuBar.Dock = DockStyle.Top;
            
            Viewport = new SBViewport();
            Viewport.Dock = DockStyle.Fill;

            BoneTree = new SBBoneTree();
            BoneTree.MaximumSize = RightBarSize;
            BoneTree.Dock = DockStyle.Top;

            MeshList = new SBMeshList();
            MeshList.MaximumSize = RightBarSize;
            MeshList.Dock = DockStyle.Top;
            
            BoneEditor = new SBBoneEditor();
            BoneEditor.Dock = DockStyle.Fill;

            MeshPanel = new SBMeshPanel();
            MeshPanel.Dock = DockStyle.Fill;

            RightPane = new SBPopoutPanel(PopoutSide.Right, "<", ">");
            RightPane.Dock = DockStyle.Right;

            animationBar = new SBAnimationBar();
            animationBar.Dock = DockStyle.Bottom;
            animationBar.Frame.Bind(Viewport, "Frame");
            animationBar.Visible = false;

            ResetControls();

            Controls.Add(Viewport);
            Controls.Add(animationBar);
            Controls.Add(BottomPane);
            Controls.Add(RightPane);
            Controls.Add(LeftPane);
            Controls.Add(MenuBar);

            RenderTimer = new Timer();
            RenderTimer.Interval = 1000 / 120;
            RenderTimer.Tick += new EventHandler(TriggerViewportRender);
            RenderTimer.Start();

            FormClosing += MainForm_FormClosing;
            InitializeImportTypes();

            if (ApplicationSettings.LastOpenedPath != "")
                projectTree.SetRoot(ApplicationSettings.LastOpenedPath);
        }

        /// <summary>
        /// Resets and hides all controls
        /// </summary>
        public void ResetControls()
        {
            BoneEditor.HideControl();
            MeshPanel.Visible = false;
            animationBar.Visible = false;
        }
        
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ClearWorkspace())
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Opens the render settings menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OpenRenderSettings(object sender, EventArgs args)
        {
            ApplicationSettingsEditor.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ExportRenderToFile(object sender, EventArgs args)
        {
            MakeRender();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Viewport_ResetCameraPosition(object sender, EventArgs args)
        {
            ResetViewportCamera();
        }

        /// <summary>
        /// Resets the viewport camera back to its original position
        /// </summary>
        public void ResetViewportCamera()
        {
            Viewport.Camera.ResetToDefaultPosition();
            Viewport.Updated = true;
        }

        /// <summary>
        /// Opens the material
        /// The material type is retrieved from the scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OpenMaterialEditor(object sender, EventArgs args)
        {
            if(_viewport.Scene == null)
            {
                MessageBox.Show("No Scene is currently set");
                return;
            }
            MaterialEditor.SetMaterialsFromScene(_viewport.Scene);
            MaterialEditor.Show();
        }

        /// <summary>
        /// Closes the workspace
        /// </summary>
        private void CloseWorkspace(object sender, EventArgs args)
        {
            ClearWorkspace();
        }

        /// <summary>
        /// Clears the workspace, removes and unloads the scene, cleanups up interface
        /// </summary>
        /// <returns>true if workspace is successfully cleared and false otherwise</returns>
        private bool ClearWorkspace()
        {
            if(_viewport.Scene != null)
            {
                DialogResult result = MessageBox.Show("Save current scene?", "Warning",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    //code for Yes
                }
                else if (result == DialogResult.No)
                {
                    //code for No
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }
            //TODO: save progress dialog

            ResetControls();

            MaterialEditor.Hide();
            ApplicationSettingsEditor.Hide();

            BoneTree.Nodes.Clear();
            MeshList.Items.Clear();
            Viewport.Scene = null;

            GC.Collect();
            return true;
        }

        /// <summary>
        /// Selects a bone and display the bone tool strip
        /// </summary>
        /// <param name="bone"></param>
        public void SelectBone(SBBone bone)
        {
            ResetControls();
            BoneEditor.BindBone(bone);
            BoneEditor.Visible = true;
        }


        /// <summary>
        /// Shows the meshpanel and have it load the selected meshes
        /// </summary>
        /// <param name="Mesh"></param>
        public void SelectMesh(ISBMesh[] Mesh)
        {
            ResetControls();
            if (Viewport.Scene == null) return;
            MeshPanel.SetSelectedMeshFromScene(Viewport.Scene);
            if(Mesh != null && Mesh.Length > 0)
                MeshPanel.Visible = true;
        }

        /// <summary>
        /// Saves the viewport's render to file
        /// </summary>
        private void MakeRender()
        {
            Viewport.SaveRender($"Render_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.png");
        }

        /// <summary>
        /// Sets up the controls for the scene
        /// </summary>
        /// <param name="scene"></param>
        private void SetupScene(SBScene scene)
        {
            Viewport.Scene = scene;

            BoneTree.LoadFromScene(scene);
            MeshList.LoadFromScene(scene);

            RightPane.Contents.Clear();
            if (scene.HasBones)
            {
                RightPane.Contents.Add(BoneEditor);
                RightPane.Contents.Add(new Splitter() { Dock = DockStyle.Top });
            }
            if (scene.HasMesh)
            {
                RightPane.Contents.Add(MeshPanel);
                RightPane.Contents.Add(new Splitter() { Dock = DockStyle.Top });
            }
            if (scene.HasMesh)
            {
                RightPane.Contents.Add(MeshList);
                RightPane.Contents.Add(new Splitter() { Dock = DockStyle.Top });
            }
            if (scene.HasBones)
            {
                RightPane.Contents.Add(BoneTree);
                RightPane.Contents.Add(new Splitter() { Dock = DockStyle.Top });
            }
        }

        /// <summary>
        /// Raises a render frame event for the viewport.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TriggerViewportRender(object sender, EventArgs e)
        {
            if (Viewport.IsIdle)
            {
                if (ApplicationSettingsEditor.Visible || animationBar.Visible)
                    Viewport.Updated = true;
                Viewport.RenderFrame();
                animationBar.Process();
            }
        }

        /// <summary>
        /// Trys to open the file at specified path
        /// </summary>
        /// <param name="FilePath"></param>
        public void OpenFile(string FilePath)
        {
            // Animation
            animationBar.Visible = false;
            foreach (var openableAnimation in AnimationImporters)
            {
                if (FilePath.EndsWith(openableAnimation.Extension))
                {
                    if (Viewport.Scene == null) // most animation formats need the base skeleton, so a scene needs to be loaded
                        return;
                    Viewport.Animation = openableAnimation.ImportSBAnimation(FilePath, (SBSkeleton)Viewport.Scene.Skeleton);
                    animationBar.FrameCount = (int)Viewport.Animation.FrameCount;
                    animationBar.Visible = true;
                    return;
                }
            }

            // Scenes
            Dictionary<string, Type> extensionToType = new Dictionary<string, Type>();
            foreach (var type in SceneTypes)
            {
                foreach (var attr in type.GetCustomAttributes(false))
                {
                    if (attr is SceneFileInformation info)
                    {
                        if (FilePath.EndsWith(info.Extension))
                        {
                            object ob = Activator.CreateInstance(type);

                            if (ob is SBScene scene)
                            {
                                CloseWorkspace(null, null);
                                scene.LoadFromFile(FilePath);
                                SetupScene(scene);
                                return;
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Opens a folder and sets it in the file tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OpenFolder(object sender, EventArgs args)
        {
            string Folder = StudioSB.Tools.FileTools.TryOpenFolder();
            if(Folder != "")
            {
                projectTree.SetRoot(Folder);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OpenFile(object sender, EventArgs args)
        {
            if (!ClearWorkspace())
                return;

            //Create filter
            Dictionary<string, Type> extensionToType = new Dictionary<string, Type>();
            string Filter = "";
            foreach(var type in SceneTypes)
            {
                foreach(var attr in type.GetCustomAttributes(false))
                {
                    if(attr is SceneFileInformation info)
                    {
                        Filter += $"*{info.Extension};";
                        extensionToType.Add(info.Extension, type);
                    }
                }
            }

            string FileName;
            if(Tools.FileTools.TryOpenFile(out FileName, "Supported Files|" + Filter))
            {
                projectTree.SetRoot(System.IO.Path.GetDirectoryName(FileName));

                OpenFile(FileName);
            }
        }

        /// <summary>
        /// Exports the scene to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SaveScene(object sender, EventArgs args)
        {
            if (Viewport.Scene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }

            string Filter = "";
            string SceneExtension = "";
            foreach (var attr in Viewport.Scene.GetType().GetCustomAttributes(false))
            {
                if (attr is SceneFileInformation info)
                {
                    SceneExtension = info.Extension;
                    Filter += $"*{info.Extension};";
                }
            }

            //Create filter
            Dictionary<string, IExportableModelType> extensionToExporter = new Dictionary<string, IExportableModelType>();
            foreach (IExportableModelType exporter in ModelExporters)
            {
                string Extension = exporter.Extension;
                Filter += $"*{Extension};";
                extensionToExporter.Add(Extension, exporter);
            }


            string FileName;
            if (Tools.FileTools.TrySaveFile(out FileName, "Supported Files|" + Filter))
            {
                foreach (var extension in extensionToExporter.Keys)
                {
                    if (FileName.EndsWith(SceneExtension))
                    {
                        Viewport.Scene.ExportSceneToFile(FileName);
                    }else
                    if (FileName.EndsWith(extension))
                    {
                        extensionToExporter[extension].ExportIOModel(FileName, Viewport.Scene.GetIOModel());
                    }
                }
            }
        }

        private void ExportAnimationToFile(object sender, EventArgs args)
        {
            if (Viewport.Scene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }
            string Filter = "";

            //Create filter
            Dictionary<string, IExportableAnimation> extensionToExporter = new Dictionary<string, IExportableAnimation>();
            foreach (IExportableAnimation exporter in AnimationExporters)
            {
                string Extension = exporter.Extension;
                Filter += $"*{Extension};";
                extensionToExporter.Add(Extension, exporter);
            }

            string FileName;
            if (Tools.FileTools.TrySaveFile(out FileName, "Supported Files|" + Filter))
            {
                foreach (var extension in extensionToExporter.Keys)
                {
                    if (FileName.EndsWith(extension))
                    {
                        extensionToExporter[extension].ExportSBAnimation(FileName, Viewport.Animation, (SBSkeleton)Viewport.Scene.Skeleton);
                    }
                }
            }
        }

        /// <summary>
        /// Imports supported animation file into scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ImportAnimationToScene(object sender, EventArgs args)
        {
            if (Viewport.Scene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }
            string Filter = "";

            //Create filter
            Dictionary<string, IImportableAnimation> extensionToExporter = new Dictionary<string, IImportableAnimation>();
            foreach (IImportableAnimation exporter in AnimationImporters)
            {
                string Extension = exporter.Extension;
                Filter += $"*{Extension};";
                extensionToExporter.Add(Extension, exporter);
            }

            string FileName;
            if (Tools.FileTools.TryOpenFile(out FileName, "Supported Files|" + Filter))
            {
                foreach (var extension in extensionToExporter.Keys)
                {
                    if (FileName.EndsWith(extension))
                    {
                        OpenFile(FileName);
                    }
                }
            }
        }

            /// <summary>
            /// Imports a model to scene
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="args"></param>
        public void ImportToScene(object sender, EventArgs args)
        {
            if (Viewport.Scene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }

            string Filter = "";

            //Create filter
            Dictionary<string, IImportableModelType> extensionToExporter = new Dictionary<string, IImportableModelType>();
            foreach (IImportableModelType exporter in ModelImporters)
            {
                string Extension = exporter.Extension;
                Filter += $"*{Extension};";
                extensionToExporter.Add(Extension, exporter);
            }
            
            string FileName;
            if (Tools.FileTools.TryOpenFile(out FileName, "Supported Files|" + Filter))
            {
                foreach (var extension in extensionToExporter.Keys)
                {
                    if (FileName.EndsWith(extension))
                    {
                        Viewport.Scene.FromIOModel(extensionToExporter[extension].ImportIOModel(FileName));
                        SetupScene(Viewport.Scene);
                    }
                }
            }
        }

        private void InitializeImportTypes()
        {
            var assemblyTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                assemblyTypes.AddRange(assembly.GetTypes());
            }

            // initialize model importers
            var exportableModelTypes = from type in assemblyTypes
                where typeof(IExportableModelType).IsAssignableFrom(type) select type;

            foreach (var type in exportableModelTypes)
            {
                if (type != typeof(IExportableModelType))
                    ModelExporters.Add((IExportableModelType)Activator.CreateInstance(type));
            }

            var importableModelTypes = from type in assemblyTypes
                where typeof(IImportableModelType).IsAssignableFrom(type) select type;

            foreach (var type in importableModelTypes)
            {
                if (type != typeof(IImportableModelType))
                    ModelImporters.Add((IImportableModelType)Activator.CreateInstance(type));
            }


            // initialize model importers
            var exportableAnimationTypes = from type in assemblyTypes
                 where typeof(IExportableAnimation).IsAssignableFrom(type) select type;

            foreach (var type in exportableAnimationTypes)
            {
                if (type != typeof(IExportableAnimation))
                    AnimationExporters.Add((IExportableAnimation)Activator.CreateInstance(type));
            }

            var importableAnimationTypes = from type in assemblyTypes
                where typeof(IImportableAnimation).IsAssignableFrom(type) select type;

            foreach (var type in importableAnimationTypes)
            {
                if (type != typeof(IImportableAnimation))
                {
                    var importer = (IImportableAnimation)Activator.CreateInstance(type);
                    AnimationImporters.Add(importer);
                    OpenableExtensions.Add(importer.Extension);
                }
            }

            foreach (var type in SceneTypes)
            {
                foreach (var attr in type.GetCustomAttributes(false))
                {
                    if (attr is SceneFileInformation info)
                    {
                        OpenableExtensions.Add(info.Extension);
                    }
                }
            }
        }

    }
}
