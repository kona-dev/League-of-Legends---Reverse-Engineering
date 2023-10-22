using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioDictionary audioDictionary;

    public AudioMixer mixer;

    public static AudioManager instance;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        foreach (Sound s in audioDictionary.sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.mixerGroup;
        }

    }

    public void SetMaster(float volume)
    {
        mixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }
    public void SetMusic(float volume)
    {
        mixer.SetFloat("Music", Mathf.Log10(volume) * 20);
    }
    public void SetSFX(float volume)
    {
        mixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }

    public void Play (string name)
    {
        
        Sound s = Array.Find(audioDictionary.sounds, sound => sound.name == name);
        if (s == null) return;
       
        s.pitch = 1;
        s.source.Play();

    }

    public void Play (string name, float pitch)
    {
        Sound s = Array.Find(audioDictionary.sounds, sound => sound.name == name);
        if (s == null) return;
        s.pitch = pitch;
        
        s.source.Play();
        
    }

    public void PlayUnique(string name  )
    {
        Sound s = Array.Find(audioDictionary.sounds, sound => sound.name == name);
        if (s == null) return;
      
        s.source.PlayOneShot(s.source.clip);
        
    }

    public void PlayDelayed(string name, float delay)
    {
        Sound s = Array.Find(audioDictionary.sounds, sound => sound.name == name);
        if (s == null) return;
      
        s.source.PlayDelayed(delay);

    }

    public void StopAll()
    {
        foreach (var s in audioDictionary.sounds) s.source.Stop();
    }

    
}
