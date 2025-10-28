using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager I;
    [Header("Mixer")]
    public AudioMixer mixer;
    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource uiSource;
    [Header("Clips")]
    public AudioClip pelletClip;
    public AudioClip cherrySpawnClip;
    public AudioClip cherryEatClip;
    public AudioClip deathClip;
    public AudioClip buttonClip;
    [Range(0f, 0.25f)] public float pitchJitter = 0.05f;
    void Start()
    {
        
    }
    void Update()
    {

    }
    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
    public void PlaySFX(AudioClip clip, float vol = 1f, float pitch = 1f, bool jitter = true)
    {
        if (!clip || !sfxSource) return;
        float p = jitter ? pitch + Random.Range(-pitchJitter, pitchJitter) : pitch;
        sfxSource.pitch = p;
        sfxSource.PlayOneShot(clip, vol);
    }
    public void PlayUI(AudioClip clip, float vol = 1f)
    {
        if (!clip || !uiSource) return;
        uiSource.PlayOneShot(clip, vol);
    }
    public void PlayButtonClick()
    {
        PlayUI(buttonClip);
    }
}
