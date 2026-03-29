using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public static SoundManager active;
    [SerializeField] 
    private SoundInfoSO soundInfoSO;

    private void Awake()
    {
        active = this;
    }

    public void PlayAtPos(Vector3 pos, string soundName, float spatialBlend = 1f)
    {
        SoundInfo soundInfo = GetSoundInfo(soundName);
        if (soundInfo != null)
        {
            GameObject soundObject = new GameObject(soundName);
            soundObject.transform.position = pos;
            
            AudioSource source = soundObject.AddComponent<AudioSource>();
            
            source.clip = soundInfo.AudioClips[Random.Range(0, soundInfo.AudioClips.Count - 1)];
            source.volume = soundInfo.volume;
            source.pitch = Random.Range(soundInfo.pitch.x, soundInfo.pitch.y);

            source.spatialBlend = spatialBlend;

            source.Play();
            
            Destroy(soundObject,source.clip.length);
        }
        else
        {
            Debug.LogWarning("Sound Not Found: " + soundName);
        }
    }
    public void Play(string soundName)
    {
        SoundInfo soundInfo = GetSoundInfo(soundName);
        if (soundInfo != null)
        {
            GameObject soundObject = new GameObject(soundName);

            AudioSource source = soundObject.AddComponent<AudioSource>();

            source.clip = soundInfo.AudioClips[Random.Range(0, soundInfo.AudioClips.Count - 1)];
            source.volume = soundInfo.volume;
            source.pitch = Random.Range(soundInfo.pitch.x, soundInfo.pitch.y);

            source.spatialBlend = 0f;

            source.Play();

            Destroy(soundObject, source.clip.length);
        }
        else
        {
            Debug.LogWarning("Sound Not Found: " + soundName);
        }
    }
    private SoundInfo GetSoundInfo(string soundName)
    {
        for (int i = 0; i < soundInfoSO.soundList.Count; i++)
        {
            if (soundInfoSO.soundList[i].name == soundName)
                return soundInfoSO.soundList[i];
        }

        return null;
    }
}
