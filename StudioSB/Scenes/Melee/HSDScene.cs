using System;
using System.Collections.Generic;
using HSDRaw;
using HSDRaw.Tools;
using HSDRaw.Common;
using SFGraphics.Cameras;
using OpenTK;
using StudioSB.Rendering;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.BufferObjects;
using SFGraphics.GLObjects.Textures;
using StudioSB.GUI.Attachments;
using StudioSB.GUI;
using StudioSB.Tools;
using HSDRaw.GX;
using System.Drawing;
using System.Linq;
using IONET.Core;
using IONET.Core.Model;

namespace StudioSB.Scenes.Melee
{
    [SceneFileInformation("HSD", ".dat", "Hal Sys Dat")]
    public class HSDScene : SBScene
    {
        private HSDRawFile HSDFile { get; set; }

        private List<SBHsdMesh> Mesh = new List<SBHsdMesh>();

        private Dictionary<HSDStruct, SBSurface> tobjToSurface = new Dictionary<HSDStruct, SBSurface>();

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
                    if (roots.Data is HSD_JOBJ jobj)
                        return jobj;
                    if (roots.Data is HSD_SOBJ sobj)
                        return sobj.JOBJDescs?.Array[0]?.RootJoint;
                    /*if (roots.Data is KAR_VcStarVehicle star)
                        return star.ModelData.JOBJRoot;
                    if (roots.Data is KAR_GrModel model)
                        return model.MainModel.JOBJRoot;*/
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

        /*private List<T> GetAllOfType<T>(HSDAccessor root) where T : HSDAccessor
        {
            List<T> list = new List<T>();

            SearchType<T>(root, ref list);

            return list;
        }

        private void SearchType<T>(HSDAccessor root, ref List<T> list) where T : HSDAccessor
        {
            if (root == null)
                return;
            foreach (var p in root.GetType().GetProperties())
            {
                if (p.PropertyType.IsSubclassOf(typeof(HSDAccessor)))
                {
                    SearchType((HSDAccessor)p.GetValue(root), ref list);
                }
                if (p.PropertyType == typeof(T))
                {
                    var v = (T)p.GetValue(root);
                    if(v != null)
                        list.Add(v);
                }
            }
        }*/

        private List<HSD_DOBJ> GetDOBJS()
        {
            var dobjs = new List<HSD_DOBJ>();
            var jobj = RootJOBJ.BreathFirstList;
            foreach(var j in jobj)
            {
                if (j.Dobj == null)
                    continue;

                dobjs.AddRange(j.Dobj.List);
            }
            return dobjs;
        }

        private List<HSD_TOBJ> GetTOBJS()
        {
            var tobjs = new List<HSD_TOBJ>();
            var dobjs = GetDOBJS();
            foreach(var d in dobjs)
            {
                if (d.Mobj == null || d.Mobj.Textures == null)
                    continue;

                tobjs.AddRange(d.Mobj.Textures.List);
            }
            return tobjs;
        }

        /*private void RemakeVertexData()
        {
            var dobjs = GetDOBJS();
            GX_VertexCompressor c = new GX_VertexCompressor();
            foreach(var dobj in dobjs)
            {
                Console.WriteLine(dobj.Mobj.RenderFlags.ToString());
                dobj.Mobj.RenderFlags = RENDER_MODE.ALPHA_COMPAT | RENDER_MODE.DIFFSE_MAT;
                dobj.Mobj.Textures = null;
                if (dobj.Mobj.Textures != null)
                foreach (var tobj in dobj.Mobj.Textures.List)
                {
                    tobj.Flags = 0;
                    tobj.ImageData = null;
                    tobj.TlutData = null;
                }
                foreach (var pobj in dobj.Pobj.List)
                {
                    int off = 0;
                    var displayList = pobj.ToDisplayList();
                    var vertices = GX_VertexAttributeAccessor.GetDecodedVertices(displayList, pobj);
                    GX_DisplayList newdl = new GX_DisplayList();
                    foreach (var dl in displayList.Primitives)
                    {
                        var vs = new List<GX_Vertex>();
                        for(int i = 0; i < dl.Count; i++)
                        {
                            vs.Add(vertices[off+i]);
                        }
                        off += dl.Count;
                        //newdl.Primitives.Add(c.Compress(dl.PrimitiveType, vs.ToArray(), pobj.Attributes));
                    }
                    pobj.FromDisplayList(newdl); 
                }
            }
            c.SaveChanges();
        }*/

        #region Properties
            
        public Texture TOBJtoRenderTexture(HSD_TOBJ tobj)
        {
            if (tobjToSurface.ContainsKey(tobj._s))
                return tobjToSurface[tobj._s].GetRenderTexture();
            else
                return DefaultTextures.Instance.defaultBlack;
        }

        public override ISBMesh[] GetMeshObjects()
        {
            return Mesh.ToArray();
        }

        public override ISBMaterial[] GetMaterials()
        {
            ISBMaterial[] material = new ISBMaterial[Mesh.Count];
            for (int i = 0; i < material.Length; i++)
                material[i] = Mesh[i].Material;
            return material;
        }

        #endregion

        #region IO

        public override void LoadFromFile(string FileName)
        {
            if (System.IO.File.Exists(FileName.Replace(".dat", ".jcv")))
                IO.Formats.IO_8MOT.Settings.JVCPath = FileName.Replace(".dat", ".jcv");

            HSDFile = new HSDRawFile(FileName);
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
            var tobjs = GetTOBJS();
            List<HSD_Image> Process = new List<HSD_Image>();
            foreach (var tobj in tobjs)
            {
                var surface = new SBSurface();
                surface.Name = "TOBJ_" + tobjs.IndexOf(tobj);
                surface.Arrays.Add(new MipArray() { Mipmaps = new List<byte[]>() { tobj.GetDecodedImageData() } });
                surface.Width = tobj.ImageData.Width;
                surface.Height = tobj.ImageData.Height;
                surface.PixelFormat = PixelFormat.Bgra;
                surface.PixelType = PixelType.UnsignedByte;
                surface.InternalFormat = InternalFormat.Rgba;
                tobjToSurface.Add(tobj._s, surface);
                Surfaces.Add(surface);
            }
        }

        private void CreateMesh()
        {
            Mesh.Clear();

            if (RootJOBJ == null)
                return;

            var dobjs = GetDOBJS();
            var jobjs = RootJOBJ.BreathFirstList;// GetAllOfType<HSD_JOBJ>(RootJOBJ);
            foreach (var dobj in dobjs)
            {
                //SBConsole.WriteLine(dobj + " " + dobj.List.Count);
                SBBone parent = null;
                if(Skeleton.Bones.Length > 0)
                    parent = Skeleton.Bones[0];
                foreach (var b in Skeleton.Bones)
                {
                    if(b is SBHsdBone bone)
                    {
                        if(bone.GetJOBJ().Dobj != null)
                        if (bone.GetJOBJ().Dobj.List.Contains(dobj))
                        {
                            parent = b;
                            break;
                        }
                    }
                }
                var mesh = new SBHsdMesh(dobj, parent);
                mesh.Name = $"DOBJ_{Mesh.Count}";
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
                return HSDFile.Roots.Find(e => e.Data is HSDRaw.Common.Animation.HSD_MatAnimJoint) != null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearMaterialAnimations()
        {
            foreach(var roots in HSDFile.Roots)
            {
                if(roots.Data is HSDRaw.Common.Animation.HSD_MatAnimJoint matjoint)
                {
                    foreach (var v in matjoint.BreathFirstList)
                    {
                        if(v.MaterialAnimation != null)
                        foreach(var matanim in v.MaterialAnimation.List)
                        {
                                if (matanim.TextureAnimation != null)
                                {
                                    Console.WriteLine(matanim.TextureAnimation.ImageCount + " " +
                                        matanim.TextureAnimation.TlutCount + " " +
                                        (matanim.TextureAnimation.List==null) + " " +
                                        matanim.TextureAnimation.GXTexMapID);

                                    matanim.TextureAnimation.AnimationObject.Flags = HSDRaw.Common.Animation.AOBJ_Flags.NO_ANIM;
                                    matanim.TextureAnimation.AnimationObject.FObjDesc = null;
                                    matanim.TextureAnimation.AnimationObject.EndFrame = 0;
                                    matanim.TextureAnimation.GXTexMapID = (int)HSDRaw.GX.GXTexMapID.GX_TEXMAP0;
                                    //matanim.TextureAnimation.ImageCount = 0;
                                    //matanim.TextureAnimation.TlutCount = 0;
                                    //TODO:
                                    /*if (matanim.TextureAnimation. != null)
                                    {
                                        matanim.TextureAnimation.ImageArray.Elements = null;
                                        matanim.TextureAnimation.ImageArray.SetSize = 0;
                                    }
                                    if(matanim.TextureAnimation.TlutArray != null)
                                    {
                                        matanim.TextureAnimation.TlutArray.Elements = null;
                                        matanim.TextureAnimation.TlutArray.SetSize = 0;
                                    }*/
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

            Dictionary<HSD_JOBJ, HSD_JOBJ> childToParent = new Dictionary<HSD_JOBJ, HSD_JOBJ>();
            Dictionary<HSD_JOBJ, SBHsdBone> jobjToBone = new Dictionary<HSD_JOBJ, SBHsdBone>();

            foreach (var jobj in RootJOBJ.BreathFirstList)
            {
                foreach (var c in jobj.Children)
                    childToParent.Add(c, jobj);
                
                SBHsdBone bone = new SBHsdBone();
                bone.Name = "JOBJ_" + Skeleton.Bones.Length;

                bone.Transform = Matrix4.Identity;
                bone.Translation = new Vector3(jobj.TX, jobj.TY, jobj.TZ);
                bone.RotationEuler = new Vector3(jobj.RX, jobj.RY, jobj.RZ);
                bone.Scale = new Vector3(jobj.SX, jobj.SY, jobj.SZ);
                bone.SetJOBJ(jobj);

                jobjToBone.Add(jobj, bone);

                if(childToParent.ContainsKey(jobj))
                    bone.Parent = jobjToBone[childToParent[jobj]];
                else
                    ((SBSkeleton)Skeleton).AddRoot(bone);
            }

            //RecursivlyAddChildren(RootJOBJ, null);
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
            bone.Translation = new OpenTK.Vector3(jobj.TX, jobj.TY, jobj.TZ);
            bone.RotationEuler = new OpenTK.Vector3(jobj.RX, jobj.RY, jobj.RZ);
            bone.Scale = new OpenTK.Vector3(jobj.SX, jobj.SY, jobj.SZ);
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

            // Optimize Texture Sharing

            Dictionary<byte[], HSD_Image> imageDataToBuffer = new Dictionary<byte[], HSD_Image>(new ByteArrayComparer());

            foreach(var tobj in tobjToSurface)
            {
                var t = new HSD_TOBJ();
                t._s = tobj.Key;

                if (!imageDataToBuffer.ContainsKey(t.ImageData.ImageData))
                    imageDataToBuffer.Add(t.ImageData.ImageData, t.ImageData);

                t.ImageData = imageDataToBuffer[t.ImageData.ImageData];
            }

            // Save to file

            HSDFile.Save(FileName);
        }

        public class ByteArrayComparer : EqualityComparer<byte[]>
        {
            public override bool Equals(byte[] first, byte[] second)
            {
                if (first == null || second == null)
                {
                    // null == null returns true.
                    // non-null == null returns false.
                    return first == second;
                }
                if (ReferenceEquals(first, second))
                {
                    return true;
                }
                if (first.Length != second.Length)
                {
                    return false;
                }
                // Linq extension method is based on IEnumerable, must evaluate every item.
                return first.SequenceEqual(second);
            }
            public override int GetHashCode(byte[] obj)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException("obj");
                }
                // quick and dirty, instantly identifies obviously different
                // arrays as being different
                return obj.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        private static void InvertSingleBinds(IOModel model)
        {
            foreach (var m in model.Meshes)
                foreach (var v in m.Vertices)
                    if (v.Envelope.Weights.Count > 0 && v.Envelope.Weights[0].Weight == 1)
                    {
                        System.Numerics.Matrix4x4.Invert(model.Skeleton.GetBoneByName(v.Envelope.Weights[0].BoneName).WorldTransform, out System.Numerics.Matrix4x4 inv);
                        v.Transform(inv);
                    }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iomodel"></param>
        public override void FromIOModel(IOScene iomodel)
        {
            var model = iomodel.Models[0];

            InvertSingleBinds(model);

            // dobjs to import to
            var dobjs = GetMeshObjects();

            // clear pobjs
            foreach (SBHsdMesh m in GetMeshObjects())
                m.DOBJ.Pobj = null;

            var attributeGroup = MakeRiggedAttributes();
            var singleAttributeGroup = MakeSingleAttributes();

            // get a compressor ready
            // the compressor will handle making the compressed attribute buffers
            POBJ_Generator compressor = new POBJ_Generator();

            // import the iomeshes into their respective dobjs
            foreach (var iomesh in model.Meshes)
            {
                int dobjId = -1;
                int.TryParse(iomesh.Name.Replace("DOBJ_", ""), out dobjId);

                SBConsole.WriteLine(iomesh.Name + " imported:" + (dobjId != -1));

                if (dobjId != -1)
                {
                    var dobj = (SBHsdMesh)dobjs[dobjId];
                    dobj.ImportPOBJs(iomesh, (SBSkeleton)Skeleton, compressor, dobj.ParentBone == "JOBJ_0" ? attributeGroup : singleAttributeGroup);
                }
            }

            // finalizes and remakes the buffer
            compressor.SaveChanges();

            // refresh everything
            RefreshRendering(); 
        }

        private GXAttribName[] MakeRiggedAttributes()
        {
            return new GXAttribName[]
            {
                GXAttribName.GX_VA_PNMTXIDX,
                GXAttribName.GX_VA_POS,
                GXAttribName.GX_VA_NRM,
                GXAttribName.GX_VA_TEX0
            };
        }

        private GXAttribName[] MakeSingleAttributes()
        {
            return new GXAttribName[]
            {
                GXAttribName.GX_VA_POS,
                GXAttribName.GX_VA_NRM,
                GXAttribName.GX_VA_TEX0
            };
        }

        public override IOScene GetIOModel()
        {
            IOScene scene = new IOScene();

            IOModel iomodel = new IOModel();
            scene.Models.Add(iomodel);

            iomodel.Skeleton = ((SBSkeleton)Skeleton).ToIOSkeleton();

            // bone indices
            List<SBHsdBone> bones = new List<SBHsdBone>();

            foreach (SBHsdBone bone in Skeleton.Bones)
                bones.Add(bone);
            

            // gather textures
            Dictionary<HSDStruct, string> tobjToName = new Dictionary<HSDStruct, string>();

            List<SBSurface> textures = new List<SBSurface>();

            foreach (var tex in tobjToSurface)
            {
                tex.Value.Name = $"TOBJ_{textures.Count}";
                tobjToName.Add(tex.Key, tex.Value.Name);
                textures.Add(tex.Value);
            }


            // process mesh
            foreach (SBHsdMesh me in GetMeshObjects())
            {
                var dobj = me.DOBJ;

                var parent = Skeleton.Bones[0];
                foreach (var b in Skeleton.Bones)
                {
                    if (b is SBHsdBone bone)
                    {
                        if (bone.GetJOBJ().Dobj != null)
                            if (bone.GetJOBJ().Dobj.List.Contains(dobj))
                            {
                                parent = b;
                                break;
                            }
                    }
                }

                var iomesh = new IOMesh();
                iomesh.Name = me.Name;
                iomodel.Meshes.Add(iomesh);
                IOPolygon poly = new IOPolygon();
                iomesh.Polygons.Add(poly);

                if (dobj.Pobj != null)
                    foreach (var pobj in dobj.Pobj.List)
                    {
                        var dl = pobj.ToDisplayList();
                        var vertices = GX_VertexAccessor.GetDecodedVertices(dl, pobj);

                        HSD_Envelope[] bindGroups = null; ;
                        if (pobj.EnvelopeWeights != null)
                            bindGroups = pobj.EnvelopeWeights;

                        var offset = 0;
                        foreach (var v in dl.Primitives)
                        {
                            List<GX_Vertex> strip = new List<GX_Vertex>();
                            for (int i = 0; i < v.Count; i++)
                                strip.Add(vertices[offset + i]);
                            offset += v.Count;
                            iomesh.Vertices.AddRange(ConvertGXDLtoTriangleList(v.PrimitiveType, SBHsdMesh.GXVertexToHsdVertex(strip, bones, bindGroups), (SBHsdBone)parent));
                        }
                    }
                

                // flip faces
                for (int i = 0; i < iomesh.Vertices.Count; i += 3)
                {
                    if (i + 2 < iomesh.Vertices.Count)
                    {
                        poly.Indicies.Add(i + 2);
                        poly.Indicies.Add(i + 1);
                        poly.Indicies.Add(i);
                    }
                    else
                        break;
                }

                poly.MaterialName = iomesh.Name + "_material";


                // create material
                IOMaterial mat = new IOMaterial();

                mat.Name = iomesh.Name + "_material";
                if (dobj.Mobj.Material != null)
                {
                    mat.DiffuseColor = new System.Numerics.Vector4(
                        dobj.Mobj.Material.DiffuseColor.R / 255f,
                        dobj.Mobj.Material.DiffuseColor.G / 255f,
                        dobj.Mobj.Material.DiffuseColor.B / 255f,
                        dobj.Mobj.Material.DiffuseColor.A / 255f);
                    mat.SpecularColor = new System.Numerics.Vector4(
                        dobj.Mobj.Material.SpecularColor.R / 255f,
                        dobj.Mobj.Material.SpecularColor.G / 255f,
                        dobj.Mobj.Material.SpecularColor.B / 255f,
                        dobj.Mobj.Material.SpecularColor.A / 255f);
                    mat.AmbientColor = new System.Numerics.Vector4(
                        dobj.Mobj.Material.AmbientColor.R / 255f,
                        dobj.Mobj.Material.AmbientColor.G / 255f,
                        dobj.Mobj.Material.AmbientColor.B / 255f,
                        dobj.Mobj.Material.AmbientColor.A / 255f);
                    mat.Alpha = dobj.Mobj.Material.Alpha;
                    mat.Shininess = dobj.Mobj.Material.Shininess;
                }

                if (dobj.Mobj.Textures != null)
                    mat.DiffuseMap = new IOTexture()
                    {
                        Name = tobjToName[dobj.Mobj.Textures._s],
                        FilePath = tobjToName[dobj.Mobj.Textures._s]
                    };
                scene.Materials.Add(mat);
            }

            return scene;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="vertices"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<IOVertex> ConvertGXDLtoTriangleList(GXPrimitiveType type, List<SBHsdVertex> vertices, SBHsdBone parent)
        {
            var list = new List<IOVertex>();

            foreach(var v in vertices)
            {
                var vertex = new IOVertex()
                {
                    Position = new System.Numerics.Vector3(v.POS.X, v.POS.Y, v.POS.Z),
                    Normal = new System.Numerics.Vector3(v.NRM.X, v.NRM.Y, v.NRM.Z),
                };
                vertex.SetUV(v.UV0.X, v.UV0.Y, 0);

                for(int i = 0; i < 4; i++)
                {
                    if(v.Weight[i] > 0)
                    {
                        vertex.Envelope.Weights.Add(new IOBoneWeight()
                        {
                            BoneName = "JOBJ_" + (int)v.Bone[i],
                            Weight = v.Weight[i]
                        });
                    }
                }

                if(parent != null)
                {
                    vertex.Transform(TktoNumeric(parent.WorldTransform));

                    if (vertex.Envelope.Weights.Count == 0)
                    {
                        vertex.Envelope.Weights.Add(new IOBoneWeight()
                        {
                            BoneName = parent.Name,
                            Weight = 1
                        });
                    }
                }
                if(v.Weight.X == 1)
                {
                    vertex.Transform(TktoNumeric(Skeleton.Bones[(int)v.Bone.X].WorldTransform));
                }
                list.Add(vertex);
            }

            if (type == GXPrimitiveType.TriangleStrip)
                TriangleConvert.StripToList(list, out list);

            if (type == GXPrimitiveType.Quads)
                TriangleConvert.QuadToList(list, out list);

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public System.Numerics.Matrix4x4 TktoNumeric(Matrix4 m)
        {
            return new System.Numerics.Matrix4x4(m.M11, m.M21, m.M31, m.M41,
                m.M12, m.M22, m.M32, m.M42,
                m.M13, m.M23, m.M33, m.M43,
                m.M14, m.M24, m.M34, m.M44);
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

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

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
                if(bones.Length > boneTransforms.Length)
                {
                    boneTransforms = new Matrix4[bones.Length];
                    boneBinds = new Matrix4[bones.Length];
                }
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

            List<SBHsdMesh> sortedMesh = new List<SBHsdMesh>();
            List<SBHsdMesh> opaqueMesh = new List<SBHsdMesh>();

            for(int i = Mesh.Count - 1; i >= 0; i--)
            {
                var m = Mesh[i];
                if (m.ParentBone == null || m.ParentBone == "")
                    opaqueMesh.Add(m);
                else
                    sortedMesh.Add(m);
            }
            sortedMesh = sortedMesh.OrderBy(c =>
                {
                    Matrix4 transform = Skeleton[c.ParentBone].WorldTransform * camera.MvpMatrix;
                    var v = Vector3.TransformPosition(c.BoundingSphere.Position, transform);
                    v = Vector3.TransformPosition(c.BoundingSphere.Position, camera.MvpMatrix);
                    return v.Z + c.BoundingSphere.Radius;
                }).ToList();
            opaqueMesh.AddRange(sortedMesh);

            // TODO: sort opaque mesh
            foreach(var rm in opaqueMesh)
            {
                if (!rm.Visible)
                    continue;
                shader.SetBoolToInt("renderWireframe", ApplicationSettings.EnableWireframe || rm.Selected);
                rm.Draw(this, shader);
            }

            base.RenderShader(camera);

            // render zones
            /*if(HSDFile != null && HSDFile.Roots.Count > 1 && HSDFile.Roots[1].Node is KAR_GrModel model)
            {
                foreach( var el in model.MainModel.ModelUnk1.GroupsUnk1_1.Elements)
                {
                    var min = new Vector3(el.UnkFloat1, el.UnkFloat2, el.UnkFloat3);
                    var max = new Vector3(el.UnkFloat4, el.UnkFloat5, el.UnkFloat6);
                    var mid = (max + min) / 2;
                    var size = max - min;
                    Rendering.Shapes.RectangularPrism.DrawRectangularPrism(camera, mid, size, Matrix4.Identity);
                }
            }*/
        }

        private static int JOBJDistance(Vector3 p1, Vector3 p2)
        {
            // no need for square root
            return (int)(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2));
        }

        #endregion
    }
}
