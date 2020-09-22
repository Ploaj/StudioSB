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
using StudioSB.GUI.Attachments;
using StudioSB.Scenes.Animation;
using StudioSB.Scenes.Ultimate;
using OpenTK;
using StudioSB.Rendering;

namespace StudioSB
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainForm : Form
    {
        public static MainForm Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MainForm();
                return _instance;
            }
        }
        private static MainForm _instance;

        private SBViewportPanel viewportPanel { get; set; }

        private SBMenuBar MenuBar { get; set; }

        // constant panels

        private SBProjectTree projectTree { get; set; }

        private SBPopoutPanel LeftPane { get; set; }

        private SBPopoutPanel BottomPane { get; set; }

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

        private static List<IImportableAnimation> AnimationImporters = new List<IImportableAnimation>();
        private static List<IExportableAnimation> AnimationExporters = new List<IExportableAnimation>();

        private static List<IImportableSkeleton> SkeletonImporters = new List<IImportableSkeleton>();
        private static List<IExportableSkeleton> SkeletonExporters = new List<IExportableSkeleton>();

        public static List<IAttachment> AttachmentTypes = new List<IAttachment>();

        public string[] ARGS;

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
                        folder.Click += projectTree.OpenFolder;
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
                        var scene = new SBToolStripMenuItem("Model");
                        scene.Click += ImportToScene;
                        open.DropDownItems.Add(scene);

                        var animimport = new SBToolStripMenuItem("Animation Into Scene");
                        animimport.Click += ImportAnimationToScene;
                        open.DropDownItems.Add(animimport);

                        var skeletonimport = new SBToolStripMenuItem("Skeleton To Model");
                        skeletonimport.Click += ImportSkeletonToScene;
                        open.DropDownItems.Add(skeletonimport);
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

                        var skeletonexport = new SBToolStripMenuItem("Skeleton to File");
                        skeletonexport.Click += ExportSkeletonToFile;
                        open.DropDownItems.Add(skeletonexport);
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
                    var rsettings = new SBToolStripMenuItem("Application Settings");
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

            viewportPanel = new SBViewportPanel();
            viewportPanel.Dock = DockStyle.Fill;

            Controls.Add(viewportPanel);
            Controls.Add(BottomPane);
            Controls.Add(LeftPane);
            Controls.Add(MenuBar);

            FormClosing += MainForm_FormClosing;
            InitializeImportTypes();

            if (ApplicationSettings.LastOpenedPath != "")
                projectTree.SetRoot(ApplicationSettings.LastOpenedPath);

            OpenTKResources.Init();
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
            viewportPanel.Viewport.Camera.ResetToDefaultPosition();
        }

        /// <summary>
        /// Opens the material
        /// The material type is retrieved from the scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OpenMaterialEditor(object sender, EventArgs args)
        {
            if(viewportPanel.LoadedScene == null)
            {
                MessageBox.Show("No Scene is currently set");
                return;
            }
            MaterialEditor.SetMaterialsFromScene(viewportPanel.LoadedScene);
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
            if(viewportPanel.LoadedScene != null && ApplicationSettings.ShowSaveChangesPopup)
            {
                DialogResult result = MessageBox.Show("Discard current scene?", "Warning",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {

                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            MaterialEditor.Hide();
            ApplicationSettingsEditor.Hide();

            viewportPanel.Clear();

            GC.Collect();
            return true;
        }

        /// <summary>
        /// Saves the viewport's render to file
        /// </summary>
        private void MakeRender()
        {
            viewportPanel.Viewport.SaveRender($"Render_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.png");
        }

        /// <summary>
        /// Trys to open the file at specified path
        /// </summary>
        /// <param name="FilePath"></param>
        public void OpenFile(string FilePath)
        {
            // Attachments
            foreach(var attachment in AttachmentTypes)
            {
                if ((!attachment.OverlayScene() && viewportPanel.LoadedScene != null) 
                    || attachment.Extension() == null)
                    continue;

                foreach (var extension in attachment.Extension())
                {
                    if (FilePath.ToLower().EndsWith(extension))
                    {
                        attachment.Open(FilePath);
                        viewportPanel.AddAttachment(attachment);
                        return;
                    }
                }
            }

            // Animation
            if (viewportPanel.Viewport.Scene != null)
            {
                var anim = LoadAnimation(FilePath, (SBSkeleton)viewportPanel.Viewport.Scene.Skeleton);
                if (anim != null)
                {
                    var animattach = new SBAnimAttachment(anim);
                    viewportPanel.AddAttachment(animattach);
                    return;
                }
            }

            // Scenes
            var scene = LoadScene(FilePath);
            if(scene != null)
            {
                CloseWorkspace(null, null);
                viewportPanel.SetScene(scene);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        public static SBAnimation LoadAnimation(string FilePath, SBSkeleton skeleton)
        {
            foreach (var openableAnimation in AnimationImporters)
            {
                if (FilePath.ToLower().EndsWith(openableAnimation.Extension))
                {
                    var animation = openableAnimation.ImportSBAnimation(FilePath, skeleton);
                    return animation;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static SBScene LoadScene(string FilePath)
        {
            Dictionary<string, Type> extensionToType = new Dictionary<string, Type>();
            foreach (var type in SceneTypes)
            {
                foreach (var attr in type.GetCustomAttributes(false))
                {
                    if (attr is SceneFileInformation info)
                    {
                        if (FilePath.ToLower().EndsWith(info.Extension))
                        {
                            object ob = Activator.CreateInstance(type);

                            if (ob is SBScene scene)
                            {
                                scene.LoadFromFile(FilePath);
                                return scene;
                            }
                        }
                    }
                }
            }
            return null;
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
            foreach (var type in AttachmentTypes)
            {
                if (type.Extension() == null) continue;
                foreach(var extension in type.Extension())
                {
                    Filter += $"*{extension};";
                    extensionToType.Add(extension, type.GetType());
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
            if (viewportPanel.LoadedScene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }

            string Filter = "";
            string SceneExtension = "";
            foreach (var attr in viewportPanel.LoadedScene.GetType().GetCustomAttributes(false))
            {
                if (attr is SceneFileInformation info)
                {
                    SceneExtension = info.Extension;
                    Filter += $"Scene|*{info.Extension}|";
                }
            }
            
            string FileName;
            if (Tools.FileTools.TrySaveFile(out FileName, Filter + IONET.IOManager.GetModelExportFileFilter()))
            {
                if (FileName.EndsWith(SceneExtension))
                {
                    viewportPanel.LoadedScene.ExportSceneToFile(FileName);
                }
                else
                {
                    var ioScene = viewportPanel.LoadedScene.GetIOModel();
                    IONET.IOManager.ExportScene(ioScene, FileName);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ExportAnimationToFile(object sender, EventArgs args)
        {
            if (viewportPanel.LoadedScene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }

            SBAnimAttachment anim = viewportPanel.GetAttachment<SBAnimAttachment>();
            SBAnimation animation;
            if (anim != null)
                animation = anim.GetAnimation();
            else
            {
                MessageBox.Show("No animation is loaded in the scene");
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
                    if (FileName.ToLower().EndsWith(extension))
                    {
                        DialogResult Result = DialogResult.OK;
                        if(extensionToExporter[extension].Settings != null)
                        using (var dialog = new SBCustomDialog(extensionToExporter[extension].Settings))
                            Result = dialog.ShowDialog();

                        if(Result == DialogResult.OK)
                            extensionToExporter[extension].ExportSBAnimation(FileName, animation, (SBSkeleton)viewportPanel.LoadedScene.Skeleton);
                    }
                }
            }
        }

        /// <summary>
        /// Exports the IOModel's skeleton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ExportSkeletonToFile(object sender, EventArgs args)
        {
            if (viewportPanel.LoadedScene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }

            string Filter = "";

            //Create filter
            Dictionary<string, IExportableSkeleton> extensionToExporter = new Dictionary<string, IExportableSkeleton>();
            foreach (IExportableSkeleton exporter in SkeletonExporters)
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
                    if (FileName.ToLower().EndsWith(extension))
                    {
                        SBSkeleton skeleton = viewportPanel.LoadedScene.Skeleton as SBSkeleton;
                        extensionToExporter[extension].ExportSBSkeleton(FileName, skeleton);
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
            if (viewportPanel.LoadedScene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }
            string Filter = "";

            //Create filter
            Dictionary<string, IImportableAnimation> extensionToImporter = new Dictionary<string, IImportableAnimation>();
            foreach (IImportableAnimation importer in AnimationImporters)
            {
                string Extension = importer.Extension;
                Filter += $"*{Extension};";
                extensionToImporter.Add(Extension, importer);
            }

            string FileName;
            if (Tools.FileTools.TryOpenFile(out FileName, "Supported Files|" + Filter))
            {
                foreach (var extension in extensionToImporter.Keys)
                {
                    if (FileName.EndsWith(extension))
                    {
                        OpenFile(FileName);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ImportSkeletonToScene(object sender, EventArgs args)
        {
            if (viewportPanel.LoadedScene == null)
            {
                MessageBox.Show("No scene is selected");
                return;
            }

            string Filter = "";

            //Create filter
            Dictionary<string, IImportableSkeleton> extensionToImporter = new Dictionary<string, IImportableSkeleton>();
            foreach (IImportableSkeleton importer in SkeletonImporters)
            {
                string Extension = importer.Extension;
                Filter += $"*{Extension};";
                extensionToImporter.Add(Extension, importer);
            }

            string FileName;
            if (Tools.FileTools.TryOpenFile(out FileName, "Supported Files|" + Filter))
            {
                foreach (var extension in extensionToImporter.Keys)
                {
                    if (FileName.ToLower().EndsWith(extension))
                    {
                        SBSkeleton skeleton = viewportPanel.LoadedScene.Skeleton as SBSkeleton;
                        extensionToImporter[extension].ImportSBSkeleton(FileName, skeleton);
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
            string FileName;

            if (Tools.FileTools.TryOpenFile(out FileName, IONET.IOManager.GetModelImportFileFilter()))
            {
                var scene = LoadScene(FileName, viewportPanel.LoadedScene);
                viewportPanel.SetScene(scene);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static IExportableAnimation GetExportableAnimationFromExtension(string extension)
        {
            foreach (IExportableAnimation exporter in AnimationExporters)
            {
                if (exporter.Extension == extension)
                    return exporter;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static IExportableSkeleton GetExportableSkeletonFromExtension(string extension)
        {
            foreach (IExportableSkeleton exporter in SkeletonExporters)
            {
                if (exporter.Extension == extension)
                    return exporter;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string[] SupportedAnimExportTypes()
        {
            string[] s = new string[AnimationExporters.Count];
            for(int i =0; i < AnimationExporters.Count
                 ; i++)
            {
                s[i] = AnimationExporters[i].Extension;
            }
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SBScene LoadScene(string filePath, SBScene loadedScene)
        {
            DialogResult Result = DialogResult.OK;

            var settings = new IONET.ImportSettings();
            using (SBCustomDialog d = new SBCustomDialog(settings))
                Result = d.ShowDialog();

            if (Result == DialogResult.OK)
            {
                var ioModel = IONET.IOManager.LoadScene(filePath, settings);

                SBScene scene;
                if (loadedScene == null)
                {
                    SBConsole.WriteLine("No scene loaded, defaulted to Smash Ultimate scene");
                    scene = new SBSceneSSBH();

                    using (var dialog = new SBCustomDialog(SBSceneSSBH.NewImportSettings))
                        Result = dialog.ShowDialog();

                    if (Result == DialogResult.OK)
                    {
                        if (SBSceneSSBH.NewImportSettings.NumatbFile != null && SBSceneSSBH.NewImportSettings.NumatbFile != "")
                            MATL_Loader.Open(SBSceneSSBH.NewImportSettings.NumatbFile, scene);

                        if (SBSceneSSBH.NewImportSettings.NusktbFile != null && SBSceneSSBH.NewImportSettings.NusktbFile != "")
                            ioModel.Models[0].Skeleton = SKEL_Loader.Open(SBSceneSSBH.NewImportSettings.NusktbFile, scene).ToIOSkeleton();
                    }
                    else
                    {
                        MessageBox.Show("Failed to import model");
                        return null;
                    }
                }
                else
                {
                    scene = loadedScene;

                    using (var dialog = new SBCustomDialog(SBSceneSSBH.ImportSettings))
                        Result = dialog.ShowDialog();

                    if (Result == DialogResult.OK)
                    {
                        if (SBSceneSSBH.ImportSettings.UseExistingSkeleton)
                        {
                            ioModel.Models[0].Skeleton = ((SBSkeleton)scene.Skeleton).ToIOSkeleton();
                            //ioModel.ConvertToSkeleton((SBSkeleton)scene.Skeleton);
                            // ionet uses bone names so no need to convert bone mapping indices
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to import model");
                        return null;
                    }
                }

                scene.FromIOModel(ioModel);
                return scene;
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void InitializeImportTypes()
        {
            var assemblyTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                assemblyTypes.AddRange(assembly.GetTypes());
            }

            // initialize animation importers/exporters
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

            // initialize skeleton importers/exporters
            var exportableSkeletonTypes = from type in assemblyTypes
                where typeof(IExportableSkeleton).IsAssignableFrom(type) select type;

            foreach (var type in exportableSkeletonTypes)
            {
                if (type != typeof(IExportableSkeleton))
                    SkeletonExporters.Add((IExportableSkeleton)Activator.CreateInstance(type));
            }

            var importableSkeletonTypes = from type in assemblyTypes
                where typeof(IImportableSkeleton).IsAssignableFrom(type) select type;

            foreach (var type in importableSkeletonTypes)
            {
                if (type != typeof(IImportableSkeleton))
                    SkeletonImporters.Add((IImportableSkeleton)Activator.CreateInstance(type));
            }

            var attachments = from type in assemblyTypes
                where typeof(IAttachment).IsAssignableFrom(type)
                select type;

            foreach (var type in attachments)
            {
                if (type != typeof(IAttachment))
                {
                    var importer = (IAttachment)Activator.CreateInstance(type);
                    AttachmentTypes.Add(importer);
                    if (importer.Extension() != null)
                    {
                        OpenableExtensions.AddRange(importer.Extension());
                    }
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
