using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using StudioSB.Rendering.Shapes;
using StudioSB.Rendering;
using SFGraphics.Cameras;
using IONET.Core.Skeleton;

namespace StudioSB.Scenes
{
    /// <summary>
    /// A very generic implementation of a skeleton
    /// Can be easily shared between many scene types
    /// </summary>
    public class SBSkeleton : ISBSkeleton
    {
        private List<SBBone> RootBones = new List<SBBone>();

        private Dictionary<string, SBBone> BoneNameToBone = new Dictionary<string, SBBone>();

        public SBBone this[string i]
        {
            get
            {
                if (BoneNameToBone == null || !BoneNameToBone.ContainsKey(i) || BoneNameToBone[i].Name != i)
                {
                    BoneNameToBone.Clear();
                    foreach (var bone in Bones)
                    {
                        if (!BoneNameToBone.ContainsKey(bone.Name))
                            BoneNameToBone.Add(bone.Name, bone);
                    }
                }
                if (!BoneNameToBone.ContainsKey(i))
                    return null;
                return BoneNameToBone[i];
            }
        }

        public SBBone[] Roots
        {
            get
            {
                return RootBones.ToArray();
            }
        }

        public SBBone[] Bones
        {
            get
            {
                return GetBones().ToArray();
            }
        }

        private List<SBBone> GetBones()
        {
            List<SBBone> Bones = new List<SBBone>();
            foreach (var bone in RootBones)
            {
                QueueBones(bone, Bones);
            }
            return Bones;
        }

        private void QueueBones(SBBone Bone, List<SBBone> Bones)
        {
            Bones.Add(Bone);
            foreach (var child in Bone.Children)
                QueueBones(child, Bones);
        }

        public void AddRoot(SBBone bone)
        {
            RootBones.Add(bone);
        }

        public bool ContainsBone(string Name)
        {
            return this[Name] != null;
        }

        public void Reset()
        {
            foreach (var v in Bones)
                v.AnimatedTransform = v.Transform;
        }

        #region Rendering
        
        //Rendering
        private static BonePrism bonePrism;
        private static Matrix4 prismRotation = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), 1.5708f);

        public void RenderLegacy()
        {
            GL.LineWidth(2f);
            GL.PointSize(5f);
            foreach (var bone in Bones)
            {
                if (bone.Parent != null)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(0f, 1f, 0f);
                    GL.Vertex3(Vector3.TransformPosition(Vector3.Zero, bone.AnimatedWorldTransform));
                    GL.Color3(0f, 0f, 1f);
                    GL.Vertex3(Vector3.TransformPosition(Vector3.Zero, bone.Parent.AnimatedWorldTransform));
                    GL.End();
                }
                GL.Color3(1f, 0.25f, 0.25f);
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(Vector3.TransformPosition(Vector3.Zero, bone.WorldTransform));
                GL.End();
            }
        }

        public void RenderShader(Camera camera)
        {
            GL.LineWidth(1f);

            if (bonePrism == null)
                bonePrism = new BonePrism();

            var boneShader = ShaderManager.GetShader("Bone");

            boneShader.UseProgram();

            boneShader.SetMatrix4x4("mvpMatrix", camera.MvpMatrix);

            var color = Tools.CrossMath.ColorToVector(ApplicationSettings.BoneColor);

            var selcolor = Tools.CrossMath.ColorToVector(ApplicationSettings.SelectedBoneColor);

            boneShader.SetMatrix4x4("rotation", ref prismRotation);

            foreach (var b in Bones)
            {
                if (b.Selected)
                    boneShader.SetVector4("color", selcolor);
                else
                    boneShader.SetVector4("color", color);

                Matrix4 transform = b.AnimatedWorldTransform;
                boneShader.SetMatrix4x4("bone", ref transform);
                boneShader.SetInt("hasParent", b.Parent != null ? 1 : 0);
                if (b.Parent != null)
                {
                    Matrix4 parenttransform = b.Parent.AnimatedWorldTransform;
                    boneShader.SetMatrix4x4("parent", ref parenttransform);
                }
                bonePrism.Draw(boneShader);

                // leaf node
                boneShader.SetInt("hasParent", 0);
                bonePrism.Draw(boneShader);
            }

            if(ApplicationSettings.RenderBoneNames)
            foreach (var b in Bones)
            {
                TextRenderer.Draw(camera, b.Name, b.AnimatedWorldTransform);
            }
        }

        public void ClearSelection()
        {
            foreach(var b in Bones)
            {
                b.Selected = false;
            }
        }

        public int IndexOfBone(SBBone bone)
        {
            int index = 0;
            foreach(var b in Bones)
            {
                if (b == bone)
                    return index;
                index++;
            }
            return -1;
        }

        public Matrix4[] GetBindTransforms()
        {
            List<Matrix4> transforms = new List<Matrix4>();
            foreach(var bone in Bones)
            {
                transforms.Add(bone.AnimatedBindMatrix);
            }
            return transforms.ToArray();
        }

        public Matrix4[] GetWorldTransforms()
        {
            List<Matrix4> transforms = new List<Matrix4>();
            foreach (var bone in Bones)
            {
                transforms.Add(bone.WorldTransform);
            }
            return transforms.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        public static SBSkeleton FromIOSkeleton(IOSkeleton ioskel)
        {
            SBSkeleton skel = new SBSkeleton();

            Dictionary<IOBone, SBBone> iotosb = new Dictionary<IOBone, SBBone>();
            
            foreach(var iobone in ioskel.BreathFirstOrder())
            {
                SBConsole.WriteLine(iobone.Name + " " + iobone.Scale + " " + iobone.Rotation + " " + iobone.Translation);
                SBBone bone = new SBBone()
                {
                    Name = iobone.Name,
                    SX = iobone.ScaleX,
                    SY = iobone.ScaleY,
                    SZ = iobone.ScaleZ,
                    RX = iobone.RotationEuler.X,
                    RY = iobone.RotationEuler.Y,
                    RZ = iobone.RotationEuler.Z,
                    X = iobone.TranslationX,
                    Y = iobone.TranslationY,
                    Z = iobone.TranslationZ,
                };

                SBConsole.WriteLine(bone.Name + " " + bone.Translation.ToString() + " " + bone.Scale.ToString() + " " + bone.RotationQuaternion.ToString());

                iotosb.Add(iobone, bone);

                if (iobone.Parent == null)
                    skel.AddRoot(bone);
                else
                    bone.Parent = iotosb[iobone.Parent];
            }

            return skel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IOSkeleton ToIOSkeleton()
        {
            IOSkeleton ioskel = new IOSkeleton();

            Dictionary<SBBone, IOBone> sbtoio = new Dictionary<SBBone, IOBone>();

            foreach (var b in GetBones())
            {
                IOBone bone = new IOBone()
                {
                    Name = b.Name,
                    Scale = new System.Numerics.Vector3(b.SX, b.SY, b.SZ),
                    RotationEuler = new System.Numerics.Vector3(b.RX, b.RY, b.RZ),
                    Translation = new System.Numerics.Vector3(b.X, b.Y, b.Z),
                };

                sbtoio.Add(b, bone);

                if (b.Parent == null)
                    ioskel.RootBones.Add(bone);
                else
                    bone.Parent = sbtoio[b.Parent];
            }

            return ioskel;
        }

        #endregion
    }

    
}
