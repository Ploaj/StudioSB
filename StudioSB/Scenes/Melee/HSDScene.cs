using System;
using System.Collections.Generic;
using System.Linq;
using HSDLib;
using HSDLib.Helpers;
using HSDLib.Common;
using SFGraphics.Cameras;
using OpenTK;
using StudioSB.Rendering;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.BufferObjects;
using SFGraphics.GLObjects.Textures;
using StudioSB.IO.Models;
using StudioSB.GUI.Attachments;
using StudioSB.GUI;
using StudioSB.Tools;
using HSDLib.GX;
using HSDLib.KAR;

namespace StudioSB.Scenes.Melee
{
    [SceneFileInformation("HSD", ".dat", "Hal Sys Dat")]
    public class HSDScene : SBScene
    {
        private HSDFile HSDFile { get; set; }

        private List<SBHsdMesh> Mesh = new List<SBHsdMesh>();

        private Dictionary<HSD_TOBJ, SBSurface> tobjToSurface = new Dictionary<HSD_TOBJ, SBSurface>();

        private BufferObject boneBindUniformBuffer;
        private BufferObject boneTransformUniformBuffer;
        private Matrix4[] boneBinds = new Matrix4[200];
        private Matrix4[] boneTransforms = new Matrix4[200];

        public HSD_JOBJ RootJOBJ
        {
            get
            {
                foreach(var roots in HSDFile.Roots)
                {
                    if (roots.Node is HSD_JOBJ jobj)
                        return jobj;
                    if (roots.Node is KAR_VcStarVehicle star)
                        return star.ModelData.JOBJRoot;
                }
                return null;
            }
        }

        public HSDScene()
        {
            AttachmentTypes.Remove(typeof(SBMeshList));
            AttachmentTypes.Remove(typeof(SBTextureList));
            AttachmentTypes.Add(typeof(SBDobjAttachment));
            boneBindUniformBuffer = new BufferObject(BufferTarget.UniformBuffer);
            boneTransformUniformBuffer = new BufferObject(BufferTarget.UniformBuffer);
        }

        /*public HSD_DOBJ GetDOBJs()
        {

        }*/

        private void RemakeVertexData()
        {
            var dobjs = RootJOBJ.GetAllOfType<HSD_DOBJ>();
            VertexCompressor c = new VertexCompressor();
            foreach(var dobj in dobjs)
            {
                Console.WriteLine(dobj.MOBJ.RenderFlags.ToString());
                dobj.MOBJ.RenderFlags = RENDER_MODE.ALPHA_COMPAT | RENDER_MODE.DIFFSE_MAT;
                dobj.MOBJ.Textures = null;
                if (dobj.MOBJ.Textures != null)
                foreach (var tobj in dobj.MOBJ.Textures.List)
                {
                    tobj.Flags = 0;
                    tobj.ImageData = null;
                    tobj.Tlut = null;
                }
                foreach (var pobj in dobj.POBJ.List)
                {
                    int off = 0;
                    var vertices = VertexAccessor.GetDecodedVertices(pobj);
                    var displayList = VertexAccessor.GetDisplayList(pobj);
                    GXDisplayList newdl = new GXDisplayList();
                    foreach (var dl in VertexAccessor.GetDisplayList(pobj).Primitives)
                    {
                        var vs = new List<GXVertex>();
                        for(int i = 0; i < dl.Count; i++)
                        {
                            vs.Add(vertices[off+i]);
                        }
                        off += dl.Count;
                        newdl.Primitives.Add(c.Compress(dl.PrimitiveType, vs.ToArray(), pobj.VertexAttributes));
                    }
                    pobj.DisplayListBuffer = newdl.ToBuffer(pobj.VertexAttributes);
                }
            }
            c.SaveChanges();
        }

        #region Properties
            
        public Texture TOBJtoRenderTexture(HSD_TOBJ tobj)
        {
            if (tobjToSurface.ContainsKey(tobj))
                return tobjToSurface[tobj].GetRenderTexture();
            else
                return DefaultTextures.Instance.defaultBlack;
        }

        public override ISBMesh[] GetMeshObjects()
        {
            return Mesh.ToArray();
        }

        public override ISBMaterial[] GetMaterials()
        {
            return new ISBMaterial[0];
        }

        #endregion

        #region IO

        public override void LoadFromFile(string FileName)
        {
            HSDFile = new HSDFile(FileName);
            RefreshSkeleton();
            CreateMesh();
            RefreshRendering();
        }

        public void RefreshRendering()
        {
            RefreshTextures();
            RefreshMesh();
        }

        private void RefreshTextures()
        {
            if (RootJOBJ == null)
                return;
            Surfaces.Clear();
            tobjToSurface.Clear();
            var tobjs = RootJOBJ.GetAllOfType<HSD_TOBJ>();
            List<HSD_Image> Process = new List<HSD_Image>();
            foreach (var tobj in tobjs)
            {
                var bm = tobj.ToBitmap();
                var surface = SBSurface.FromBitmap(bm);
                surface.Name = "TOBJ_" + tobjs.IndexOf(tobj);
                tobjToSurface.Add(tobj, surface);
                bm.Dispose();
                Surfaces.Add(surface);
            }
        }

        private void CreateMesh()
        {
            Mesh.Clear();

            if (RootJOBJ == null)
                return;

            var dobjs = RootJOBJ.GetAllOfType<HSD_DOBJ>();
            var jobjs = RootJOBJ.GetAllOfType<HSD_JOBJ>();
            foreach (var dobj in dobjs)
            {
                SBBone parent = null;
                if(Skeleton.Bones.Length > 0)
                    parent = Skeleton.Bones[0];
                foreach (var b in Skeleton.Bones)
                {
                    if(b is SBHsdBone bone)
                    {
                        if(bone.GetJOBJ().DOBJ != null)
                        if (bone.GetJOBJ().DOBJ.List.Contains(dobj))
                        {
                            parent = b;
                            break;
                        }
                    }
                }
                var mesh = new SBHsdMesh(dobj, parent);
                Mesh.Add(mesh);
            }
        }

        public void RefreshMesh()
        {
            foreach (SBHsdMesh mesh in GetMeshObjects())
                mesh.Refresh(this);
        }

        public bool HasMaterialAnimations
        {
            get
            {
                return HSDFile.Roots.Find(e => e.Node is HSDLib.MaterialAnimation.HSD_MatAnimJoint) != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearMaterialAnimations()
        {
            foreach(var roots in HSDFile.Roots)
            {
                if(roots.Node is HSDLib.MaterialAnimation.HSD_MatAnimJoint matjoint)
                {
                    foreach (var v in matjoint.DepthFirstList)
                    {
                        if(v.MaterialAnimation != null)
                        foreach(var matanim in v.MaterialAnimation.List)
                        {
                                if (matanim.TextureAnimation != null)
                                {
                                    Console.WriteLine(matanim.TextureAnimation.ImageCount + " " +
                                        matanim.TextureAnimation.TlutCount + " " +
                                        (matanim.TextureAnimation.ImageArray==null) + " " +
                                        matanim.TextureAnimation.GXTexMapID);

                                    matanim.TextureAnimation.AnimationObject.Flags = HSDLib.Animation.AOBJ_Flags.NO_ANIM;
                                    matanim.TextureAnimation.AnimationObject.FObjDesc = null;
                                    matanim.TextureAnimation.AnimationObject.EndFrame = 0;
                                    matanim.TextureAnimation.GXTexMapID = (int)HSDLib.GX.GXTexMapID.GX_TEXMAP0;
                                    matanim.TextureAnimation.ImageCount = 0;
                                    matanim.TextureAnimation.TlutCount = 0;
                                    if (matanim.TextureAnimation.ImageArray != null)
                                    {
                                        matanim.TextureAnimation.ImageArray.Elements = null;
                                        matanim.TextureAnimation.ImageArray.SetSize = 0;
                                    }
                                    if(matanim.TextureAnimation.TlutArray != null)
                                    {
                                        matanim.TextureAnimation.TlutArray.Elements = null;
                                        matanim.TextureAnimation.TlutArray.SetSize = 0;
                                    }
                                }
                                matanim.AnimationObject = null;
                                //matanim.TextureAnimation = null;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Refreshes the skeleton to update state of the HSDFile
        /// </summary>
        private void RefreshSkeleton()
        {
            Skeleton = new SBSkeleton();

            RecursivlyAddChildren(RootJOBJ, null);
        }

        /// <summary>
        /// Attaches SKELETON JOBJs to skeleton for rendering and editing
        /// </summary>
        /// <param name="jobj"></param>
        /// <param name="parent"></param>
        private void RecursivlyAddChildren(HSD_JOBJ jobj, SBHsdBone parent)
        {
            /*if (!(jobj.Flags.HasFlag(JOBJ_FLAG.SKELETON) || jobj.Flags.HasFlag(JOBJ_FLAG.SKELETON_ROOT)))
            {
                SBConsole.WriteLine(jobj.Flags.ToString());
                return;
            }*/

            SBHsdBone bone = new SBHsdBone();
            bone.Name = "JOBJ_" + Skeleton.Bones.Length;

            bone.Transform = OpenTK.Matrix4.Identity;
            bone.Translation = new OpenTK.Vector3(jobj.Transforms.TX, jobj.Transforms.TY, jobj.Transforms.TZ);
            bone.RotationEuler = new OpenTK.Vector3(jobj.Transforms.RX, jobj.Transforms.RY, jobj.Transforms.RZ);
            bone.Scale = new OpenTK.Vector3(jobj.Transforms.SX, jobj.Transforms.SY, jobj.Transforms.SZ);
            bone.SetJOBJ(jobj);
            
            if(parent == null)
                ((SBSkeleton)Skeleton).AddRoot(bone);
            else
            {
                bone.Parent = parent;
            }
            foreach (var child in jobj.Children)
                RecursivlyAddChildren(child, bone);
        }

        public override void ExportSceneToFile(string FileName)
        {
            // update new single binds

            // update textures

            //RemakeVertexData();

            HSDFile.Save(FileName);
        }

        public override void FromIOModel(IOModel iomodel)
        {
            //System.Windows.Forms.MessageBox.Show("Importing Model to DAT not supported");

            // use the existing skeleton always
            //iomodel.ConvertToSkeleton((SBSkeleton)Skeleton);

            // single bound vertices are stored in inverse transform positions
            iomodel.InvertSingleBinds();

            // dobjs to import to
            var dobjs = GetMeshObjects();

            // make attribute group
            HSD_AttributeGroup attributeGroup = new HSD_AttributeGroup();
            //TODO: redo how these are generated
            {
                GXVertexBuffer buff = new GXVertexBuffer();
                buff.Name = GXAttribName.GX_VA_PNMTXIDX;
                buff.AttributeType = GXAttribType.GX_DIRECT;
                attributeGroup.Attributes.Add(buff);
            }
            {
                GXVertexBuffer buff = new GXVertexBuffer();
                buff.Name = GXAttribName.GX_VA_POS;
                buff.AttributeType = GXAttribType.GX_INDEX16;
                buff.CompCount = GXCompCnt.PosXYZ;
                buff.CompType = GXCompType.Int16;
                buff.Scale = 10;
                buff.Stride = 6;
                attributeGroup.Attributes.Add(buff);
            }
            {
                GXVertexBuffer buff = new GXVertexBuffer();
                buff.Name = GXAttribName.GX_VA_NRM;
                buff.AttributeType = GXAttribType.GX_INDEX16;
                buff.CompCount = GXCompCnt.NrmXYZ;
                buff.CompType = GXCompType.Int8;
                buff.Scale = 6;
                buff.Stride = 3;
                attributeGroup.Attributes.Add(buff);
            }
            {
                GXVertexBuffer buff = new GXVertexBuffer();
                buff.Name = GXAttribName.GX_VA_TEX0;
                buff.AttributeType = GXAttribType.GX_INDEX16;
                buff.CompCount = GXCompCnt.TexST;
                buff.CompType = GXCompType.Int16;
                buff.Scale = 13;
                buff.Stride = 4;
                attributeGroup.Attributes.Add(buff);
            }

            foreach (SBHsdMesh m in GetMeshObjects())
            {
                m.DOBJ.POBJ = null;
            }

            // get a compressor ready
            // the compressor will handle making the compressed attribute buffers
            VertexCompressor compressor = new VertexCompressor();

            // import the iomeshes into their respective dobjs
            foreach (var iomesh in iomodel.Meshes)
            {
                int dobjId = -1;
                int.TryParse(iomesh.Name.Replace("DOBJ_", ""), out dobjId);

                SBConsole.WriteLine(iomesh.Name + " imported:" + (dobjId!=-1));

                if (dobjId != -1)
                {
                    var dobj = (SBHsdMesh)dobjs[dobjId];
                    dobj.ImportPBOJs(iomesh, (SBSkeleton)Skeleton, compressor, attributeGroup);
                }
            }

            // finalizes and remakes the buffer
            compressor.SaveChanges();

            // refresh everything
            RefreshRendering();
        }

        public override IOModel GetIOModel()
        {
            var iomodel = new IOModel();

            iomodel.Skeleton = (SBSkeleton)Skeleton;
            
            List<SBHsdBone> bones = new List<SBHsdBone>();

            foreach (SBHsdBone bone in Skeleton.Bones)
                bones.Add(bone);

            foreach (SBHsdMesh me in GetMeshObjects())
            {
                var dobj = me.DOBJ;

                var parent = Skeleton.Bones[0];
                foreach (var b in Skeleton.Bones)
                {
                    if (b is SBHsdBone bone)
                    {
                        if (bone.GetJOBJ().DOBJ != null)
                            if (bone.GetJOBJ().DOBJ.List.Contains(dobj))
                            {
                                parent = b;
                                break;
                            }
                    }
                }
                
                var iomesh = new IOMesh();
                iomesh.Name = me.Name;
                iomodel.Meshes.Add(iomesh);

                iomesh.HasPositions = true;
                iomesh.HasColor = true;
                iomesh.HasNormals = true;
                iomesh.HasBoneWeights = true;
                iomesh.HasUV0 = true;

                if (dobj.POBJ != null)
                    foreach (var pobj in dobj.POBJ.List)
                    {
                        var vertices = VertexAccessor.GetDecodedVertices(pobj);
                        var dl = VertexAccessor.GetDisplayList(pobj);

                        HSD_JOBJWeight[] bindGroups = null; ;
                        if (pobj.BindGroups != null)
                            bindGroups = pobj.BindGroups.Elements;

                        var offset = 0;
                        foreach (var v in dl.Primitives)
                        {
                            List<GXVertex> strip = new List<GXVertex>();
                            for (int i = 0; i < v.Count; i++)
                                strip.Add(vertices[offset + i]);
                            offset += v.Count;
                            iomesh.Vertices.AddRange(ConvertGXDLtoTriangleList(v.PrimitiveType, SBHsdMesh.GXVertexToHsdVertex(strip, bones, bindGroups), (SBHsdBone)parent));
                        }
                    }
                
                for (int i = 0; i < iomesh.Vertices.Count; i++)
                    iomesh.Indices.Add((uint)i);

                iomesh.Optimize();
            }

            return iomodel;
        }

        private List<IOVertex> ConvertGXDLtoTriangleList(GXPrimitiveType type, List<SBHsdVertex> vertices, SBHsdBone parent)
        {
            var list = new List<IOVertex>();

            foreach(var v in vertices)
            {
                var vertex = new IOVertex()
                {
                    Position = GXtoGL.GLVector3(v.POS),
                    Normal = GXtoGL.GLVector3(v.NRM),
                    UV0 = GXtoGL.GLVector2(v.UV0),
                    BoneIndices = v.Bone,
                    BoneWeights = v.Weight,
                };
                if(parent != null)
                {
                    vertex.Position = Vector3.TransformPosition(vertex.Position, parent.WorldTransform);
                    vertex.Normal = Vector3.TransformNormal(vertex.Normal, parent.WorldTransform);

                    if (vertex.BoneWeights.X == 0)
                    {
                        vertex.BoneWeights.X = 1;
                        vertex.BoneIndices.X = Skeleton.IndexOfBone(parent);
                    }
                }
                if(v.Weight.X == 1)
                {
                    vertex.Position = Vector3.TransformPosition(vertex.Position, Skeleton.Bones[(int)v.Bone.X].WorldTransform);
                    vertex.Normal = Vector3.TransformNormal(vertex.Normal, Skeleton.Bones[(int)v.Bone.X].WorldTransform);
                }
                list.Add(vertex);
            }

            if (type == GXPrimitiveType.TriangleStrip)
                TriangleConvert.StripToList(list, out list);

            return list;
        }

        #endregion

        #region Rendering

        public override void RenderLegacy()
        {

            base.RenderLegacy();
        }

        public override void RenderShader(Camera camera)
        {
            var shader = ShaderManager.GetShader("HSD");
            if (ApplicationSettings.UseDebugShading)
                shader = ShaderManager.GetShader("HSDDebug");
            if (!shader.LinkStatusIsOk)
                return;

            shader.UseProgram();

            Matrix4 sphereMatrix = camera.ModelViewMatrix;
            sphereMatrix.Invert();
            sphereMatrix.Transpose();
            shader.SetMatrix4x4("sphereMatrix", ref sphereMatrix);

            shader.SetBoolToInt("renderDiffuse", ApplicationSettings.EnableDiffuse);
            shader.SetBoolToInt("renderSpecular", ApplicationSettings.EnableSpecular);
            shader.SetBoolToInt("renderAlpha", true);
            shader.SetBoolToInt("renderNormalMap", ApplicationSettings.RenderNormalMaps);
            shader.SetInt("renderMode", (int)ApplicationSettings.ShadingMode);

            shader.SetVector3("cameraPos", camera.Position);

            shader.SetMatrix4x4("mvp", camera.MvpMatrix);

            // Bones
            if (Skeleton != null)
            {
                //boneBinds = Skeleton.GetBindTransforms();
                var bones = Skeleton.Bones;
                for (int i = 0; i < bones.Length; i++)
                {
                    boneTransforms[i] = bones[i].AnimatedWorldTransform;
                    boneBinds[i] = bones[i].AnimatedBindMatrix;
                }
            }

            int blockIndex2 = GL.GetUniformBlockIndex(shader.Id, "BoneTransforms");
            boneTransformUniformBuffer.BindBase(BufferRangeTarget.UniformBuffer, blockIndex2);
            boneTransformUniformBuffer.SetData(boneTransforms, BufferUsageHint.DynamicDraw);

            //TODO update sf grapics so it can be used to bind this
            shader.SetMatrix4x4("binds", boneBinds);

            foreach (var rm in Mesh)
            {
                if (!rm.Visible)
                    continue;
                shader.SetBoolToInt("renderWireframe", ApplicationSettings.EnableWireframe || rm.Selected);
                rm.Draw(this, shader);
            }

            base.RenderShader(camera);
        }

        #endregion
    }
}
