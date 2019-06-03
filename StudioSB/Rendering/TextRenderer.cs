using SFGraphics.Cameras;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;
using SFGenericModel.VertexAttributes;

namespace StudioSB.Rendering
{
    public class TextRenderer
    {
        private static CharacterShape characterShape;
        
        public static void Draw(Camera camera, string Text)
        {
            Draw(camera, Text, Matrix4.Identity, Vector3.One, Vector2.Zero, Blend: true);
        }

        public static void Draw(Camera camera, string Text, Vector3 Color)
        {
            Draw(camera, Text, Matrix4.Identity, Color, Vector2.Zero, Blend: true);
        }

        public static void Draw(Camera camera, string Text, Matrix4 Transform)
        {
            Draw(camera, Text, Transform, Vector3.One, Vector2.Zero, Blend: true, Center: true);
        }

        public static void Draw(Camera camera, string Text, Matrix4 Transform, Vector3 Color)
        {
            Draw(camera, Text, Transform, Color, Vector2.Zero);
        }

        public static void DrawOrtho(Camera camera, string Text, Vector2 Position)
        {
            Draw(camera, Text, Matrix4.Identity, Vector3.One, Position, Blend: true, RelativeToWorld: false);
        }

        private static void Draw(Camera camera, string Text, Matrix4 Transform, Vector3 Color, Vector2 WindowPosition, int Size = 16, bool Blend = false, bool Center = false, bool RelativeToWorld = true)
        {
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.Enable(EnableCap.Texture2D);

            if (Blend)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            if (characterShape == null)
                characterShape = new CharacterShape();

            var shader = ShaderManager.GetShader("Text");
            if (shader != null && shader.LinkStatusIsOk)
            {
                shader.UseProgram();

                shader.SetTexture("fontSheet", DefaultTextures.Instance.renderFont, 0);
                
                shader.SetMatrix4x4("transform", ref Transform);

                shader.SetFloat("letterSize", Size);
                shader.SetVector2("windowSize", new Vector2(camera.RenderHeight, camera.RenderWidth));
                shader.SetVector2("windowPosition", WindowPosition);
                shader.SetBoolToInt("relativeToWorld", RelativeToWorld);

                int offset = 0;
                int LetterSize = Size - (int)(0.45f * Size);
                if (Center)
                    offset = -(Text.Length * LetterSize) / 2;
                // TODO: drawing each letter individually can be slow
                // maybe build a mesh for them?
                foreach(char c in Text.ToCharArray())
                {
                    shader.SetFloat("letterIndex", c - ' ');

                    shader.SetVector3("letterColor", Color);
                    shader.SetVector2("letterPosition", new Vector2(offset, 0));
                    characterShape.Draw(shader, camera);

                    //Drop shadow
                    shader.SetVector3("letterColor", new Vector3(0, 0, 0));
                    shader.SetVector2("letterPosition", new Vector2(offset + 1, -1));
                    characterShape.Draw(shader, camera);

                    offset += LetterSize;
                }
            }
            else
            {
                SBConsole.WriteLine(shader.GetErrorLog());
            }
            

            GL.PopAttrib();
        }

    }

    public class CharacterShape : SFGenericModel.GenericMesh<Vector2>
    {
        private static List<Vector2> screenPositions = new List<Vector2>()
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        public CharacterShape() : base(screenPositions, PrimitiveType.Quads)
        {

        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("point", ValueCount.Four, VertexAttribPointerType.Float),
            };
        }
    }
}
