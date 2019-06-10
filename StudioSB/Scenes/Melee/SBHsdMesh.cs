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

        public SBHsdMesh(HSDScene scene, HSD_DOBJ dobj, SBBone parent)
        {
            Name = "DOBJ";

            _dobj = dobj;

            material = new SBHsdMaterial(dobj);

            Visible = true;

            ParentBone = parent.Name;
            
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

        public void Draw(HSDScene scene, Shader shader)
        {
            if (!Visible)
                return;

            material.Bind(scene, shader);

            shader.SetMatrix4x4("singleBind", scene.Skeleton[ParentBone].WorldTransform);

            foreach (var rm in renderMesh)
            {
                rm.Draw(shader);
            }
        }
        
        private List<SBHsdVertex> GXVertexToHsdVertex(List<GXVertex> vertices, List<SBHsdBone> bones, HSD_JOBJWeight[] jobjweights)
        {
            List<SBHsdVertex> newvertices = new List<SBHsdVertex>();

            foreach(var v in vertices)
            {
                Vector4 bone = new Vector4();
                Vector4 weights = new Vector4();

                if (jobjweights != null)
                {
                    var m0xid = v.PMXID;
                    var jobjWeight = jobjweights[m0xid / 3];

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
