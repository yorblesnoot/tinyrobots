using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public static SoundLibrary Library;
    public static AudioSource FXSource;
    public static AudioSource MusicSource;

    public static void PlaySound(SoundType type)
    {
        if (Library.Cliptionary.TryGetValue(type, out AudioClip clip))
            PlaySound(clip);
    }

    public static void PlaySound(SoundTypeEffect type)
    {
        if (Library.EffectCliptionary.TryGetValue(type, out AudioClip clip))
            PlaySound(clip);
    }

    public static void PlaySound(AudioClip clip)
    {
        SourceCheck();
        if(clip != null)
            FXSource.PlayOneShot(clip);
    }

    public static void PlayMusic(SoundType type, bool loop = true)
    {
        SourceCheck();
        if (!Library.Cliptionary.TryGetValue(type, out AudioClip clip)) return;
        MusicSource.clip = clip;
        MusicSource.loop = loop;
        MusicSource.Play();
    }

    static void SourceCheck()
    {
        if (FXSource == null)
        {
            GameObject speaker = new("Speaker");
            FXSource = speaker.AddComponent<AudioSource>();
        }
        if (MusicSource == null)
        {
            GameObject speaker = new("Speaker");
            MusicSource = speaker.AddComponent<AudioSource>();
        }
    }

    public static void UpdateVolume()
    {
        SourceCheck();
        MusicSource.volume = Settings.Sound[SoundSetting.music_volume] * Settings.Sound[SoundSetting.master_volume] * Settings.Admin.MusicVolumeMod;
        FXSource.volume = Settings.Sound[SoundSetting.fx_volume] * Settings.Sound[SoundSetting.master_volume] * Settings.Admin.FXVolumeMod;
    }
}
