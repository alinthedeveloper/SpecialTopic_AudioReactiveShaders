using UnityEngine;

public class FullSongShaderLink : MonoBehaviour
{
    [SerializeField] private ProcessAudio audioProcessor;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string shaderPropertyName = "_AllLevel";

    private Material _material;
    // Start is called before the first frame update
    void Start()
    {   
        if (audioProcessor == null || targetRenderer == null)
        {
            Debug.LogWarning("FullSongShaderLink: Missing references!");
            return;
        }

        _material = targetRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (audioProcessor == null || _material == null) return;

        float value = audioProcessor.AllLevel;
        _material.SetFloat(shaderPropertyName, value);
    }
}
