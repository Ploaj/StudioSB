using System;
using System.Windows.Forms;
using StudioSB.GUI.Menus;
using System.IO;
using StudioSB.Properties;

namespace StudioSB.GUI.Projects
{
    public class SBProjectTree : Panel
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

        private SBTreeView fileTree;
        private SBToolStrip toolStrip;

        private ImageList iconList = new ImageList();
        
        public SBProjectTree()
        {
            fileTree = new SBTreeView();
            fileTree.Dock = DockStyle.Fill;
            iconList.Images.Add("unknown", Resources.icon_unknown);
            iconList.Images.Add("folder", Resources.icon_folder);
            iconList.Images.Add("file", Resources.icon_file);
            fileTree.ImageList = iconList;
            fileTree.ImageList.ImageSize = new System.Drawing.Size(24, 24);

            fileTree.Indent = 16;

            fileTree.AfterExpand += folderTree_AfterExpand;
            fileTree.BeforeExpand += folderTree_BeforeExpand;

            fileTree.DoubleClick += DoubleClicked;

            toolStrip = new SBToolStrip();
            toolStrip.Dock = DockStyle.Top;
            toolStrip.BackColor = ApplicationSettings.MiddleColor;

            ToolStripButton item = new ToolStripButton();
            item.Image = Resources.icon_folder;
            item.Click += OpenFolder;
            item.ToolTipText = "Choose Folder";
            toolStrip.Items.Add(item);
            
            Controls.Add(fileTree);
            Controls.Add(toolStrip);

        }


        /// <summary>
        /// Opens a folder and sets it in the file tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OpenFolder(object sender, EventArgs args)
        {
            string Folder = StudioSB.Tools.FileTools.TryOpenFolder();
            if (Folder != "")
            {
                SetRoot(Folder);
            }
        }


        private void folderTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            ((SBFolderNode)e.Node).BeforeExpand();
        }

        private void folderTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            //if (ExpandQueue.Count == 0) return;
            //((SBFolderNode)e.Node).ExpandNode(); //ExpandQueue.Dequeue()
        }

        public void SetRoot(string RootPath)
        {
            fileTree.Nodes.Clear();
            fileTree.Nodes.Add(new SBFolderNode(RootPath));
            ApplicationSettings.LastOpenedPath = RootPath;
            ApplicationSettings.SaveStatic();
        }
        
        private void DoubleClicked(object sender, EventArgs args)
        {
            if(fileTree.SelectedNode != null)
            {
                if(fileTree.SelectedNode is SBFileNode filenode)
                {
                    if (File.Exists(filenode.FilePath))
                        ParentForm.OpenFile(filenode.FilePath);
                    else
                        filenode.Parent.Nodes.Remove(filenode);
                }
            }
        }
    }
}
