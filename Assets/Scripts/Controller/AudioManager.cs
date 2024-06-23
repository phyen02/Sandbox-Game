using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSrc;
    [SerializeField] AudioSource sfxSrc;
    [Header("Audio Clip")]
    public AudioClip background;
    public AudioClip jump;

    private void Start()
    {
        musicSrc.clip = background;
        musicSrc.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSrc.PlayOneShot(clip);
    }
}