using System;
using System.Collections.Generic;
using StudioSB.Rendering.Bounding;
using System.Windows.Forms;
using StudioSB.Scenes.Melee;
using System.Drawing;
using StudioSB.Tools;
using HSDLib.GX;
using HSDLib.Common;
using System.ComponentModel;

namespace StudioSB.GUI.Attachments
{
    public enum TOBJTextureType
    {
        Diffuse,
        Specular,
        Bump
    }

    public enum TOBJUVType
    {
        TextureCoord,
        Sphere
    }

    public class TOBJImportSettings
    {
        [DisplayName("Texture Type"), Description("")]
        public TOBJTextureType TextureType { get; set; }

        [DisplayName("UV Type"), Description("")]
        public TOBJUVType UVType { get; set; }

        [DisplayName("Use Blending"), Description("")]
        public bool UseBlending { get; set; }

        [DisplayName("Image Format"), Description("Palette format, if applicable")]
        public GXTexFmt ImageFormat { get; set; } = GXTexFmt.RGB565;

        [DisplayName("Palette Format"), Description("Palette format, if applicable")]
        public GXTlutFmt PaletteFormat { get; set; } = GXTlutFmt.RGB5A3;
    }

    public class SBDobjAttachment : Panel, IAttachment
    {
        private SBTreeView dobjList;
        private PropertyGrid propertyGrid;
        private SBButton clearTextures;
        private GroupBox optionPanel;

        //TODO: remove texture button
        private SBButton removeTexture;
        private SBButton exportTexture;
        private SBButton importTexture;

        private HSDScene scene;

        public SBDobjAttachment()
        {
            Text = "DOBJ List";
            Dock = DockStyle.Fill;
            //this.s
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
            propertyGrid.Size = new Size(200, 400);
            propertyGrid.SelectedObjectsChanged += (sender, args) =>
            {
                removeTexture.Visible = propertyGrid.SelectedObject is HSD_TOBJ;
                exportTexture.Visible = propertyGrid.SelectedObject is HSD_TOBJ;
            };


            clearTextures = new SBButton("Clear Textures");
            clearTextures.Dock = DockStyle.Top;
            clearTextures.Click += (sender, args) =>
            {
                if(scene != null)
                {
                    foreach (SBHsdMesh m in scene.GetMeshObjects())
                        m.ClearTextures();
                    RefreshList();
                }
            };

            optionPanel = new GroupBox();
            optionPanel.Text = "Options";
            ApplicationSettings.SkinControl(optionPanel);
            optionPanel.Dock = DockStyle.Top;

            AutoScroll = true;


            exportTexture = new SBButton("Export Texture");
            exportTexture.Dock = DockStyle.Top;
            optionPanel.Controls.Add(exportTexture);
            exportTexture.Click += (sender, args) =>
            {
                string filePath;
                if(FileTools.TrySaveFile(out filePath, "PNG (*.png)|*.png;"))
                {
                    if(propertyGrid.SelectedObject is HSD_TOBJ tobj)
                    {
                        var bmp = tobj.ToBitmap();
                        bmp.Save(filePath);
                        bmp.Dispose();
                    }
                }
            };

            removeTexture = new SBButton("Remove Texture");
            removeTexture.Dock = DockStyle.Top;
            optionPanel.Controls.Add(removeTexture);
            removeTexture.Click += (sender, args) =>
            {
                if(dobjList.SelectedNode.Tag is HSD_TOBJ tobj)
                {
                    // remove tobj from list
                    var mobj = (HSD_MOBJ)dobjList.SelectedNode.Parent.Tag;

                    HSD_TOBJ prevTexture = null;
                    if(mobj.Textures != null)
                        foreach(var tex in mobj.Textures.List)
                        {
                            if (tex == tobj)
                            {
                                if (prevTexture == null)
                                    mobj.Textures = tex.Next;
                                else
                                    prevTexture.Next = tex.Next;
                                // update texture and flag stuff
                                break;
                            }
                            prevTexture = tex;
                        }

                    RefreshList();

                }
            };

            importTexture = new SBButton("Import Texture");
            importTexture.Dock = DockStyle.Top;
            optionPanel.Controls.Add(importTexture);
            importTexture.Click += (sender, args) =>
            {
                // select texture
                HSD_MOBJ mobj = null;
                if(dobjList.SelectedNode.Tag is SBHsdMesh mesh)
                {
                    if(mesh.DOBJ.MOBJ != null)
                    {
                        mobj = mesh.DOBJ.MOBJ;
                    }
                }
                if (dobjList.SelectedNode.Tag is HSD_MOBJ m)
                {
                    mobj = m;
                }
                if (dobjList.SelectedNode.Tag is HSD_TOBJ)
                {
                    if (dobjList.SelectedNode.Parent.Tag is HSD_MOBJ mo)
                    {
                        mobj = mo;
                    }
                }
                if (mobj == null)
                    return;
                string filePath;
                if(FileTools.TryOpenFile(out filePath, "PNG (*.png)|*.png"))
                {
                    var settings = new TOBJImportSettings();
                    // select textue import options
                    using (SBCustomDialog d = new SBCustomDialog(settings))
                    {
                        if(d.ShowDialog() == DialogResult.OK)
                        {
                            // create tobj and attach to selected mobj
                            HSD_TOBJ tobj = new HSD_TOBJ();
                            tobj.GXTexGenSrc = 4;
                            tobj.MagFilter = GXTexFilter.GX_LINEAR;
                            tobj.HScale = 1;
                            tobj.WScale = 1;
                            tobj.WrapS = GXWrapMode.REPEAT;
                            tobj.WrapT = GXWrapMode.REPEAT;
                            tobj.Transform = new HSD_Transforms();
                            tobj.Transform.SX = 1;
                            tobj.Transform.SY = 1;
                            tobj.Transform.SZ = 1;
                            var bmp = new Bitmap(filePath);
                            tobj.SetFromBitmap(bmp, settings.ImageFormat, settings.PaletteFormat);
                            bmp.Dispose();

                            if (settings.UseBlending && mobj.PixelProcessing == null)
                                mobj.PixelProcessing = new HSD_PixelProcessing();
                            
                            //TODO: set flags for texture types
                            if(settings.TextureType == TOBJTextureType.Diffuse)
                            {
                                mobj.RenderFlags |= RENDER_MODE.DIFFUSE;
                                tobj.Flags |= TOBJ_FLAGS.LIGHTMAP_DIFFUSE;
                            }
                            if (settings.TextureType == TOBJTextureType.Specular)
                            {
                                mobj.RenderFlags |= RENDER_MODE.SPECULAR;
                                tobj.Flags |= TOBJ_FLAGS.LIGHTMAP_SPECULAR;
                            }
                            if (settings.TextureType == TOBJTextureType.Bump)
                            {
                                tobj.Flags |= TOBJ_FLAGS.BUMP;
                            }

                            switch (settings.UVType)
                            {
                                case TOBJUVType.Sphere:
                                    tobj.Flags |= TOBJ_FLAGS.COORD_REFLECTION;
                                    break;
                                case TOBJUVType.TextureCoord:
                                    tobj.Flags |= TOBJ_FLAGS.COORD_UV;
                                    break;
                            }

                            if (mobj.Textures == null)
                            {
                                mobj.Textures = tobj;
                                tobj.TexMapID = GXTexMapID.GX_TEXMAP0;
                                tobj.Flags |= TOBJ_FLAGS.COLORMAP_REPLACE;
                            }
                            else
                            {
                                tobj.Flags |= TOBJ_FLAGS.COLORMAP_BLEND;
                                var current = mobj.Textures;
                                while (current.Next != null)
                                    current = current.Next;
                                current.Next = tobj;
                                tobj.TexMapID = (GXTexMapID)mobj.Textures.List.Length;
                            }
                            propertyGrid.SelectedObject = tobj;

                            RefreshList();
                        }
                    }
                }

            };

            Controls.Add(propertyGrid);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(optionPanel);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(dobjList);
            Controls.Add(clearTextures);
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
                this.scene = scene;
                RefreshList();
            }
        }

        private void RefreshList()
        {
            scene.RefreshRendering();

            dobjList.Nodes.Clear();
            int DOBJIndex = 0;
            foreach (SBHsdMesh m in scene.GetMeshObjects())
            {
                SBTreeNode item = new SBTreeNode("DOBJ_" + DOBJIndex++);
                item.Tag = m;

                if (m.DOBJ.MOBJ != null)
                {
                    SBTreeNode child = new SBTreeNode("MOBJ");
                    child.Tag = m.DOBJ.MOBJ;
                    item.Nodes.Add(child);
                    if (m.DOBJ.MOBJ.MaterialColor != null)
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
                        foreach (var tex in m.DOBJ.MOBJ.Textures.List)
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
