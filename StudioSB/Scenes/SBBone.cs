using System.Collections.Generic;
using OpenTK;

namespace StudioSB.Scenes
{
    /// <summary>
    /// A very generic implementation of a bone
    /// used by <see cref="SBSkeleton"/>
    /// </summary>
    public class SBBone
    {
        public string Name;

        public int Type { get; set; }

        public SBBone Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if (_parent != null)
                    _parent.RemoveChild(this);
                _parent = value;
                if (_parent != null)
                    _parent.AddChild(this);
            }
        }
        private SBBone _parent;

        public SBBone[] Children
        {
            get
            {
                return _children.ToArray();
            }
        }
        private List<SBBone> _children = new List<SBBone>();
        
        public float X
        {
            get
            {
                return Translation.X;
            }
            set
            {
                Translation = new Vector3(value, Translation.Y, Translation.Z);
            }
        }

        public float Y
        {
            get
            {
                return Translation.Y;
            }
            set
            {
                Translation = new Vector3(Translation.X, value, Translation.Z);
            }
        }

        public float Z
        {
            get
            {
                return Translation.Z;
            }
            set
            {
                Translation = new Vector3(Translation.X, Translation.Y, value);
            }
        }

        public float RX
        {
            get
            {
                return RotationEuler.X;
            }
            set
            {
                RotationEuler = new Vector3(value, RotationEuler.Y, RotationEuler.Z);
            }
        }

        public float RY
        {
            get
            {
                return RotationEuler.Y;
            }
            set
            {
                RotationEuler = new Vector3(RotationEuler.X, value, RotationEuler.Z);
            }
        }
        
        public float RZ
        {
            get
            {
                return RotationEuler.Z;
            }
            set
            {
                RotationEuler = new Vector3(RotationEuler.X, RotationEuler.Y, value);
            }
        }

        public float SX
        {
            get
            {
                return Scale.X;
            }
            set
            {
                Scale = new Vector3(value, Scale.Y, Scale.Z);
            }
        }

        public float SY
        {
            get
            {
                return Scale.Y;
            }
            set
            {
                Scale = new Vector3(Scale.X, value, Scale.Z);
            }
        }

        public float SZ
        {
            get
            {
                return Scale.Z;
            }
            set
            {
                Scale = new Vector3(Scale.X, Scale.Y, value);
            }
        }

        public Vector3 Translation
        {
            get
            {
                return Transform.ExtractTranslation();
            }
            set
            {
                CreateTransform(value, RotationQuaternion, Scale);
            }
        }
        public Vector3 RotationEuler
        {
            get
            {
                return Tools.CrossMath.ToEulerAngles(RotationQuaternion.Inverted());
            }
            set
            {
                CreateTransform(Translation, Tools.CrossMath.ToQuaternion(value), Scale);
            }
        }
        public Quaternion RotationQuaternion
        {
            get
            {
                return Transform.ExtractRotation();
            }
            set
            {
                CreateTransform(Translation, value, Scale);
            }
        }
        public Vector3 Scale
        {
            get
            {
                return Transform.ExtractScale();
            }
            set
            {
                CreateTransform(Translation, RotationQuaternion, value);
            }
        }

        public Matrix4 Transform
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform = value;
                AnimatedTransform = value;
            }
        }
        private Matrix4 _transform;

        public Matrix4 AnimatedTransform { get; set; }

        public float AnimatedCompensateScale { get; set; }
        public bool EnableAnimatedCompensateScale { get; set; } = false;

        public Matrix4 AnimatedBindMatrix
        {
            get
            {
                return InvWorldTransform * AnimatedWorldTransform;
            }
        }

        public Matrix4 AnimatedWorldTransform
        {
            get
            {
                if (EnableAnimatedCompensateScale)
                {
                    Matrix4 transform = AnimatedTransform;
                    if (_parent != null)
                    {
                        Vector3 parentScale = AnimatedTransform.ExtractScale();
                        transform *= Matrix4.CreateScale(
                            1f / parentScale.X,
                            1f / parentScale.Y,
                            1f / parentScale.Z);
                        transform *= _parent.AnimatedWorldTransform;
                    }
                    return transform;
                }
                else
                    return _parent == null ? AnimatedTransform : AnimatedTransform * _parent.AnimatedWorldTransform;
            }
        }

        public Matrix4 WorldTransform
        {
            get
            {
                return _parent == null ? Transform : Transform * _parent.WorldTransform;
            }
        }

        public Matrix4 InvWorldTransform
        {
            get
            {
                return WorldTransform.Inverted();
            }
        }

        /// <summary>
        /// Creates a new transform and sets it
        /// </summary>
        /// <param name="Trans"></param>
        /// <param name="Rot"></param>
        /// <param name="Sca"></param>
        private void CreateTransform(Vector3 Trans, Quaternion Rot, Vector3 Sca)
        {
            Transform = Matrix4.CreateScale(Sca) *
                Matrix4.CreateFromQuaternion(Rot) *
                Matrix4.CreateTranslation(Trans);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Name of bone</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Removes a child node from the skeleton
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(SBBone child)
        {
            if (_children.Contains(child))
            {
                child.Parent = null;
                _children.Remove(child);
            }
        }

        /// <summary>
        /// Adds a child node to the skeleton
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(SBBone child)
        {
            _children.Add(child);
        }
    }
}
