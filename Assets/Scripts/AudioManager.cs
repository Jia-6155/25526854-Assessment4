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
    public AudioSource bgmSource;
    [Header("Clips - UI & Basic SFX")]
    public AudioClip pelletClip;
    public AudioClip cherrySpawnClip;
    public AudioClip cherryEatClip;
    public AudioClip deathClip;
    public AudioClip buttonClip;
    [Header("Clips - Ghost & Game BGM")]
    public AudioClip ghostEatenClip;
    public AudioClip bumpClip;
    public AudioClip bgmNormal;
    public AudioClip bgmScared;
    public AudioClip bgmGameOver;
    [Range(0f, 0.25f)] public float pitchJitter = 0.05f;
    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        
    }
    void Update()
    {

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
    public void PlaySFX_EatPellet()
    { PlaySFX(pelletClip, 0.8f); }
    public void PlaySFX_EatCherry()
    { PlaySFX(cherryEatClip, 1f); }
    public void PlaySFX_SpawnCherry()
    { PlaySFX(cherrySpawnClip, 1f, 1f, false); }
    public void PlaySFX_PlayerDie()
    { PlaySFX(deathClip, 1f, 1f, false); }
    public void PlaySFX_GhostEaten()
    { PlaySFX(ghostEatenClip, 1f, 1f, false); }
    public void PlaySFX_Bump()
    { PlaySFX(bumpClip, 0.8f, 1f, false); }
    public void PlayBGM(AudioClip clip, bool loop = true, float vol = 0.5f) // ★新增：通用BGM播放
    {
        if (!bgmSource || !clip) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = vol;
        bgmSource.Play();
    }
    public void StopBGM()
    {
        if (bgmSource) bgmSource.Stop();
    }
    public void PlayBGM_Normal()
    { PlayBGM(bgmNormal, true, 0.4f); }
    public void PlayBGM_Scared()
    { PlayBGM(bgmScared, true, 0.4f); }
    public void PlayBGM_GameOver()
    { PlayBGM(bgmGameOver, false, 0.5f); }
    public void StopAll()
    {
        if (bgmSource) bgmSource.Stop();
        if (sfxSource) sfxSource.Stop();
    }
}
