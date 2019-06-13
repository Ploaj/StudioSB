using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSDLib.Common;
using HSDLib.Helpers;
using SFGenericModel;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using SFGraphics.GLObjects.Shaders;
using StudioSB.IO.Models;

namespace StudioSB.Scenes.Melee
{
    public class SBHsdRenderMesh : GenericMesh<SBHsdVertex>
    {
        public SBHsdRenderMesh(List<SBHsdVertex> vertices, PrimitiveType type) : base(vertices, type)
        {

        }
    }

    public class SBHsdMesh : ISBMesh
    {
        public HSD_DOBJ DOBJ{ get => _dobj; }
        private HSD_DOBJ _dobj { get; set; }

        public override int PolyCount => base.PolyCount;
        public override int VertexCount => base.VertexCount;

        private List<SBHsdRenderMesh> renderMesh = new List<SBHsdRenderMesh>();

        private SBHsdMaterial material;

        //TODO: update dobj when parent string name changes

        public override string ToString()
        {
            return Name;
        }

        public SBHsdMesh(HSD_DOBJ dobj, SBBone parent)
        {
            Name = "DOBJ";

            _dobj = dobj;

            material = new SBHsdMaterial(dobj);

            ParentBone = parent.Name;

            Visible = true;
        }

        public void Refresh(HSDScene scene)
        {
            RefreshRendering(scene.Skeleton);
        }

        private void RefreshRendering(ISBSkeleton skeleton)
        {
            BoundingSphere = new Rendering.Bounding.BoundingSphere(0, 0, 0, 0);
            List<SBHsdBone> bones = new List<SBHsdBone>();

            foreach (SBHsdBone bone in skeleton.Bones)
                bones.Add(bone);
            if (_dobj.POBJ != null)
                foreach (var pobj in _dobj.POBJ.List)
                {
                    //foreach (var va in pobj.VertexAttributes.Attributes)
                    //    Console.WriteLine($"{va.Name} {va.AttributeType} {va.CompCount} {va.CompType} {va.Scale} {va.Stride}");
                    var vertices = VertexAccessor.GetDecodedVertices(pobj);
                    var dl = VertexAccessor.GetDisplayList(pobj);

                    var offset = 0;
                    foreach (var v in dl.Primitives)
                    {
                        List<GXVertex> strip = new List<GXVertex>();
                        for (int i = 0; i < v.Count; i++)
                            strip.Add(vertices[offset + i]);
                        offset += v.Count;

                        var rm = new SBHsdRenderMesh(GXVertexToHsdVertex(strip, bones, pobj.BindGroups == null ? null : pobj.BindGroups.Elements), GXtoGL.GLPrimitiveType(v.PrimitiveType));
                        
                        renderMesh.Add(rm);
                    }

                }
            
        }

        /// <summary>
        /// 
        /// </summary>
        public void ImportPBOJs(IOMesh mesh, SBSkeleton skeleton, VertexCompressor compressor, HSD_AttributeGroup attrGroup)
        {
            DOBJ.POBJ = null;

            // get gx vertices and rigging groups
            List<HSD_JOBJWeight> weightList = null;
            var verts = IOVertexToGXVertex(mesh.Vertices, skeleton, out weightList);

            // reorder to triangle strips
            List<GXVertex> triStrip = new List<GXVertex>();
            foreach (var i in mesh.Indices)
                triStrip.Add(verts[(int)i]);
            
            // compress and generate display lists
            DOBJ.POBJ = compressor.CreatePOBJ(triStrip, attrGroup, weightList);
        }

        private List<GXVertex> IOVertexToGXVertex(List<IOVertex> ioverts, SBSkeleton skeleton, out List<HSD_JOBJWeight> weightList)
        {
            weightList = new List<HSD_JOBJWeight>();

            List<GXVertex> gxverts = new List<GXVertex>();
            foreach(var iovert in ioverts)
            {
                HSD_JOBJWeight weight = new HSD_JOBJWeight();
                for(int i = 0; i < 4; i++)
                {
                    if(iovert.BoneWeights[i] != 0)
                    {
                        var jobj = ((SBHsdBone)skeleton.Bones[(int)iovert.BoneIndices[i]]).GetJOBJ();
                        if (jobj == null)
                            throw new Exception("Error getting JOBJ for rigging");
                        weight.JOBJs.Add(jobj);
                        weight.Weights.Add(iovert.BoneWeights[i]);
                    }
                }
                int index = weightList.IndexOf(weight);
                if(index == -1)
                {
                    index = weightList.Count;
                    weightList.Add(weight);
                }

                GXVertex gxvert = new GXVertex();
                gxvert.PMXID = (byte)(index*3);
                gxvert.Pos = new GXVector3(iovert.Position.X, iovert.Position.Y, iovert.Position.Z);
                gxvert.Nrm = new GXVector3(iovert.Normal.X, iovert.Normal.Y, iovert.Normal.Z);
                gxvert.TEX0 = new GXVector2(iovert.UV0.X, iovert.UV0.Y);
                gxvert.Clr0 = new GXColor4(iovert.Color.X, iovert.Color.Y, iovert.Color.Z, iovert.Color.W);
                gxverts.Add(gxvert);
            }
            return gxverts;
        }

        /// <summary>
        /// Clears the texture in the mobj
        /// </summary>
        public void ClearTextures()
        {
            DOBJ.MOBJ.RenderFlags = RENDER_MODE.ALPHA_COMPAT | RENDER_MODE.DIFFUSE;
            DOBJ.MOBJ.Textures = null;
        }

        public void Draw(HSDScene scene, Shader shader)
        {
            if (!Visible)
                return;

            material.Bind(scene, shader);

            shader.SetMatrix4x4("singleBind", scene.Skeleton[ParentBone].AnimatedWorldTransform);

            foreach (var rm in renderMesh)
            {
                rm.Draw(shader);
            }
        }
        
        public static List<SBHsdVertex> GXVertexToHsdVertex(List<GXVertex> vertices, List<SBHsdBone> bones, HSD_JOBJWeight[] jobjweights)
        {
            List<SBHsdVertex> newvertices = new List<SBHsdVertex>();

            foreach(var v in vertices)
            {
                Vector4 bone = new Vector4();
                Vector4 weights = new Vector4();

                if (jobjweights != null)
                {
                    if(v.PMXID / 3 >= jobjweights.Length)
                        Console.WriteLine(jobjweights.Length+ " " + (v.PMXID));
                    var jobjWeight = jobjweights[v.PMXID / 3];

                    for (int i = 0; i < Math.Min(4, jobjWeight.JOBJs.Count); i++)
                        bone[i] = bones.FindIndex(e=>e.GetJOBJ() == jobjWeight.JOBJs[i]);

                    for (int i = 0; i < Math.Min(4, jobjWeight.Weights.Count); i++)
                        weights[i] = jobjWeight.Weights[i];
                }

                newvertices.Add(new SBHsdVertex(v.Pos, v.Nrm, v.TEX0, bone, weights));

            } 

            return newvertices;
        }
        
    }
}
