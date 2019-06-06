using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using System.Collections.Generic;

namespace StudioSB.Scenes.Ultimate
{
    public class UltimateRenderMesh : GenericMesh<UltimateVertex>
    {
        public UltimateRenderMesh(List<UltimateVertex> vertices, List<uint> indices) : base(vertices, indices, PrimitiveType.Triangles)
        {
            
        }

        /*public void SetRenderState(Material material)
        {
            renderSettings.alphaBlendSettings = new SFGenericModel.RenderState.AlphaBlendSettings(true, material.BlendSrc, material.BlendDst, BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
        }*/
    }
}
