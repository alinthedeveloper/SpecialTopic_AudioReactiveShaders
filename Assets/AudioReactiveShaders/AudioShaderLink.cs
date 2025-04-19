using UnityEngine;
using AudioTools;

public class AudioShaderLink : MonoBehaviour
{
    [SerializeField] private ProcessAudio audioProcessor;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string shaderPropertyName = "_BassAmplitude";
    [SerializeField] private string frequencyBinName = "Bass";
    [SerializeField] private bool usePulse = false;

    private Material _material;
    private FrequencyBin _targetBin;

    void Start()
    {
        if (audioProcessor == null || targetRenderer == null)
        {
            Debug.LogWarning("AudioShaderLink: Missing references!");
            return;
        }

        _material = targetRenderer.material;

        foreach (var bin in audioProcessor.GetFrequencyBins())
        {
            if (bin.Name == frequencyBinName)
            {
                _targetBin = bin;
                break;
            }
        }

        if (_targetBin == null)
        {
            Debug.LogError($"AudioShaderLink: No frequency bin named '{frequencyBinName}' found.");
        }
    }

    void Update()
    {
        if (_targetBin == null || _material == null) return;

        float value = usePulse ? (_targetBin.PulseActive ? 1f : 0f) : _targetBin.NormalizedAmplitude;
        _material.SetFloat(shaderPropertyName, value);
    }
}
