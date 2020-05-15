using OpenTK;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace StudioSB
{
    public enum SettingsGroupType
    {
        Misc,
        Material,
        Lighting,
        Skeleton,
        Application, // cannot be editor when application is running
        Viewport,
    }
    public enum RenderMode
    {
        Shaded,
        Basic,
        Col,
        Prm,
        Nor,
        Emi,
        BakeLit,
        Gao,
        Proj,
        ColorSet1,
        ColorSet2,
        ColorSet21,
        ColorSet22,
        ColorSet23,
        ColorSet3,
        ColorSet4,
        ColorSet5,
        ColorSet6,
        ColorSet7,
        Normals,
        Tangents,
        Bitangents,
        BakeUV,
        UVPattern
    }

    public enum TransitionMode
    {
        Ditto,
        Ink,
        Gold,
        Metal
    }

    public class SettingsGroup : Attribute
    {
        public SettingsGroupType Type { get; set; }
        public String Name { get; set; }
        public string Description { get; set; }

        public SettingsGroup(SettingsGroupType Type, string Description = "", string Name = "")
        {
            this.Type = Type;
            this.Description = Description;
            this.Name = Name;
        }
    }

    public class ApplicationSettings
    {
        [SettingsGroup(SettingsGroupType.Misc, "When closing the scene show save warning", "Show Scene Close Warning")]
        public static bool ShowSaveChangesPopup { get; set; } = true;
        
        // Skinning for application

        [SettingsGroup(SettingsGroupType.Application)]
        public static string LastOpenedPath { get; set; } = "";

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color ForegroundColor { get; internal set; } = Color.Ivory;

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color MiddleColor { get; internal set; } = Color.FromArgb(0xFF, 0x5C, 0x70, 0x70);

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color BackgroundColor { get; internal set; } = Color.FromArgb(0xFF, 0x3C, 0x50, 0x50);
        
        [SettingsGroup(SettingsGroupType.Application)]
        public static Color PoppedInColor { get; internal set; } = Color.FromArgb(0xFF, 0x20, 0x30, 0x30);

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color PoppedOutColor { get; internal set; } = Color.FromArgb(0xFF, 0x50, 0x60, 0x60);

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color ButtonColor { get; internal set; } = Color.FromArgb(0xFF, 0x50, 0x60, 0x60);

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color ErrorColor { get; internal set; } = Color.DarkRed;

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color FontColor { get; internal set; } = Color.Ivory;

        [SettingsGroup(SettingsGroupType.Application)]
        public static Color SeletectedToolColor { get; internal set; } = Color.SkyBlue;

        // Settings for Renderer
        [SettingsGroup(SettingsGroupType.Viewport, "Use legacy OpenGL rendering instead of shaders", "Use Legacy Rendering (Very Slow)")]
        public static bool UseLegacyRendering { get; set; } = false;
        
        [SettingsGroup(SettingsGroupType.Material, "Renders the mesh wireframes (not available with legacy rendering)", "Render Wireframes")]
        public static bool EnableWireframe { get; set; } = false;

        [SettingsGroup(SettingsGroupType.Viewport, "Renders the polygon and vertex counts for the scene", "Render Scene Information")]
        public static bool RenderSceneInformation { get; set; } = false;
        
        [SettingsGroup(SettingsGroupType.Viewport, "Enables/Disables the background gradient", "Render Background Gradient")]
        public static bool RenderBackgroundGradient { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Viewport, "Sets whether the grid is currently enabled", "Show the Grid")]
        public static bool EnableGridDisplay { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Viewport, "The size of the grid", "Grid Spacing")]
        public static int GridSize { get; set; } = 5;
        
        [SettingsGroup(SettingsGroupType.Viewport, "Top Background Color in Viewport", "Back Gradient")]
        public static Color BGColor1 { get; set; } = Color.DarkSlateGray;

        [SettingsGroup(SettingsGroupType.Viewport, "Bottom Background Color in Viewport", "Back Gradient")]
        public static Color BGColor2 { get; set; } = Color.DarkGray;

        [SettingsGroup(SettingsGroupType.Viewport, "Color of the viewport grid's lines", "Grid Color")]
        public static Color GridLineColor { get; set; } = Color.Ivory;

        //skeleton
        [SettingsGroup(SettingsGroupType.Skeleton, "Render the bones in the viewport", "Render Bones")]
        public static bool RenderBones { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Skeleton, "Draw bones on top of everything to make them more visible", "Overlay Bones")]
        public static bool BoneOverlay { get; set; } = false;

        [SettingsGroup(SettingsGroupType.Skeleton, "Renders the bone names on top of the bones", "Render Bone Names")]
        public static bool RenderBoneNames { get; set; } = false;

        [SettingsGroup(SettingsGroupType.Skeleton, "", "Enable Compensate Scale")]
        public static bool EnableCompensateScale { get; set; } = false;

        [SettingsGroup(SettingsGroupType.Skeleton, "The color the bones are rendered with", "Bone Color")]
        public static Color BoneColor { get; set; } = Color.Ivory;

        [SettingsGroup(SettingsGroupType.Skeleton, "", "Selected Bone Color")]
        public static Color SelectedBoneColor { get; set; } = Color.Goldenrod;

        // ported from render settings

        [SettingsGroup(SettingsGroupType.Lighting, "Which mode to render with (only availiable with non-legacy rendering)", "Debug Shading")]
        public static RenderMode ShadingMode { get; set; } = RenderMode.Shaded;

        public static bool UseDebugShading { get => ShadingMode != 0; }
        
        [SettingsGroup(SettingsGroupType.Lighting, "Enables diffuse shading", "Enable Diffuse")]
        public static bool EnableDiffuse { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Lighting, "Enables specular shading", "Enable Specular")]
        public static bool EnableSpecular { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Lighting, "Enables emissive shading", "Enable Emission")]
        public static bool EnableEmission { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Lighting, "Enables rim lighting", "Enable Rim Lighting")]
        public static bool EnableRimLighting { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Lighting, "Enables experimental shading", "Enable Experimental Shading")]
        public static bool EnableExperimental { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Material, "Enables the use of normal maps", "Enable Normal Maps")]
        public static bool RenderNormalMaps { get; set; } = true;

        [SettingsGroup(SettingsGroupType.Material, "Enables the rendering of vertex colors", "Enable Vertex Color")]
        public static bool RenderVertexColor { get; set; } = true;
        
        public static Vector4 RenderChannels { get; set; } = new Vector4(1, 1, 1, 1);

        /// <summary>
        /// Skins and applies application setting to the given control.
        /// This should be applied to the creation of all SB controls.
        /// </summary>
        /// <param name="control">The control to skin</param>
        public static void SkinControl(object control)
        {
            if (control is Control con)
            {
                con.BackColor = BackgroundColor;
                con.ForeColor = ForegroundColor;
            }

            if (control is Button button)
            {
                button.BackColor = ButtonColor;
            }

            if (control is TextBox textBox)
            {

            }
        }

        /// <summary>
        /// Loads application settings from file if it exists
        /// Filename is Application.cfg
        /// </summary>
        public static void Init(string path = "ApplicationSettings.cfg")
        {
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(new FileStream("ApplicationSettings.cfg", FileMode.Open)))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (!line.Contains("="))
                            continue;
                        
                        string[] args = line.Split('=');
                        string propertyName = args[0].Trim();
                        string value = args[1].Trim();

                        var property = typeof(ApplicationSettings).GetProperty(propertyName);

                        if (property == null)
                            continue;
                        if (property.PropertyType == typeof(Color) && value.Contains("#"))
                        {
                            Color col = ColorTranslator.FromHtml(value);
                            property.SetValue(null, col);
                        }
                        else
                        {
                            try
                            {
                                var converter = TypeDescriptor.GetConverter(property.PropertyType);
                                if (converter != null)
                                {
                                    property.SetValue(null, converter.ConvertFromString(value));
                                }
                            }
                            catch (NotSupportedException)
                            {

                            }
                        }
                    }
                }
            }
            else
            {
                SaveSettings();
            }
        }

        /// <summary>
        /// Saves the application settings to a file
        /// </summary>
        public static void SaveSettings(string path = "ApplicationSettings.cfg")
        {
            using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                foreach(var prop in typeof(ApplicationSettings).GetProperties())
                {
                    if (prop.GetCustomAttribute(typeof(SettingsGroup)) == null) continue;
                    if(prop.PropertyType == typeof(Color))
                    {
                        Color col = (Color)prop.GetValue(null);
                        writer.WriteLine($"{prop.Name}=#" + col.ToArgb().ToString("X"));
                    }
                    else
                    {
                        writer.WriteLine($"{prop.Name}={prop.GetValue(null)}");
                    }
                }
            }
        }

        /// <summary>
        /// saves only the last opened filepath
        /// </summary>
        public static void SaveStatic()
        {
            var tempname = Path.GetTempFileName();
            SaveSettings(tempname);
            string templastopen = LastOpenedPath;

            Init();
            LastOpenedPath = templastopen;
            SaveSettings();

            Init(tempname);
            LastOpenedPath = templastopen;
            File.Delete(tempname);
        }

    }
}
