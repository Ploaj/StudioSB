using System.Collections.Generic;
namespace StudioSB.Scenes.Animation
{
    public class SBKeyGroup<T>
    {
        public List<SBAnimKey<T>> Keys = new List<SBAnimKey<T>>();
        
        public SBAnimKey<T> GetKey(float Frame)
        {
            //TODO: actually grab the right frame

            if (Frame >= Keys.Count)
                return Keys[0];

            return Keys[(int)Frame];
        }

        public T GetValue(float Frame)
        {
            return GetKey(Frame).Value;
        }
    }
}
