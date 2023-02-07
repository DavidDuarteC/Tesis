using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;

    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;

    [SerializeField] float fadeDuration = 0.75f;

    AudioClip currMusic;
    AudioClip prevMusic;

    float originalMusicVol;
    Dictionary<AudioId, AudioData> sfxLookup;


    public static AudioManager i { get; private set; }//Sigleton

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        originalMusicVol = musicPlayer.volume;

        sfxLookup = sfxList.ToDictionary(x => x.id);
    }

    public void PlaySfx(AudioClip clip, bool pauseMusic = false) //Reproduce los efectos
    {
        if (clip == null) return;

        if (pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnPauseMusic(clip.length));
        }

        sfxPlayer.PlayOneShot(clip); //PlayOnShot no cancela el ruido que esta sonando
    }

    public void PlaySfx(AudioId audioId, bool pauseMusic = false) //Revisa si los efectos fueron asignados
    {
        if(!sfxLookup.ContainsKey(audioId)) return;

        var audioData = sfxLookup[audioId];
        PlaySfx(audioData.clip, pauseMusic);
    }


    public void PlayMusic(AudioClip clip, bool loop = true, bool fade = false) 
    {
        if (clip == null || clip == currMusic) return;

        if(currMusic == null)
            prevMusic = clip;
        else
            prevMusic = currMusic;

        currMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    public void PrevPlayMusic(bool loop = true, bool fade = false)
    {
        var prev = currMusic;
        currMusic = prevMusic;
        prevMusic = prev as AudioClip;
        StartCoroutine(PlayMusicAsync(currMusic, loop, fade));
    }

    IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)//Reproduce la musica
    {
        if (fade)
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        if (fade)
            yield return musicPlayer.DOFade(originalMusicVol, fadeDuration).WaitForCompletion();

    }

    IEnumerator UnPauseMusic(float delay)
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVol, fadeDuration);
    }
}

public enum AudioId { UISelect, Hit, Faint, ExpGain, ItemObteined, PokemonObtained }

[Serializable]
public class AudioData
{
    public AudioId id;
    public AudioClip clip;
}
