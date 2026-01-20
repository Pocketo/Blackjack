using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sistema de combate que integra Blackjack con mecánicas de daño y vida
/// </summary>
public class CombatSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BlackjackGame blackjackGame;

    [Header("Player Stats")]
    [SerializeField] private int playerMaxHealth = 100;
    [SerializeField] private int playerCurrentHealth;

    [Header("Dealer/Enemy Stats")]
    [SerializeField] private int dealerMaxHealth = 100;
    [SerializeField] private int dealerCurrentHealth;

    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevels = 6;
    [SerializeField] private LevelConfig[] levelConfigs;

    [Header("Combat State")]
    [SerializeField] private bool playerIsVulnerable = false;
    [SerializeField] private float vulnerableDamageMultiplier = 2f;

    [Header("Events")]
    public UnityEvent<int, int> OnPlayerHealthChanged;      // current, max
    public UnityEvent<int, int> OnDealerHealthChanged;      // current, max
    public UnityEvent<int> OnPlayerTookDamage;              // damage amount
    public UnityEvent<int> OnDealerTookDamage;              // damage amount
    public UnityEvent<int> OnLevelChanged;                  // new level
    public UnityEvent OnLevelComplete;
    public UnityEvent OnGameOver;
    public UnityEvent OnVictory;
    public UnityEvent OnPlayerBecameVulnerable;
    public UnityEvent OnPlayerVulnerabilityCleared;

    // Properties públicas
    public int PlayerHealth => playerCurrentHealth;
    public int PlayerMaxHealth => playerMaxHealth;
    public int DealerHealth => dealerCurrentHealth;
    public int DealerMaxHealth => dealerMaxHealth;
    public int CurrentLevel => currentLevel;
    public bool IsPlayerVulnerable => playerIsVulnerable;
    public LevelConfig CurrentLevelConfig => GetCurrentLevelConfig();

    private void Awake()
    {
        // Crear configuraciones por defecto si no existen
        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            CreateDefaultLevelConfigs();
        }
    }

    private void Start()
    {
        if (blackjackGame != null)
        {
            blackjackGame.OnGameEnded.AddListener(OnBlackjackRoundEnded);
        }

        // Delay para asegurar que todos los listeners estén suscritos
        Invoke(nameof(DelayedInitialize), 0.1f);
    }

    private void DelayedInitialize()
    {
        InitializeCombat();
    }

    private void OnDestroy()
    {
        if (blackjackGame != null)
        {
            blackjackGame.OnGameEnded.RemoveListener(OnBlackjackRoundEnded);
        }
    }

    /// <summary>
    /// Inicializa el combate para el nivel actual
    /// </summary>
    public void InitializeCombat()
    {
        LevelConfig config = GetCurrentLevelConfig();
        
        playerMaxHealth = config.playerHealth;
        playerCurrentHealth = playerMaxHealth;
        
        dealerMaxHealth = config.dealerHealth;
        dealerCurrentHealth = dealerMaxHealth;
        
        playerIsVulnerable = false;

        OnPlayerHealthChanged?.Invoke(playerCurrentHealth, playerMaxHealth);
        OnDealerHealthChanged?.Invoke(dealerCurrentHealth, dealerMaxHealth);
        OnLevelChanged?.Invoke(currentLevel);

        Debug.Log($"=== NIVEL {currentLevel} INICIADO ===");
        Debug.Log($"Jugador: {playerCurrentHealth}/{playerMaxHealth} HP");
        Debug.Log($"Dealer: {dealerCurrentHealth}/{dealerMaxHealth} HP");
        Debug.Log($"Umbral mínimo para dañar: {config.minimumScoreToDamage}");
    }

    /// <summary>
    /// Procesa el resultado de una mano de Blackjack
    /// </summary>
    private void OnBlackjackRoundEnded(GameResult result)
    {
        int playerScore = blackjackGame.PlayerScore;
        int dealerScore = blackjackGame.DealerScore;
        
        LevelConfig config = GetCurrentLevelConfig();

        Debug.Log($"--- Resultado de la mano ---");
        Debug.Log($"Jugador: {playerScore} | Dealer: {dealerScore}");

        switch (result)
        {
            case GameResult.PlayerBlackjack:
                // Blackjack = Daño crítico al dealer
                int critDamage = CalculateCriticalDamage(config);
                DealDamageToDealerInternal(critDamage);
                Debug.Log($"¡BLACKJACK! Daño crítico: {critDamage}");
                ClearVulnerability();
                break;

            case GameResult.PlayerWins:
                // Jugador gana - hace daño al dealer
                if (playerScore >= config.minimumScoreToDamage)
                {
                    int damage = CalculateDamage(playerScore, config);
                    DealDamageToDealerInternal(damage);
                    Debug.Log($"Jugador hace {damage} de daño");
                }
                else
                {
                    Debug.Log($"Puntaje {playerScore} menor al umbral {config.minimumScoreToDamage}, sin daño");
                }
                ClearVulnerability();
                break;

            case GameResult.DealerBlackjack:
                // Dealer tiene Blackjack - daño crítico al jugador
                int dealerCritDamage = CalculateCriticalDamage(config);
                DealDamageToPlayerInternal(dealerCritDamage);
                Debug.Log($"¡DEALER BLACKJACK! Daño crítico: {dealerCritDamage}");
                break;

            case GameResult.DealerWins:
                // Verificar si el jugador se pasó (bust)
                if (playerScore > 21)
                {
                    // Jugador se pasó - queda vulnerable
                    SetPlayerVulnerable();
                    Debug.Log("¡Jugador se pasó! Queda VULNERABLE");
                }
                else
                {
                    // Dealer gana normalmente - hace daño al jugador
                    if (dealerScore >= config.minimumScoreToDamage)
                    {
                        int damage = CalculateDamage(dealerScore, config);
                        DealDamageToPlayerInternal(damage);
                        Debug.Log($"Dealer hace {damage} de daño");
                    }
                }
                break;

            case GameResult.Push:
                // Empate - nadie recibe daño
                Debug.Log("Empate - Sin daño");
                ClearVulnerability();
                break;
        }

        // Verificar condiciones de victoria/derrota
        CheckCombatEnd();
    }

    /// <summary>
    /// Calcula el daño basado en el puntaje
    /// </summary>
    private int CalculateDamage(int score, LevelConfig config)
    {
        // Tabla de daño base según puntaje
        int baseDamage = score switch
        {
            21 => config.damage21,      // 21 (sin blackjack)
            20 => config.damage20,
            19 => config.damage19,
            18 => config.damage18,
            17 => config.damage17,
            16 => config.damage16,
            15 => config.damage15,
            14 => config.damage14,
            13 => config.damage13,
            _ => config.damageMinimum   // 12 o menor (si aplica)
        };

        return baseDamage;
    }

    /// <summary>
    /// Calcula el daño crítico (Blackjack)
    /// </summary>
    private int CalculateCriticalDamage(LevelConfig config)
    {
        return config.blackjackDamage;
    }

    /// <summary>
    /// Aplica daño al jugador
    /// </summary>
    private void DealDamageToPlayerInternal(int damage)
    {
        // Aplicar multiplicador de vulnerabilidad
        if (playerIsVulnerable)
        {
            damage = Mathf.RoundToInt(damage * vulnerableDamageMultiplier);
            Debug.Log($"¡VULNERABLE! Daño aumentado a {damage}");
            ClearVulnerability();
        }

        playerCurrentHealth = Mathf.Max(0, playerCurrentHealth - damage);
        
        OnPlayerTookDamage?.Invoke(damage);
        OnPlayerHealthChanged?.Invoke(playerCurrentHealth, playerMaxHealth);
    }

    /// <summary>
    /// Aplica daño al dealer
    /// </summary>
    private void DealDamageToDealerInternal(int damage)
    {
        dealerCurrentHealth = Mathf.Max(0, dealerCurrentHealth - damage);
        
        OnDealerTookDamage?.Invoke(damage);
        OnDealerHealthChanged?.Invoke(dealerCurrentHealth, dealerMaxHealth);
    }

    /// <summary>
    /// Marca al jugador como vulnerable
    /// </summary>
    private void SetPlayerVulnerable()
    {
        playerIsVulnerable = true;
        OnPlayerBecameVulnerable?.Invoke();
    }

    /// <summary>
    /// Limpia el estado de vulnerabilidad
    /// </summary>
    private void ClearVulnerability()
    {
        if (playerIsVulnerable)
        {
            playerIsVulnerable = false;
            OnPlayerVulnerabilityCleared?.Invoke();
            Debug.Log("Vulnerabilidad eliminada");
        }
    }

    /// <summary>
    /// Verifica si el combate terminó
    /// </summary>
    private void CheckCombatEnd()
    {
        if (playerCurrentHealth <= 0)
        {
            Debug.Log("=== GAME OVER ===");
            OnGameOver?.Invoke();
        }
        else if (dealerCurrentHealth <= 0)
        {
            Debug.Log($"=== NIVEL {currentLevel} COMPLETADO ===");
            OnLevelComplete?.Invoke();
            
            // Auto-avanzar al siguiente nivel después de un delay
            // O puedes manejarlo desde UI
        }
    }

    /// <summary>
    /// Avanza al siguiente nivel
    /// </summary>
    public void AdvanceToNextLevel()
    {
        if (currentLevel < maxLevels)
        {
            currentLevel++;
            InitializeCombat();
            
            if (blackjackGame != null)
            {
                blackjackGame.StartNewGame();
            }
        }
        else
        {
            Debug.Log("=== ¡VICTORIA TOTAL! ===");
            OnVictory?.Invoke();
        }
    }

    /// <summary>
    /// Reinicia el juego desde el nivel 1
    /// </summary>
    public void RestartGame()
    {
        currentLevel = 1;
        playerIsVulnerable = false;
        InitializeCombat();
        
        if (blackjackGame != null)
        {
            blackjackGame.StartNewGame();
        }
    }

    /// <summary>
    /// Obtiene la configuración del nivel actual
    /// </summary>
    private LevelConfig GetCurrentLevelConfig()
    {
        if (levelConfigs != null && currentLevel - 1 < levelConfigs.Length)
        {
            return levelConfigs[currentLevel - 1];
        }
        return LevelConfig.CreateDefault(currentLevel);
    }

    /// <summary>
    /// Crea configuraciones por defecto para los 6 niveles
    /// </summary>
    private void CreateDefaultLevelConfigs()
    {
        levelConfigs = new LevelConfig[maxLevels];
        
        for (int i = 0; i < maxLevels; i++)
        {
            levelConfigs[i] = LevelConfig.CreateDefault(i + 1);
        }
    }

    /// <summary>
    /// Obtiene el umbral mínimo actual para hacer daño
    /// </summary>
    public int GetCurrentMinimumScore()
    {
        return GetCurrentLevelConfig().minimumScoreToDamage;
    }
}
