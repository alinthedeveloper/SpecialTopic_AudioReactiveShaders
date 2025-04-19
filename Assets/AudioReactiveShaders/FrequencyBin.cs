using UnityEngine;

namespace AudioTools
{
    [System.Serializable]
    // This class represents a frequency bin for audio processing.
    // It is essentially a range of frequencies and the amplitude of those frequencies.
    public class FrequencyBin
    {
        // Serialized fields for Unity Inspector
        [Header("Frequency Bin Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private string name;
        [SerializeField] private float minRange;
        [SerializeField] private float maxRange;
        [SerializeField] [Min(0f)] private float minAmplitude;
        [SerializeField] [Min(0f)] private float maxAmplitude;
        [SerializeField, Range(0f, 1f)] private float smoothingFactor = 0.1f;
        [SerializeField] private float cooldownTime = 0.2f;

        // Properties for external access
        public float NormalizedAmplitude { get; private set; } = 0f;
        public float PulseThreshold => minAmplitude + (maxAmplitude - minAmplitude) * 0.5f;
        public bool PulseActive {get; private set;} = false;
        public float SmoothingFactor => smoothingFactor;
        public string Name => name;
        public float MinRange => minRange;
        public float MaxRange => maxRange;
        public float MinAmplitude => minAmplitude;
        public float MaxAmplitude => maxAmplitude;
        public bool DebugMode => debugMode;

        // Private fields for internal use
        private float _previousAmplitude = 0f;
        private float _pulseCooldown = 0f;
        public FrequencyBin(string name, float minRange, float maxRange, float minAmplitude, float maxAmplitude)
        {
            this.name = name;
            this.minRange = minRange;
            this.maxRange = maxRange;
            this.minAmplitude = minAmplitude;
            this.maxAmplitude = maxAmplitude;
        }

        // This method detects peaks in the audio signal based on the current normalized amplitude.
        // This is used to determine if a "pulse" is active.
        // Currently the audio processor doesn't use this method in a meaningful way but it is here for future use.
        public void DetectPeaks(float currentNormalizedAmplitude, float deltaTime)
        {
            _pulseCooldown -= deltaTime;

            if (!_previousAmplitude.Equals(0) 
            && currentNormalizedAmplitude > PulseThreshold 
            && _previousAmplitude > PulseThreshold
            && _pulseCooldown <= 0f)
            {
                PulseActive = true;
                _pulseCooldown = cooldownTime;
                if (debugMode)
                {
                    Debug.Log($"[{name}] Pulse detected! Amplitude: {currentNormalizedAmplitude:F3}");
                }
            }
            else
            {
                PulseActive = false;
            }

            _previousAmplitude = currentNormalizedAmplitude;
        }

        // This method normalizes the amplitude of the audio signal based on the current amplitude in comparison to the min and max amplitude.
        public void UpdateAmplitude(float currentAmplitude)
        {
            NormalizedAmplitude = Mathf.Clamp01((currentAmplitude - MinAmplitude) / (MaxAmplitude - MinAmplitude));

            if (debugMode)
            {
                Debug.Log($"[{name}] Amplitude: {currentAmplitude:F3}, Normalized: {NormalizedAmplitude:F3}");
            }
        }
    }
}
