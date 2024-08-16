using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioClip mergeSound;
    [SerializeField] private AudioSource soundEffectSource;
    [SerializeField] private AudioClip dynamiteSound; // Dinamit patlama sesi

    private bool isMusicMuted;
    private bool areEffectsMuted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
    }

    public void PlayMergeSound()
    {
        if (!areEffectsMuted && soundEffectSource != null && mergeSound != null)
        {
            soundEffectSource.PlayOneShot(mergeSound);
        }
    }

    public void PlayDynamiteSound()
    {
        if (!areEffectsMuted && soundEffectSource != null && dynamiteSound != null)
        {
            soundEffectSource.PlayOneShot(dynamiteSound);
        }
    }

    // Arka plan müziðini aç/kapat
    public void ToggleBackgroundMusic()
    {
        isMusicMuted = !isMusicMuted;

        if (backgroundMusic != null)
        {
            backgroundMusic.mute = isMusicMuted;
        }
    }

    // Ses efektlerini aç/kapat
    public void ToggleSoundEffects()
    {
        areEffectsMuted = !areEffectsMuted;

        if (soundEffectSource != null)
        {
            soundEffectSource.mute = areEffectsMuted;
        }
    }
}
