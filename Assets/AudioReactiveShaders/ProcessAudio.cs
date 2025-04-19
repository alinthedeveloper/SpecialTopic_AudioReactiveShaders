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

    // This list 
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
    private Coroutine processingCoroutine;

    private void Start()
    {
        spectrumData = new float[(int)spectrumSize]; 

        //default bins.
        frequencyBins.Add(new FrequencyBin("Bass", 20, 100, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("Low", 100, 500, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("Mid", 500, 2000, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("High", 2000, 6000, 0.1f, 0.3f));
        frequencyBins.Add(new FrequencyBin("Air", 6000, 20000, 0.1f, 0.3f));
        
        // stores the corutine's status in a variable so we have a break case if we need it.
        processingCoroutine = StartCoroutine(UpdateFrequencyBins());
    }

    //hopefully never called but it is here to break the while loop in the coroutine.
    private void OnDisable()
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
            processingCoroutine = null;
        }
    }

    // This method is called every frame to update the normalized amplitude of each frequency bin.
    // In order to avoid any performance issues, the method waits to loop based on the refresh rate.
    private IEnumerator UpdateFrequencyBins()
    {
        while (true)
        {
            // Get the spectrum data from the AudioListener
            // The spectrum data is a float array that contains the amplitude of different frequency ranges
            AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

            foreach (var bin in frequencyBins)
            {
                float amplitude = CalculateAmplitude(spectrumData, bin.MinRange, bin.MaxRange);
                float normalized = Mathf.Clamp01((amplitude - bin.MinAmplitude) / (bin.MaxAmplitude - bin.MinAmplitude));

                // Smooth the differences between amplitude values by a chosen factor.
                // This is done to avoid any jittering in the amplitude values.
                float smoothed = Mathf.Lerp(bin.NormalizedAmplitude, normalized, bin.SmoothingFactor);
                bin.UpdateAmplitude(smoothed);
                bin.DetectPeaks(bin.NormalizedAmplitude, Time.deltaTime);

                if (bin.DebugMode)
                {
                    Debug.Log($"[{bin.Name}] Norm: {bin.NormalizedAmplitude:F3}, Pulse: {bin.PulseActive}");
                }
            }

            yield return new WaitForSeconds(1f / refreshRate);
        }
    }

    // debugging method to log the raw amplitude of each frequency bin to help with tuning the values.
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

    // This method calculates the amplitude of a given frequency range based on the spectrum data.
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
