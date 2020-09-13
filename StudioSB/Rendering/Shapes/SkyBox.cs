using SFGenericModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.VertexAttributes;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Cameras;
using OpenTK;

namespace StudioSB.Rendering.Shapes
{
    public class SkyBox : GenericMesh<Vector3>
    {
        private static Vector3[] SkyboxVertices = new Vector3[]{
    // positions          
    new Vector3(-1.0f,  1.0f, -1.0f),
    new Vector3(-1.0f, -1.0f, -1.0f),
    new Vector3( 1.0f, -1.0f, -1.0f),
     new Vector3(1.0f, -1.0f, -1.0f),
     new Vector3(1.0f,  1.0f, -1.0f),
    new Vector3(-1.0f,  1.0f, -1.0f),

    new Vector3(-1.0f, -1.0f,  1.0f),
    new Vector3(-1.0f, -1.0f, -1.0f),
    new Vector3(-1.0f,  1.0f, -1.0f),
    new Vector3(-1.0f,  1.0f, -1.0f),
    new Vector3(-1.0f,  1.0f,  1.0f),
    new Vector3(-1.0f, -1.0f,  1.0f),

     new Vector3(1.0f, -1.0f, -1.0f),
     new Vector3(1.0f, -1.0f,  1.0f),
     new Vector3(1.0f,  1.0f,  1.0f),
     new Vector3(1.0f,  1.0f,  1.0f),
     new Vector3(1.0f,  1.0f, -1.0f),
     new Vector3(1.0f, -1.0f, -1.0f),

    new Vector3(-1.0f, -1.0f,  1.0f),
    new Vector3(-1.0f,  1.0f,  1.0f),
    new Vector3( 1.0f,  1.0f,  1.0f),
    new Vector3( 1.0f,  1.0f,  1.0f),
    new Vector3( 1.0f, -1.0f,  1.0f),
   new Vector3( -1.0f, -1.0f,  1.0f),

new Vector3(    -1.0f,  1.0f, -1.0f),
 new Vector3(    1.0f,  1.0f, -1.0f),
 new Vector3(    1.0f,  1.0f,  1.0f),
 new Vector3(    1.0f,  1.0f,  1.0f),
  new Vector3(  -1.0f,  1.0f,  1.0f),
  new Vector3(  -1.0f,  1.0f, -1.0f),

 new Vector3(   -1.0f, -1.0f, -1.0f),
  new Vector3(  -1.0f, -1.0f,  1.0f),
 new Vector3(    1.0f, -1.0f, -1.0f),
 new Vector3(    1.0f, -1.0f, -1.0f),
 new Vector3(   -1.0f, -1.0f,  1.0f),
 new Vector3(    1.0f, -1.0f,  1.0f)
        };

        private static SkyBox UnitSkyBox = null;

        public static void RenderSkyBox(Camera camera, TextureCubeMap cubemap, int mipLevel = 0)
        {
            if (UnitSkyBox == null)
                UnitSkyBox = new SkyBox();

            var shader = ShaderManager.GetShader("CubeMap");
            
            shader.UseProgram();

            shader.SetInt("mipLevel", mipLevel);

            Matrix4 view = camera.ModelViewMatrix;
            var mat = new Matrix3(view);
            view = new Matrix4(mat);
            shader.SetMatrix4x4("view", ref view);
            
            var projection = Matrix4.CreatePerspectiveFieldOfView(1.3f, camera.RenderWidth / (float)camera.RenderHeight, 0.1f, 100.0f);
            shader.SetMatrix4x4("projection", ref projection);

            shader.SetTexture("skybox", cubemap, 0);
            cubemap.MagFilter = TextureMagFilter.Nearest;
            
            UnitSkyBox.Draw(shader);
        }

        public SkyBox() : base(SkyboxVertices, PrimitiveType.Triangles)
        {

        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("Position", ValueCount.Three, VertexAttribPointerType.Float, false),
            };
        }
    }
}