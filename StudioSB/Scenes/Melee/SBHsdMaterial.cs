using System;
using SFGraphics.GLObjects.Shaders;
using HSDRaw.Common;
using HSDRaw.GX;
using StudioSB.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;
using SFGraphics.GLObjects.Textures;

namespace StudioSB.Scenes.Melee
{
    public class SBHsdMaterial : ISBMaterial
    {
        private HSD_MOBJ _mobj;

        public string Name { get; set; }
        public string Label { get; set; }

        public Dictionary<GXTexMapID, Texture> mapIdToAnimSurface = new Dictionary<GXTexMapID, Texture>();

        public SBHsdMaterial(HSD_DOBJ dobj)
        {
            _mobj = dobj.Mobj;

            /*if (_mobj.Textures != null)
                foreach (var tex in _mobj.Textures.List)
                    Console.WriteLine(tex.Flags.ToString());*/
        }

        public void Bind(SBScene scene, Shader shader)
        {
            int TextureUnit = 1;
            shader.SetTexture("uvPattern", DefaultTextures.Instance.uvPattern, TextureUnit++);

            if (_mobj == null)
                return;

            if (_mobj.RenderFlags.HasFlag(RENDER_MODE.XLU))
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }

            shader.SetBoolToInt("hasDF", _mobj.RenderFlags.HasFlag(RENDER_MODE.DF_NONE));
            shader.SetBoolToInt("hasDiffuseMaterial", _mobj.RenderFlags.HasFlag(RENDER_MODE.DIFFSE_MAT));
            shader.SetBoolToInt("enableDiffuseLighting", _mobj.RenderFlags.HasFlag(RENDER_MODE.DIFFUSE) || _mobj.RenderFlags.HasFlag(RENDER_MODE.DIFFSE_VTX));
            shader.SetBoolToInt("enableDiffuseVertex", _mobj.RenderFlags.HasFlag(RENDER_MODE.DIFFSE_VTX));
            shader.SetBoolToInt("enableSpecular", _mobj.RenderFlags.HasFlag(RENDER_MODE.SPECULAR));

            shader.SetFloat("glossiness", 0);
            shader.SetFloat("transparency", 1);
            shader.SetVector4("ambientColor", Vector4.Zero);
            shader.SetVector4("diffuseColor", Vector4.One);
            shader.SetVector4("specularColor", Vector4.One);

            shader.SetInt("TEX0Flag", _mobj.RenderFlags.HasFlag(RENDER_MODE.TEX0) ? 1 : 0);

            if (_mobj.Material != null)
            {
                var matcol = _mobj.Material;

                shader.SetFloat("glossiness", matcol.Shininess);
                shader.SetFloat("transparency", matcol.Alpha);

                shader.SetVector4("ambientColor", matcol.AMB_R / 255f, matcol.AMB_G / 255f, matcol.AMB_B / 255f, matcol.AMB_A / 255f);
                shader.SetVector4("diffuseColor", matcol.DIF_R / 255f, matcol.DIF_G / 255f, matcol.DIF_B / 255f, matcol.DIF_A / 255f);
                shader.SetVector4("specularColor", matcol.SPC_R / 255f, matcol.SPC_G / 255f, matcol.SPC_B / 255f, matcol.SPC_A / 255f);
            }

            bool hasDiffuse = false;
            bool hasSpecular = false;
            bool hasExt = false;
            bool hasBumpMap = false;

            if (_mobj.Textures != null)
            {
                foreach (var texture in _mobj.Textures.List)
                {
                    var rTexture = ((HSDScene)scene).TOBJtoRenderTexture(texture);

                    int coordType = 0;
                    if (texture.Flags.HasFlag(TOBJ_FLAGS.COORD_REFLECTION))
                        coordType = 1;

                    if (mapIdToAnimSurface.ContainsKey(texture.TexMapID))
                        rTexture = mapIdToAnimSurface[texture.TexMapID];

                    var texScale = new Vector2(texture.WScale, texture.HScale);

                    // bugged
                    GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GXtoGL.GLWrapMode(texture.WrapS));
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GXtoGL.GLWrapMode(texture.WrapT));

                    rTexture.TextureWrapS = GXtoGL.GLWrapMode(texture.WrapS);
                    rTexture.TextureWrapT = GXtoGL.GLWrapMode(texture.WrapT);

                    if (texture.Flags.HasFlag(TOBJ_FLAGS.LIGHTMAP_DIFFUSE))
                    {
                        hasDiffuse = true;
                        shader.SetInt("diffuseCoordType", coordType);
                        shader.SetVector2("diffuseScale", texScale);
                        shader.SetFloat("diffuseBlending", texture.Blending);
                        shader.SetBoolToInt("hasDiffuseAlphaBlend", texture.Flags.HasFlag(TOBJ_FLAGS.ALPHAMAP_BLEND) || texture.Flags.HasFlag(TOBJ_FLAGS.ALPHAMAP_REPLACE));
                        shader.SetTexture("diffuseTex", rTexture, TextureUnit++);
                    }
                    if (texture.Flags.HasFlag(TOBJ_FLAGS.LIGHTMAP_SPECULAR))
                    {
                        hasSpecular = true;
                        shader.SetInt("specularCoordType", coordType);
                        shader.SetVector2("specularScale", texScale);
                        shader.SetTexture("specularTex", rTexture, TextureUnit++);
                    }
                    if (texture.Flags.HasFlag(TOBJ_FLAGS.LIGHTMAP_EXT))
                    {
                        hasExt = true;
                        shader.SetInt("extCoordType", coordType);
                        shader.SetVector2("extScale", texScale);
                        shader.SetTexture("extTex", rTexture, TextureUnit++);
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
            }

            shader.SetBoolToInt("hasDiffuse", hasDiffuse);
            shader.SetBoolToInt("hasSpecular", hasSpecular);
            shader.SetBoolToInt("hasBumpMap", hasBumpMap);
            shader.SetBoolToInt("hasExt", hasExt);
        }

        public void ExportMaterial(string FileName)
        {
        }

        public void ImportMaterial(string FileName)
        {
        }

        public void AnimateParam(string ParamName, object Value)
        {
            GXTexMapID mapid;
            if (Enum.TryParse<GXTexMapID>(ParamName, out mapid) && Value is Texture tex)
            {
                if(!mapIdToAnimSurface.ContainsKey(mapid))
                    mapIdToAnimSurface.Add(mapid, tex);
            }
        }

        public void ClearAnimations()
        {
            mapIdToAnimSurface.Clear();
        }
    }
}
