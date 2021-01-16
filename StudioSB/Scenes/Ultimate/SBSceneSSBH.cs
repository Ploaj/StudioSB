using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using StudioSB.Rendering;
using System.Collections.Generic;
using SSBHLib;
using SSBHLib.Formats;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.BufferObjects;
using StudioSB.IO.Formats;
using System.Linq;
using StudioSB.GUI.Attachments;
using System.ComponentModel;
using StudioSB.Scenes.Ultimate.Loaders;
using IONET.Core.Model;
using IONET.Core;
using StudioSB.GUI;
using System.Drawing.Design;

namespace StudioSB.Scenes.Ultimate
{
    public class SSBHExportSettings
    {
        [DisplayName("Export Mesh and Model (.numshb, .numdlb)")]
        public bool ExportModel { get; set; } = true;

        [DisplayName("Export Mesh Extended (.numshexb)")]
        public bool ExportModelEx { get; set; } = false;

        [DisplayName("Export Skeleton (.nusktb)")]
        public bool ExportSkeleton { get; set; } = true;

        //[DisplayName("Export Materials (.numatb)")]
        //public bool ExportMaterials { get; set; } = false;

        [DisplayName("Export Textures (.nutexb)")]
        public bool ExportTextures { get; set; } = false;

        [DisplayName("Use \"Model\" as FileName")]
        public bool UseModelName { get; set; } = true;

        [Editor(typeof(FilteredFileNameEditor), typeof(UITypeEditor)),
        DisplayName("NUSKTB FilePath (Recommended)"),
        Description("Path to a reference .nusktb for exporting bones in the correct order")]
        public string ReferenceNusktbFile { get; set; } = "";
    }

    public enum UltimateMaterialTransitionMode
    {
        Ditto,
        Ink,
        Gold,
        Metal
    }

    [SceneFileInformation("NU Model File Binary", ".numdlb", "The model specification for Smash Ultimate models", sceneCode: "SSBU")]
    public class SBSceneSSBH : SBScene
    {
        public static SSBHExportSettings ExportSettings { get; } = new SSBHExportSettings();
        public static SBUltimateImportSettings ImportSettings { get; } = new SBUltimateImportSettings();
        public static SBUltimateNewImportSettings NewImportSettings { get; } = new SBUltimateNewImportSettings();
        
        public ISBModel<SBUltimateMesh> Model { get; set; }

        // Rendering
        public Dictionary<SBUltimateMesh, UltimateRenderMesh> sbMeshToRenderMesh = new Dictionary<SBUltimateMesh, UltimateRenderMesh>();
        private Dictionary<string, SBSurface> nameToSurface = new Dictionary<string, SBSurface>();
        private BufferObject boneUniformBuffer;
        private Matrix4[] boneBinds = new Matrix4[200];

        public float MaterialBlend { get; set; } = 0;
        public UltimateMaterialTransitionMode MaterialMode { get; set; } = UltimateMaterialTransitionMode.Metal;

        public SBSceneSSBH()
        {
            HasMesh = true;
            HasBones = true;
            boneUniformBuffer = new BufferObject(BufferTarget.UniformBuffer);
            AttachmentTypes.Add(typeof(SBUltimateSettingsAttachment));
        }

        public SBSurface GetSurfaceFromName(string name)
        {
            if (!nameToSurface.ContainsKey(name))
            {
                if (name == "" || name.Contains("#replace"))
                    return null;
                var search = Surfaces.Find(e => e.Name.ToLower() == name);
                if (search == null)
                {
                    // no surface with this name exists
                    return null;
                }
                else
                {
                    nameToSurface.Clear();
                    foreach (var surface in Surfaces)
                        nameToSurface.Add(surface.Name.ToLower(), surface);
                }
            }
            return nameToSurface[name];
        }

        /// <summary>
        /// Loads the scene from a NUMDLB file
        /// </summary>
        /// <param name="FileName"></param>
        public override void LoadFromFile(string FileName)
        {
            string folderPath = Path.GetDirectoryName(FileName);

            SsbhFile File;
            if (!Ssbh.TryParseSsbhFile(FileName, out File))
                return;

            Modl modl = (Modl)File;

            string meshPath = "";
            string skelPath = "";
            string matlPath = "";
            foreach (string file in Directory.EnumerateFiles(folderPath))
            {
                // load textures
                if (file.EndsWith(".nutexb"))
                {
                    Surfaces.Add(IO_NUTEXB.Open(file));
                }

                string fileName = Path.GetFileName(file);
                if (fileName.Equals(modl.SkeletonFileName))
                {
                    skelPath = file;
                }
                if (fileName.Equals(modl.MeshString))
                {
                    meshPath = file;
                }
                if (fileName.Equals(modl.MaterialFileNames[0].MaterialFileName))
                {
                    matlPath = file;
                }
            }
            
            // import order Skeleton+Textures->Materials->Mesh
            // mesh needs to be loaded after skeleton
            if (skelPath != "")
            {
                SBConsole.WriteLine($"Importing skeleton: {Path.GetFileName(skelPath)}");
                SKEL_Loader.Open(skelPath, this);
            }
            
            if (matlPath != "")
            {
                SBConsole.WriteLine($"Importing materials: {Path.GetFileName(matlPath)}");
                MATL_Loader.Open(matlPath, this);
            }
            
            if (meshPath != "")
            {
                SBConsole.WriteLine($"Importing mesh: {Path.GetFileName(meshPath)}");
                MESH_Loader.Open(meshPath, this);
                
                // set materials
                foreach(var entry in modl.ModelEntries)
                {
                    UltimateMaterial currentMaterial = null;
                    foreach (UltimateMaterial matentry in Materials)
                    {
                        if (matentry.Label.Equals(entry.MaterialLabel))
                        {
                            currentMaterial = matentry;
                            break;
                        }
                    }
                    if (currentMaterial == null)
                        continue;
                    
                    Dictionary<string, int> subindexMatcher = new Dictionary<string, int>();
                    if (Model != null)
                    {
                        foreach (var mesh in Model.Meshes)
                        {
                            if (!subindexMatcher.ContainsKey(mesh.Name))
                                subindexMatcher.Add(mesh.Name, 0);

                            int subindex = subindexMatcher[mesh.Name]++;
                            
                            if (subindex == entry.SubIndex && mesh.Name.Equals(entry.MeshName))
                            {
                                mesh.Material = currentMaterial;
                                break;
                            }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        public override void ExportSceneToFile(string FileName)
        {
            if (Model == null) return;

            using (GUI.SBCustomDialog d = new GUI.SBCustomDialog(ExportSettings))
            {
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
            }

            string name = Path.GetDirectoryName(FileName) + "/" + Path.GetFileNameWithoutExtension(FileName);
            string simpleName = Path.GetFileNameWithoutExtension(FileName);

            if (ExportSettings.UseModelName)
                simpleName = "model";

            if (ExportSettings.ExportModel)
            {
                SBConsole.WriteLine("Creating MODL...");
                var modl = MODL_Loader.CreateMODLFile((SBUltimateModel)Model);
                modl.ModelFileName = simpleName;
                modl.SkeletonFileName = $"{simpleName}.nusktb";
                modl.MeshString = $"{simpleName}.numshb";
                modl.Unk1 = 0;
                modl.MaterialFileNames = new ModlMaterialName[] { new ModlMaterialName() { MaterialFileName = $"{simpleName}.numatb" } };
                SBConsole.WriteLine("Done");
                Ssbh.TrySaveSsbhFile(FileName, modl);

                SBConsole.WriteLine($"Creating MESH... {name}.numshb");
                MESHEX_Loader meshEx;
                var mesh = MESH_Loader.CreateMESH((SBUltimateModel)Model, (SBSkeleton)Skeleton, out meshEx);
                if (ExportSettings.ExportModelEx)
                    meshEx.Save(name + ".numshexb");
                SBConsole.WriteLine("Done");
                Ssbh.TrySaveSsbhFile(name + ".numshb", mesh);
            }

            if (ExportSettings.ExportSkeleton)
            {
                SBConsole.WriteLine($"Creating SKEL.. {name}.nusktb");
                SKEL_Loader.Save(name + ".nusktb", this, ExportSettings.ReferenceNusktbFile);
                SBConsole.WriteLine("Done");
            }

            if (ExportSettings.ExportTextures)
            {
                SBConsole.WriteLine($"Creating Textures");
                foreach (var tex in Surfaces)
                {
                    SBConsole.WriteLine($"Exporting.. {tex.Name}.nutexb");
                    IO_NUTEXB.Export(Path.GetDirectoryName(FileName) + "/" + tex.Name + ".nutexb", tex);
                }
                SBConsole.WriteLine("Done");
            }



            //SBConsole.WriteLine("Creating MATL...");

        }

        #region IO

        /// <summary>
        /// Gets the model information in this scene as an IO Model
        /// </summary>
        /// <returns></returns>
        public override IOScene GetIOModel()
        {
            IOScene scene = new IOScene();

            IOModel iomodel = new IOModel();
            scene.Models.Add(iomodel);
            iomodel.Skeleton = ((SBSkeleton)Skeleton).ToIOSkeleton();

            // TODO: exporting textures
            // Surfaces

            foreach (UltimateMaterial mat in Materials)
            {
                //if(scene.Materials.Find(e=>e.Name == mat.Label))
                var m = new IOMaterial();
                m.Name = mat.Label;
                m.DiffuseMap = new IOTexture() { Name = mat.Texture0.Value, FilePath = mat.Texture0.Value };
                scene.Materials.Add(m);
            }

            Dictionary<string, IOMesh> nameToMesh = new Dictionary<string, IOMesh>();

            foreach (var mesh in Model.Meshes)
            {
                if (!nameToMesh.ContainsKey(mesh.Name))
                {
                    IOMesh m = new IOMesh();
                    m.Name = mesh.Name;
                    iomodel.Meshes.Add(m);
                    nameToMesh.Add(mesh.Name, m);
                }

                IOMesh iomesh = nameToMesh[mesh.Name];

                IOPolygon poly = new IOPolygon();
                iomesh.Polygons.Add(poly);

                poly.MaterialName = mesh.Material.Label;
                
                poly.Indicies.AddRange(mesh.Indices.Select(e=>(int)e + iomesh.Vertices.Count));

                foreach (var vertex in mesh.Vertices)
                {
                    var iovertex = new IOVertex();

                    // export basic attributes
                    iovertex.Position = new System.Numerics.Vector3(vertex.Position0.X, vertex.Position0.Y, vertex.Position0.Z);
                    iovertex.Normal = new System.Numerics.Vector3(vertex.Normal0.X, vertex.Normal0.Y, vertex.Normal0.Z);
                    iovertex.Tangent = new System.Numerics.Vector3(vertex.Tangent0.X, vertex.Tangent0.Y, vertex.Tangent0.Z);

                    // export uv channels
                    if (mesh.ExportMap1)
                        iovertex.SetUV(vertex.Map1.X, vertex.Map1.Y, 0);
                    if (mesh.ExportUVSet1)
                        iovertex.SetUV(vertex.UvSet.X, vertex.UvSet.Y, 1);
                    if (mesh.ExportUVSet2)
                        iovertex.SetUV(vertex.UvSet1.X, vertex.UvSet1.Y, 2);
                    if (mesh.ExportUVSet3)
                        iovertex.SetUV(vertex.UvSet2.X, vertex.UvSet2.Y, 3);
                    if (mesh.ExportBake1)
                        iovertex.SetUV(vertex.Bake1.X, vertex.Bake1.Y, 4);

                    // export color sets
                    if (mesh.ExportColorSet1)
                        iovertex.SetColor(vertex.ColorSet1.X, vertex.ColorSet1.Y, vertex.ColorSet1.Z, vertex.ColorSet1.W, 0);
                    if (mesh.ExportColorSet2)
                        iovertex.SetColor(vertex.ColorSet2.X, vertex.ColorSet2.Y, vertex.ColorSet2.Z, vertex.ColorSet2.W, 1);
                    if (mesh.ExportColorSet21)
                        iovertex.SetColor(vertex.ColorSet21.X, vertex.ColorSet21.Y, vertex.ColorSet21.Z, vertex.ColorSet21.W, 2);
                    if (mesh.ExportColorSet22)
                        iovertex.SetColor(vertex.ColorSet22.X, vertex.ColorSet22.Y, vertex.ColorSet22.Z, vertex.ColorSet22.W, 3);
                    if (mesh.ExportColorSet23)
                        iovertex.SetColor(vertex.ColorSet23.X, vertex.ColorSet23.Y, vertex.ColorSet23.Z, vertex.ColorSet23.W, 4);
                    if (mesh.ExportColorSet3)
                        iovertex.SetColor(vertex.ColorSet3.X, vertex.ColorSet3.Y, vertex.ColorSet3.Z, vertex.ColorSet3.W, 5);
                    if (mesh.ExportColorSet4)
                        iovertex.SetColor(vertex.ColorSet4.X, vertex.ColorSet4.Y, vertex.ColorSet4.Z, vertex.ColorSet4.W, 6);
                    if (mesh.ExportColorSet5)
                        iovertex.SetColor(vertex.ColorSet5.X, vertex.ColorSet5.Y, vertex.ColorSet5.Z, vertex.ColorSet5.W, 7);
                    if (mesh.ExportColorSet6)
                        iovertex.SetColor(vertex.ColorSet6.X, vertex.ColorSet6.Y, vertex.ColorSet6.Z, vertex.ColorSet6.W, 8);
                    if (mesh.ExportColorSet7)
                        iovertex.SetColor(vertex.ColorSet7.X, vertex.ColorSet7.Y, vertex.ColorSet7.Z, vertex.ColorSet7.W, 9);

                    // export bone weights
                    for (int i = 0; i < 4; i++)
                    {
                        if(vertex.BoneWeights[i] > 0)
                        {
                            iovertex.Envelope.Weights.Add(new IOBoneWeight()
                            {
                                BoneName = Skeleton.Bones[vertex.BoneIndices[i]].Name,
                                Weight = vertex.BoneWeights[i]
                            });
                        }
                    }
                    
                    // normalize parent binding
                    if(mesh.ParentBone != "" && Skeleton != null)
                    {
                        var parentBone = Skeleton[mesh.ParentBone];
                        if (parentBone != null)
                        {
                            var tpos = OpenTK.Vector3.TransformPosition(vertex.Position0, parentBone.WorldTransform);
                            var tn = OpenTK.Vector3.TransformNormal(vertex.Normal0, parentBone.WorldTransform);

                            iovertex.Position = new System.Numerics.Vector3(tpos.X, tpos.Y, tpos.Z);
                            iovertex.Normal = new System.Numerics.Vector3(tn.X, tn.Y, tn.Z);

                            iovertex.Envelope.Weights.Add(new IOBoneWeight()
                            {
                                BoneName = parentBone.Name,
                                Weight = 1
                            });
                        }
                    }

                    iomesh.Vertices.Add(iovertex);
                }
            }

            return scene;
        }

        /// <summary>
        /// 
        /// </summary>
        public class AttributeMapper
        {
            [Category("0 UV Channels"), DisplayName("Map1"), Description("")]
            public int Map1Channel { get; set; } = 0;

            [Category("0 UV Channels"), DisplayName("UvSet1"), Description("")]
            public int UvSet1Channel { get; set; } = 1;

            [Category("0 UV Channels"), DisplayName("UvSet2"), Description("")]
            public int UvSet2Channel { get; set; } = 2;

            [Category("0 UV Channels"), DisplayName("UvSet3"), Description("")]
            public int UvSet3Channel { get; set; } = 3;

            [Category("0 UV Channels"), DisplayName("Bake1"), Description("")]
            public int Bake1Channel { get; set; } = -1;


            [Category("1 Color Channels"), DisplayName("ColorSet1"), Description("")]
            public int ColorSet1Channel { get; set; } = 0;

            [Category("1 Color Channels"), DisplayName("ColorSet2"), Description("")]
            public int ColorSet2Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet21"), Description("")]
            public int ColorSet21Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet22"), Description("")]
            public int ColorSet22Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet23"), Description("")]
            public int ColorSet23Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet3"), Description("")]
            public int ColorSet3Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet4"), Description("")]
            public int ColorSet4Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet5"), Description("")]
            public int ColorSet5Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet6"), Description("")]
            public int ColorSet6Channel { get; set; } = -1;

            [Category("1 Color Channels"), DisplayName("ColorSet7"), Description("")]
            public int ColorSet7Channel { get; set; } = -1;

            /// <summary>
            /// Checks if this uv channel attribtue should be enabled
            /// </summary>
            /// <param name="mesh"></param>
            /// <param name="channel"></param>
            /// <returns></returns>
            public static bool UVEnabled(IOMesh mesh, int channel)
            {
                return channel != -1 && mesh.HasUVSet(channel);
            }

            /// <summary>
            /// Checks if this color channel attribtue should be enabled
            /// </summary>
            /// <param name="mesh"></param>
            /// <param name="channel"></param>
            /// <returns></returns>
            public static bool ColorEnabled(IOMesh mesh, int channel)
            {
                return channel != -1 && mesh.HasColorSet(channel);
            }

            /// <summary>
            /// Returns uv for given channel
            /// </summary>
            /// <param name="iov"></param>
            /// <param name="channel"></param>
            /// <returns></returns>
            public static Vector2 RemapUV(IOVertex iov, int channel)
            {
                if (channel != -1 && channel < iov.UVs.Count)
                    return NumVecToTk(iov.UVs[channel]);
                else
                    return Vector2.Zero;
            }

            /// <summary>
            /// Returns color for given channel
            /// </summary>
            /// <param name="iov"></param>
            /// <param name="channel"></param>
            /// <returns></returns>
            public static OpenTK.Vector4 RemapColor(IOVertex iov, int channel)
            {
                if (channel != -1 && channel < iov.Colors.Count)
                    return NumVecToTk(iov.Colors[channel]);
                else
                    return OpenTK.Vector4.One;
            }
        }

        /// <summary>
        /// Imports information into this scene from an IO Model
        /// </summary>
        public override void FromIOModel(IOScene ioscene)
        {
            // check if any models are in scene
            if (ioscene.Models.Count == 0)
                return;

            // get use mapper settings
            AttributeMapper mapper = new AttributeMapper();

            using (var d = new SBCustomDialog(mapper))
            {
                if(d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    // if canceled use default options
                    mapper = new AttributeMapper();
                }
            }

            // select first model
            var iomodel = ioscene.Models[0];

            // load skeleton
            Skeleton = SBSkeleton.FromIOSkeleton(iomodel.Skeleton);
            

            // make temp material
            UltimateMaterial material;

            if (Materials.Count == 0)
            {
                material = new UltimateMaterial();
                material.Name = "SFX_PBS";
                material.Label = "new_material";

                //TODO more elegant material management
                Materials.Add(material);
            }
            else
                material = (UltimateMaterial)Materials[0];

            
            // convert meshes
            SBUltimateModel model = Model == null ? new SBUltimateModel() : (SBUltimateModel)Model;

            foreach (var iomesh in iomodel.Meshes)
            {
                SBUltimateMesh mesh = new SBUltimateMesh();
                mesh.Name = iomesh.Name;
                mesh.ParentBone = "";
                mesh.Material = material;

                model.Meshes.Add(mesh);

                mesh.Indices = new List<uint>();
                foreach(var p in iomesh.Polygons)
                {
                    if (p.PrimitiveType != IOPrimitive.TRIANGLE)
                        continue;

                    mesh.Indices.AddRange(p.Indicies.Select(e=>(uint)e));
                }
                
                // generate tangents and bitangents
                // this can optionally be done during the importing post process, but we need to make sure...
                iomesh.GenerateTangentsAndBitangents();

                //optimization single bind
                bool isSingleBound = true;
                string bonename = iomesh.Vertices.Count > 0 && iomesh.Vertices[0].Envelope.Weights.Count > 0 ? iomesh.Vertices[0].Envelope.Weights[0].BoneName : "";
                foreach (var vertex in iomesh.Vertices)
                {
                    if (vertex.Envelope.Weights.Count == 0 || vertex.Envelope.Weights.Count > 1 || vertex.Envelope.Weights[0].BoneName != bonename)
                    {
                        isSingleBound = false;
                        break;
                    }
                }
                SBBone parentBone = null;
                if (isSingleBound)
                    parentBone = Skeleton[bonename];

                // because the vertex cannot be changed after creation, and we don't know if we need to single bind,
                // we have to go through the vertices again after determining if this mesh is single bound
                foreach (var vertex in iomesh.Vertices)
                {
                    mesh.Vertices.Add(IOToUltimateVertex(vertex, (SBSkeleton)Skeleton, isSingleBound, parentBone == null ? Matrix4.Identity : parentBone.InvWorldTransform, mapper));
                }

                if (isSingleBound)
                    mesh.ParentBone = parentBone.Name;
                

                mesh.EnableAttribute(UltimateVertexAttribute.Position0);
                mesh.EnableAttribute(UltimateVertexAttribute.Normal0);
                mesh.EnableAttribute(UltimateVertexAttribute.Tangent0);


                if (AttributeMapper.UVEnabled(iomesh, mapper.Map1Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.Map1);

                if (AttributeMapper.UVEnabled(iomesh, mapper.UvSet1Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.UvSet);

                if (AttributeMapper.UVEnabled(iomesh, mapper.UvSet2Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.UvSet1);

                if (AttributeMapper.UVEnabled(iomesh, mapper.UvSet3Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.UvSet2);

                if (AttributeMapper.UVEnabled(iomesh, mapper.Bake1Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.Bake1);


                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet1Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet1);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet2Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet2);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet21Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet21);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet22Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet22);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet23Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet23);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet3Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet3);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet4Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet4);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet5Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet5);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet6Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet6);

                if (AttributeMapper.ColorEnabled(iomesh, mapper.ColorSet7Channel))
                    mesh.EnableAttribute(UltimateVertexAttribute.ColorSet7);


                // calculate bounding information
                mesh.CalculateBounding();

            }

            Model = model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private static Vector2 NumVecToTk(System.Numerics.Vector2 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private static OpenTK.Vector3 NumVecToTk(System.Numerics.Vector3 vec)
        {
            return new OpenTK.Vector3(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        private static OpenTK.Vector4 NumVecToTk(System.Numerics.Vector4 vec)
        {
            return new OpenTK.Vector4(vec.X, vec.Y, vec.Z, vec.W);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iov"></param>
        /// <param name="singleBound"></param>
        /// <param name="parentBoneInverse"></param>
        /// <returns></returns>
        private static UltimateVertex IOToUltimateVertex(IOVertex iov, SBSkeleton skeleton, bool singleBound, Matrix4 parentBoneInverse, AttributeMapper mapper)
        {
            // convert and map uvs
            var map = AttributeMapper.RemapUV(iov, mapper.Map1Channel);
            var uv1 = AttributeMapper.RemapUV(iov, mapper.UvSet1Channel);
            var uv2 = AttributeMapper.RemapUV(iov, mapper.UvSet2Channel);
            var uv3 = AttributeMapper.RemapUV(iov, mapper.UvSet3Channel);
            var bake = AttributeMapper.RemapUV(iov, mapper.Bake1Channel);

            // convert and map colors
            var color1 = AttributeMapper.RemapColor(iov, mapper.ColorSet1Channel);
            var color2 = AttributeMapper.RemapColor(iov, mapper.ColorSet2Channel);
            var color21 = AttributeMapper.RemapColor(iov, mapper.ColorSet21Channel);
            var color22 = AttributeMapper.RemapColor(iov, mapper.ColorSet22Channel);
            var color23 = AttributeMapper.RemapColor(iov, mapper.ColorSet23Channel);
            var color3 = AttributeMapper.RemapColor(iov, mapper.ColorSet3Channel);
            var color4 = AttributeMapper.RemapColor(iov, mapper.ColorSet4Channel);
            var color5 = AttributeMapper.RemapColor(iov, mapper.ColorSet5Channel);
            var color6 = AttributeMapper.RemapColor(iov, mapper.ColorSet6Channel);
            var color7 = AttributeMapper.RemapColor(iov, mapper.ColorSet7Channel);

            // convert weights
            var boneIndices = new IVec4();
            var boneWeights = OpenTK.Vector4.Zero;
            if (!singleBound)
                for (int i = 0; i < iov.Envelope.Weights.Count; i++)
                {
                    if (i >= 4)
                        break;
                    boneIndices[i] = skeleton.IndexOfBone(skeleton[iov.Envelope.Weights[i].BoneName]);
                    boneWeights[i] = iov.Envelope.Weights[i].Weight;
                }

            // create and return vertex
            return new UltimateVertex(
                 singleBound ? OpenTK.Vector3.TransformPosition(NumVecToTk(iov.Position), parentBoneInverse) : NumVecToTk(iov.Position),
                 singleBound ? OpenTK.Vector3.TransformNormal(NumVecToTk(iov.Normal), parentBoneInverse) : NumVecToTk(iov.Normal),
                 singleBound ? OpenTK.Vector3.TransformNormal(NumVecToTk(iov.Tangent), parentBoneInverse) : NumVecToTk(iov.Tangent),
                 singleBound ? OpenTK.Vector3.TransformNormal(NumVecToTk(iov.Binormal), parentBoneInverse) : NumVecToTk(iov.Binormal),
                 map,
                 uv1,
                 uv2,
                 uv3,
                 boneIndices,
                 boneWeights,
                 bake,
                 color1,
                 color2,
                 color21,
                 color22,
                 color23,
                 color3,
                 color4,
                 color5,
                 color6,
                 color7);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ISBMesh[] GetMeshObjects()
        {
            return Model.Meshes.ToArray();
        }

        public override void RemoveMeshObjects(ISBMesh[] MeshToRemove)
        {
            foreach(SBUltimateMesh mesh in MeshToRemove){
                if(mesh != null)
                    Model.Meshes.Remove(mesh);
            }
            RefreshRendering();
        }
        
        public override void SetMeshObjects(ISBMesh[] mesh)
        {
            Model.Meshes.Clear();
            foreach (var v in mesh)
                if (v is SBUltimateMesh m)
                    Model.Meshes.Add(m);
            RefreshRendering();
        }

        public override ISBMaterial[] GetMaterials()
        {
            return Materials.ToArray();
        }

        #region Rendering

        /// <summary>
        /// FOR THE LOVE OF SAKURAI DON'T USE THIS
        /// </summary>
        public override void RenderLegacy()
        {
            if(Model != null)
            foreach(var mesh in Model.Meshes)
                {
                    if (!mesh.Visible)
                        continue;

                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Back);

                    GL.PushMatrix();
                    Matrix4 transform = Matrix4.Identity;
                    if (Skeleton != null && mesh.ParentBone != "" && Skeleton.ContainsBone(mesh.ParentBone))
                        transform = Skeleton[mesh.ParentBone].WorldTransform;
                    GL.MultMatrix(ref transform);

                    GL.Begin(PrimitiveType.Triangles);
                    foreach(var index in mesh.Indices)
                    {
                        var vertex = mesh.Vertices[(int)index];
                        var color = new OpenTK.Vector3(0.5f) + vertex.Normal0 / 2;
                        GL.Color3(color);
                        GL.Vertex3(vertex.Position0);
                    }
                    GL.End();
                    GL.PopMatrix();
            }

            // the base SBScene can render the skeleton
            base.RenderLegacy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModelViewProjection"></param>
        public override void RenderShader(Camera camera)
        {
            RenderModelShader(camera);

            // the base SBScene can render the skeleton
            base.RenderShader(camera);
        }


        /// <summary>
        /// Updates the render meshes and textures to reflect changes
        /// </summary>
        public void RefreshRendering()
        {
            sbMeshToRenderMesh.Clear();

            if (Model != null)
            {
                foreach (var mesh in Model.Meshes)
                {
                    UltimateRenderMesh rmesh = new UltimateRenderMesh(mesh.Vertices, mesh.Indices);
                    sbMeshToRenderMesh.Add(mesh, rmesh);
                }
            }
        }

        /// <summary>
        /// Renders the model using a shader
        /// </summary>
        /// <param name="ModelViewProjection"></param>
        private void RenderModelShader(Camera camera)
        {
            if (Model == null) return;

            var shader = GetShader();
            if (!shader.LinkStatusIsOk)
                return;

            shader.UseProgram();
            
            // Bones
            int blockIndex = GL.GetUniformBlockIndex(shader.Id, "Bones");
            boneUniformBuffer.BindBase(BufferRangeTarget.UniformBuffer, blockIndex);
            if (Skeleton != null)
            {
                boneBinds = Skeleton.GetBindTransforms();
            }
            boneUniformBuffer.SetData(boneBinds, BufferUsageHint.DynamicDraw);
            
            // set uniforms
            SetShaderUniforms(shader);
            SetShaderCamera(shader, camera);
            
            // sort mesh
            var opaqueZSorted = new List<ISBMesh>();
            var transparentZSorted = new List<ISBMesh>();

            foreach (var m in Model.Meshes)
            {
                // no need to sort invisible meshes
                if (!m.Visible) 
                    continue;
                
                if (m.Material != null && ((UltimateMaterial)m.Material).HasBlending)
                {
                    transparentZSorted.Add(m);
                }
                else
                    opaqueZSorted.Add(m);
            }

            // TODO: Account for bounding sphere center and transform in depth sorting.
            // do we need to sort opaque?
            //opaqueZSorted = opaqueZSorted.OrderBy(m => m.BoundingSphere.Radius).ToList();
            transparentZSorted = transparentZSorted.OrderBy(c =>
            {
                Matrix4 transform = Matrix4.Identity;
                if (Skeleton != null && c.ParentBone != "" && Skeleton.ContainsBone(c.ParentBone))
                    transform = Skeleton[c.ParentBone].AnimatedWorldTransform;
                var v = OpenTK.Vector3.TransformPosition(c.BoundingSphere.Position, transform);
                v = OpenTK.Vector3.TransformPosition(c.BoundingSphere.Position, camera.MvpMatrix);
                return v.Z + c.BoundingSphere.Radius;
            }).ToList();

            opaqueZSorted.AddRange(transparentZSorted);

            foreach (SBUltimateMesh mesh in opaqueZSorted)
            {
                shader.SetBoolToInt("renderWireframe", mesh.Selected || ApplicationSettings.EnableWireframe);

                // refresh rendering if this not is not setup
                if (!sbMeshToRenderMesh.ContainsKey(mesh))
                {
                    RefreshRendering();
                }

                // bind material
                if (mesh.Material != null)
                    mesh.Material.Bind(this, shader);

                // single bind transforms
                Matrix4 transform = Matrix4.Identity;
                if (Skeleton != null && mesh.ParentBone != "" && Skeleton.ContainsBone(mesh.ParentBone))
                    transform = Skeleton[mesh.ParentBone].AnimatedWorldTransform;
                shader.SetMatrix4x4("transform", ref transform);
                
                // draw mesh
                var rmesh = sbMeshToRenderMesh[mesh];
                rmesh.Draw(shader);
            }

#if DEBUG
            foreach (var mesh in Model.Meshes)
            {
                if (!mesh.Selected) continue;

                Matrix4 transform = Matrix4.Identity;
                if (Skeleton != null && mesh.ParentBone != "" && Skeleton.ContainsBone(mesh.ParentBone))
                    transform = Skeleton[mesh.ParentBone].AnimatedWorldTransform;
                var sphereTransform = transform;

                mesh.BoundingSphere.Render(camera, transform);
                mesh.AABoundingBox.Render(camera, transform);
                mesh.OrientedBoundingBox.Render(camera, transform);
            }
# endif
        }

        private Shader GetShader()
        {
            if (ApplicationSettings.UseDebugShading)
                return ShaderManager.GetShader("UltimateModelDebug");
            else
                return ShaderManager.GetShader("UltimateModel");
        }

        private void SetShaderCamera(Shader shader, Camera camera)
        {
            Matrix4 mvp = camera.MvpMatrix;

            shader.SetMatrix4x4("mvp", ref mvp);
            
            shader.SetVector3("cameraPos", camera.Position);
        }

        private void SetShaderUniforms(Shader shader)
        {
            shader.SetVector4("renderChannels", ApplicationSettings.RenderChannels);
            shader.SetInt("renderMode", (int)ApplicationSettings.ShadingMode);

            //TODO:
            // this is smash ultimate specific
            shader.SetInt("transitionEffect", (int)MaterialMode);
            shader.SetFloat("transitionFactor", MaterialBlend);

            shader.SetBoolToInt("renderDiffuse", ApplicationSettings.EnableDiffuse);
            shader.SetBoolToInt("renderSpecular", ApplicationSettings.EnableSpecular);
            shader.SetBoolToInt("renderEmission", ApplicationSettings.EnableEmission);
            shader.SetBoolToInt("renderRimLighting", ApplicationSettings.EnableRimLighting);
            shader.SetBoolToInt("renderExperimental", ApplicationSettings.EnableExperimental);

            shader.SetBoolToInt("renderNormalMaps", ApplicationSettings.RenderNormalMaps);
            shader.SetBoolToInt("renderVertexColor", ApplicationSettings.RenderVertexColor);

            shader.SetFloat("iblIntensity", 1.0f);
            shader.SetFloat("directLightIntensity", 1.0f);

            shader.SetVector3("chrLightDir", GetLightDirectionFromQuaternion(-0.453154f, -0.365998f, -0.211309f, 0.784886f));
        }

        private static OpenTK.Vector3 GetLightDirectionFromQuaternion(float x, float y, float z, float w)
        {
            var quaternion = new Quaternion(x, y, z, w);
            var matrix = Matrix4.CreateFromQuaternion(quaternion);
            var lightDirection = OpenTK.Vector4.Transform(new OpenTK.Vector4(0, 0, 1, 0), matrix);
            return lightDirection.Normalized().Xyz;
        }

        #endregion
    }
}
