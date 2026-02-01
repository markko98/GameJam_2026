using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 1f;
    
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = startPos + Vector3.up * yOffset;
    }
}