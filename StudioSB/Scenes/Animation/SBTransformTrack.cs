using System;

namespace StudioSB.Scenes.Animation
{
    public enum SBTrackType
    {
        TranslateX,
        TranslateY,
        TranslateZ,
        RotateX,
        RotateY,
        RotateZ,
        ScaleX,
        ScaleY,
        ScaleZ,
        CompensateScale
    }

    /// <summary>
    /// A track for <see cref="SBTransformAnimation"/>
    /// See <see cref="SBTrackType"/> for supported types
    /// </summary>
    public class SBTransformTrack
    {
        public SBTrackType Type { get; internal set; }

        public SBKeyGroup<float> Keys { get; } = new SBKeyGroup<float>();

        public SBTransformTrack(SBTrackType type)
        {
            Type = type;
        }

        public float GetValueAt(float frame)
        {
            return Keys.GetValue(frame);
        }

        public void AddKey(float frame, float value, InterpolationType interpolationType = InterpolationType.Linear)
        {
            Keys.AddKey(frame, value, interpolationType);
        }
    }
}
