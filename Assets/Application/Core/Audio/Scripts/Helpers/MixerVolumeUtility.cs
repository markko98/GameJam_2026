using UnityEngine;

public static class MixerVolumeUtility
{
    public static float LinearToDecibel(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    }

    public static float DecibelToLinear(float dB)
    {
        return Mathf.Pow(10f, dB / 20f);
    }
}