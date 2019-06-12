using HSDLib.Common;

namespace StudioSB.Scenes.Melee
{
    /// <summary>
    /// A extension of the bone that will also edit an HSD_JOBJ
    /// </summary>
    public class SBHsdBone : SBBone
    {
        private HSD_JOBJ _jobj;

        public HSD_JOBJ GetJOBJ()
        {
            return _jobj;
        }

        public void SetJOBJ(HSD_JOBJ jobj)
        {
            _jobj = jobj;
        }

        public new float X
        {
            get => base.X;
            set
            {
                if(_jobj != null)
                    _jobj.Transforms.TX = value;
                base.X = value;
            }
        }
        public new float Y
        {
            get => base.Y;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.TY = value;
                base.Y = value;
            }
        }
        public new float Z
        {
            get => base.Z;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.TZ = value;
                base.Z = value;
            }
        }


        public new float RX
        {
            get => base.RX;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.RX = value;
                base.RX = value;
            }
        }
        public new float RY
        {
            get => base.RY;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.RY = value;
                base.RY = value;
            }
        }
        public new float RZ
        {
            get => base.RZ;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.RZ = value;
                base.RZ = value;
            }
        }


        public new float SX
        {
            get => base.SX;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.SX = value;
                base.SX = value;
            }
        }
        public new float SY
        {
            get => base.SY;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.SY = value;
                base.SY = value;
            }
        }
        public new float SZ
        {
            get => base.SZ;
            set
            {
                if (_jobj != null)
                    _jobj.Transforms.SZ = value;
                base.SZ = value;
            }
        }
    }
}
