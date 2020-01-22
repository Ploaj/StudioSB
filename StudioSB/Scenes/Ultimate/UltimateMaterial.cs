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

        [DisplayName("Rasterizer State")]
        public MatlAttribute.MatlRasterizerState RasterizerState { get; set; } = new MatlAttribute.MatlRasterizerState();

        [DisplayName("Blend State")]
        public MatlAttribute.MatlBlendState BlendState { get; set; } = new MatlAttribute.MatlBlendState();
        
        #region Vectors
        
        public SBMatAttrib<Vector4> CustomVector0 { get; } = new SBMatAttrib<Vector4>("CustomVector0", new Vector4(0), description: "Alpha offset.");
        
        public SBMatAttrib<Vector4> CustomVector3 { get; } = new SBMatAttrib<Vector4>("CustomVector3", new Vector4(1), description: "Some sort of emission color.");
        
        public SBMatAttrib<Vector4> CustomVector6 { get; } = new SBMatAttrib<Vector4>("CustomVector6", new Vector4(1, 1, 0, 0), description: "UV Transform layer 1 for emissive map layer 1");
        
        public SBMatAttrib<Vector4> CustomVector8 { get; } = new SBMatAttrib<Vector4>("CustomVector8", new Vector4(1), description: "Diffuse color multiplier?");

        [Browsable(false)]
        public SBMatAttrib<Vector4> CustomVector11 { get; } = new SBMatAttrib<Vector4>("CustomVector11", new Vector4(0), description: "Some sort of skin subsurface color", isColor: true);

        [Category("Colors"), DisplayName("CustomVector11"), Description("Some sort of skin subsurface color")]
        public Color paramA3View
        {
            get => Color.FromArgb((byte)(CustomVector11.Value.W * 255), (byte)(CustomVector11.Value.X * 255), (byte)(CustomVector11.Value.Y * 255), (byte)(CustomVector11.Value.Z* 255));
            set => CustomVector11.Value = new Vector4(value.R / 255f, value.B / 255f, value.G / 255f, value.A / 255f);
        } 
        
        public SBMatAttrib<Vector4> CustomVector13 { get; } = new SBMatAttrib<Vector4>("CustomVector13", new Vector4(1), description: "Diffuse color multiplier?");
        
        public SBMatAttrib<Vector4> CustomVector14 { get; } = new SBMatAttrib<Vector4>("CustomVector14", new Vector4(1), description: "Assume no edge lighting if not present.");
        
        public SBMatAttrib<Vector4> CustomVector18 { get; } = new SBMatAttrib<Vector4>("CustomVector18", new Vector4(1), description: "Sprite sheet UV parameters.");
        
        public SBMatAttrib<Vector4> CustomVector30 { get; } = new SBMatAttrib<Vector4>("CustomVector30", new Vector4(1, 0, 0, 0), description: "");
        
        public SBMatAttrib<Vector4> CustomVector31 { get; } = new SBMatAttrib<Vector4>("CustomVector31", new Vector4(1, 1, 0, 0), description: "UV transform");
        
        public SBMatAttrib<Vector4> CustomVector32 { get; } = new SBMatAttrib<Vector4>("CustomVector32", new Vector4(1, 1, 0, 0), description: "UV transform");
        
        public SBMatAttrib<Vector4> CustomVector42 { get; } = new SBMatAttrib<Vector4>("CustomVector42", new Vector4(0), description: "");

        public bool hasCustomVector42 { get => CustomVector42.Used; }
        
        public SBMatAttrib<Vector4> CustomVector44 { get; } = new SBMatAttrib<Vector4>("CustomVector44", new Vector4(0), description: "Wii Fit trainer stage color.");

        public bool hasCustomVector44 { get => CustomVector44.Used; }
        
        public SBMatAttrib<Vector4> CustomVector45 { get; } = new SBMatAttrib<Vector4>("CustomVector45", new Vector4(0), description: "Wii Fit trainer stage color.");
        
        public SBMatAttrib<Vector4> CustomVector47 { get; } = new SBMatAttrib<Vector4>("CustomVector47", new Vector4(0), description: "");

        public bool hasCustomVector47 { get => CustomVector47.Used; }

        #endregion

        #region Booleans
        public SBMatAttrib<bool> CustomBoolean1 { get; } = new SBMatAttrib<bool>("CustomBoolean1", false, description: "Enables/disables specular occlusion");
        
        public SBMatAttrib<bool> CustomBoolean2 { get; } = new SBMatAttrib<bool>("CustomBoolean2", true, description: "");

        public SBMatAttrib<bool> CustomBoolean5 { get; } = new SBMatAttrib<bool>("CustomBoolean5", false, description: "Enables/disables UV scrolling animations");

        public SBMatAttrib<bool> CustomBoolean6 { get; } = new SBMatAttrib<bool>("CustomBoolean6", false, description: "Enables/disables UV scrolling animations");
        
        public SBMatAttrib<bool> CustomBoolean7 { get; } = new SBMatAttrib<bool>("CustomBoolean7", true, description: "Enables/disables UV scrolling animations");
        
        public SBMatAttrib<bool> CustomBoolean9 { get; } = new SBMatAttrib<bool>("CustomBoolean9", true, description: "Some sort of sprite sheet scale toggle");

        #endregion

        #region Floats

        public SBMatAttrib<float> CustomFloat4 { get; } = new SBMatAttrib<float>("CustomFloat4", 0.0f, description: "");

        public SBMatAttrib<float> CustomFloat8 { get; } = new SBMatAttrib<float>("CustomFloat8", 0.0f, description: "Controls specular IOR");
        
        public SBMatAttrib<float> CustomFloat10 { get; } = new SBMatAttrib<float>("CustomFloat10", 0.0f, description: "Controls anisotropic specular");
        
        public SBMatAttrib<float> CustomFloat19 { get; } = new SBMatAttrib<float>("CustomFloat19", 0.0f, description: "");

        #endregion

        #region Textures

        [DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> Texture0 { get; } = new SBMatAttrib<string>("colMap", "", description: "Color Map");
        public bool hasColMap { get => Texture0.Used; }
        
        [DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> Texture1 { get; } = new SBMatAttrib<string>("col2Map", "", description: "Color Map 2");
        public bool hasCol2Map { get => Texture1.Used; }

        [DefaultTextureName("defaultBlackCube")]
        public SBMatAttrib<string> Texture2 { get; } = new SBMatAttrib<string>("irrCubemap", "", description: "Irradiance Cubemap");
        public bool hasIrrCubemap { get => Texture2.Used; }

        [DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> Texture3 { get; } = new SBMatAttrib<string>("gaoMap", "", description: "GAO Map");

        [DefaultTextureName("defaultNormal")]
        public SBMatAttrib<string> Texture4 { get; } = new SBMatAttrib<string>("norMap", "", description: "Normal Map");

        [DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> Texture5 { get; } = new SBMatAttrib<string>("emiMap", "", description: "Emission Map");
        public bool hasEmi { get => Texture5 != null; }

        [DefaultTextureName("defaultPrm")]
        public SBMatAttrib<string> Texture6 { get; } = new SBMatAttrib<string>("prmMap", "", description: "PRM Map");
        public bool hasPrmMap { get => Texture6.Used; }

        [DefaultTextureName("defaultSpecCube")]
        public SBMatAttrib<string> Texture7 { get; } = new SBMatAttrib<string>("specularPbrCube", "", description: "Specular PBR Map");

        [DefaultTextureName("defaultBlackCube")]
        public SBMatAttrib<string> Texture8 { get; } = new SBMatAttrib<string>("difCubeMap", "", description: "Diffuse Cube Map");
        public bool hasDifCubeMap { get => Texture8.Used; }

        [DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> Texture9 { get; } = new SBMatAttrib<string>("bakeLitMap", "", description: "Bake Light Map");

        [DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> Texture10 { get; } = new SBMatAttrib<string>("difMap", "", description: "Diffuse Map");
        public bool hasDiffuse { get => Texture10.Used; }
        
        [DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> Texture11 { get; } = new SBMatAttrib<string>("dif2Map", "", description: "Diffuse Map 2nd Layer");
        public bool hasDiffuse2 { get => Texture11.Used; }
        
        [DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> Texture12 { get; } = new SBMatAttrib<string>("dif3Map", "", description: "Diffuse Map 3rd Layer");
        public bool hasDiffuse3 { get => Texture12.Used; }

        [DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> Texture13 { get; } = new SBMatAttrib<string>("projMap", "", description: "Projection Map");

        [DefaultTextureName("defaultBlack")]
        public SBMatAttrib<string> Texture14 { get; } = new SBMatAttrib<string>("emi2Map", "", description: "Emission Map 2");
        public bool hasEmi2 { get => Texture14 != null; }

        [DefaultTextureName("defaultWhite")]
        public SBMatAttrib<string> Texture16 { get; } = new SBMatAttrib<string>("inkNorMap", "", description: "Ink Normal Map");
        public bool hasInkNorMap { get => Texture16.Used; }

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

        public void SetProperty(string name, object value)
        {
            if (!paramNameToPropertyName.ContainsKey(name))
                return;

            if (value is bool b)
                ((SBMatAttrib<bool>)MatAttribs[paramNameToPropertyName[name]]).Value = b;
            if (value is Vector4 vec)
                ((SBMatAttrib<Vector4>)MatAttribs[paramNameToPropertyName[name]]).Value = vec;
            if (value is int i)
                ((SBMatAttrib<int>)MatAttribs[paramNameToPropertyName[name]]).Value = i;
            if (value is float f)
                ((SBMatAttrib<float>)MatAttribs[paramNameToPropertyName[name]]).Value = f;
            if (value is string s)
                ((SBMatAttrib<string>)MatAttribs[paramNameToPropertyName[name]]).Value = s;
        }

        public UltimateMaterial()
        {
            foreach(var prop in Properties)
            {
                var paramname = prop.Name;
                if (paramname != null)
                    paramNameToPropertyName.Add(paramname, prop.Name);
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
                if (p.PropertyType == typeof(SBMatAttrib<string>))
                {
                    var texName = ((SBMatAttrib<string>)MatAttribs[p.Name]).Name;
                    if (shader.GetUniformLocation(texName) == -1)
                        continue;
                    var value = ((SBMatAttrib<string>)MatAttribs[p.Name]).AnimatedValue.ToLower();
                    SBSurface surface = ssbhScene.GetSurfaceFromName(value);
                    var surfaceInfo = nameToDefaultTexture[p.Name];
                    BindSurface(shader, ssbhScene, surface, surfaceInfo, texName, TextureUnit++);
                }
                if (shader.GetUniformLocation(p.Name) != -1 || shader.GetAttribLocation(p.Name) != -1)
                {
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

            shader.SetTexture("diffusePbrCube", DefaultTextures.Instance.diffusePbr, TextureUnit++);
            shader.SetTexture("iblLut", DefaultTextures.Instance.iblLut, TextureUnit++);
            shader.SetTexture("uvPattern", DefaultTextures.Instance.uvPattern, TextureUnit++);
        }

        /// <summary>
        /// Sets Blend State information
        /// </summary>
        private void BindBlendState()
        {
            BlendingFactor src = BlendingFactor.One;
            BlendingFactor dst = BlendingFactor.OneMinusSrcAlpha;

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
        }

        /// <summary>
        /// Sets Rasterizer State information
        /// </summary>
        private void BindRasterizerState()
        {
            MaterialFace mf = MaterialFace.Back;
            PolygonMode pm = PolygonMode.Fill;
            CullFaceMode cullMode = CullFaceMode.Back;

            GL.Enable(EnableCap.CullFace);

            if (RasterizerState != null)
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
                        cullMode = CullFaceMode.Front;
                        break;
                    case 2:
                        GL.Disable(EnableCap.CullFace);
                        mf = MaterialFace.FrontAndBack;
                        cullMode = CullFaceMode.FrontAndBack;
                        break;
                }
            }

            GL.CullFace(cullMode);
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
                shader.SetTexture(attributeName, surface.GetRenderTexture(), TextureUnit);
            else
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

        public void ClearAnimations()
        {
            
        }
    }
}
