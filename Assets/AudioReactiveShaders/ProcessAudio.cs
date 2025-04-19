using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioTools;

/// <summary>
/// This class processes audio data and calculates the amplitude of 
/// Different frequency ranges based on the output of the frequency bin class.
/// </summary>
public class ProcessAudio : MonoBehaviour
{
    public enum ScaleFactor
    {
        x1 = 1,
        x10 = 10,
        x100 = 100,
    }

    public enum SpectrumSize
    {
        x64 = 64,
        x128 = 128,
        x256 = 256,
        x512 = 512,
    }

    public List<FrequencyBin> GetFrequencyBins()
    {
        return frequencyBins;
    }


    [SerializeField] private SpectrumSize spectrumSize = SpectrumSize.x512; // Default to 512
    [SerializeField] private ScaleFactor scaleFactor = ScaleFactor.x10;     // Default to 10
    [Range(60, 240)] public int refreshRate = 60;
    public bool debugMode = false;
    [SerializeField] private List<FrequencyBin> frequencyBins = new List<FrequencyBin>();

    private float[] spectrumData;

    void Start()
    {
        spectrumData = new float[(int)spectrumSize]; 

        //default bins.
        frequencyBins.Add(new FrequencyBin("Bass", 20, 100, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("Low", 100, 500, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("Mid", 500, 2000, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("High", 2000, 6000, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("Air", 6000, 20000, 0.1f, 0.3f));
        
        StartCoroutine(UpdateFrequencyBins());
    }

    private IEnumerator UpdateFrequencyBins()
    {
        while (true)
        {
            AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

            foreach (var bin in frequencyBins)
            {
                float amplitude = CalculateAmplitude(spectrumData, bin.MinRange, bin.MaxRange);
                float normalized = Mathf.Clamp01((amplitude - bin.MinAmplitude) / (bin.MaxAmplitude - bin.MinAmplitude));

                // Smooth
                float smoothed = Mathf.Lerp(bin.NormalizedAmplitude, normalized, bin.SmoothingFactor);
                bin.UpdateAmplitude(smoothed);
                // 🔥 Peak detection
                bin.DetectPeaks(bin.NormalizedAmplitude, Time.deltaTime);

                if (bin.DebugMode)
                {
                    Debug.Log($"[{bin.Name}] Norm: {bin.NormalizedAmplitude:F3}, Pulse: {bin.PulseActive}");
                }
            }

            yield return new WaitForSeconds(1f / refreshRate);
        }
    }

    private void DebugLogBinData(FrequencyBin bin)
    {
        int startIdx = Mathf.FloorToInt(bin.MinRange / AudioSettings.outputSampleRate * 2 * spectrumData.Length);
        int endIdx = Mathf.FloorToInt(bin.MaxRange / AudioSettings.outputSampleRate * 2 * spectrumData.Length);

        float sum = 0f;
        int count = 0;

        for (int i = startIdx; i <= endIdx && i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
            count++;
        }

        float avgAmplitude = count > 0 ? sum / count : 0f;

        Debug.Log($"[{bin.Name}] Raw Average Amplitude: {avgAmplitude:F6}");
    }

    private float CalculateAmplitude(float[] spectrumData, float minRange, float maxRange)
    {
        int startIdx = Mathf.FloorToInt(minRange / AudioSettings.outputSampleRate * 2 * spectrumData.Length);
        int endIdx = Mathf.FloorToInt(maxRange / AudioSettings.outputSampleRate * 2 * spectrumData.Length);

        float sum = 0f;
        int count = 0;

        for (int i = startIdx; i <= endIdx && i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
            count++;
        }

        float averageAmplitude = count > 0 ? sum / count : 0f;

        return averageAmplitude * (int)scaleFactor;
    }
}
