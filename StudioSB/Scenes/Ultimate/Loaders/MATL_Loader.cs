using SSBHLib;
using SSBHLib.Formats.Materials;
using System;
using OpenTK;
using System.Collections.Generic;
using System.Reflection;

namespace StudioSB.Scenes.Ultimate
{
    public class MATL_Loader
    {
        /// <summary>
        /// Load an ultimate mesh from file to the given scene 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Scene"></param>
        public static void Open(string FileName, SBScene Scene)
        {
            SsbhFile File;
            if (Ssbh.TryParseSsbhFile(FileName, out File))
            {
                if (File is Matl matl)
                {
                    if (matl.MajorVersion != 1 && matl.MinorVersion != 6)
                        SBConsole.WriteLine($"Warning: Mesh Version {matl.MajorVersion}.{matl.MinorVersion} not supported");

                    var MaterialProps = typeof(UltimateMaterial).GetProperties();
                    // use dictionary for faster lookup
                    Dictionary<string, PropertyInfo> NameToProperty = new Dictionary<string, PropertyInfo>();
                    foreach (var prop in MaterialProps)
                    {
                        var attrName = prop.Name;
                        if(attrName != null)
                            NameToProperty.Add(attrName, prop);
                    }

                    foreach(var entry in matl.Entries)
                    {
                        UltimateMaterial material = new UltimateMaterial();
                        material.Name = entry.ShaderLabel;
                        material.Label = entry.MaterialLabel;

                        Scene.Materials.Add(material);

                        foreach(var attr in entry.Attributes)
                        {
                            if (NameToProperty.ContainsKey(attr.ParamId.ToString()))
                            {
                                var prop = NameToProperty[attr.ParamId.ToString()];
                                if (prop.PropertyType == typeof(SBMatAttrib<string>))
                                {
                                    material.SetProperty(attr.ParamId.ToString(), attr.DataObject.ToString());
                                }else
                                if(prop.PropertyType == typeof(SBMatAttrib<Vector4>))
                                {
                                    material.SetProperty(attr.ParamId.ToString(), MATLVectorToGLVector((MatlAttribute.MatlVector4)attr.DataObject));
                                }else
                                    material.SetProperty(attr.ParamId.ToString(), attr.DataObject);
                            }
                            else
                            switch (attr.ParamId)
                            {
                                case MatlEnums.ParamId.RasterizerState0:
                                    material.RasterizerState = (MatlAttribute.MatlRasterizerState)attr.DataObject;
                                    break;
                                case MatlEnums.ParamId.BlendState0:
                                    material.BlendState = (MatlAttribute.MatlBlendState)attr.DataObject;
                                    break;
                                    case MatlEnums.ParamId.Sampler0:
                                    case MatlEnums.ParamId.Sampler1:
                                    case MatlEnums.ParamId.Sampler2:
                                    case MatlEnums.ParamId.Sampler3:
                                    case MatlEnums.ParamId.Sampler4:
                                    case MatlEnums.ParamId.Sampler5:
                                    case MatlEnums.ParamId.Sampler6:
                                    case MatlEnums.ParamId.Sampler7:
                                    case MatlEnums.ParamId.Sampler8:
                                    case MatlEnums.ParamId.Sampler9:
                                    case MatlEnums.ParamId.Sampler10:
                                    case MatlEnums.ParamId.Sampler11:
                                    case MatlEnums.ParamId.Sampler12:
                                    case MatlEnums.ParamId.Sampler13:
                                    case MatlEnums.ParamId.Sampler14:
                                    case MatlEnums.ParamId.Sampler15:
                                    case MatlEnums.ParamId.Sampler16:
                                    case MatlEnums.ParamId.Sampler17:
                                        // TODO: rework texture loading system
                                        material.TextureToSampler.Add(attr.ParamId.ToString().Replace("Sampler", "Texture"), (MatlAttribute.MatlSampler)attr.DataObject);
                                        break;
                                    default:
                                    //SBConsole.WriteLine("Extra Param: " + attr.ParamID.ToString() + " = " + attr.DataObject.ToString());
                                    material.extraParams.Add(attr.ParamId, attr.DataObject);
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
