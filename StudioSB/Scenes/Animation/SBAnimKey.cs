namespace StudioSB.Scenes.Animation
{
    public enum InterpolationType
    {
        Constant,
        Linear,
        Hermite,
        Step
    }

    public class SBAnimKey<T>
    {
        public float Frame { get; set; } // todo: this needs to be read only or something
        public float InTan { get; set; }
        public float OutTan { get; set; }
        public T Value;
        public InterpolationType InterpolationType { get; set; }
    }
}
