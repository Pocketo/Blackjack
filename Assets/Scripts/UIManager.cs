using UnityEngine;
using UnityEngine.UI;
using TMPro;


/// <summary>
/// Maneja toda la interfaz de usuario del juego
/// Conecta los eventos del juego con los elementos visuales
/// Controla Hit (Q), Stand (W) y New Game (E) usando Input tradicional
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Game Reference")]
    [SerializeField] private BlackjackGame game;

    [Header("Score Displays")]
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI dealerScoreText;

    [Header("Buttons (Visuales)")]
    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Button newGameButton;

    [Header("Result Display")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Labels")]
    [SerializeField] private TextMeshProUGUI playerLabel;
    [SerializeField] private TextMeshProUGUI dealerLabel;

    [Header("Colors")]
    [SerializeField] private Color winColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color loseColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color pushColor = new Color(0.8f, 0.8f, 0.2f);

    private void Start()
    {
        // Suscribirse a eventos del juego
        if (game != null)
        {
            game.OnGameStateChanged.AddListener(OnGameStateChanged);
            game.OnPlayerScoreChanged.AddListener(OnPlayerScoreChanged);
            game.OnDealerScoreChanged.AddListener(OnDealerScoreChanged);
            game.OnGameEnded.AddListener(OnGameEnded);
            game.OnCardDealt.AddListener(OnCardDealt);
        }

        // Estado inicial
        HideResult();
        UpdateButtonStates(GameState.GameOver);
    }

    private void Update()
    {
        // Detectar input del teclado
        DetectKeyboardInput();
    }

    private void OnDestroy()
    {
        // Limpiar listeners de eventos del juego
        if (game != null)
        {
            game.OnGameStateChanged.RemoveListener(OnGameStateChanged);
            game.OnPlayerScoreChanged.RemoveListener(OnPlayerScoreChanged);
            game.OnDealerScoreChanged.RemoveListener(OnDealerScoreChanged);
            game.OnGameEnded.RemoveListener(OnGameEnded);
            game.OnCardDealt.RemoveListener(OnCardDealt);
        }
    }

    #region Input Detection

    /// <summary>
    /// Detecta el input del teclado en cada frame
    /// Q = Hit, W = Stand, E = New Game
    /// </summary>
    private void DetectKeyboardInput()
    {
        // Q para Hit
        if (Input.GetKeyDown(KeyCode.Q) && hitButton != null && hitButton.interactable)
        {
            OnHitClicked();
        }

        // W para Stand
        if (Input.GetKeyDown(KeyCode.W) && standButton != null && standButton.interactable)
        {
            OnStandClicked();
        }

        // E para New Game
        if (Input.GetKeyDown(KeyCode.E) && newGameButton != null && newGameButton.interactable)
        {
            OnNewGameClicked();
        }
    }

    #endregion

    #region Button Handlers

    private void OnHitClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (game != null && game.CanPlayerAct())
            game.PlayerHit();
    }

    private void OnStandClicked()
    {
        if (game != null && game.CanPlayerAct())
        {
            game.PlayerStand();
        }
    }

    private void OnNewGameClicked()
    {
        if (game != null)
        {
            HideResult();
            game.StartNewGame();
        }
    }

    #endregion

    #region Event Handlers

    private void OnGameStateChanged(GameState newState)
    {
        UpdateButtonStates(newState);

        // Actualizar label del dealer según estado
        if (dealerLabel != null)
        {
            if (newState == GameState.PlayerTurn)
            {
                dealerLabel.text = "DEALER";
            }
        }
    }

    private void OnPlayerScoreChanged(int score)
    {
        if (playerScoreText != null)
        {
            playerScoreText.text = score.ToString();
        }
    }

    private void OnDealerScoreChanged(int score)
    {
        if (dealerScoreText != null)
        {
            // Durante el turno del jugador, mostrar "?" para la carta oculta
            if (game != null && game.CurrentState == GameState.PlayerTurn)
            {
                dealerScoreText.text = $"{score}+?";
            }
            else
            {
                dealerScoreText.text = score.ToString();
            }
        }
    }

    private void OnGameEnded(GameResult result)
    {
        ShowResult(result);

        // Reproducir sonido según resultado
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayResultSound(result);
        }
    }

    private void OnCardDealt()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCardDeal();
        }
    }

    #endregion

    #region UI Updates

    private void UpdateButtonStates(GameState state)
    {
        bool canPlay = state == GameState.PlayerTurn;

        if (hitButton != null)
            hitButton.interactable = canPlay;

        if (standButton != null)
            standButton.interactable = canPlay;

        if (newGameButton != null)
            newGameButton.interactable = state == GameState.GameOver;
    }

    private void ShowResult(GameResult result)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            switch (result)
            {
                case GameResult.PlayerWins:
                    resultText.text = "¡GANASTE!";
                    resultText.color = winColor;
                    break;

                case GameResult.PlayerBlackjack:
                    resultText.text = "¡BLACKJACK!";
                    resultText.color = winColor;
                    break;

                case GameResult.DealerWins:
                    resultText.text = "DEALER GANA";
                    resultText.color = loseColor;
                    break;

                case GameResult.DealerBlackjack:
                    resultText.text = "DEALER BLACKJACK";
                    resultText.color = loseColor;
                    break;

                case GameResult.Push:
                    resultText.text = "EMPATE";
                    resultText.color = pushColor;
                    break;

                default:
                    resultText.text = "";
                    break;
            }
        }
    }

    private void HideResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Muestra un mensaje temporal en pantalla
    /// </summary>
    public void ShowMessage(string message, float duration = 2f)
    {
        if (resultText != null && resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultText.text = message;
            resultText.color = Color.white;

            if (duration > 0)
            {
                Invoke(nameof(HideResult), duration);
            }
        }
    }

    /// <summary>
    /// Actualiza manualmente los scores (útil para debug)
    /// </summary>
    public void RefreshScores()
    {
        if (game != null)
        {
            OnPlayerScoreChanged(game.PlayerScore);
            OnDealerScoreChanged(game.DealerVisibleScore);
        }
    }

    #endregion
}