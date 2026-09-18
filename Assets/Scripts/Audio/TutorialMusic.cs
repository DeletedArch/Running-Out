using UnityEngine;

public class TutorialMusic : MonoBehaviour
{
    [Header("Ambient Sound Layers (Played Together)")]
    [Tooltip("All SoundData effects in this array will play simultaneously on loop.")]
    [SerializeField] private SoundData[] soundEffects;

    [Header("Optional Main Song")]
    [Tooltip("Optional background song to play alongside the ambient effects.")]
    [SerializeField] private SoundData song;

    private void Start()
    {
        // 1. Play all ambient effects simultaneously on loop
        if (soundEffects != null)
        {
            foreach (var effect in soundEffects)
            {
                if (effect == null) continue;

                AudioClip clip = effect.GetRandomClip();
                if (clip == null) continue;

                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.clip = clip;
                src.outputAudioMixerGroup = effect.MixerGroup;
                src.volume = effect.Volume;
                src.pitch = effect.Pitch;
                src.loop = true;
                src.spatialBlend = 0f; // 2D global sound
                src.playOnAwake = false;
                src.Play();
            }
        }

        // 2. Play optional song alongside the effects if provided
        if (song != null)
        {
            AudioClip clip = song.GetRandomClip();
            if (clip != null)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.clip = clip;
                src.outputAudioMixerGroup = song.MixerGroup;
                src.volume = song.Volume;
                src.pitch = song.Pitch;
                src.loop = true;
                src.spatialBlend = 0f;
                src.playOnAwake = false;
                src.Play();
            }
        }
    }
}
