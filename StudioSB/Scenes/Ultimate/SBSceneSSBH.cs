using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using StudioSB.Rendering;
using System.Collections.Generic;
using SFGraphics.GLObjects.Textures;
using SSBHLib;
using SSBHLib.Formats;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.Cameras;
using StudioSB.IO.Models;
using SFGraphics.GLObjects.BufferObjects;
using StudioSB.IO.Formats;
using System.Linq;

namespace StudioSB.Scenes.Ultimate
{
    [SceneFileInformation("NU Model File Binary", ".numdlb", "The model specification for Smash Ultimate models", sceneCode: "SSBU")]
    public class SBSceneSSBH : SBScene
    {
        public static SBUltimateImportSettings ImportSettings = new SBUltimateImportSettings();
        public static SBUltimateNewImportSettings NewImportSettings = new SBUltimateNewImportSettings();
        
        public ISBModel<SBUltimateMesh> Model { get; set; }

        // Rendering
        public Dictionary<SBUltimateMesh, UltimateRenderMesh> sbMeshToRenderMesh = new Dictionary<SBUltimateMesh, UltimateRenderMesh>();
        private BufferObject boneUniformBuffer;
        private Matrix4[] boneBinds = new Matrix4[200];

        public SBSceneSSBH()
        {
            HasMesh = true;
            HasBones = true;
            boneUniformBuffer = new BufferObject(BufferTarget.UniformBuffer);
        }

        /// <summary>
        /// Loads the scene from a NUMDLB file
        /// </summary>
        /// <param name="FileName"></param>
        public override void LoadFromFile(string FileName)
        {
            string folderPath = Path.GetDirectoryName(FileName);

            ISSBH_File File;
            if (!SSBH.TryParseSSBHFile(FileName, out File))
                return;

            MODL modl = (MODL)File;

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
                        if (matentry.Label.Equals(entry.MaterialName))
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

            string name = Path.GetDirectoryName(FileName) + "/" + Path.GetFileNameWithoutExtension(FileName);
            string simpleName = Path.GetFileNameWithoutExtension(FileName);

            SBConsole.WriteLine("Creating MODL...");
            var modl = MODL_Loader.CreateMODLFile((SBUltimateModel)Model);
            modl.ModelFileName = simpleName;
            modl.SkeletonFileName = $"{simpleName}.nusktb";
            modl.MeshString = $"{simpleName}.numshb";
            modl.UnknownFileName = null;
            modl.MaterialFileNames = new MODL_MaterialName[] { new MODL_MaterialName() { MaterialFileName = $"{simpleName}.numatb" } };
            SBConsole.WriteLine("Done");
            SSBH.TrySaveSSBHFile(FileName, modl);

            SBConsole.WriteLine($"Creating MESH... {name}.numshb");
            var mesh = MESH_Loader.CreateMESH((SBUltimateModel)Model, (SBSkeleton)Skeleton);
            SBConsole.WriteLine("Done");
            SSBH.TrySaveSSBHFile(name + ".numshb", mesh);

            SBConsole.WriteLine($"Creating SKEL.. {name}.nusktb");
            SKEL_Loader.Save(name + ".nusktb", this);
            SBConsole.WriteLine("Done");

            //SBConsole.WriteLine("Creating MATL...");

        }

        #region IO

        /// <summary>
        /// Gets the model information in this scene as an IO Model
        /// </summary>
        /// <returns></returns>
        public override IOModel GetIOModel()
        {
            IOModel iomodel = new IOModel();
            iomodel.Skeleton = (SBSkeleton)Skeleton;

            foreach (var mesh in Model.Meshes)
            {
                var iomesh = new IOMesh();
                iomesh.Name = mesh.Name;
                iomodel.Meshes.Add(iomesh);

                iomesh.HasPositions = mesh.ExportAttributes.Contains(UltimateVertexAttribute.Position0);
                iomesh.HasNormals = mesh.ExportAttributes.Contains(UltimateVertexAttribute.Normal0);
                iomesh.HasUV0 = mesh.ExportAttributes.Contains(UltimateVertexAttribute.map1);
                iomesh.HasUV1 = mesh.ExportAttributes.Contains(UltimateVertexAttribute.uvSet);
                iomesh.HasUV2 = mesh.ExportAttributes.Contains(UltimateVertexAttribute.uvSet1);
                iomesh.HasUV3 = mesh.ExportAttributes.Contains(UltimateVertexAttribute.uvSet2);
                iomesh.HasColor = mesh.ExportAttributes.Contains(UltimateVertexAttribute.colorSet1);

                iomesh.HasBoneWeights = true;

                iomesh.Indices.AddRange(mesh.Indices);

                foreach (var vertex in mesh.Vertices)
                {
                    var iovertex = new IOVertex();

                    iovertex.Position = vertex.Position0;
                    iovertex.Normal = vertex.Normal0;
                    iovertex.Tangent = vertex.Tangent0;
                    iovertex.UV0 = vertex.Map1;
                    iovertex.UV1 = vertex.UvSet;
                    iovertex.UV2 = vertex.UvSet1;
                    iovertex.Color = vertex.ColorSet1;
                    iovertex.BoneIndices = new Vector4(vertex.BoneIndices.X, vertex.BoneIndices.Y, vertex.BoneIndices.Z, vertex.BoneIndices.W);
                    iovertex.BoneWeights = vertex.BoneWeights;

                    // single bind fix
                    if(mesh.ParentBone != "" && Skeleton != null)
                    {
                        var parentBone = Skeleton[mesh.ParentBone];
                        if (parentBone != null)
                        {
                            iovertex.Position = Vector3.TransformPosition(vertex.Position0, parentBone.WorldTransform);
                            iovertex.Normal = Vector3.TransformNormal(vertex.Normal0, parentBone.WorldTransform);
                            iovertex.BoneIndices = new Vector4(Skeleton.IndexOfBone(parentBone), 0, 0, 0);
                            iovertex.BoneWeights = new Vector4(1, 0, 0, 0);
                        }
                    }

                    iomesh.Vertices.Add(iovertex);
                }
            }

            return iomodel;
        }

        /// <summary>
        /// Imports information into this scene from an IO Model
        /// </summary>
        public override void FromIOModel(IOModel iomodel)
        {
            // copy skeleton
            Skeleton = iomodel.Skeleton;

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

                mesh.Indices = iomesh.Indices;

                iomesh.GenerateTangentsAndBitangents();

                //optimization single bind
                bool isSingleBound = true;
                int bone = iomesh.Vertices.Count > 0 ? (int)iomesh.Vertices[0].BoneIndices.X : -1;
                foreach (var vertex in iomesh.Vertices)
                {
                    if (vertex.BoneWeights.X != 1 || vertex.BoneIndices.X != bone)
                    {
                        isSingleBound = false;
                        break;
                    }
                }
                SBBone parentBone = null;
                if(isSingleBound)
                    parentBone = iomodel.Skeleton.Bones[bone];

                bool Has2ndUVChannel = false;

                // because the vertex cannot be changed after creation, and we don't know if we need to single bind,
                // we have to go through the vertices again after determining if this mesh is single bound
                foreach (var vertex in iomesh.Vertices)
                {
                    if (vertex.UV1 != Vector2.Zero)
                        Has2ndUVChannel = true;
                    mesh.Vertices.Add(IOToUltimateVertex(vertex, isSingleBound, parentBone == null ? Matrix4.Identity : parentBone.InvWorldTransform));
                }

                if (isSingleBound)
                    mesh.ParentBone = parentBone.Name;

                //TODO: make more customizable through import settings
                mesh.ExportAttributes.Add(UltimateVertexAttribute.Position0);
                mesh.ExportAttributes.Add(UltimateVertexAttribute.Normal0);
                mesh.ExportAttributes.Add(UltimateVertexAttribute.Tangent0);
                mesh.ExportAttributes.Add(UltimateVertexAttribute.map1);
                if(Has2ndUVChannel)
                    mesh.ExportAttributes.Add(UltimateVertexAttribute.uvSet);
                mesh.ExportAttributes.Add(UltimateVertexAttribute.colorSet1);

                // calculate bounding information
                mesh.CalculateBounding();
            }

            Model = model;
        }

        private static UltimateVertex IOToUltimateVertex(IOVertex iov, bool singleBound, Matrix4 parentBoneInverse)
        {
            return new UltimateVertex(
                singleBound ? Vector3.TransformPosition(iov.Position, parentBoneInverse) : iov.Position,
                singleBound ? Vector3.TransformNormal(iov.Normal, parentBoneInverse) : iov.Normal, 
                iov.Tangent, iov.Bitangent, iov.UV0, iov.UV1,
                iov.UV2, singleBound ? new IVec4() : new IVec4() { X = (int)iov.BoneIndices.X,
                    Y = (int)iov.BoneIndices.Y,
                    Z = (int)iov.BoneIndices.Z,
                    W = (int)iov.BoneIndices.W },
                 singleBound ? Vector4.Zero : iov.BoneWeights, Vector2.Zero, iov.Color, Vector4.One, Vector4.One);
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
                        var color = new Vector3(0.5f) + vertex.Normal0 / 2;
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

            /*nameToSurface.Clear();
            foreach(var tex in Surfaces)
            {
                nameToSurface.Add(tex.Name.ToLower(), tex);
            }*/
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

            SetShaderUniforms(shader);
            SetShaderCamera(shader, camera);

            var opaqueZSorted = new List<ISBMesh>();
            var transparentZSorted = new List<ISBMesh>();

            foreach (var m in Model.Meshes)
            {
                if (((UltimateMaterial)m.Material).HasBlending)
                    transparentZSorted.Add(m);
                else
                    opaqueZSorted.Add(m);
            }

            // TODO: Account for bounding sphere center and transform in depth sorting.
            opaqueZSorted = opaqueZSorted.OrderBy(m => m.BoundingSphere.Radius).ToList();
            transparentZSorted = transparentZSorted.OrderBy(m => m.BoundingSphere.Radius).ToList();

            opaqueZSorted.AddRange(transparentZSorted);

            foreach (SBUltimateMesh mesh in opaqueZSorted)
            {
                if (!mesh.Visible) continue;
                
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

            //Vector3 size = ((SBUltimateModel)Model).VolumeSize;
            //StudioSB.Rendering.Shapes.Sphere.DrawRectangularPrism(((SBUltimateModel)Model).VolumeCenter, size.X, size.Y, size.Z, true);

            //StudioSB.Rendering.Shapes.RectangularPrism.DrawRectangularPrism(camera, ((SBUltimateModel)Model).OBBPosition, ((SBUltimateModel)Model).OBBSize * 2, ((SBUltimateModel)Model).OBBTransform);
            //StudioSB.Rendering.Shapes.Sphere.DrawSphereLegacy(Model.BoundingSphere.Xyz, Model.BoundingSphere.W, 20, true);
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
            shader.SetInt("transitionEffect", 0);
            shader.SetFloat("transitionFactor", 0);

            shader.SetBoolToInt("renderDiffuse", ApplicationSettings.EnableDiffuse);
            shader.SetBoolToInt("renderSpecular", ApplicationSettings.EnableSpecular);
            shader.SetBoolToInt("renderEmission", ApplicationSettings.EnableEmission);
            shader.SetBoolToInt("renderRimLighting", ApplicationSettings.EnableRimLighting);
            shader.SetBoolToInt("renderExperimental", ApplicationSettings.EnableExperimental);

            shader.SetBoolToInt("renderNormalMaps", ApplicationSettings.RenderNormalMaps);
            shader.SetBoolToInt("renderVertexColor", ApplicationSettings.RenderVertexColor);
        }

        #endregion
    }
}
