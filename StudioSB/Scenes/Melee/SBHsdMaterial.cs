using System;
using SFGraphics.GLObjects.Shaders;
using HSDLib.Common;
using StudioSB.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK;

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

            shader.SetInt("flags", (int)_mobj.RenderFlags);
            
            //todo move or remove flags from shader
            shader.SetBoolToInt("enableDiffuseLighting", _mobj.RenderFlags.HasFlag(RENDER_MODE.DIFFUSE));
            shader.SetBoolToInt("enableSpecular", _mobj.RenderFlags.HasFlag(RENDER_MODE.SPECULAR));

            shader.SetFloat("glossiness", 0);
            shader.SetFloat("transparency", 1);
            shader.SetVector4("ambientColor", Vector4.Zero);
            shader.SetVector4("diffuseColor", Vector4.One);
            shader.SetVector4("specularColor", Vector4.One);

            if (_mobj.MaterialColor != null)
            {
                var matcol = _mobj.MaterialColor;

                shader.SetFloat("glossiness", matcol.Shininess);
                shader.SetFloat("transparency", matcol.Alpha);

                shader.SetVector4("ambientColor", matcol.AMB_R / 255f, matcol.AMB_G / 255f, matcol.AMB_B / 255f, matcol.AMB_A / 255f);
                shader.SetVector4("diffuseColor", matcol.DIF_R / 255f, matcol.DIF_G / 255f, matcol.DIF_B / 255f, matcol.DIF_A / 255f);
                shader.SetVector4("specularColor", matcol.SPC_R / 255f, matcol.SPC_G / 255f, matcol.SPC_B / 255f, matcol.SPC_A / 255f);
            }

            if(_mobj.Textures != null)
            {
                bool hasSphere0 = false;
                bool hasDiffuse0 = false;
                bool hasSphere1 = false;
                bool hasDiffuse1 = false;
                bool hasSpecular = false;
                bool hasBumpMap = false;
                foreach (var texture in _mobj.Textures.List)
                {
                    var rTexture = ((HSDScene)scene).TOBJtoRenderTexture(texture);
                    var texScale = new Vector2(texture.WScale, texture.HScale);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GXtoGL.GLWrapMode(texture.WrapS));
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GXtoGL.GLWrapMode(texture.WrapT));
                    // bugged
                    //rTexture.TextureWrapS = GXtoGL.GLWrapMode(texture.WrapS);
                    //rTexture.TextureWrapT = GXtoGL.GLWrapMode(texture.WrapT);

                    if (texture.Flags.HasFlag(TOBJ_FLAGS.LIGHTMAP_DIFFUSE))
                    {
                        hasDiffuse0 = true;
                        shader.SetVector2("diffuseScale0", texScale);
                        shader.SetTexture("diffuseTex0", rTexture, TextureUnit++);

                        /*if (texture.Flags.HasFlag(TOBJ_FLAGS.LIGHTMAP_SPECULAR))
                        {
                            hasDiffuse1 = true;
                            shader.SetVector2("diffuseScale1", texScale);
                            shader.SetTexture("diffuseTex1", rTexture, TextureUnit++);
                        }*/
                    }
                    if (texture.Flags.HasFlag(TOBJ_FLAGS.LIGHTMAP_SPECULAR))
                    {
                        hasSpecular = true;
                        shader.SetVector2("specularScale", texScale);
                        shader.SetTexture("specularTex", rTexture, TextureUnit++);
                    }
                    if (texture.Flags.HasFlag(TOBJ_FLAGS.BUMP))
                    {
                        hasBumpMap = true;
                        shader.SetInt("bumpMapWidth", texture.ImageData.Width);
                        shader.SetInt("bumpMapHeight", texture.ImageData.Height);
                        shader.SetVector2("bumpMapTexScale", texScale);
                        shader.SetTexture("bumpMapTex", rTexture, TextureUnit++);
                    }
                }

                shader.SetBoolToInt("hasSphere0", hasSphere0);
                shader.SetBoolToInt("hasSphere1", hasSphere1);
                shader.SetBoolToInt("hasDiffuse0", hasDiffuse0);
                shader.SetBoolToInt("hasDiffuse1", hasDiffuse1);
                shader.SetBoolToInt("hasSpecular", hasSpecular);
                shader.SetBoolToInt("hasBumpMap", hasBumpMap);
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
