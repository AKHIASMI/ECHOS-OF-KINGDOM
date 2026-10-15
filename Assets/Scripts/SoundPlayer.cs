using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public static SoundPlayer Instance;
    private AudioSource source;

    [Header("Clips")]
    public AudioClip demonGrowl;
    public AudioClip portalActivate;

    void Awake()
    {
        Instance = this;
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null) source.PlayOneShot(clip, volume);
    }
}