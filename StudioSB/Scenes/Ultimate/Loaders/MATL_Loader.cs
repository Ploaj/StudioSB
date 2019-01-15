using SSBHLib;
using SSBHLib.Formats.Materials;
using System;
using OpenTK;
using System.Collections.Generic;
using System.Reflection;

namespace StudioSB.Scenes.Ultimate
{
    /// <summary>
    /// Used to make the names used when reading/writing a mat file
    /// </summary>
    public class MATLLoaderAttributeName : Attribute
    {
        public string Name { get; set; }

        public MATLLoaderAttributeName(string name)
        {
            Name = name;
        }
    }

    public class MATL_Loader
    {
        /// <summary>
        /// Load an ultimate mesh from file to the given scene 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Scene"></param>
        public static void Open(string FileName, SBScene Scene)
        {
            ISSBH_File File;
            if (SSBH.TryParseSSBHFile(FileName, out File))
            {
                if (File is MATL matl)
                {
                    if (matl.MajorVersion != 1 && matl.MinorVersion != 6)
                        SBConsole.WriteLine($"Warning: Mesh Version {matl.MajorVersion}.{matl.MinorVersion} not supported");

                    var MaterialProps = typeof(UltimateMaterial).GetProperties();
                    // use dictionary for faster lookup
                    Dictionary<string, PropertyInfo> NameToProperty = new Dictionary<string, PropertyInfo>();
                    foreach (var prop in MaterialProps)
                    {
                        var attrName = (MATLLoaderAttributeName)prop.GetCustomAttribute(typeof(MATLLoaderAttributeName));
                        if(attrName != null)
                            NameToProperty.Add(attrName.Name, prop);
                    }

                    foreach(var entry in matl.Entries)
                    {
                        UltimateMaterial material = new UltimateMaterial();
                        material.Name = entry.MaterialName;
                        material.Label = entry.MaterialLabel;

                        Scene.Materials.Add(material);

                        foreach(var attr in entry.Attributes)
                        {
                            if (NameToProperty.ContainsKey(attr.ParamID.ToString()))
                            {
                                var prop = NameToProperty[attr.ParamID.ToString()];
                                if (prop.PropertyType == typeof(SBMatAttrib<string>))
                                {
                                    ((SBMatAttrib<string>)prop.GetValue(material)).Value = attr.DataObject.ToString();
                                    //foreach (var surface in Scene.Surfaces)
                                    //    if (surface.Name == attr.DataObject.ToString())
                                    //        prop.SetValue(material, surface);
                                }
                                if(prop.PropertyType == typeof(SBMatAttrib<Vector4>))
                                {
                                    ((SBMatAttrib<Vector4>)prop.GetValue(material)).Value = MATLVectorToGLVector((MatlAttribute.MatlVector4)attr.DataObject);
                                }
                                if (prop.PropertyType == typeof(SBMatAttrib<float>))
                                {
                                    ((SBMatAttrib<float>)prop.GetValue(material)).Value = (float)attr.DataObject;
                                }
                                if (prop.PropertyType == typeof(SBMatAttrib<bool>))
                                {
                                    ((SBMatAttrib<bool>)prop.GetValue(material)).Value = (bool)attr.DataObject;
                                }
                            }
                            else
                            switch (attr.ParamID)
                            {
                                case MatlEnums.ParamId.RasterizerState0:
                                    material.RasterizerState = (MatlAttribute.MatlRasterizerState)attr.DataObject;
                                    break;
                                case MatlEnums.ParamId.BlendState0:
                                    material.BlendState = (MatlAttribute.MatlBlendState)attr.DataObject;
                                    break;
                                default:
                                        SBConsole.WriteLine("Extra Param: " + attr.ParamID.ToString() + " = " + attr.DataObject.ToString());
                                    material.extraParams.Add(attr.ParamID, attr.DataObject);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private static Vector4 MATLVectorToGLVector(MatlAttribute.MatlVector4 vector)
        {
            return new Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

    }
}
