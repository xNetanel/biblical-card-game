using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioClip[] musicTracks;
    public float musicVolume = 0.4f;
    private AudioSource musicSource;
    private int currentTrackIndex = 0;

    [Header("SFX")]
    public float sfxVolume = 0.8f;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume;

        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        musicSource.volume = musicVolume;
        if (musicTracks != null && musicTracks.Length > 0)
            StartCoroutine(PlayMusicLoop());
    }

    IEnumerator PlayMusicLoop()
    {
        while (true)
        {
            musicSource.clip = musicTracks[currentTrackIndex];
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
            currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = volume;
    }

    // =====================
    // SFX GENERATORS
    // =====================

    public void PlayCardPlayed()
    {
        StartCoroutine(GenerateTone(440f, 0.08f, 0.12f));
        StartCoroutine(DelayedTone(0.08f, 550f, 0.06f, 0.1f));
    }

    public void PlayCombatHit()
    {
        StartCoroutine(GenerateNoise(0.15f, 0.08f));
        StartCoroutine(DelayedTone(0.05f, 180f, 0.12f, 0.08f));
    }

    public void PlayBuff()
    {
        StartCoroutine(GenerateTone(520f, 0.07f, 0.08f));
        StartCoroutine(DelayedTone(0.07f, 660f, 0.07f, 0.08f));
        StartCoroutine(DelayedTone(0.14f, 780f, 0.07f, 0.1f));
    }

    public void PlayDebuff()
    {
        StartCoroutine(GenerateTone(400f, 0.07f, 0.08f));
        StartCoroutine(DelayedTone(0.07f, 300f, 0.07f, 0.08f));
        StartCoroutine(DelayedTone(0.14f, 220f, 0.07f, 0.1f));
    }

    public void PlayWin()
    {
        StartCoroutine(GenerateTone(523f, 0.1f, 0.15f));
        StartCoroutine(DelayedTone(0.12f, 659f, 0.1f, 0.15f));
        StartCoroutine(DelayedTone(0.24f, 784f, 0.1f, 0.15f));
        StartCoroutine(DelayedTone(0.36f, 1047f, 0.2f, 0.25f));
    }

    public void PlayLose()
    {
        StartCoroutine(GenerateTone(392f, 0.15f, 0.2f));
        StartCoroutine(DelayedTone(0.18f, 311f, 0.15f, 0.2f));
        StartCoroutine(DelayedTone(0.36f, 233f, 0.25f, 0.3f));
    }

    // =====================
    // TONE GENERATORS
    // =====================

    IEnumerator DelayedTone(float delay, float frequency, float amplitude, float duration)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(GenerateTone(frequency, amplitude, duration));
    }

    IEnumerator GenerateTone(float frequency, float amplitude, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / duration));
            data[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * amplitude * envelope;
        }

        AudioClip clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        sfxSource.PlayOneShot(clip, sfxVolume);
        yield return null;
    }

    IEnumerator GenerateNoise(float amplitude, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / duration));
            data[i] = UnityEngine.Random.Range(-1f, 1f) * amplitude * envelope;
        }

        AudioClip clip = AudioClip.Create("noise", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        sfxSource.PlayOneShot(clip, sfxVolume);
        yield return null;
    }
}