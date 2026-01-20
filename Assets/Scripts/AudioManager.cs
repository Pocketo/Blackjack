using UnityEngine;

/// <summary>
/// Maneja los efectos de sonido del juego
/// Opcional pero mejora la experiencia
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip cardDealSound;
    [SerializeField] private AudioClip cardFlipSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip pushSound;
    [SerializeField] private AudioClip blackjackSound;
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Settings")]
    [SerializeField] private float sfxVolume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayCardDeal()
    {
        PlaySound(cardDealSound);
    }

    public void PlayCardFlip()
    {
        PlaySound(cardFlipSound);
    }

    public void PlayWin()
    {
        PlaySound(winSound);
    }

    public void PlayLose()
    {
        PlaySound(loseSound);
    }

    public void PlayPush()
    {
        PlaySound(pushSound);
    }

    public void PlayBlackjack()
    {
        PlaySound(blackjackSound);
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

    /// <summary>
    /// Reproduce un sonido basado en el resultado del juego
    /// </summary>
    public void PlayResultSound(GameResult result)
    {
        switch (result)
        {
            case GameResult.PlayerWins:
                PlayWin();
                break;
            case GameResult.PlayerBlackjack:
                PlayBlackjack();
                break;
            case GameResult.DealerWins:
            case GameResult.DealerBlackjack:
                PlayLose();
                break;
            case GameResult.Push:
                PlayPush();
                break;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void SetVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
