namespace AudioTools
{
    public interface IFrequencyBin
    {
        string Name { get; }
        float MinRange { get; }
        float MaxRange { get; }
        float MinAmplitude { get; set; }
        float MaxAmplitude { get; set; }
        float NormalizedAmplitude { get; }
        void UpdateAmplitude(float currentAmplitude);
    }
}