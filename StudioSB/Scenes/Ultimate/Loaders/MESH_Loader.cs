using System;
using SSBHLib;
using SSBHLib.Formats.Meshes;
using System.Collections.Generic;
using OpenTK;
using SSBHLib.Tools;

namespace StudioSB.Scenes.Ultimate
{
    public class MESH_Loader
    {
        public static void Open(string FileName, SBScene Scene)
        {
            ISSBH_File File;
            if (SSBH.TryParseSSBHFile(FileName, out File))
            {
                if (File is MESH mesh)
                {
                    if (mesh.VersionMajor != 1 && mesh.VersionMinor != 10)
                    {
                        SBConsole.WriteLine($"Mesh Version {mesh.VersionMajor}.{mesh.VersionMinor} not supported");
                        return;
                    }
                    if (mesh.UnknownOffset != 0 || mesh.UnknownSize != 0)
                    {
                        SBConsole.WriteLine($"Warning: Unknown Mesh format detected");
                    }

                    SBUltimateModel model = new SBUltimateModel();
                    model.Name = mesh.ModelName;
                    model.BoundingSphere = new Vector4(mesh.BoundingSphereX, mesh.BoundingSphereY, mesh.BoundingSphereZ, mesh.BoundingSphereRadius);

                    model.OBBTransform = new Matrix4(mesh.UnknownFloats[0], mesh.UnknownFloats[1], mesh.UnknownFloats[2], mesh.UnknownFloats[3],
                        mesh.UnknownFloats[4], mesh.UnknownFloats[5], mesh.UnknownFloats[6], mesh.UnknownFloats[7],
                        mesh.UnknownFloats[8], mesh.UnknownFloats[9], mesh.UnknownFloats[10], mesh.UnknownFloats[11],
                        mesh.UnknownFloats[12], mesh.UnknownFloats[13], mesh.UnknownFloats[14], mesh.UnknownFloats[15]);

                    var matrix = new Matrix3(
                        mesh.UnknownFloats[3], mesh.UnknownFloats[4], mesh.UnknownFloats[5], 
                        mesh.UnknownFloats[6], mesh.UnknownFloats[7], mesh.UnknownFloats[8], 
                        mesh.UnknownFloats[9], mesh.UnknownFloats[10], mesh.UnknownFloats[11]);

                    model.OBBTransform = new Matrix4(matrix);
                    model.OBBPosition = new Vector3(mesh.UnknownFloats[0], mesh.UnknownFloats[1], mesh.UnknownFloats[2]);
                    model.OBBSize = new Vector3(mesh.UnknownFloats[12], mesh.UnknownFloats[13], mesh.UnknownFloats[14]);

                    Vector3 min = new Vector3(mesh.MinBoundingBoxX, mesh.MinBoundingBoxY, mesh.MinBoundingBoxZ);
                    Vector3 max = new Vector3(mesh.MaxBoundingBoxX, mesh.MaxBoundingBoxY, mesh.MaxBoundingBoxZ);

                    /*SBConsole.WriteLine($"Sphere {model.BoundingSphere.ToString()}");
                    SBConsole.WriteLine($"Min: {min.ToString()} Max: {max.ToString()}");
                    SBConsole.WriteLine($"Matrix: {model.VolumeTransform.ToString()}");
                    SBConsole.WriteLine($"RotQ: {matrix.ExtractRotation().ToString()}");
                    SBConsole.WriteLine($"Scale: {matrix.ExtractScale().ToString()}");*/

                    model.VolumeCenter = (max + min) / 2;
                    model.VolumeSize = (max - min) / 2;

                    ((SBSceneSSBH)Scene).Model = model;

                    SSBHVertexAccessor accessor = new SSBHVertexAccessor(mesh);
                    {
                        foreach (var meshObject in mesh.Objects)
                        {
                            SBUltimateMesh<UltimateVertex> sbMesh = new SBUltimateMesh<UltimateVertex>();
                            foreach (var attr in meshObject.Attributes)
                            {
                                foreach (var atstring in attr.AttributeStrings)
                                {
                                    UltimateVertexAttribute at;
                                    if(Enum.TryParse<UltimateVertexAttribute>(atstring.Name, out at))
                                    {
                                        //SBConsole.WriteLine("\tLoaded:" + at.ToString());
                                        sbMesh.ExportAttributes.Add(at);
                                    }
                                }
                            }
                            sbMesh.Name = meshObject.Name;
                            sbMesh.ParentBone = meshObject.ParentBoneName;
                            
                            sbMesh.BoundingSphere = new Vector4(meshObject.BoundingSphereX, meshObject.BoundingSphereY, meshObject.BoundingSphereZ, meshObject.BoundingSphereRadius);
                            
                            sbMesh.AABBMin = new Vector3(meshObject.MinBoundingBoxX, meshObject.MinBoundingBoxY, meshObject.MinBoundingBoxZ);

                            sbMesh.AABBMax = new Vector3(meshObject.MaxBoundingBoxX, meshObject.MaxBoundingBoxY, meshObject.MaxBoundingBoxZ);
                            
                            sbMesh.Indices = new List<uint>(accessor.ReadIndices(0, meshObject.IndexCount, meshObject));
                            sbMesh.Vertices = CreateVertices(mesh, Scene.Skeleton, meshObject, accessor, sbMesh.Indices.ToArray());
                            model.Meshes.Add(sbMesh);
                        }
                    }
                }
            }
        }

        private static List<UltimateVertex> CreateVertices(MESH mesh, ISBSkeleton Skeleton, MeshObject meshObject, SSBHVertexAccessor vertexAccessor, uint[] vertexIndices)
        {
            // Read attribute values.
            var positions = vertexAccessor.ReadAttribute("Position0", 0, meshObject.VertexCount, meshObject);
            var normals = vertexAccessor.ReadAttribute("Normal0", 0, meshObject.VertexCount, meshObject);
            var tangents = vertexAccessor.ReadAttribute("Tangent0", 0, meshObject.VertexCount, meshObject);
            var map1Values = vertexAccessor.ReadAttribute("map1", 0, meshObject.VertexCount, meshObject);
            var uvSetValues = vertexAccessor.ReadAttribute("uvSet", 0, meshObject.VertexCount, meshObject);
            var uvSet1Values = vertexAccessor.ReadAttribute("uvSet1", 0, meshObject.VertexCount, meshObject);
            var bake1Values = vertexAccessor.ReadAttribute("bake1", 0, meshObject.VertexCount, meshObject);
            var colorSet1Values = vertexAccessor.ReadAttribute("colorSet1", 0, meshObject.VertexCount, meshObject);
            var colorSet5Values = vertexAccessor.ReadAttribute("colorSet5", 0, meshObject.VertexCount, meshObject);

            var generatedBitangents = GenerateBitangents(vertexIndices, positions, map1Values);

            var riggingAccessor = new SSBHRiggingAccessor(mesh);
            var influences = riggingAccessor.ReadRiggingBuffer(meshObject.Name, (int)meshObject.SubMeshIndex);
            var indexByBoneName = new Dictionary<string, int>();

            if (Skeleton != null)
            {
                var Bones = Skeleton.Bones;
                for (int i = 0; i < Bones.Length; i++)
                {
                    indexByBoneName.Add(Bones[i].Name, i);
                }
            }

            GetRiggingData(positions, influences, indexByBoneName, out IVec4[] boneIndices, out Vector4[] boneWeights);

            var vertices = new List<UltimateVertex>(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                var position = GetVector4(positions[i]).Xyz;

                var normal = GetVector4(normals[i]).Xyz;
                var tangent = GetVector4(tangents[i]).Xyz;
                var bitangent = GetBitangent(generatedBitangents, i, normal);

                var map1 = GetVector4(map1Values[i]).Xy;

                var uvSet = map1;
                if (uvSetValues.Length != 0)
                    uvSet = GetVector4(uvSetValues[i]).Xy;
                var uvSet1 = new Vector2(0);
                if (uvSet1Values.Length != 0)
                    uvSet1 = GetVector4(uvSet1Values[i]).Xy;

                var bones = boneIndices[i];
                var weights = boneWeights[i];

                // Accessors return length 0 when the attribute isn't present.
                var bake1 = new Vector2(0);
                if (bake1Values.Length != 0)
                    bake1 = GetVector4(bake1Values[i]).Xy;

                // The values are read as float, so we can't use OpenGL to convert.
                // Convert the range [0, 128] to [0, 255]. 
                var colorSet1 = new Vector4(1);
                if (colorSet1Values.Length != 0)
                    colorSet1 = GetVector4(colorSet1Values[i]) / 128.0f;

                var colorSet5 = new Vector4(1);
                if (colorSet5Values.Length != 0)
                    colorSet5 = GetVector4(colorSet5Values[i]) / 128.0f;

                vertices.Add(new UltimateVertex(position, normal, tangent, bitangent, map1, uvSet, uvSet1, bones, weights, bake1, colorSet1, colorSet5));
            }

            return vertices;
        }

        private static void GetRiggingData(SSBHVertexAttribute[] positions, SSBHVertexInfluence[] influences, Dictionary<string, int> indexByBoneName, out IVec4[] boneIndices, out Vector4[] boneWeights)
        {
            boneIndices = new IVec4[positions.Length];
            boneWeights = new Vector4[positions.Length];
            foreach (SSBHVertexInfluence influence in influences)
            {
                // Some influences refer to bones that don't exist in the skeleton.
                // _eff bones?
                if (!indexByBoneName.ContainsKey(influence.BoneName))
                    continue;

                if (boneWeights[influence.VertexIndex].X == 0)
                {
                    boneIndices[influence.VertexIndex].X = indexByBoneName[influence.BoneName];
                    boneWeights[influence.VertexIndex].X = influence.Weight;
                }
                else if (boneWeights[influence.VertexIndex].Y == 0)
                {
                    boneIndices[influence.VertexIndex].Y = indexByBoneName[influence.BoneName];
                    boneWeights[influence.VertexIndex].Y = influence.Weight;
                }
                else if (boneWeights[influence.VertexIndex].Z == 0)
                {
                    boneIndices[influence.VertexIndex].Z = indexByBoneName[influence.BoneName];
                    boneWeights[influence.VertexIndex].Z = influence.Weight;
                }
                else if (boneWeights[influence.VertexIndex].W == 0)
                {
                    boneIndices[influence.VertexIndex].W = indexByBoneName[influence.BoneName];
                    boneWeights[influence.VertexIndex].W = influence.Weight;
                }
            }
        }

        private static Vector3[] GenerateBitangents(uint[] indices, SSBHVertexAttribute[] positions, SSBHVertexAttribute[] uvs)
        {
            var generatedBitangents = new Vector3[positions.Length];
            for (int i = 0; i < indices.Length; i += 3)
            {
                SFGraphics.Utils.VectorUtils.GenerateTangentBitangent(GetVector4(positions[indices[i]]).Xyz, GetVector4(positions[indices[i + 1]]).Xyz, GetVector4(positions[indices[i + 2]]).Xyz,
                    GetVector4(uvs[indices[i]]).Xy, GetVector4(uvs[indices[i + 1]]).Xy, GetVector4(uvs[indices[i + 2]]).Xy, out Vector3 tangent, out Vector3 bitangent);

                generatedBitangents[indices[i]] += bitangent;
                generatedBitangents[indices[i + 1]] += bitangent;
                generatedBitangents[indices[i + 2]] += bitangent;
            }

            return generatedBitangents;
        }


        private static Vector3 GetBitangent(Vector3[] generatedBitangents, int i, Vector3 normal)
        {
            // Account for mirrored normal maps.
            var bitangent = SFGraphics.Utils.VectorUtils.Orthogonalize(generatedBitangents[i], normal);
            bitangent *= -1;
            return bitangent;
        }

        private static Vector4 GetVector4(SSBHVertexAttribute values)
        {
            return new Vector4(values.X, values.Y, values.Z, values.W);
        }

        public static MESH CreateMESH(SBUltimateModel model, SBSkeleton Skeleton)
        {
            SSBHMeshMaker maker = new SSBHMeshMaker();

            string[] BoneNames = new string[Skeleton.Bones.Length];
            int BoneIndex = 0;
            foreach(var bone in Skeleton.Bones)
            {
                BoneNames[BoneIndex++] = bone.Name;
            }

            List<Vector3> allVertices = new List<Vector3>();
            List<int> vertexCounts = new List<int>();

            foreach(var mesh in model.Meshes)
            {
                // preprocess

                List<SSBHVertexAttribute> Position0 = new List<SSBHVertexAttribute>();
                List<SSBHVertexAttribute> Normal0 = new List<SSBHVertexAttribute>();
                List<SSBHVertexAttribute> Tangent0 = new List<SSBHVertexAttribute>();
                List<SSBHVertexAttribute> Map1 = new List<SSBHVertexAttribute>();
                List<SSBHVertexAttribute> UvSet = new List<SSBHVertexAttribute>();
                List<SSBHVertexAttribute> colorSet1 = new List<SSBHVertexAttribute>();

                List<SSBHVertexInfluence> Influences = new List<SSBHVertexInfluence>();

                vertexCounts.Add(mesh.Vertices.Count);

                int VertexIndex = 0;
                foreach(var vertex in mesh.Vertices)
                {
                    allVertices.Add(vertex.Position0);

                    Position0.Add(vectorToAttribute(vertex.Position0));
                    Normal0.Add(vectorToAttribute(vertex.Normal0));
                    Tangent0.Add(vectorToAttribute(vertex.Tangent0));
                    Map1.Add(vectorToAttribute(vertex.Map1));
                    UvSet.Add(vectorToAttribute(vertex.UvSet));
                    colorSet1.Add(vectorToAttribute(new Vector4(128, 128, 128, 128)));
                    
                    if(vertex.BoneWeights.X > 0)
                        Influences.Add(CreateInfluence((ushort)VertexIndex, BoneNames[vertex.BoneIndices.X], vertex.BoneWeights.X));

                    if (vertex.BoneWeights.Y > 0)
                        Influences.Add(CreateInfluence((ushort)VertexIndex, BoneNames[vertex.BoneIndices.Y], vertex.BoneWeights.Y));

                    if (vertex.BoneWeights.Z > 0)
                        Influences.Add(CreateInfluence((ushort)VertexIndex, BoneNames[vertex.BoneIndices.Z], vertex.BoneWeights.Z));

                    if (vertex.BoneWeights.W > 0)
                        Influences.Add(CreateInfluence((ushort)VertexIndex, BoneNames[vertex.BoneIndices.W], vertex.BoneWeights.W));

                    VertexIndex++;
                }

                // set the triangle indices
                uint[] Indices = mesh.Indices.ToArray();

                // start creating the mesh object
                maker.StartMeshObject(mesh.Name, Indices, Position0.ToArray(), mesh.ParentBone);

                // Add attributes
                if(mesh.ExportAttributes.Contains(UltimateVertexAttribute.Normal0))
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.Normal0, Normal0.ToArray());
                if (mesh.ExportAttributes.Contains(UltimateVertexAttribute.Tangent0))
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.Tangent0, Tangent0.ToArray());
                if (mesh.ExportAttributes.Contains(UltimateVertexAttribute.map1))
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.map1, Map1.ToArray());
                if (mesh.ExportAttributes.Contains(UltimateVertexAttribute.uvSet))
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.uvSet, UvSet.ToArray());
                if (mesh.ExportAttributes.Contains(UltimateVertexAttribute.colorSet1))
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.colorSet1, colorSet1.ToArray());

                // Add rigging
                if(mesh.ParentBone == "")
                    maker.AttachRiggingToMeshObject(Influences.ToArray());
            }

            MESH meshFile = maker.GetMeshFile();

            int vertexoffset = 0;
            int meshindex = 0;
            foreach(var meshObject in meshFile.Objects)
            {
                GenerateBB(meshObject, allVertices.GetRange(vertexoffset, vertexCounts[meshindex]));
                vertexoffset += vertexCounts[meshindex];
                meshindex++;
            }

            Vector3 maxBB, minBB;
            Tools.BBGenerator.GenerateAABB(allVertices, out maxBB, out minBB);
            var BoundingSphere = SFGraphics.Utils.BoundingSphereGenerator.GenerateBoundingSphere(allVertices);

            meshFile.BoundingSphereX = BoundingSphere.X;
            meshFile.BoundingSphereY = BoundingSphere.Y;
            meshFile.BoundingSphereZ = BoundingSphere.Z;
            meshFile.BoundingSphereRadius = BoundingSphere.W;

            meshFile.MaxBoundingBoxX = maxBB.X;
            meshFile.MaxBoundingBoxY = maxBB.Y;
            meshFile.MaxBoundingBoxZ = maxBB.Z;

            meshFile.MinBoundingBoxX = minBB.X;
            meshFile.MinBoundingBoxY = minBB.Y;
            meshFile.MinBoundingBoxZ = minBB.Z;


            //TODO: oriented bounding box
            // using Axis aligned for now

            /*Vector3 BBCenter = (maxBB + minBB) / 2;
            Vector3 BBSize = (maxBB + minBB) / 2;
            var OBBTransform = Matrix3.Identity;

            meshFile.UnknownFloats[0] = BBCenter.X;
            meshFile.UnknownFloats[1] = BBCenter.Y;
            meshFile.UnknownFloats[2] = BBCenter.Z;

            meshFile.UnknownFloats[3] = OBBTransform.M11; meshFile.UnknownFloats[4] = OBBTransform.M12; meshFile.UnknownFloats[5] = OBBTransform.M13;
            meshFile.UnknownFloats[6] = OBBTransform.M21; meshFile.UnknownFloats[7] = OBBTransform.M22; meshFile.UnknownFloats[8] = OBBTransform.M23;
            meshFile.UnknownFloats[9] = OBBTransform.M31; meshFile.UnknownFloats[10] = OBBTransform.M32; meshFile.UnknownFloats[11] = OBBTransform.M33;

            meshFile.UnknownFloats[12] = BBSize.X;
            meshFile.UnknownFloats[13] = BBSize.Y;
            meshFile.UnknownFloats[14] = BBSize.Z;*/

            return meshFile;
        }

        private static void GenerateBB(MeshObject meshObject, List<Vector3> positions)
        {
            Vector3 maxBB, minBB;
            Tools.BBGenerator.GenerateAABB(positions, out maxBB, out minBB);
            var BoundingSphere = SFGraphics.Utils.BoundingSphereGenerator.GenerateBoundingSphere(positions);

            meshObject.BoundingSphereX = BoundingSphere.X;
            meshObject.BoundingSphereY = BoundingSphere.Y;
            meshObject.BoundingSphereZ = BoundingSphere.Z;
            meshObject.BoundingSphereRadius = BoundingSphere.W;

            meshObject.MaxBoundingBoxX = maxBB.X;
            meshObject.MaxBoundingBoxY = maxBB.Y;
            meshObject.MaxBoundingBoxZ = maxBB.Z;

            meshObject.MinBoundingBoxX = minBB.X;
            meshObject.MinBoundingBoxY = minBB.Y;
            meshObject.MinBoundingBoxZ = minBB.Z;


            //TODO: oriented bounding box
            // using Axis aligned for now

            /*Vector3 BBCenter = (maxBB + minBB) / 2;
            Vector3 BBSize = (maxBB + minBB) / 2;
            var OBBTransform = Matrix3.Identity;

            meshObject.UnknownFloats[0] = BBCenter.X;
            meshObject.UnknownFloats[1] = BBCenter.Y;
            meshObject.UnknownFloats[2] = BBCenter.Z;

            meshObject.UnknownFloats[3] = OBBTransform.M11; meshObject.UnknownFloats[4] = OBBTransform.M12; meshObject.UnknownFloats[5] = OBBTransform.M13;
            meshObject.UnknownFloats[6] = OBBTransform.M21; meshObject.UnknownFloats[7] = OBBTransform.M22; meshObject.UnknownFloats[8] = OBBTransform.M23;
            meshObject.UnknownFloats[9] = OBBTransform.M31; meshObject.UnknownFloats[10] = OBBTransform.M32; meshObject.UnknownFloats[11] = OBBTransform.M33;

            meshObject.UnknownFloats[12] = BBSize.X;
            meshObject.UnknownFloats[13] = BBSize.Y;
            meshObject.UnknownFloats[14] = BBSize.Z;*/
        }

        private static SSBHVertexInfluence CreateInfluence(ushort VertexIndex, string BoneName, float Weight)
        {
            return new SSBHVertexInfluence()
            {
                VertexIndex = VertexIndex,
                BoneName = BoneName,
                Weight = Weight
            };
        }

        private static SSBHVertexAttribute vectorToAttribute(Vector2 value)
        {
            return new SSBHVertexAttribute()
            {
                X = value.X,
                Y = value.Y
            };
        }

        private static SSBHVertexAttribute vectorToAttribute(Vector3 value)
        {
            return new SSBHVertexAttribute()
            {
                X = value.X,
                Y = value.Y,
                Z = value.Z
            };
        }

        private static SSBHVertexAttribute vectorToAttribute(Vector4 value)
        {
            return new SSBHVertexAttribute()
            {
                X = value.X,
                Y = value.Y,
                Z = value.Z,
                W = value.W
            };
        }
    }
}
