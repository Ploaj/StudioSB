namespace StudioSB.Scenes.Animation
{
    public enum InterpolationType
    {
        Linear,
        Hermite,
        Step
    }

    public class SBAnimKey<T>
    {
        public float Frame { get; set; } // todo: this needs to be read only or something
        public T Value;
        public InterpolationType InterpolationType { get; set; }
    }
}
