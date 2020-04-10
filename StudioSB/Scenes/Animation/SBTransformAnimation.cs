using System;
using System.Collections.Generic;
using OpenTK;
using StudioSB.Scenes.Melee;

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

        public bool UseQuat => Tracks.Find(e => e.Type == SBTrackType.RotateW) != null;

        public bool HasTranslation => Tracks.Find(e => e.Type == SBTrackType.TranslateX || e.Type == SBTrackType.TranslateY || e.Type == SBTrackType.TranslateZ) != null;

        public bool HasRotation => Tracks.Find(e => e.Type == SBTrackType.RotateX || e.Type == SBTrackType.RotateY || e.Type == SBTrackType.RotateZ || e.Type == SBTrackType.RotateW) != null;

        public bool HasScale => Tracks.Find(e => e.Type == SBTrackType.ScaleX || e.Type == SBTrackType.ScaleY || e.Type == SBTrackType.ScaleZ) != null;

        /// <summary>
        /// adds a new key frame to the animation
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="interpolationType"></param>
        public void AddKey(float frame, float value, SBTrackType type, InterpolationType interpolationType = InterpolationType.Linear)
        {
            var track = Tracks.Find(e => e.Type == type);
            if(track == null)
            {
                track = new SBTransformTrack(type);
                Tracks.Add(track);
            }
            track.AddKey(frame, value, interpolationType);
        }

        /// <summary>
        /// gets the interpolated values for track type at frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public float GetTrackValueAt(float frame, SBTrackType type)
        {
            var track = Tracks.Find(e => e.Type == type);

            if (track == null)
                return 0;

            return track.Keys.GetValue(frame);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Frame"></param>
        /// <param name="skeleton"></param>
        /// <returns></returns>
        public Matrix4 GetTransformAt(float Frame, SBSkeleton skeleton)
        {
            return GetTransformAt(Frame, skeleton[Name]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Frame"></param>
        /// <param name="bone"></param>
        /// <returns></returns>
        public Matrix4 GetTransformAt(float Frame, SBBone bone)
        {
            if (bone == null)
                return Matrix4.Identity;

            // temp for less matrix calculations
            Vector3 newPos = new Vector3(bone.X, bone.Y, bone.Z);
            Vector3 newRot = new Vector3(bone.RX, bone.RY, bone.RZ);
            Vector3 newSca = bone.Scale;

            if (bone is SBHsdBone hsdBone)
            {
                newRot = new Vector3(hsdBone.RX, hsdBone.RY, hsdBone.RZ);
            }

            bool useQuatRotation = false;
            float f1 = 0;
            float f2 = 0;
            var q1 = new Quaternion();
            var q2 = new Quaternion();

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
                        q1.X = track.Keys.GetKey(Frame).Value;
                        q2.X = track.Keys.GetNextKey(Frame).Value;
                        break;
                    case SBTrackType.RotateY:
                        newRot.Y = track.Keys.GetValue(Frame);
                        q1.Y = track.Keys.GetKey(Frame).Value;
                        q2.Y = track.Keys.GetNextKey(Frame).Value;
                        break;
                    case SBTrackType.RotateZ:
                        newRot.Z = track.Keys.GetValue(Frame);
                        q1.Z = track.Keys.GetKey(Frame).Value;
                        q2.Z = track.Keys.GetNextKey(Frame).Value;
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
                    case SBTrackType.RotateW:
                        useQuatRotation = true;
                        q1.W = track.Keys.GetKey(Frame).Value;
                        q2.W = track.Keys.GetNextKey(Frame).Value;
                        f1 = track.Keys.GetKey(Frame).Frame;
                        f2 = track.Keys.GetNextKey(Frame).Frame;
                        break;
                }
            }

            SBBone temp = new SBBone();
            temp.Transform = Matrix4.Identity;

            temp.Scale = newSca;
            temp.Translation = newPos;
            if (useQuatRotation)
                temp.RotationQuaternion = Quaternion.Slerp(q1, q2, (Frame - f1) / (f2 - f1));
            else
                temp.RotationEuler = newRot;

            return temp.Transform;
        }

        
        /// <summary>
        /// 
        /// </summary>
        public void ConvertRotationToEuler(int FrameCount)
        {
            if (!UseQuat)
                return;

            var xtrack = Tracks.Find(e=>e.Type == SBTrackType.RotateX);
            var ytrack = Tracks.Find(e => e.Type == SBTrackType.RotateY);
            var ztrack = Tracks.Find(e => e.Type == SBTrackType.RotateZ);
            var wtrack = Tracks.Find(e => e.Type == SBTrackType.RotateW);

            SBTransformTrack eulX = new SBTransformTrack(SBTrackType.RotateX);
            SBTransformTrack eulY = new SBTransformTrack(SBTrackType.RotateY);
            SBTransformTrack eulZ = new SBTransformTrack(SBTrackType.RotateZ);

            var dummyBone = new SBBone();
            dummyBone.Transform = Matrix4.Identity;

            for(int i = 0; i < FrameCount; i++)
            {
                var key = GetTransformAt(i, dummyBone);
                var eul = Tools.CrossMath.ToEulerAngles(key.ExtractRotation().Inverted());

                eulX.AddKey(i, eul.X);
                eulY.AddKey(i, eul.Y);
                eulZ.AddKey(i, eul.Z);
            }

            Tracks.Remove(xtrack);
            Tracks.Remove(ytrack);
            Tracks.Remove(ztrack);
            Tracks.Remove(wtrack);

            Tracks.Add(eulX);
            Tracks.Add(eulY);
            Tracks.Add(eulZ);
        }

    }

}
