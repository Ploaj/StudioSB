using System;
using System.Collections.Generic;
using SFGraphics.GLObjects.Shaders;

namespace StudioSB.Scenes
{
    /// <summary>
    /// Interface for materials
    /// </summary>
    public interface ISBMaterial
    {
        string Name { get; set; }

        string Label { get; set; }

        void Bind(SBScene scene, Shader shader);

        void AnimateParam(string ParamName, object Value);

        void ExportMaterial(string FileName);

        void ImportMaterial(string FileName);
    }

    /// <summary>
    /// Info about binding a surface
    /// </summary>
    public class DefaultTextureName : Attribute
    {
        public string DefaultTexture;

        public DefaultTextureName(string defaultTexture)
        {
            DefaultTexture = defaultTexture;
        }
    }

    /// <summary>
    /// A generic material attribute for the shader
    /// </summary>
    public class SBMatAttrib<T>
    {
        public string Name { get; set; }
        public T DefaultValue { get; set; }
        public T Value { get
            {
                return _value;
            }
            set
            {
                Used = true;
                _value = value;
                AnimatedValue = value;
            }
        }
        public T AnimatedValue { get; set; }
        private T _value;
        public string Description { get; set; }
        public bool IsColor { get; set; }

        public bool Used { get; set; } = false;

        public SBMatAttrib(string name, T value, string description = "", bool isColor = false)
        {
            Name = name;
            DefaultValue = value;
            _value = DefaultValue;
            AnimatedValue = DefaultValue;
            Description = description;
            IsColor = isColor;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void TryParse(string value)
        {
            //TODO:
        }
    }
}
