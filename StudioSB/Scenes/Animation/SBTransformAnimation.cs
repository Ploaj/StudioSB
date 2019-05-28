using System;
using System.Collections.Generic;
using OpenTK;

namespace StudioSB.Scenes.Animation
{
    /// <summary>
    /// Stores animation data for transformations
    /// Information is stored in tracks <see cref="SBTransformTrack"/>
    /// </summary>
    public class SBTransformAnimation
    {
        public string Name { get; set; }

        public List<SBTransformTrack> Tracks = new List<SBTransformTrack>();

        public float GetTrackValueAt(float frame, SBTrackType type)
        {
            var track = Tracks.Find(e => e.Type == type);

            if (track == null)
                return 0;

            return track.Keys.GetValue(frame);
        }

        public Matrix4 GetTransformAt(float Frame, SBSkeleton skeleton)
        {
            return GetTransformAt(Frame, skeleton[Name]);
        }

        public Matrix4 GetTransformAt(float Frame, SBBone bone)
        {
            if (bone == null)
                return Matrix4.Identity;

            SBBone temp = new SBBone();
            temp.Transform = bone.Transform;

            // temp for less matrix calculations
            Vector3 newPos = temp.Translation;
            Vector3 newRot = temp.RotationEuler;
            Vector3 newSca = temp.Scale;

            foreach (var track in Tracks)
            {
                switch (track.Type)
                {
                    case SBTrackType.TranslateX:
                        newPos.X = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.TranslateY:
                        newPos.Y = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.TranslateZ:
                        newPos.Z = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.RotateX:
                        newRot.X = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.RotateY:
                        newRot.Y = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.RotateZ:
                        newRot.Z = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.ScaleX:
                        newSca.X = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.ScaleY:
                        newSca.Y = track.Keys.GetValue(Frame);
                        break;
                    case SBTrackType.ScaleZ:
                        newSca.Z = track.Keys.GetValue(Frame);
                        break;
                }
            }

            temp.Scale = newSca;
            temp.Translation = newPos;
            temp.RotationEuler = newRot;

            return temp.Transform;
        }
    }

}
