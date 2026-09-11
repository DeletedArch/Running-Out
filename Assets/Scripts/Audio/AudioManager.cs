using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                instance = Object.FindFirstObjectByType<AudioManager>();
#else
                instance = Object.FindObjectOfType<AudioManager>();
#endif
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    [Header("Audio Mixer")]
    [Tooltip("Optional. If assigned, mixer exposed parameters will be controlled.")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("AudioSource Pool (SFX)")]
    [SerializeField] private int initialPoolSize = 16;
    [SerializeField] private int maxPoolSize = 32;

    [Header("Default Music Settings")]
    [SerializeField] private float defaultMusicFadeDuration = 1.0f;

    // Internal pool
    private readonly List<AudioSource> sfxPool = new List<AudioSource>();
    private Transform poolContainer;

    // Dual sources for seamless crossfading
    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource activeMusicSource;
    private Coroutine musicFadeCoroutine;

    // Cooldown tracker per SoundData
    private readonly Dictionary<SoundData, float> lastPlayTimes = new Dictionary<SoundData, float>();

    // Fallback volume multipliers (used if AudioMixer is not yet configured)
    private float masterVolumeLinear = 1f;
    private float musicVolumeLinear = 1f;
    private float sfxVolumeLinear = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePool();
        InitializeMusicSources();
    }

    private void InitializePool()
    {
        GameObject poolObj = new GameObject("SFX_Pool");
        poolObj.transform.SetParent(transform);
        poolContainer = poolObj.transform;

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledAudioSource();
        }
    }

    private AudioSource CreatePooledAudioSource()
    {
        GameObject sourceObj = new GameObject($"Pooled_AudioSource_{sfxPool.Count}");
        sourceObj.transform.SetParent(poolContainer);
        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        sfxPool.Add(source);
        return source;
    }

    private void InitializeMusicSources()
    {
        GameObject musicAObj = new GameObject("Music_Source_A");
        musicAObj.transform.SetParent(transform);
        musicSourceA = musicAObj.AddComponent<AudioSource>();
        musicSourceA.playOnAwake = false;
        musicSourceA.loop = true;

        GameObject musicBObj = new GameObject("Music_Source_B");
        musicBObj.transform.SetParent(transform);
        musicSourceB = musicBObj.AddComponent<AudioSource>();
        musicSourceB.playOnAwake = false;
        musicSourceB.loop = true;

        activeMusicSource = musicSourceA;
    }

    #region SFX Playback

    public AudioSource Play(SoundData soundData, Vector3 position = default)
    {
        if (soundData == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null SoundData!");
            return null;
        }

        // Check cooldown to prevent sound stacking
        if (soundData.Cooldown > 0f)
        {
            if (lastPlayTimes.TryGetValue(soundData, out float lastTime))
            {
                if (Time.unscaledTime - lastTime < soundData.Cooldown)
                {
                    return null; // Dropped due to cooldown
                }
            }
            lastPlayTimes[soundData] = Time.unscaledTime;
        }

        AudioClip clipToPlay = soundData.GetRandomClip();
        if (clipToPlay == null) return null;

        AudioSource source = GetAvailableAudioSource();
        if (source == null) return null;

        // Position
        source.transform.position = position;
        source.spatialBlend = soundData.SpatialBlend;
        source.minDistance = soundData.MinDistance;
        source.maxDistance = soundData.MaxDistance;

        // Clip and Mixer Group
        source.clip = clipToPlay;
        source.outputAudioMixerGroup = soundData.MixerGroup;
        source.loop = soundData.Loop;

        // In-Editor Tuned Volume & Pitch Variations
        float calculatedVolume = soundData.GetCalculatedVolume();
        if (audioMixer == null)
        {
            // Fallback volume calculation if no mixer assigned
            source.volume = calculatedVolume * masterVolumeLinear * sfxVolumeLinear;
        }
        else
        {
            source.volume = calculatedVolume;
        }

        source.pitch = soundData.GetCalculatedPitch();

        source.Play();
        return source;
    }

    public AudioSource Play(SoundData soundData, Transform attachTo)
    {
        Vector3 pos = attachTo != null ? attachTo.position : Vector3.zero;
        AudioSource source = Play(soundData, pos);
        if (source != null && attachTo != null)
        {
            StartCoroutine(FollowTransformRoutine(source.transform, attachTo, source));
        }
        return source;
    }

    private IEnumerator FollowTransformRoutine(Transform sourceTransform, Transform target, AudioSource source)
    {
        while (source != null && source.isPlaying && target != null)
        {
            sourceTransform.position = target.position;
            yield return null;
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (!sfxPool[i].isPlaying)
            {
                return sfxPool[i];
            }
        }

        // If all are busy and we haven't reached max pool size, expand
        if (sfxPool.Count < maxPoolSize)
        {
            return CreatePooledAudioSource();
        }

        // Steal the first source if limit reached
        return sfxPool[0];
    }

    public void StopAllSFX()
    {
        for (int i = 0; i < sfxPool.Count; i++)
        {
            if (sfxPool[i] != null && sfxPool[i].isPlaying)
            {
                sfxPool[i].Stop();
            }
        }
    }

    #endregion

    #region Music Playback & Crossfading

    public void PlayMusic(SoundData musicData, float fadeDuration = -1f)
    {
        if (musicData == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null music SoundData!");
            return;
        }

        AudioClip clip = musicData.GetRandomClip();
        if (clip == null) return;

        if (fadeDuration < 0f) fadeDuration = defaultMusicFadeDuration;

        // If the same clip is already playing on active source, do nothing
        if (activeMusicSource.isPlaying && activeMusicSource.clip == clip)
        {
            return;
        }

        // Swap to the other source for crossfading
        AudioSource incomingSource = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;
        AudioSource outgoingSource = activeMusicSource;

        incomingSource.clip = clip;
        incomingSource.outputAudioMixerGroup = musicData.MixerGroup;
        incomingSource.loop = true;
        incomingSource.spatialBlend = 0f; // Music is 2D
        incomingSource.pitch = musicData.Pitch;

        float targetVolume = musicData.Volume;
        if (audioMixer == null)
        {
            targetVolume *= masterVolumeLinear * musicVolumeLinear;
        }

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(CrossfadeMusicRoutine(outgoingSource, incomingSource, targetVolume, fadeDuration));
        activeMusicSource = incomingSource;
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        if (fadeDuration < 0f) fadeDuration = defaultMusicFadeDuration;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(FadeOutMusicRoutine(activeMusicSource, fadeDuration));
    }

    public void PauseMusic()
    {
        if (activeMusicSource != null && activeMusicSource.isPlaying)
        {
            activeMusicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (activeMusicSource != null)
        {
            activeMusicSource.UnPause();
        }
    }

    private IEnumerator CrossfadeMusicRoutine(AudioSource outgoing, AudioSource incoming, float targetVolume, float duration)
    {
        float startOutgoingVol = outgoing.isPlaying ? outgoing.volume : 0f;
        incoming.volume = 0f;
        incoming.Play();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            if (outgoing.isPlaying)
            {
                outgoing.volume = Mathf.Lerp(startOutgoingVol, 0f, progress);
            }
            incoming.volume = Mathf.Lerp(0f, targetVolume, progress);

            yield return null;
        }

        if (outgoing.isPlaying)
        {
            outgoing.Stop();
            outgoing.clip = null;
        }
        incoming.volume = targetVolume;
        musicFadeCoroutine = null;
    }

    private IEnumerator FadeOutMusicRoutine(AudioSource source, float duration)
    {
        if (!source.isPlaying) yield break;

        float startVol = source.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.clip = null;
        musicFadeCoroutine = null;
    }

    #endregion

    #region Volume & Mixer Control

    public void SetMasterVolume(float linearVolume)
    {
        masterVolumeLinear = Mathf.Clamp01(linearVolume);
        ApplyMixerVolume(masterVolumeParam, masterVolumeLinear);
    }

    public void SetSFXVolume(float linearVolume)
    {
        sfxVolumeLinear = Mathf.Clamp01(linearVolume);
        ApplyMixerVolume(sfxVolumeParam, sfxVolumeLinear);
    }

    public void SetMusicVolume(float linearVolume)
    {
        musicVolumeLinear = Mathf.Clamp01(linearVolume);
        ApplyMixerVolume(musicVolumeParam, musicVolumeLinear);
    }

    public float GetMasterVolume() => masterVolumeLinear;
    public float GetSFXVolume() => sfxVolumeLinear;
    public float GetMusicVolume() => musicVolumeLinear;

    private void ApplyMixerVolume(string paramName, float linearVolume)
    {
        if (audioMixer == null) return;

        // Convert linear (0..1) to decibel scale (-80dB to 0dB)
        float dB = (linearVolume > 0.0001f) ? Mathf.Log10(linearVolume) * 20f : -80f;
        if (!audioMixer.SetFloat(paramName, dB))
        {
            // Fallback in case Master was exposed with default Unity name
            if (paramName == masterVolumeParam)
            {
                audioMixer.SetFloat("MyExposedParam", dB);
            }
        }
    }

    #endregion
}
