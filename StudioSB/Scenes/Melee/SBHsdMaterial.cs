using System;
using SFGraphics.GLObjects.Shaders;
using HSDLib.Common;
using StudioSB.Rendering;
using OpenTK.Graphics.ES10;

namespace StudioSB.Scenes.Melee
{
    public class SBHsdMaterial : ISBMaterial
    {
        private HSD_MOBJ _mobj;

        public string Name { get; set; }
        public string Label { get; set; }

        public SBHsdMaterial(HSD_DOBJ dobj)
        {
            _mobj = dobj.MOBJ;
        }

        public void AnimateParam(string ParamName, object Value)
        {
        }

        public void Bind(SBScene scene, Shader shader)
        {
            int TextureUnit = 0;
            shader.SetTexture("uvPattern", DefaultTextures.Instance.uvPattern, TextureUnit++);

            if (_mobj == null)
                return;

            if(_mobj.Textures != null)
            {
                var texture = _mobj.Textures;
                var rTexture = ((HSDScene)scene).TOBJtoRenderTexture(texture);
                rTexture.TextureWrapS = GXtoGL.GLWrapMode(texture.WrapS);
                rTexture.TextureWrapT = GXtoGL.GLWrapMode(texture.WrapT);
                shader.SetVector2("textureScale", new OpenTK.Vector2(texture.WScale, texture.HScale));
                shader.SetTexture("diffuse", rTexture, TextureUnit++);
            }
        }

        public void ExportMaterial(string FileName)
        {
        }

        public void ImportMaterial(string FileName)
        {
        }
    }
}
