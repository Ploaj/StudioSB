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
using StudioSB.Scenes;
using StudioSB.IO.Formats;

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

    public class SBDobjAttachment : GroupBox, IAttachment
    {
        private SBTreeView dobjList;
        private PropertyGrid propertyGrid;
        private SBButton clearTextures;
        private GroupBox optionPanel;
        private GroupBox propertyPanel;

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
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.Size = new Size(200, 400);
            propertyGrid.SelectedObjectsChanged += (sender, args) =>
            {
                removeTexture.Visible = propertyGrid.SelectedObject is HSD_TOBJ;
                exportTexture.Visible = propertyGrid.SelectedObject is HSD_TOBJ;
                importTexture.Visible = propertyGrid.SelectedObject != null;
            };


            clearTextures = new SBButton("Clear Textures");
            clearTextures.Dock = DockStyle.Top;
            clearTextures.Click += (sender, args) =>
            {
                if(scene != null)
                {
                    if (scene.HasMaterialAnimations)
                    {
                        MessageBox.Show("Eror: DATs with material animations must keep their textures intact");
                    }
                    else
                    {
                        if(MessageBox.Show("Are you sure? This cannot be undone", "Clear Textures", MessageBoxButtons.OKCancel) != DialogResult.OK)
                            return;
                        scene.ClearMaterialAnimations();
                        //return;
                        foreach (SBHsdMesh m in scene.GetMeshObjects())
                            m.ClearTextures();
                        RefreshList();
                    }
                }
            };

            optionPanel = new GroupBox();
            optionPanel.Text = "Options";
            ApplicationSettings.SkinControl(optionPanel);
            optionPanel.Dock = DockStyle.Top;

            //AutoScroll = true;
            
            exportTexture = new SBButton("Export Texture");
            exportTexture.Dock = DockStyle.Top;
            exportTexture.Visible = false;
            optionPanel.Controls.Add(exportTexture);
            exportTexture.Click += (sender, args) =>
            {
                string filePath;
                if(FileTools.TrySaveFile(out filePath, "PNG (*.png)|*.png;"))
                {
                    if(propertyGrid.SelectedObject is HSD_TOBJ tobj)
                    {
                        //TODO: dds export / import
                        /*if(tobj.ImageData != null && tobj.ImageData.Format == GXTexFmt.CMP)
                        {
                            SBSurface s = new SBSurface();
                            s.Width = tobj.ImageData.Width;
                            s.Height = tobj.ImageData.Height;
                            s.InternalFormat = OpenTK.Graphics.OpenGL.InternalFormat.CompressedRgbaS3tcDxt1Ext;
                            s.Arrays.Add(new MipArray() { Mipmaps = new List<byte[]>() { HSDLib.Helpers.TPL.ToCMP(tobj.ImageData.Data, tobj.ImageData.Width, tobj.ImageData.Height) } });

                            IO_DDS.Export(System.IO.Path.GetDirectoryName(filePath)+"\\" + System.IO.Path.GetFileNameWithoutExtension(filePath) + ".dds", s);
                        }
                        else*/
                        {
                            var bmp = tobj.ToBitmap();
                            bmp.Save(filePath);
                            bmp.Dispose();
                        }
                    }
                }
            };

            removeTexture = new SBButton("Remove Texture");
            removeTexture.Dock = DockStyle.Top;
            removeTexture.Visible = false;
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

                    FixMOBJTexIDs(mobj);
                    var root = dobjList.SelectedNode.Parent.Parent;
                    root.Nodes.Clear();
                    root.Nodes.Add(CreateMOBJNode(mobj));
                    scene.RefreshRendering();
                }
            };

            importTexture = new SBButton("Import Texture");
            importTexture.Dock = DockStyle.Top;
            importTexture.Visible = false;
            optionPanel.Controls.Add(importTexture);
            importTexture.Click += (sender, args) =>
            {
                // select texture
                HSD_MOBJ mobj = null;
                SBTreeNode root = null;
                if(dobjList.SelectedNode.Tag is SBHsdMesh mesh)
                {
                    if(mesh.DOBJ.MOBJ != null)
                    {
                        mobj = mesh.DOBJ.MOBJ;
                        root = (SBTreeNode)dobjList.SelectedNode;
                    }
                }
                if (dobjList.SelectedNode.Tag is HSD_MOBJ m)
                {
                    mobj = m;
                    root = (SBTreeNode)dobjList.SelectedNode.Parent;
                }
                if (dobjList.SelectedNode.Tag is HSD_TOBJ)
                {
                    if (dobjList.SelectedNode.Parent.Tag is HSD_MOBJ mo)
                    {
                        mobj = mo;
                        root = (SBTreeNode)dobjList.SelectedNode.Parent.Parent;
                    }
                }
                if (mobj == null)
                    return;
                string filePath;
                if(FileTools.TryOpenFile(out filePath, "Supported Formats (*.png*.dds)|*.png;*.dds"))
                {
                    var settings = new TOBJImportSettings();
                    // select textue import options
                    using (SBCustomDialog d = new SBCustomDialog(settings))
                    {
                        if(d.ShowDialog() == DialogResult.OK)
                        {
                            // create tobj and attach to selected mobj
                            HSD_TOBJ tobj = new HSD_TOBJ();
                            tobj.MagFilter = GXTexFilter.GX_LINEAR;
                            tobj.HScale = 1;
                            tobj.WScale = 1;
                            tobj.WrapS = GXWrapMode.REPEAT;
                            tobj.WrapT = GXWrapMode.REPEAT;
                            tobj.Transform = new HSD_Transforms();
                            tobj.Transform.SX = 1;
                            tobj.Transform.SY = 1;
                            tobj.Transform.SZ = 1;

                            if(System.IO.Path.GetExtension(filePath.ToLower())  == ".dds")
                            {
                                var dxtsurface = IO_DDS.Import(filePath);

                                if (dxtsurface.InternalFormat != OpenTK.Graphics.OpenGL.InternalFormat.CompressedRgbaS3tcDxt1Ext)
                                    throw new NotSupportedException("DDS format " + dxtsurface.InternalFormat.ToString() + " not supported");

                                HSD_Image i = new HSD_Image();
                                i.Width = (ushort)dxtsurface.Width;
                                i.Height = (ushort)dxtsurface.Height;
                                i.Data = HSDLib.Helpers.TPL.ToCMP(dxtsurface.Arrays[0].Mipmaps[0], i.Width, i.Height);
                                i.Format = GXTexFmt.CMP;
                                tobj.ImageData = i;
                            }
                            else
                            {
                                var bmp = new Bitmap(filePath);
                                tobj.SetFromBitmap(bmp, settings.ImageFormat, settings.PaletteFormat);
                                bmp.Dispose();
                            }

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
                                tobj.Flags |= TOBJ_FLAGS.COLORMAP_REPLACE;
                            }
                            else
                            {
                                tobj.Flags |= TOBJ_FLAGS.COLORMAP_BLEND;
                            }
                            propertyGrid.SelectedObject = tobj;

                            FixMOBJTexIDs(mobj);

                            root.Nodes.Clear();
                            root.Nodes.Add(CreateMOBJNode(mobj));
                            scene.RefreshRendering();
                        }
                    }
                }

            };

            propertyPanel = new GroupBox();
            propertyPanel.Text = "Properties";
            propertyPanel.Dock = DockStyle.Top;
            propertyPanel.Controls.Add(propertyGrid);
            propertyPanel.Height = 300;
            ApplicationSettings.SkinControl(propertyPanel);

            Controls.Add(propertyPanel);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10, BackColor = ApplicationSettings.BGColor2 });
            Controls.Add(optionPanel);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10, BackColor = ApplicationSettings.BGColor2 });
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
                    item.Nodes.Add(CreateMOBJNode(m.DOBJ.MOBJ));
                }

                dobjList.Nodes.Add(item);
            };
        }

        private SBTreeNode CreateMOBJNode(HSD_MOBJ mobj)
        {
            SBTreeNode child = new SBTreeNode("MOBJ");
            child.Tag = mobj;
            if (mobj.MaterialColor != null)
            {
                SBTreeNode mc = new SBTreeNode("Material Color");
                mc.Tag = mobj.MaterialColor;
                child.Nodes.Add(mc);
            }
            if (mobj.PixelProcessing != null)
            {
                SBTreeNode mc = new SBTreeNode("Pixel Processing");
                mc.Tag = mobj.PixelProcessing;
                child.Nodes.Add(mc);
            }
            if (mobj.Textures != null)
            {
                foreach (var tex in mobj.Textures.List)
                {
                    SBTreeNode mc = new SBTreeNode(tex.Flags.ToString());
                    mc.Tag = tex;
                    child.Nodes.Add(mc);
                }
            }
            return child;
        }

        private void FixMOBJTexIDs(HSD_MOBJ mobj)
        {
            int index = 0;
            if(mobj.Textures != null)
                foreach(var t in mobj.Textures.List)
                {
                    t.GXTexGenSrc = (uint)(4 + index);
                    t.TexMapID = GXTexMapID.GX_TEXMAP0 + index;
                    index++;
                }
        }

    }
}
