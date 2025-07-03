using UnityEngine;
using UnityEngine.Audio;
using System;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public Sound[] sounds;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.pitch = s.pitch;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.spatialBlend;
            s.source.playOnAwake = s.playOnAwake;
            s.source.outputAudioMixerGroup = s.output;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found");
            return;
        }
        if (!s.source.isPlaying)
            s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found");
            return;
        }
        if (s.source.isPlaying)
            s.source.Stop();
    }
}
