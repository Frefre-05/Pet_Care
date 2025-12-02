using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source ----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("---------- Audio Clip ----------")]
    public AudioClip background;
    public AudioClip death;
    public AudioClip checkpoint;
    public AudioClip jump;
    public AudioClip footsteps;
    public AudioClip endpoint;
    public AudioClip foodcollect;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }
}
