using System;
using System.Windows.Forms;
using StudioSB.Rendering.Bounding;
using StudioSB.Scenes.Ultimate;

namespace StudioSB.GUI.Attachments
{
    /// <summary>
    /// Settings and options exclusive to SSBH scenes
    /// </summary>
    public class SBUltimateSettingsAttachment : GroupBox, IAttachment
    {
        private SBSceneSSBH scene;

        private GroupBox renderSettings;
        private TrackBar materialBlend;
        private GenericBindingComboBox<UltimateMaterialTransitionMode> materialMode;

        public SBUltimateSettingsAttachment()
        {
            Text = "SSBH Settings";
            Dock = DockStyle.Fill;
            ApplicationSettings.SkinControl(this);

            renderSettings = new GroupBox();
            renderSettings.Text = "Render Settings";
            renderSettings.Dock = DockStyle.Top;
            ApplicationSettings.SkinControl(renderSettings);

            materialBlend = new TrackBar();
            materialBlend.Maximum = 100;
            materialBlend.TickFrequency = 1;
            materialBlend.Dock = DockStyle.Top;
            materialBlend.ValueChanged += (sender, args) =>
            {
                if(scene != null)
                {
                    scene.MaterialBlend = materialBlend.Value / 100f;
                }
            };
            ApplicationSettings.SkinControl(materialBlend);

            materialMode = new GenericBindingComboBox<UltimateMaterialTransitionMode>("Material Mode");
            materialMode.Dock = DockStyle.Top;

            renderSettings.Controls.Add(materialBlend);
            renderSettings.Controls.Add(materialMode);

            Controls.Add(renderSettings);

        }

        public bool AllowMultiple()
        {
            return false;
        }

        public string[] Extension()
        {
            return null;
        }

        public void OnAttach(SBViewportPanel viewportPanel)
        {
            viewportPanel.TabPanel.AddTab("Settings", this);
        }

        public void OnRemove(SBViewportPanel viewportPanel)
        {
            scene = null;
            materialBlend.Value = 0;
            materialMode.SelectedValue = UltimateMaterialTransitionMode.Ditto;
        }

        public void Open(string FilePath)
        {
        }

        public bool OverlayScene()
        {
            return true;
        }

        public void Pick(Ray ray)
        {
        }

        public void Render(SBViewport viewport, float frame = 0)
        {
        }

        public void Save(string FilePath)
        {
        }

        public void Step(SBViewport viewport)
        {
        }

        public void Update(SBViewport viewport)
        {
            if (viewport.Scene is SBSceneSSBH ultimate)
            {
                scene = ultimate;
                materialMode.Bind(ultimate, "MaterialMode");
            }
        }
    }
}
