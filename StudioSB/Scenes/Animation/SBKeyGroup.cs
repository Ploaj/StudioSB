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

        public void AddKey(float frame, T value, InterpolationType type = InterpolationType.Linear)
        {
            if (_keys.ContainsKey(frame))
                throw new System.Exception("Two keys cannot share a frame");

            SBAnimKey<T> key = new SBAnimKey<T>();
            key.Frame = frame;
            key.Value = value;
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

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                if (upper == lower)
                    return lower;
                if (frame == _keys.Values[middle].Frame)
                    return middle;
                else if (frame < _keys.Values[middle].Frame)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return lower;
        }

        /// <summary>
        /// Gets the interpolated value at given frame
        /// </summary>
        /// <param name="Frame"></param>
        /// <returns></returns>
        public T GetValue(float Frame)
        {
            int left = BinarySearchKeys(Frame);
            int right = left + 1;

            if(left == 0 || right >= _keys.Count)
                return _keys.Values[left].Value;

            if (_keys.Values[left].Value is float)
            {
                if(_keys.Values[left].InterpolationType == InterpolationType.Linear)
                {
                    float leftValue = (float)(object)_keys.Values[left].Value;
                    float rightValue = (float)(object)_keys.Values[right].Value;
                    float leftFrame = _keys.Keys[left];
                    float rightFrame = _keys.Keys[right];

                    return (T)(object)Interpolation.Lerp(leftValue, rightValue, leftFrame, rightFrame, Frame);
                }
            }

            return _keys.Values[left].Value;
        }
    }
}
