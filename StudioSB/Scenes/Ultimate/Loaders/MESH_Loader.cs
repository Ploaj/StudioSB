﻿using System;
using SSBHLib;
using SSBHLib.Formats.Meshes;
using System.Collections.Generic;
using OpenTK;
using SSBHLib.Tools;
using StudioSB.Rendering.Bounding;
using StudioSB.Scenes.Ultimate.Loaders;
using StudioSB.Tools;

namespace StudioSB.Scenes.Ultimate
{
    public class MESH_Loader
    {
        public static void Open(string FileName, SBScene Scene)
        {
            SsbhFile File;
            if (Ssbh.TryParseSsbhFile(FileName, out File))
            {
                if (File is Mesh mesh)
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
                    model.BoundingSphere = new Vector4(mesh.BoundingSphereCenter.X, mesh.BoundingSphereCenter.Y, mesh.BoundingSphereCenter.Z, mesh.BoundingSphereRadius);
                    
                    ((SBSceneSSBH)Scene).Model = model;

                    SsbhVertexAccessor accessor = new SsbhVertexAccessor(mesh);
                    {
                        foreach (var meshObject in mesh.Objects)
                        {
                            SBUltimateMesh sbMesh = new SBUltimateMesh();
                            sbMesh.EnableAttributes(meshObject);

                            sbMesh.Name = meshObject.Name;
                            sbMesh.ParentBone = meshObject.ParentBoneName;
                            
                            sbMesh.BoundingSphere = new BoundingSphere(meshObject.BoundingSphereCenter.X, meshObject.BoundingSphereCenter.Y, meshObject.BoundingSphereCenter.Z, meshObject.BoundingSphereRadius);
                            sbMesh.AABoundingBox = new AABoundingBox(new Vector3(meshObject.BoundingBoxMin.X, meshObject.BoundingBoxMin.Y, meshObject.BoundingBoxMin.Z),
                                 new Vector3(meshObject.BoundingBoxMax.X, meshObject.BoundingBoxMax.Y, meshObject.BoundingBoxMax.Z));
                            sbMesh.OrientedBoundingBox = new OrientedBoundingBox(
                                meshObject.OrientedBoundingBoxCenter.ToOpenTK(),
                                meshObject.OrientedBoundingBoxSize.ToOpenTK(),
                                meshObject.OrientedBoundingBoxTransform.ToOpenTK());
                            
                            sbMesh.Indices = new List<uint>(accessor.ReadIndices(0, meshObject.IndexCount, meshObject));
                            sbMesh.Vertices = CreateVertices(mesh, Scene.Skeleton, meshObject, accessor, sbMesh.Indices.ToArray());
                            model.Meshes.Add(sbMesh);
                        }
                    }
                }
            }
        }

        private static List<UltimateVertex> CreateVertices(Mesh mesh, ISBSkeleton Skeleton, MeshObject meshObject, SsbhVertexAccessor vertexAccessor, uint[] vertexIndices)
        {
            // Read attribute values.
            var positions = vertexAccessor.ReadAttribute("Position0", meshObject.VertexCount, meshObject);
            var normals = vertexAccessor.ReadAttribute("Normal0", meshObject.VertexCount, meshObject);
            var tangents = vertexAccessor.ReadAttribute("Tangent0", meshObject.VertexCount, meshObject);
            var map1Values = vertexAccessor.ReadAttribute("map1", meshObject.VertexCount, meshObject);
            var uvSetValues = vertexAccessor.ReadAttribute("uvSet", meshObject.VertexCount, meshObject);
            var uvSet1Values = vertexAccessor.ReadAttribute("uvSet1", meshObject.VertexCount, meshObject);
            var uvSet2Values = vertexAccessor.ReadAttribute("uvSet2", meshObject.VertexCount, meshObject);
            var bake1Values = vertexAccessor.ReadAttribute("bake1", meshObject.VertexCount, meshObject);
            var colorSet1Values = vertexAccessor.ReadAttribute("colorSet1", meshObject.VertexCount, meshObject);
            var colorSet2Values = vertexAccessor.ReadAttribute("colorSet2", meshObject.VertexCount, meshObject);
            var colorSet21Values = vertexAccessor.ReadAttribute("colorSet21", meshObject.VertexCount, meshObject);
            var colorSet22Values = vertexAccessor.ReadAttribute("colorSet22", meshObject.VertexCount, meshObject);
            var colorSet23Values = vertexAccessor.ReadAttribute("colorSet23", meshObject.VertexCount, meshObject);
            var colorSet3Values = vertexAccessor.ReadAttribute("colorSet3", meshObject.VertexCount, meshObject);
            var colorSet4Values = vertexAccessor.ReadAttribute("colorSet4", meshObject.VertexCount, meshObject);
            var colorSet5Values = vertexAccessor.ReadAttribute("colorSet5", meshObject.VertexCount, meshObject);
            var colorSet6Values = vertexAccessor.ReadAttribute("colorSet6", meshObject.VertexCount, meshObject);
            var colorSet7Values = vertexAccessor.ReadAttribute("colorSet7", meshObject.VertexCount, meshObject);

            // Generate bitangents.
            // TODO: Use vec4 tangents instead.
            var positionVectors = GetVectors3d(positions);
            var normalVectors = GetVectors3d(normals);
            var map1Vectors = GetVectors2d(map1Values);
            SFGraphics.Utils.TriangleListUtils.CalculateTangentsBitangents(positionVectors, normalVectors, map1Vectors, (int[])(object)vertexIndices, out Vector3[] tangentVectors, out Vector3[] bitangentVectors);

            var riggingAccessor = new SsbhRiggingAccessor(mesh);
            var influences = riggingAccessor.ReadRiggingBuffer(meshObject.Name, (int)meshObject.SubIndex);
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
                var bitangent = bitangentVectors[i];

                var map1 = GetVector4(map1Values[i]).Xy;

                var uvSet = map1;
                if (uvSetValues.Length != 0)
                    uvSet = GetVector4(uvSetValues[i]).Xy;
                var uvSet1 = new Vector2(0);
                if (uvSet1Values.Length != 0)
                    uvSet1 = GetVector4(uvSet1Values[i]).Xy;
                var uvSet2 = new Vector2(0);
                if (uvSet2Values.Length != 0)
                    uvSet2 = GetVector4(uvSet2Values[i]).Xy;

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

                var colorSet2 = new Vector4(1);
                if (colorSet2Values.Length != 0)
                    colorSet2 = GetVector4(colorSet2Values[i]) / 128.0f;

                var colorSet21 = new Vector4(1);
                if (colorSet21Values.Length != 0)
                    colorSet21 = GetVector4(colorSet21Values[i]) / 128.0f;

                var colorSet22 = new Vector4(1);
                if (colorSet22Values.Length != 0)
                    colorSet22 = GetVector4(colorSet22Values[i]) / 128.0f;

                var colorSet23 = new Vector4(1);
                if (colorSet23Values.Length != 0)
                    colorSet23 = GetVector4(colorSet23Values[i]) / 128.0f;

                var colorSet3 = new Vector4(1);
                if (colorSet3Values.Length != 0)
                    colorSet3 = GetVector4(colorSet3Values[i]) / 128.0f;

                var colorSet4 = new Vector4(1);
                if (colorSet4Values.Length != 0)
                    colorSet4 = GetVector4(colorSet4Values[i]) / 128.0f;

                var colorSet5 = new Vector4(1);
                if (colorSet5Values.Length != 0)
                    colorSet5 = GetVector4(colorSet5Values[i]) / 128.0f;

                var colorSet6 = new Vector4(1);
                if (colorSet6Values.Length != 0)
                    colorSet6 = GetVector4(colorSet6Values[i]) / 128.0f;

                var colorSet7 = new Vector4(1);
                if (colorSet7Values.Length != 0)
                    colorSet7 = GetVector4(colorSet7Values[i]) / 128.0f;

                vertices.Add(new UltimateVertex(position, normal, tangent, bitangent, map1, uvSet, uvSet1, uvSet2, bones, weights, bake1, colorSet1, colorSet2, colorSet21, colorSet22, colorSet23, colorSet3, colorSet4, colorSet5, colorSet6, colorSet7));
            }

            return vertices;
        }

        private static void GetRiggingData(SsbhVertexAttribute[] positions, SsbhVertexInfluence[] influences, Dictionary<string, int> indexByBoneName, out IVec4[] boneIndices, out Vector4[] boneWeights)
        {
            boneIndices = new IVec4[positions.Length];
            boneWeights = new Vector4[positions.Length];
            foreach (SsbhVertexInfluence influence in influences)
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

        private static Vector3[] GetVectors3d(SsbhVertexAttribute[] values)
        {
            var vectors = new Vector3[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                vectors[i] = new Vector3(value.X, value.Y, value.Z);
            }

            return vectors;
        }

        private static Vector2[] GetVectors2d(SsbhVertexAttribute[] values)
        {
            var vectors = new Vector2[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                vectors[i] = new Vector2(value.X, value.Y);
            }

            return vectors;
        }

        private static Vector4 GetVector4(SsbhVertexAttribute values)
        {
            return new Vector4(values.X, values.Y, values.Z, values.W);
        }

        public static Mesh CreateMESH(SBUltimateModel model, SBSkeleton Skeleton, out MESHEX_Loader meshEX)
        {
            SsbhMeshMaker maker = new SsbhMeshMaker();

            string[] BoneNames = new string[Skeleton.Bones.Length];
            int BoneIndex = 0;
            foreach(var bone in Skeleton.Bones)
            {
                BoneNames[BoneIndex++] = bone.Name;
            }

            List<Vector3> allVertices = new List<Vector3>();

            foreach (var mesh in model.Meshes)
            {
                // preprocess

                List<SsbhVertexAttribute> Position0 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> Normal0 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> Tangent0 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> Map1 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> UvSet = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> UvSet1 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> bake1 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet1 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet2 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet21 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet22 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet23 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet3 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet4 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet5 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet6 = new List<SsbhVertexAttribute>();
                List<SsbhVertexAttribute> colorSet7 = new List<SsbhVertexAttribute>();

                List<SsbhVertexInfluence> Influences = new List<SsbhVertexInfluence>();

                List<Vector3> meshVertices = new List<Vector3>();


                // Generate tangent vectors with the appropriate W component.
                // TODO: Preserve tangents for existing models?
                var positionVectors = new List<Vector3>(mesh.Vertices.Count);
                var normalVectors = new List<Vector3>(mesh.Vertices.Count);
                var map1Vectors = new List<Vector2>(mesh.Vertices.Count);
                foreach (var vertex in mesh.Vertices)
                {
                    positionVectors.Add(vertex.Position0);
                    normalVectors.Add(vertex.Normal0);
                    map1Vectors.Add(vertex.Map1);
                }
                SFGraphics.Utils.TriangleListUtils.CalculateTangents(positionVectors, normalVectors, map1Vectors, (int[])(object)mesh.Indices.ToArray(), out Vector4[] tangentVectors);

                int VertexIndex = 0;
                foreach (var vertex in mesh.Vertices)
                {
                    allVertices.Add(vertex.Position0);
                    meshVertices.Add(vertex.Position0);

                    Position0.Add(vectorToAttribute(vertex.Position0));
                    Normal0.Add(vectorToAttribute(vertex.Normal0));
                    // Flip tangent W.
                    Tangent0.Add(vectorToAttribute(new Vector4(tangentVectors[VertexIndex].Xyz, tangentVectors[VertexIndex].W * -1f)));
                    Map1.Add(vectorToAttribute(vertex.Map1));
                    UvSet.Add(vectorToAttribute(vertex.UvSet));
                    UvSet1.Add(vectorToAttribute(vertex.UvSet1));
                    bake1.Add(vectorToAttribute(vertex.Bake1));
                    colorSet1.Add(vectorToAttribute(vertex.ColorSet1 * 128));
                    colorSet2.Add(vectorToAttribute(vertex.ColorSet2 * 128));
                    colorSet21.Add(vectorToAttribute(vertex.ColorSet21 * 128));
                    colorSet22.Add(vectorToAttribute(vertex.ColorSet22 * 128));
                    colorSet23.Add(vectorToAttribute(vertex.ColorSet23 * 128));
                    colorSet3.Add(vectorToAttribute(vertex.ColorSet3 * 128));
                    colorSet4.Add(vectorToAttribute(vertex.ColorSet4 * 128));
                    colorSet5.Add(vectorToAttribute(vertex.ColorSet5 * 128));
                    colorSet6.Add(vectorToAttribute(vertex.ColorSet6 * 128));
                    colorSet7.Add(vectorToAttribute(vertex.ColorSet7 * 128));

                    if (vertex.BoneWeights.X > 0)
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

                // set bounding info

                maker.SetBoundingSphere(mesh.BoundingSphere.X, mesh.BoundingSphere.Y, mesh.BoundingSphere.Z, mesh.BoundingSphere.Radius);
                maker.SetAaBoundingBox(vectorToAttribute(mesh.AABoundingBox.Min), vectorToAttribute(mesh.AABoundingBox.Max));
                var tr = mesh.OrientedBoundingBox.Transform;
                var matxArr = new float[]
                {
                    tr.M11, tr.M12, tr.M13,
                    tr.M21, tr.M22, tr.M23,
                    tr.M31, tr.M32, tr.M33,
                };
                maker.SetOrientedBoundingBox(vectorToAttribute(mesh.OrientedBoundingBox.Position), vectorToAttribute(mesh.OrientedBoundingBox.Size), matxArr);
                //maker.SetOrientedBoundingBox(new SsbhVertexAttribute(), new SsbhVertexAttribute(), new float[9]);

                // Add attributes
                if (mesh.ExportNormal)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.Normal0, Normal0.ToArray());
                if (mesh.ExportTangent)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.Tangent0, Tangent0.ToArray());
                if (mesh.ExportMap1)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.Map1, Map1.ToArray());
                if (mesh.ExportUVSet1)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.UvSet, UvSet.ToArray());
                if (mesh.ExportUVSet2)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.UvSet1, UvSet1.ToArray());
                if (mesh.ExportBake1)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.Bake1, bake1.ToArray());
                if (mesh.ExportColorSet1)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet1, colorSet1.ToArray());
                if (mesh.ExportColorSet2)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet2, colorSet2.ToArray());
                if (mesh.ExportColorSet21)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet21, colorSet21.ToArray());
                if (mesh.ExportColorSet22)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet22, colorSet22.ToArray());
                if (mesh.ExportColorSet23)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet23, colorSet23.ToArray());
                if (mesh.ExportColorSet3)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet3, colorSet3.ToArray());
                if (mesh.ExportColorSet4)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet4, colorSet4.ToArray());
                if (mesh.ExportColorSet5)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet5, colorSet5.ToArray());
                if (mesh.ExportColorSet6)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet6, colorSet6.ToArray());
                if (mesh.ExportColorSet7)
                    maker.AddAttributeToMeshObject(UltimateVertexAttribute.ColorSet7, colorSet7.ToArray());

                // Add rigging
                if (mesh.ParentBone == "")
                    maker.AttachRiggingToMeshObject(Influences.ToArray());
            }

            Mesh meshFile = maker.GetMeshFile();

            model.BoundingSphere = new BoundingSphere(allVertices).XyzRadius;
            model.AABoundingBox = new AABoundingBox(allVertices);
            model.OrientedBoundingBox = new OrientedBoundingBox(allVertices);

            meshFile.BoundingSphereCenter = model.BoundingSphere.Xyz.ToSsbh();
            meshFile.BoundingSphereRadius = model.BoundingSphere.W;

            meshFile.BoundingBoxMax = model.AABoundingBox.Max.ToSsbh();
            meshFile.BoundingBoxMin = model.AABoundingBox.Min.ToSsbh();

            meshFile.OrientedBoundingBoxCenter = new SSBHLib.Formats.Vector3(model.OrientedBoundingBox.Position.X, model.OrientedBoundingBox.Position.Y, model.OrientedBoundingBox.Position.Z);
            meshFile.OrientedBoundingBoxSize = new SSBHLib.Formats.Vector3(model.OrientedBoundingBox.Size.X, model.OrientedBoundingBox.Size.Y, model.OrientedBoundingBox.Size.Z);
            meshFile.OrientedBoundingBoxTransform = model.OrientedBoundingBox.Transform.ToSsbh();

            meshEX = new MESHEX_Loader();
            meshEX.AllBoundingSphere = new BoundingSphere(allVertices);
            /*
            foreach(var m in model.Meshes)
            {
                meshEX.AddMeshData(m.BoundingSphere, m.Name);
            }
            */
            meshEX.AddAllMeshData(model.Meshes);
            return meshFile;
        }
        
        private static SsbhVertexInfluence CreateInfluence(ushort VertexIndex, string BoneName, float Weight)
        {
            return new SsbhVertexInfluence()
            {
                VertexIndex = VertexIndex,
                BoneName = BoneName,
                Weight = Weight
            };
        }

        private static SsbhVertexAttribute vectorToAttribute(Vector2 value)
        {
            return new SsbhVertexAttribute()
            {
                X = value.X,
                Y = value.Y
            };
        }

        private static SsbhVertexAttribute vectorToAttribute(Vector3 value)
        {
            return new SsbhVertexAttribute()
            {
                X = value.X,
                Y = value.Y,
                Z = value.Z
            };
        }

        private static SsbhVertexAttribute vectorToAttribute(Vector4 value)
        {
            return new SsbhVertexAttribute()
            {
                X = value.X,
                Y = value.Y,
                Z = value.Z,
                W = value.W
            };
        }
    }
}
