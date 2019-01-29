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
        public float Frame;
        public T Value;
        public InterpolationType InterpolationType { get; set; }
        
        public float CompensateScale { get; set; } = 0;
    }
}
