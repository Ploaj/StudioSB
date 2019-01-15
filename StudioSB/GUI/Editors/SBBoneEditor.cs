using System;
using System.Windows.Forms;
using StudioSB.Scenes;
using StudioSB.GUI.Editors;

namespace StudioSB.GUI
{
    /// <summary>
    /// And editor for the SBBone
    /// Bones can be generic representations of any transform, however
    /// </summary>
    public class SBBoneEditor : Panel
    {
        private Label NameLabel;
        private GenericBindingTextBox<float> X, Y, Z;
        private GenericBindingTextBox<float> RX, RY, RZ;
        private GenericBindingTextBox<float> SX, SY, SZ;
        private System.Drawing.Size BoxSize = new System.Drawing.Size(50, 16);
        private System.Drawing.Size LabelSize = new System.Drawing.Size(10, 16);
        private TableLayoutPanel panel;

        private SBPopoutPanel transformPanel;

        public SBBoneEditor()
        {
            ApplicationSettings.SkinControl(this);

            X = new GenericBindingTextBox<float>();
            X.Size = BoxSize;
            Y = new GenericBindingTextBox<float>();
            Y.Size = BoxSize;
            Z = new GenericBindingTextBox<float>();
            Z.Size = BoxSize;

            RX = new GenericBindingTextBox<float>();
            RX.Size = BoxSize;
            RY = new GenericBindingTextBox<float>();
            RY.Size = BoxSize;
            RZ = new GenericBindingTextBox<float>();
            RZ.Size = BoxSize;

            SX = new GenericBindingTextBox<float>();
            SX.Size = BoxSize;
            SY = new GenericBindingTextBox<float>();
            SY.Size = BoxSize;
            SZ = new GenericBindingTextBox<float>();
            SZ.Size = BoxSize;

            panel = new TableLayoutPanel();
            panel.Padding = new Padding(2, 2, 2, 2);
            panel.Margin = new Padding(2, 2, 2, 2);
            panel.Controls.Add(new Label() { Text = "Trans", AutoSize = true, Anchor = AnchorStyles.Bottom }, 1, 0);
            panel.Controls.Add(new Label() { Text = "Rot", AutoSize = true, Anchor = AnchorStyles.Bottom }, 2, 0);
            panel.Controls.Add(new Label() { Text = "Scale", AutoSize = true, Anchor = AnchorStyles.Bottom }, 3, 0);
            panel.Controls.Add(new Label() { Text = "X", MaximumSize = LabelSize, Anchor = AnchorStyles.Right }, 0, 1);
            panel.Controls.Add(new Label() { Text = "Y", MaximumSize = LabelSize, Anchor = AnchorStyles.Right }, 0, 2);
            panel.Controls.Add(new Label() { Text = "Z", MaximumSize = LabelSize, Anchor = AnchorStyles.Right }, 0, 3);
            panel.Controls.Add(X, 1, 1);
            panel.Controls.Add(Y, 1, 2);
            panel.Controls.Add(Z, 1, 3);
            panel.Controls.Add(RX, 2, 1);
            panel.Controls.Add(RY, 2, 2);
            panel.Controls.Add(RZ, 2, 3);
            panel.Controls.Add(SX, 3, 1);
            panel.Controls.Add(SY, 3, 2);
            panel.Controls.Add(SZ, 3, 3);

            panel.RowStyles.Clear();
            for (int i = 0; i < panel.RowCount; i++)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            transformPanel = new SBPopoutPanel(PopoutSide.Bottom, "Transform");
            transformPanel.OpenText = "Transform";
            transformPanel.CloseText = "Transform";
            transformPanel.Contents.Add(panel);
            transformPanel.Dock = DockStyle.Top;

            NameLabel = new Label() { Dock = DockStyle.Top};

            Controls.Add(transformPanel);
            Controls.Add(NameLabel);
        }

        public void BindBone(SBBone Bone)
        {
            NameLabel.Text = Bone.Name;
            X.Bind(Bone, "X");
            Y.Bind(Bone, "Y");
            Z.Bind(Bone, "Z");
            RX.Bind(Bone, "RX");
            RY.Bind(Bone, "RY");
            RZ.Bind(Bone, "RZ");
            SX.Bind(Bone, "SX");
            SY.Bind(Bone, "SY");
            SZ.Bind(Bone, "SZ");
        }

        public void HideControl()
        {
            Visible = false;
            X.UnBind();
            Y.UnBind();
            Z.UnBind();
            RX.UnBind();
            RY.UnBind();
            RZ.UnBind();
            SX.UnBind();
            SY.UnBind();
            SZ.UnBind();
        }
    }
}
