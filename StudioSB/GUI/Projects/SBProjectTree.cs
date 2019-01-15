using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StudioSB.GUI.Menus;
using System.IO;

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
        
        public SBProjectTree()
        {
            fileTree = new SBTreeView();
            fileTree.Dock = DockStyle.Fill;

            fileTree.Indent = 16;

            fileTree.AfterExpand += folderTree_AfterExpand;
            fileTree.BeforeExpand += folderTree_BeforeExpand;

            fileTree.DoubleClick += DoubleClicked;

            toolStrip = new SBToolStrip();
            toolStrip.Dock = DockStyle.Top;

            Controls.Add(fileTree);
            Controls.Add(toolStrip);

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
