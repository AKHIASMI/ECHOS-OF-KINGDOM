using UnityEngine;

// Makes a light flicker gently, like a torch.
[RequireComponent(typeof(Light))]
public class LampFlicker : MonoBehaviour
{
    [SerializeField] private float amount = 0.15f;
    [SerializeField] private float speed = 3f;

    private Light lamp;
    private float baseIntensity;
    private float seed;

    private void Awake()
    {
        lamp = GetComponent<Light>();
        baseIntensity = lamp.intensity;
        seed = Random.value * 100f;
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(seed, Time.time * speed) - 0.5f;
        lamp.intensity = baseIntensity * (1f + noise * 2f * amount);
    }
}
