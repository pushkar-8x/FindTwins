using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Game SFX")]
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioClip flipSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayFlipSound() => PlaySound(flipSound);
    public void PlayMatchSound() => PlaySound(matchSound);
    public void PlayFailSound() => PlaySound(failSound);

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
