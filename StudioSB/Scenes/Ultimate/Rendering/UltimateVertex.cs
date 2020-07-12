using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.VertexAttributes;

namespace StudioSB.Scenes.Ultimate
{
    public struct IVec4
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int W { get; set; }

        public int this[int i]
        {
            get
            {
                switch (i) {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default:
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }

    public struct UltimateVertex
    {
        [VertexFloat("Position0", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Position0 { get; }

        [VertexFloat("Normal0", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Normal0 { get; }

        [VertexFloat("Tangent0", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Tangent0 { get; }

        // Generated value.
        [VertexFloat("Bitangent0", ValueCount.Three, VertexAttribPointerType.Float, false)]
        public Vector3 Bitangent0 { get; }

        [VertexFloat("map1", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public Vector2 Map1 { get; }

        [VertexFloat("uvSet", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public Vector2 UvSet { get; }

        [VertexFloat("uvSet1", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public Vector2 UvSet1 { get; }

        [VertexFloat("uvSet2", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public Vector2 UvSet2 { get; }

        [VertexInt("boneIndices", ValueCount.Four, VertexAttribIntegerType.UnsignedInt)]
        public IVec4 BoneIndices { get; }

        [VertexFloat("boneWeights", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 BoneWeights { get; }

        [VertexFloat("bake1", ValueCount.Two, VertexAttribPointerType.Float, false)]
        public Vector2 Bake1 { get; }

        [VertexFloat("colorSet1", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet1 { get; }

        [VertexFloat("colorSet2", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet2 { get; }

        [VertexFloat("colorSet21", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet21 { get; }

        [VertexFloat("colorSet22", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet22 { get; }

        [VertexFloat("colorSet23", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet23 { get; }

        [VertexFloat("colorSet3", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet3 { get; }

        [VertexFloat("colorSet4", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet4 { get; }

        [VertexFloat("colorSet5", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet5 { get; }

        [VertexFloat("colorSet6", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet6 { get; }

        [VertexFloat("colorSet7", ValueCount.Four, VertexAttribPointerType.Float, false)]
        public Vector4 ColorSet7 { get; }

        public UltimateVertex(Vector3 position0, Vector3 normal0, Vector3 tangent0, Vector3 bitangent0, 
            Vector2 map1, Vector2 uvSet, Vector2 uvSet1, Vector2 uvSet2, 
            IVec4 boneIndices, Vector4 boneWeights, 
            Vector2 bake1, 
            Vector4 colorSet1, 
            Vector4 colorSet2, Vector4 colorSet21, Vector4 colorSet22, Vector4 colorSet23,
            Vector4 colorSet3, Vector4 colorSet4, Vector4 colorSet5, Vector4 colorSet6, Vector4 colorSet7)
        {
            Position0 = position0;
            Normal0 = normal0;
            Tangent0 = tangent0;
            Bitangent0 = bitangent0;
            Map1 = map1;
            UvSet = uvSet;
            UvSet1 = uvSet1;
            UvSet2 = uvSet2;
            BoneIndices = boneIndices;
            BoneWeights = boneWeights;
            Bake1 = bake1;
            ColorSet1 = colorSet1;
            ColorSet2 = colorSet2;
            ColorSet21 = colorSet21;
            ColorSet22 = colorSet22;
            ColorSet23 = colorSet23;
            ColorSet3 = colorSet3;
            ColorSet4 = colorSet4;
            ColorSet5 = colorSet5;
            ColorSet6 = colorSet6;
            ColorSet7 = colorSet7;
        }
    }
}
