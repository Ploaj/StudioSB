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
                        var attrName = prop.Name;
                        if(attrName != null)
                            NameToProperty.Add(attrName, prop);
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
                                    material.SetProperty(attr.ParamID.ToString(), attr.DataObject.ToString());
                                }else
                                if(prop.PropertyType == typeof(SBMatAttrib<Vector4>))
                                {
                                    material.SetProperty(attr.ParamID.ToString(), MATLVectorToGLVector((MatlAttribute.MatlVector4)attr.DataObject));
                                }else
                                    material.SetProperty(attr.ParamID.ToString(), attr.DataObject);
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
                                    //SBConsole.WriteLine("Extra Param: " + attr.ParamID.ToString() + " = " + attr.DataObject.ToString());
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
