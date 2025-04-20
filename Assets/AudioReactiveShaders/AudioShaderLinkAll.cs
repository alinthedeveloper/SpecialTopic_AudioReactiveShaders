using UnityEngine;
using AudioTools;

public class AudioShaderLinkAll : MonoBehaviour
{
    [SerializeField] private ProcessAudio audioProcessor;
    [SerializeField] private Renderer targetRenderer;

    [Header("Shader Property and Frequency Bin Mappings")]
    [SerializeField] private string bassShaderPropertyName = "_BassLevel";
    [SerializeField] private string bassfrequencyBinName = "Bass";
    [SerializeField] private string lowShaderPropertyName = "_LowLevel";
    [SerializeField] private string lowfrequencyBinName = "Low";
    [SerializeField] private string midShaderPropertyName = "_MidLevel";
    [SerializeField] private string midfrequencyBinName = "Mid";
    [SerializeField] private string highShaderPropertyName = "_HighLevel";
    [SerializeField] private string highfrequencyBinName = "High";
    [SerializeField] private string airShaderPropertyName = "_AirLevel";
    [SerializeField] private string airfrequencyBinName = "Air";

    [SerializeField] private bool usePulse = false;

    private Material _material;

    // Track bins
    private FrequencyBin bassBin, lowBin, midBin, highBin, airBin;

    void Start()
    {
        if (audioProcessor == null || targetRenderer == null)
        {
            Debug.LogWarning("AudioShaderLinkAll: Missing references!");
            return;
        }

        _material = targetRenderer.material;

        foreach (var bin in audioProcessor.GetFrequencyBins())
        {
            switch (bin.Name)
            {
                case string name when name == bassfrequencyBinName: bassBin = bin; break;
                case string name when name == lowfrequencyBinName:  lowBin  = bin; break;
                case string name when name == midfrequencyBinName:  midBin  = bin; break;
                case string name when name == highfrequencyBinName: highBin = bin; break;
                case string name when name == airfrequencyBinName:  airBin  = bin; break;
            }
        }
    }

    void Update()
    {
        if (_material == null) return;

        if (bassBin != null)
            _material.SetFloat(bassShaderPropertyName, usePulse ? (bassBin.PulseActive ? 1f : 0f) : bassBin.NormalizedAmplitude);
        if (lowBin != null)
            _material.SetFloat(lowShaderPropertyName, usePulse ? (lowBin.PulseActive ? 1f : 0f) : lowBin.NormalizedAmplitude);
        if (midBin != null)
            _material.SetFloat(midShaderPropertyName, usePulse ? (midBin.PulseActive ? 1f : 0f) : midBin.NormalizedAmplitude);
        if (highBin != null)
            _material.SetFloat(highShaderPropertyName, usePulse ? (highBin.PulseActive ? 1f : 0f) : highBin.NormalizedAmplitude);
        if (airBin != null)
            _material.SetFloat(airShaderPropertyName, usePulse ? (airBin.PulseActive ? 1f : 0f) : airBin.NormalizedAmplitude);
    }
}
