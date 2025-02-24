using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private AudioSource soundSource;
    [SerializeField] private AudioClip soundEffect;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.volume = 1.0f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayEffectSound()
    {
        soundSource.PlayOneShot(soundEffect);
    }
}
