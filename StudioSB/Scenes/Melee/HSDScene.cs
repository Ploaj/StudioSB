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
                }
                return null;
            }
        }

        public HSDScene()
        {
            //AttachmentTypes.Remove(typeof(SBMeshList));
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
            RefreshTextures();
            RefreshSkeleton();
            RefreshMesh();
        }

        private void RefreshTextures()
        {
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

        private void RefreshMesh()
        {
            Mesh.Clear();

            var dobjs = RootJOBJ.GetAllOfType<HSD_DOBJ>();
            var jobjs = RootJOBJ.GetAllOfType<HSD_JOBJ>();
            foreach (var dobj in dobjs)
            {
                var parent = Skeleton.Bones[0];
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

                Mesh.Add(new SBHsdMesh(this, dobj, parent));
            }
        }

        /// <summary>
        /// Refreshes the skeleton to update state of the HSDFile
        /// </summary>
        private void RefreshSkeleton()
        {
            Skeleton = new SBSkeleton();
            foreach(var root in HSDFile.Roots)
            {
                if(root.Node is HSD_JOBJ jobj)
                {
                    RecursivlyAddChildren(jobj, null);
                    break;
                }
            }
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
            System.Windows.Forms.MessageBox.Show("Importing Model to DAT not supported");
        }

        public override IOModel GetIOModel()
        {
            System.Windows.Forms.MessageBox.Show("Export DAT model is not supported at this time");
            return base.GetIOModel();
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
                boneTransforms = Skeleton.GetWorldTransforms();
            }

            int blockIndex2 = GL.GetUniformBlockIndex(shader.Id, "BoneTransforms");
            boneTransformUniformBuffer.BindBase(BufferRangeTarget.UniformBuffer, blockIndex2);
            boneTransformUniformBuffer.SetData(boneTransforms, BufferUsageHint.DynamicDraw);

            foreach (var rm in Mesh)
            {
                rm.Draw(this, shader);
            }

            base.RenderShader(camera);
        }

        #endregion
    }
}
