using UnityEngine;

public class AutoSettings : MonoBehaviour
{
    void Start()
    {
        SetResolution();
        SetQuality();
    }

    void SetResolution()
    {
        Resolution currentRes = Screen.currentResolution;

        Screen.SetResolution(currentRes.width, currentRes.height, true);

        Debug.Log("Resolution set to: " + currentRes.width + "x" + currentRes.height);
    }

    void SetQuality()
    {
        int qualityLevels = QualitySettings.names.Length;

        // Simple logic based on system memory (you can tweak this)
        int systemMemory = SystemInfo.systemMemorySize;

        int qualityIndex = 0;

        if (systemMemory >= 16000) // 16GB+
            qualityIndex = qualityLevels - 1; // Ultra
        else if (systemMemory >= 8000) // 8GB+
            qualityIndex = qualityLevels - 2; // High
        else if (systemMemory >= 4000) // 4GB+
            qualityIndex = qualityLevels / 2; // Medium
        else
            qualityIndex = 0; // Low

        qualityIndex = Mathf.Clamp(qualityIndex, 0, qualityLevels - 1);

        QualitySettings.SetQualityLevel(qualityIndex, true);

        Debug.Log("Quality set to: " + QualitySettings.names[qualityIndex]);
    }
}