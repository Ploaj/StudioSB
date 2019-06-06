using SFGraphics.GLObjects.Shaders;
using System.Reflection;
using OpenTK;
using System.Collections.Generic;
using StudioSB.Rendering;
using SSBHLib.Formats.Materials;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using System.Drawing;

namespace StudioSB.Scenes.Ultimate
{
    public class UltimateMaterial : ISBMaterial
    {
        [Category("Material Info"), Description("Name used for linking to Mesh Objects")]
        public string Name { get; set; }

        [Category("Material Info"), Description("Label used for material animations and various misc")]
        public string Label { get; set; }

        /*private Dictionary<string, object> AnimatedParams = new Dictionary<string, object>();
        private Dictionary<string, float> FloatParams = new Dictionary<string, float>();
        private Dictionary<string, Vector4> VectorParams = new Dictionary<string, Vector4>();
        private Dictionary<string, bool> BoolParams = new Dictionary<string, bool>();
        private Dictionary<string, int> IntParams = new Dictionary<string, int>();*/
        
        [DisplayName("Rasterizer State")]
        public MatlAttribute.MatlRasterizerState RasterizerState { get; set; } = new MatlAttribute.MatlRasterizerState();

        [DisplayName("Blend State")]
        public MatlAttribute.MatlBlendState BlendState { get; set; } = new MatlAttribute.MatlBlendState();
        
        #region Vectors
        
        [MATLLoaderAttributeName("CustomVector0")]
        public SBMatAttrib<Vector4> param98 { get; } = new SBMatAttrib<Vector4>("CustomVector0", new Vector4(0), description: "Alpha offset.");

        [MATLLoaderAttributeName("CustomVector3")]
        public SBMatAttrib<Vector4> param9B { get; } = new SBMatAttrib<Vector4>("CustomVector3", new Vector4(1), description: "Some sort of emission color.");

        [MATLLoaderAttributeName("CustomVector6")]
        public SBMatAttrib<Vector4> param9E { get; } = new SBMatAttrib<Vector4>("CustomVector6", new Vector4(1, 1, 0, 0), description: "UV Transform layer 1 for emissive map layer 1");

        [MATLLoaderAttributeName("CustomVector8")]
        public SBMatAttrib<Vector4> paramA0 { get; } = new SBMatAttrib<Vector4>("CustomVector8", new Vector4(1), description: "Diffuse color multiplier?");

        [Browsable(false), MATLLoaderAttributeName("CustomVector11")]
        public SBMatAttrib<Vector4> paramA3 { get; } = new SBMatAttrib<Vector4>("CustomVector11", new Vector4(0), description: "Some sort of skin subsurface color", isColor: true);

        [Category("Colors"), DisplayName("CustomVector11"), Description("Some sort of skin subsurface color")]
        public Color paramA3View
        {
            get => Color.FromArgb((byte)(paramA3.Value.W * 255), (byte)(paramA3.Value.X * 255), (byte)(paramA3.Value.Y * 255), (byte)(paramA3.Value.Z* 255));
            set => paramA3.Value = new Vector4(value.R / 255f, value.B / 255f, value.G / 255f, value.A / 255f);
        } 

        [MATLLoaderAttributeName("CustomVector13")]
        public SBMatAttrib<Vector4> paramA5 { get; } = new SBMatAttrib<Vector4>("CustomVector13", new Vector4(1), description: "Diffuse color multiplier?");

        [MATLLoaderAttributeName("CustomVector14")]
        public SBMatAttrib<Vector4> paramA6 { get; } = new SBMatAttrib<Vector4>("CustomVector14", new Vector4(0), description: "Assume no edge lighting if not present.");

        [MATLLoaderAttributeName("CustomVector18")]
        public SBMatAttrib<Vector4> paramAA { get; } = new SBMatAttrib<Vector4>("CustomVector18", new Vector4(1), description: "Sprite sheet UV parameters.");

        [MATLLoaderAttributeName("CustomVector30")]
        public SBMatAttrib<Vector4> param145 { get; } = new SBMatAttrib<Vector4>("CustomVector30", new Vector4(0, 0, 0, 0), description: "");

        [MATLLoaderAttributeName("CustomVector31")]
        public SBMatAttrib<Vector4> param146 { get; } = new SBMatAttrib<Vector4>("CustomVector31", new Vector4(1, 1, 0, 0), description: "UV transform");

        [MATLLoaderAttributeName("CustomVector32")]
        public SBMatAttrib<Vector4> param147 { get; } = new SBMatAttrib<Vector4>("CustomVector32", new Vector4(1, 1, 0, 0), description: "UV transform");

        [MATLLoaderAttributeName("CustomVector42")]
        public SBMatAttrib<Vector4> param151 { get; } = new SBMatAttrib<Vector4>("CustomVector42", new Vector4(0), description: "");

        public bool hasParam151 { get => param151.Used; }

        [MATLLoaderAttributeName("CustomVector44")]
        public SBMatAttrib<Vector4> param153 { get; } = new SBMatAttrib<Vector4>("CustomVector44", new Vector4(0), description: "Wii Fit trainer stage color.");

        public bool hasParam153 { get => param153.Used; }

        [MATLLoaderAttributeName("CustomVector45")]
        public SBMatAttrib<Vector4> param154 { get; } = new SBMatAttrib<Vector4>("CustomVector45", new Vector4(0), description: "Wii Fit trainer stage color.");

        [MATLLoaderAttributeName("CustomVector47")]
        public SBMatAttrib<Vector4> param156 { get; } = new SBMatAttrib<Vector4>("CustomVector47", new Vector4(0), description: "");

        public bool hasParam156 { get => param156.Used; }

        #endregion

        #region Booleans
        [MATLLoaderAttributeName("CustomBoolean1")]
        public SBMatAttrib<bool> paramE9 { get; } = new SBMatAttrib<bool>("CustomBoolean1", true, description: "Enables/disables specular occlusion");

        [MATLLoaderAttributeName("CustomBoolean2")]
        public SBMatAttrib<bool> paramEA { get; } = new SBMatAttrib<bool>("CustomBoolean2", true, description: "");

        [MATLLoaderAttributeName("CustomBoolean6")]
        public SBMatAttrib<bool> paramEE { get; } = new SBMatAttrib<bool>("CustomBoolean6", true, description: "Enables/disables UV scrolling animations");

        [MATLLoaderAttributeName("CustomBoolean7")]
        public SBMatAttrib<bool> paramEF { get; } = new SBMatAttrib<bool>("CustomBoolean7", true, description: "Enables/disables UV scrolling animations");

        [MATLLoaderAttributeName("CustomBoolean9")]
        public SBMatAttrib<bool> paramF1 { get; } = new SBMatAttrib<bool>("CustomBoolean9", true, description: "Some sort of sprite sheet scale toggle");

        #endregion

        #region Floats

        [MATLLoaderAttributeName("CustomFloat8")]
        public SBMatAttrib<float> paramC8 { get; } = new SBMatAttrib<float>("CustomFloat8", 0.0f, description: "Controls specular IOR");

        [MATLLoaderAttributeName("CustomFloat10")]
        public SBMatAttrib<float> paramCA { get; } = new SBMatAttrib<float>("CustomFloat10", 0.0f, description: "Controls anisotropic specular");

        #endregion

        #region Textures

        [MATLLoaderAttributeName("Texture0"), DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> colMap { get; } = new SBMatAttrib<string>("Texture0", "");

        public bool hasColMap { get => colMap.Used; }
        
        [MATLLoaderAttributeName("Texture1"), DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> col2Map { get; } = new SBMatAttrib<string>("Texture1", "");

        public bool hasCol2Map { get => col2Map.Used; }
        
        [MATLLoaderAttributeName("Texture6"), DefaultTextureName("defaultPrm")]
        public SBMatAttrib<string> prmMap { get; } = new SBMatAttrib<string>("Texture6", "");

        public bool hasPrmMap { get => prmMap.Used; }
        
        [MATLLoaderAttributeName("Texture4"), DefaultTextureName("defaultNormal")]
        public SBMatAttrib<string> norMap { get; } = new SBMatAttrib<string>("Texture4", "");

        [MATLLoaderAttributeName("Texture5"), DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> emiMap { get; } = new SBMatAttrib<string>("Texture5", "");
        public bool hasEmi { get => emiMap != null; }
    
        [MATLLoaderAttributeName("Texture14"), DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> emi2Map { get; } = new SBMatAttrib<string>("Texture14", "");
        public bool hasEmi2 { get => emi2Map!= null; }
        
        [MATLLoaderAttributeName("Texture9"), DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> bakeLitMap { get; } = new SBMatAttrib<string>("Texture9", "");

        [MATLLoaderAttributeName("Texture3"), DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> gaoMap { get; } = new SBMatAttrib<string>("Texture3", "");

        [MATLLoaderAttributeName("Texture16"), DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> inkNorMap { get; } = new SBMatAttrib<string>("Texture16", "");
        public bool hasInkNorMap { get => inkNorMap.Used; }
        
        [MATLLoaderAttributeName("Texture8"), DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> difCubemap { get; } = new SBMatAttrib<string>("Texture8", "");
        public bool hasDifCubemap { get => difCubemap.Used; }
        
        [MATLLoaderAttributeName("Texture10"), DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> difMap { get; } = new SBMatAttrib<string>("Texture10", "");
        public bool hasDiffuse { get => difMap.Used; }
        
        [MATLLoaderAttributeName("Texture11"), DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> dif2Map { get; } = new SBMatAttrib<string>("Texture11", "");
        public bool hasDiffuse2 { get => dif2Map.Used; }
        
        [MATLLoaderAttributeName("Texture12"), DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> dif3Map { get; } = new SBMatAttrib<string>("Texture12", "");
        public bool hasDiffuse3 { get => dif3Map.Used; }

        [MATLLoaderAttributeName("Texture13"), DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> projMap { get; } = new SBMatAttrib<string>("Texture13", "");

        public bool HasBlending { get { return (BlendState.BlendFactor1 != 0 || BlendState.BlendFactor2 != 0); } }

        public bool emissionOverride
        {
            get
            {
                // HACK: There's probably a better way to handle blending emission and base color maps.
                var hasDiffuseMaps = hasColMap || hasCol2Map || hasDiffuse || hasDiffuse2 || hasDiffuse3;
                var hasEmiMaps = hasEmi || hasEmi2;
                return hasEmiMaps && !hasDiffuseMaps;
            }
        }

        #endregion Textures

        public Dictionary<MatlEnums.ParamId, object> extraParams = new Dictionary<MatlEnums.ParamId, object>();
        
        public static PropertyInfo[] Properties = typeof(UltimateMaterial).GetProperties();

        // faster than reflection, but still a bit slow
        private Dictionary<string, object> MatAttribs = new Dictionary<string, object>();
        private Dictionary<string, DefaultTextureName> nameToDefaultTexture = new Dictionary<string, DefaultTextureName>();
        private Dictionary<string, string> paramNameToPropertyName = new Dictionary<string, string>();

        public UltimateMaterial()
        {
            foreach(var prop in Properties)
            {
                var paramname = prop.GetCustomAttribute(typeof(MATLLoaderAttributeName));
                if (paramname != null)
                    paramNameToPropertyName.Add(((MATLLoaderAttributeName)paramname).Name, prop.Name);
                var deftex = prop.GetCustomAttribute(typeof(DefaultTextureName));
                if (deftex != null)
                    nameToDefaultTexture.Add(prop.Name, (DefaultTextureName)(deftex));
                if (prop.PropertyType == typeof(SBMatAttrib<float>) ||
                    prop.PropertyType == typeof(SBMatAttrib<Vector4>) ||
                    prop.PropertyType == typeof(SBMatAttrib<bool>) ||
                    prop.PropertyType == typeof(SBMatAttrib<string>))
                    MatAttribs.Add(prop.Name, prop.GetValue(this));
            }
        }

        /// <summary>
        /// Binds the material to shader
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="shader"></param>
        public void Bind(SBScene scene, Shader shader)
        {
            var ssbhScene = (SBSceneSSBH)scene;

            BindRasterizerState();
            BindBlendState();

            int TextureUnit = 0;
            
            foreach (var p in Properties)
            {
                if(shader.GetUniformLocation(p.Name) != -1 || shader.GetAttribLocation(p.Name) != -1)
                {
                    if(p.PropertyType == typeof(SBMatAttrib<string>))
                    {
                        var value = ((SBMatAttrib<string>)MatAttribs[p.Name]).AnimatedValue;
                        SBSurface surface = ssbhScene.nameToSurface.ContainsKey(value) ? ssbhScene.nameToSurface[value] : null;
                        var surfaceInfo = nameToDefaultTexture[p.Name]; 
                        BindSurface(shader, ssbhScene, surface, surfaceInfo, p.Name, TextureUnit++);
                    }
                    else
                    if (p.PropertyType == typeof(SBMatAttrib<Vector4>))
                        shader.SetVector4(p.Name, ((SBMatAttrib<Vector4>)MatAttribs[p.Name]).AnimatedValue);
                    else
                    if (p.PropertyType == typeof(SBMatAttrib<float>))
                        shader.SetFloat(p.Name, ((SBMatAttrib<float>)MatAttribs[p.Name]).AnimatedValue);
                    else
                    if (p.PropertyType == typeof(SBMatAttrib<bool>))
                        shader.SetBoolToInt(p.Name, ((SBMatAttrib<bool>)MatAttribs[p.Name]).AnimatedValue);
                    else
                    if (p.PropertyType == typeof(bool))
                    {
                        var value = p.GetValue(this); // slow
                        shader.SetBoolToInt(p.Name, (bool)value);
                    }
                }
            }

            shader.SetBoolToInt("hasBlending", HasBlending);

            shader.SetTexture("diffusePbrCube", DefaultTextures.Instance.diffusePbr, TextureUnit++);
            shader.SetTexture("specularPbrCube", DefaultTextures.Instance.specularPbr, TextureUnit++);
            shader.SetTexture("iblLut", DefaultTextures.Instance.iblLut, TextureUnit++);
            shader.SetTexture("uvPattern", DefaultTextures.Instance.uvPattern, TextureUnit++);
        }

        /// <summary>
        /// Sets Blend State information
        /// </summary>
        private void BindBlendState()
        {
            BlendingFactor src = BlendingFactor.One; ;
            BlendingFactor dst = BlendingFactor.One; ;
            if (BlendState.BlendFactor1 == 1)
                src = BlendingFactor.One;
            else if (BlendState.BlendFactor1 == 6)
                src = BlendingFactor.SrcAlpha;

            if (BlendState.BlendFactor2 == 1)
                dst = BlendingFactor.One;
            else if (BlendState.BlendFactor2 == 6)
                dst = BlendingFactor.OneMinusSrcAlpha;

            if (BlendState.BlendFactor1 != 0 || BlendState.BlendFactor2 != 0)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(src, dst);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
                GL.Disable(EnableCap.AlphaTest);
            }
        }

        /// <summary>
        /// Sets Rasterizer State information
        /// </summary>
        private void BindRasterizerState()
        {
            MaterialFace mf = MaterialFace.Back;
            PolygonMode pm = PolygonMode.Fill;

            if(RasterizerState != null)
            {
                switch (RasterizerState.FillMode)
                {
                    case 0:
                        pm = PolygonMode.Line;
                        break;
                    case 1:
                        pm = PolygonMode.Fill;
                        break;
                }
                switch (RasterizerState.CullMode)
                {
                    case 1:
                        mf = MaterialFace.Front;
                        break;
                    case 2:
                        mf = MaterialFace.FrontAndBack;
                        break;
                }
            }

            GL.PolygonMode(mf, pm);
        }

        /// <summary>
        /// Binds a surface to the shader
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="ssbhScene"></param>
        /// <param name="surface"></param>
        /// <param name="surfaceInfo"></param>
        /// <param name="attributeName"></param>
        /// <param name="TextureUnit"></param>
        private void BindSurface(Shader shader, SBSceneSSBH ssbhScene, SBSurface surface, DefaultTextureName surfaceInfo, string attributeName, int TextureUnit)
        {
            if (surface != null)
            {
                if (ssbhScene.surfaceToRenderTexture.ContainsKey(surface))
                {
                    shader.SetTexture(attributeName, ssbhScene.surfaceToRenderTexture[surface], TextureUnit);
                    return;
                }
            }
            shader.SetTexture(attributeName, DefaultTextures.Instance.GetTextureByName(surfaceInfo.DefaultTexture), TextureUnit);

        }

        /// <summary>
        /// Sets the animation of a param by its name
        /// </summary>
        /// <param name="ParamName"></param>
        /// <param name="Value"></param>
        public void AnimateParam(string ParamName, object Value)
        {
            if (!paramNameToPropertyName.ContainsKey(ParamName)) return;

            var attr = MatAttribs[paramNameToPropertyName[ParamName]];

            // only animate vector 4s for now
            if (Value.GetType() == typeof(Vector4) && attr is SBMatAttrib<Vector4> vector)
                vector.AnimatedValue = (Vector4)Value;
        }

        public override string ToString()
        {
            return Label;
        }

        public void ExportMaterial(string FileName)
        {
            //TODO:
        }

        public void ImportMaterial(string FileName)
        {

        }
    }
}
