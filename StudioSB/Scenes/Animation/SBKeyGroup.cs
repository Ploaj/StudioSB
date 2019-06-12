using OpenTK;
using System;
using System.Collections.Generic;

namespace StudioSB.Scenes.Animation
{
    public class SBKeyGroup<T>
    {
        /// <summary>
        /// A read-only view of the keys
        /// </summary>
        public IList<SBAnimKey<T>> Keys
        {
            get
            {
                return _keys.Values;
            }
        }

        private SortedList<float, SBAnimKey<T>> _keys = new SortedList<float, SBAnimKey<T>>();

        public void AddKey(float frame, T value, InterpolationType type = InterpolationType.Linear, float TanIn = 0, float TanOut = float.MaxValue)
        {
            if (_keys.ContainsKey(frame))
                throw new System.Exception("Two keys cannot share a frame");

            SBAnimKey<T> key = new SBAnimKey<T>();
            key.Frame = frame;
            key.Value = value;
            key.InTan = TanIn;
            key.OutTan = TanOut == float.MaxValue ? TanIn : TanOut;
            key.InterpolationType = type;
            _keys.Add(frame, key);
        }

        /// <summary>
        /// Removes key frame at
        /// </summary>
        /// <param name="frame"></param>
        public void RemoveKey(float frame)
        {
            if(_keys.ContainsKey(frame))
                _keys.Remove(frame);
        }
        
        public SBAnimKey<T> GetKey(float Frame)
        {
            int left = BinarySearchKeys(Frame);

            return _keys.Values[left];
        }
        
        private int BinarySearchKeys(float frame)
        {
            int lower = 0;
            int upper = _keys.Count - 1;
            int middle = 0;

            while (lower <= upper)
            {
                middle = (upper + lower) / 2;
                if (frame == _keys.Values[middle].Frame)
                    return middle;
                else 
                if (frame < _keys.Values[middle].Frame)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }
            

            return Math.Max(lower < upper ? lower : upper, 0);
        }

        /// <summary>
        /// Gets the interpolated value at given frame
        /// </summary>
        /// <param name="Frame"></param>
        /// <returns></returns>
        public T GetValue(float Frame)
        {
            if (_keys.Count == 1)
                return _keys.Values[0].Value;

            int left = BinarySearchKeys(Frame);
            int right = left + 1;

            if(right >= _keys.Count)
                return _keys.Values[left].Value;

            if (_keys.Values[left].Value is float)
            {
                if (_keys.Values[left].InterpolationType == InterpolationType.Step
                    || _keys.Values[left].InterpolationType == InterpolationType.Constant)
                {
                    return (T)(object)_keys.Values[left].Value;
                }
                if (_keys.Values[left].InterpolationType == InterpolationType.Linear)
                {
                    float leftValue = (float)(object)_keys.Values[left].Value;
                    float rightValue = (float)(object)_keys.Values[right].Value;
                    float leftFrame = _keys.Keys[left];
                    float rightFrame = _keys.Keys[right];

                    float value = Interpolation.Lerp(leftValue, rightValue, leftFrame, rightFrame, Frame);

                    if (float.IsNaN(value))
                        value = 0;

                    return (T)(object)value;
                }
                if (_keys.Values[left].InterpolationType == InterpolationType.Hermite)
                {
                    float leftValue = (float)(object)_keys.Values[left].Value;
                    float rightValue = (float)(object)_keys.Values[right].Value;
                    float leftTan = _keys.Values[left].OutTan;
                    float rightTan = _keys.Values[right].InTan;
                    float leftFrame = _keys.Keys[left];
                    float rightFrame = _keys.Keys[right];

                    float value = Interpolation.Hermite(Frame, leftFrame, rightFrame, leftTan, rightTan, leftValue, rightValue);

                    if (float.IsNaN(value))
                        value = 0;

                    return (T)(object)value;
                }
            }
            if (_keys.Values[left].Value is Vector4)
            {
                if (_keys.Values[left].InterpolationType == InterpolationType.Linear)
                {
                    var leftValue = (Vector4)(object)_keys.Values[left].Value;
                    var rightValue = (Vector4)(object)_keys.Values[right].Value;
                    float leftFrame = _keys.Keys[left];
                    float rightFrame = _keys.Keys[right];

                    float t = (Frame - leftFrame) / (rightFrame - leftFrame);

                    var value = Vector4.Lerp(leftValue, rightValue, t);

                    return (T)(object)value;
                }
            }

            return _keys.Values[left].Value;
        }
    }
}
