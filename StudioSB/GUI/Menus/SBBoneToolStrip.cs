using System.Windows.Forms;
using StudioSB.Scenes;

namespace StudioSB.GUI.Menus
{
    public class SBBoneToolStrip : SBToolStrip
    {
        private ToolStripLabel BoneName;
        private SBToolStripFloatBox X;
        private SBToolStripFloatBox Y;
        private SBToolStripFloatBox Z;

        private SBToolStripFloatBox RX;
        private SBToolStripFloatBox RY;
        private SBToolStripFloatBox RZ;

        private SBToolStripFloatBox SX;
        private SBToolStripFloatBox SY;
        private SBToolStripFloatBox SZ;

        public SBBoneToolStrip() : base()
        {
            BoneName = new ToolStripLabel();
            System.Drawing.Size BoxSize = new System.Drawing.Size(32, 16);

            X = new SBToolStripFloatBox();
            X.TextBox.MaximumSize = BoxSize;
            Y = new SBToolStripFloatBox();
            Y.TextBox.MaximumSize = BoxSize;
            Z = new SBToolStripFloatBox();
            Z.TextBox.MaximumSize = BoxSize;

            RX = new SBToolStripFloatBox();
            RX.TextBox.MaximumSize = BoxSize;
            RY = new SBToolStripFloatBox();
            RY.TextBox.MaximumSize = BoxSize;
            RZ = new SBToolStripFloatBox();
            RZ.TextBox.MaximumSize = BoxSize;

            SX = new SBToolStripFloatBox();
            SX.TextBox.MaximumSize = BoxSize;
            SY = new SBToolStripFloatBox();
            SY.TextBox.MaximumSize = BoxSize;
            SZ = new SBToolStripFloatBox();
            SZ.TextBox.MaximumSize = BoxSize;

            Items.Add(BoneName);
            Items.Add(new ToolStripSeparator());
            Items.Add(new ToolStripLabel() { Text = "X:" });
            Items.Add(X);
            Items.Add(new ToolStripLabel() { Text = "Y:" });
            Items.Add(Y);
            Items.Add(new ToolStripLabel() { Text = "Z:" });
            Items.Add(Z);
            Items.Add(new ToolStripSeparator());
            Items.Add(new ToolStripLabel() { Text = "RX:" });
            Items.Add(RX);
            Items.Add(new ToolStripLabel() { Text = "RY:" });
            Items.Add(RY);
            Items.Add(new ToolStripLabel() { Text = "RZ:" });
            Items.Add(RZ);
            Items.Add(new ToolStripSeparator());
            Items.Add(new ToolStripLabel() { Text = "SX:" });
            Items.Add(SX);
            Items.Add(new ToolStripLabel() { Text = "SY:" });
            Items.Add(SY);
            Items.Add(new ToolStripLabel() { Text = "SZ:" });
            Items.Add(SZ);
        }

        public void SetBone(SBBone Bone)
        {
            BoneName.Text = Bone.Name;
            X.BindValue(Bone, "X");
            Y.BindValue(Bone, "Y");
            Z.BindValue(Bone, "Z");
            RX.BindValue(Bone, "RX");
            RY.BindValue(Bone, "RY");
            RZ.BindValue(Bone, "RZ");
            SX.BindValue(Bone, "SX");
            SY.BindValue(Bone, "SY");
            SZ.BindValue(Bone, "SZ");
        }

        public void HideControl()
        {
            X.Unbind();
            Y.Unbind();
            Z.Unbind();
            RX.Unbind();
            RY.Unbind();
            RZ.Unbind();
            SX.Unbind();
            SY.Unbind();
            SZ.Unbind();
            Visible = false;
        }
    }
}
