using System;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NewSoundData", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    [Header("Audio Clips")]
    [Tooltip("If multiple clips are provided, one will be chosen randomly on play.")]
    [SerializeField] private AudioClip[] clips;

    [Header("Mixer Routing")]
    [Tooltip("The AudioMixerGroup to route this sound into (e.g. Master, SFX, Music, UI).")]
    [SerializeField] private AudioMixerGroup mixerGroup;

    [Header("Volume (In-Editor Tuning)")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Range(0f, 0.5f)]
    [Tooltip("Random volume variation (+/-). Example: 0.05 gives volume between 0.95 and 1.05.")]
    [SerializeField] private float volumeVariation = 0.05f;

    [Header("Pitch (In-Editor Tuning)")]
    [Range(0.1f, 3f)]
    [SerializeField] private float pitch = 1f;

    [Range(0f, 0.5f)]
    [Tooltip("Random pitch variation (+/-). Example: 0.1 gives pitch between 0.9 and 1.1 so repetitive sounds never fatigue the ear.")]
    [SerializeField] private float pitchVariation = 0.08f;

    [Header("Spatial Blend & 3D Settings")]
    [Range(0f, 1f)]
    [Tooltip("0 = 2D (UI, Music, Global SFX). 1 = 3D (Positional in world).")]
    [SerializeField] private float spatialBlend = 0f;

    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 25f;

    [Header("Playback Behaviour")]
    [SerializeField] private bool loop = false;

    [Tooltip("Minimum time (in seconds) before this specific sound can be triggered again. Prevents sound stacking when multiple hits happen simultaneously.")]
    [SerializeField] private float cooldown = 0.05f;

    // Public Properties
    public AudioClip[] Clips => clips;
    public AudioMixerGroup MixerGroup => mixerGroup;
    public float Volume => volume;
    public float VolumeVariation => volumeVariation;
    public float Pitch => pitch;
    public float PitchVariation => pitchVariation;
    public float SpatialBlend => spatialBlend;
    public float MinDistance => minDistance;
    public float MaxDistance => maxDistance;
    public bool Loop => loop;
    public float Cooldown => cooldown;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"[SoundData] No audio clips assigned to '{name}'!");
            return null;
        }

        if (clips.Length == 1)
        {
            return clips[0];
        }

        int randomIndex = UnityEngine.Random.Range(0, clips.Length);
        return clips[randomIndex];
    }

    public float GetCalculatedVolume()
    {
        if (volumeVariation <= 0f) return volume;
        float randomOffset = UnityEngine.Random.Range(-volumeVariation, volumeVariation);
        return Mathf.Clamp01(volume + randomOffset);
    }

    public float GetCalculatedPitch()
    {
        if (pitchVariation <= 0f) return pitch;
        float randomOffset = UnityEngine.Random.Range(-pitchVariation, pitchVariation);
        return Mathf.Clamp(pitch + randomOffset, 0.1f, 3f);
    }

    public AudioSource Play(Vector3 position = default)
    {
        if (AudioManager.Instance != null)
        {
            return AudioManager.Instance.Play(this, position);
        }
        else
        {
            Debug.LogWarning("[SoundData] Cannot play sound: AudioManager.Instance is null!");
            return null;
        }
    }
}
