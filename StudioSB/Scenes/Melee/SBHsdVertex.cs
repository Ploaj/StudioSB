using HSDLib.Helpers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.VertexAttributes;

namespace StudioSB.Scenes.Melee
{
    public struct SBHsdVertex
    {
        [VertexFloat("POS", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public GXVector3 POS { get; }

        [VertexFloat("NRM", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public GXVector3 NRM { get; }

        [VertexFloat("UV0", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public GXVector2 UV0 { get; }

        [VertexFloat("Bone", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Bone { get; }

        [VertexFloat("Weight", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 Weight { get; }

        public SBHsdVertex(GXVector3 pOS, GXVector3 nRM, GXVector2 uV0, Vector4 bone, Vector4 weight) : this()
        {
            POS = pOS;
            NRM = nRM;
            UV0 = uV0;
            Bone = bone;
            Weight = weight;
        }
    }
}
