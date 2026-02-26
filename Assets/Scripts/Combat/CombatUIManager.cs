using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Maneja la UI del sistema de combate
/// Barras de vida, indicadores de daño, estado de vulnerabilidad
/// </summary>
public class CombatUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatSystem combatSystem;

    [Header("Player Health UI")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Image playerHealthFill;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Header("Dealer Health UI")]
    [SerializeField] private Slider dealerHealthSlider;
    [SerializeField] private Image dealerHealthFill;
    [SerializeField] private TextMeshProUGUI dealerHealthText;
    [SerializeField] private TextMeshProUGUI dealerNameText;

    [Header("Level Info UI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI thresholdText;

    [Header("Damage Popup")]
    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private Transform playerDamageSpawnPoint;
    [SerializeField] private Transform dealerDamageSpawnPoint;

    [Header("Vulnerability Indicator")]
    [SerializeField] private GameObject vulnerabilityIndicator;
    [SerializeField] private TextMeshProUGUI vulnerabilityText;

    [Header("Game Over / Victory Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Colors")]
    [SerializeField] private Color healthHighColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color healthMediumColor = new Color(0.9f, 0.7f, 0.1f);
    [SerializeField] private Color healthLowColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color damageTextColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color criticalDamageColor = new Color(1f, 0.8f, 0f);
    [SerializeField] private Color vulnerableColor = new Color(0.8f, 0.2f, 0.8f);

    [Header("Animation Settings")]
    [SerializeField] private float healthAnimationDuration = 0.5f;
    [SerializeField] private float damagePopupDuration = 1.5f;
    [SerializeField] private float shakeIntensity = 10f;
    [SerializeField] private float shakeDuration = 0.3f;

    private Coroutine playerHealthCoroutine;
    private Coroutine dealerHealthCoroutine;

    private void Awake()
    {
        // Suscribirse a eventos lo antes posible
        if (combatSystem != null)
        {
            combatSystem.OnPlayerHealthChanged.AddListener(UpdatePlayerHealth);
            combatSystem.OnDealerHealthChanged.AddListener(UpdateDealerHealth);
            combatSystem.OnPlayerTookDamage.AddListener(ShowPlayerDamage);
            combatSystem.OnDealerTookDamage.AddListener(ShowDealerDamage);
            combatSystem.OnLevelChanged.AddListener(UpdateLevelInfo);
            combatSystem.OnLevelComplete.AddListener(ShowLevelComplete);
            combatSystem.OnGameOver.AddListener(ShowGameOver);
            combatSystem.OnVictory.AddListener(ShowVictory);
            combatSystem.OnPlayerBecameVulnerable.AddListener(ShowVulnerability);
            combatSystem.OnPlayerVulnerabilityCleared.AddListener(HideVulnerability);
        }
    }

    private void Start()
    {
        // Configurar botones
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        // Inicializar sliders
        if (playerHealthSlider != null)
        {
            playerHealthSlider.minValue = 0;
            playerHealthSlider.maxValue = 1;
            playerHealthSlider.value = 1;
        }
        if (dealerHealthSlider != null)
        {
            dealerHealthSlider.minValue = 0;
            dealerHealthSlider.maxValue = 1;
            dealerHealthSlider.value = 1;
        }

        // Ocultar paneles
        HideAllPanels();
        HideVulnerability();

        // Forzar actualización inicial de la UI después de un frame
        Invoke(nameof(RefreshAllUI), 0.2f);
    }

    private void OnDestroy()
    {
        if (combatSystem != null)
        {
            combatSystem.OnPlayerHealthChanged.RemoveListener(UpdatePlayerHealth);
            combatSystem.OnDealerHealthChanged.RemoveListener(UpdateDealerHealth);
            combatSystem.OnPlayerTookDamage.RemoveListener(ShowPlayerDamage);
            combatSystem.OnDealerTookDamage.RemoveListener(ShowDealerDamage);
            combatSystem.OnLevelChanged.RemoveListener(UpdateLevelInfo);
            combatSystem.OnLevelComplete.RemoveListener(ShowLevelComplete);
            combatSystem.OnGameOver.RemoveListener(ShowGameOver);
            combatSystem.OnVictory.RemoveListener(ShowVictory);
            combatSystem.OnPlayerBecameVulnerable.RemoveListener(ShowVulnerability);
            combatSystem.OnPlayerVulnerabilityCleared.RemoveListener(HideVulnerability);
        }
    }

    #region Health Updates

    private void UpdatePlayerHealth(int current, int max)
    {
        if (playerHealthCoroutine != null)
            StopCoroutine(playerHealthCoroutine);
        
        playerHealthCoroutine = StartCoroutine(AnimateHealthBar(
            playerHealthSlider, 
            playerHealthFill, 
            playerHealthText, 
            current, 
            max
        ));
    }

    private void UpdateDealerHealth(int current, int max)
    {
        if (dealerHealthCoroutine != null)
            StopCoroutine(dealerHealthCoroutine);
        
        dealerHealthCoroutine = StartCoroutine(AnimateHealthBar(
            dealerHealthSlider, 
            dealerHealthFill, 
            dealerHealthText, 
            current, 
            max
        ));
    }

    private IEnumerator AnimateHealthBar(Slider slider, Image fill, TextMeshProUGUI text, int current, int max)
    {
        if (slider == null) yield break;

        float startValue = slider.value;
        float targetValue = (float)current / max;
        float elapsed = 0f;

        while (elapsed < healthAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / healthAnimationDuration;
            
            // Easing suave
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            slider.value = Mathf.Lerp(startValue, targetValue, t);
            
            // Actualizar color según porcentaje
            if (fill != null)
            {
                fill.color = GetHealthColor(slider.value);
            }
            
            yield return null;
        }

        slider.value = targetValue;
        
        if (text != null)
        {
            text.text = $"{current}/{max}";
        }
        
        if (fill != null)
        {
            fill.color = GetHealthColor(targetValue);
        }
    }

    private Color GetHealthColor(float percentage)
    {
        if (percentage > 0.6f)
            return healthHighColor;
        else if (percentage > 0.3f)
            return healthMediumColor;
        else
            return healthLowColor;
    }

    #endregion

    #region Damage Popups

    private void ShowPlayerDamage(int damage)
    {
        SpawnDamagePopup(playerDamageSpawnPoint, damage, false);
        StartCoroutine(ShakeUI(playerHealthSlider?.transform));
    }

    private void ShowDealerDamage(int damage)
    {
        SpawnDamagePopup(dealerDamageSpawnPoint, damage, damage >= 40);
    }

    private void SpawnDamagePopup(Transform spawnPoint, int damage, bool isCritical)
    {
        if (damagePopupPrefab == null || spawnPoint == null) return;

        GameObject popup = Instantiate(damagePopupPrefab, spawnPoint.position, Quaternion.identity, transform);
        
        TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();
        if (popupText == null)
            popupText = popup.GetComponentInChildren<TextMeshProUGUI>();

        if (popupText != null)
        {
            popupText.text = isCritical ? $"-{damage}!" : $"-{damage}";
            popupText.color = isCritical ? criticalDamageColor : damageTextColor;
            popupText.fontSize = isCritical ? 48 : 36;
        }

        StartCoroutine(AnimateDamagePopup(popup));
    }

    private IEnumerator AnimateDamagePopup(GameObject popup)
    {
        if (popup == null) yield break;

        Vector3 startPos = popup.transform.position;
        float elapsed = 0f;

        while (elapsed < damagePopupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / damagePopupDuration;

            // Mover hacia arriba
            popup.transform.position = startPos + Vector3.up * (t * 50f);

            // Fade out
            TextMeshProUGUI text = popup.GetComponent<TextMeshProUGUI>();
            if (text == null) text = popup.GetComponentInChildren<TextMeshProUGUI>();
            
            if (text != null)
            {
                Color c = text.color;
                c.a = 1f - t;
                text.color = c;
            }

            yield return null;
        }

        Destroy(popup);
    }

    private IEnumerator ShakeUI(Transform target)
    {
        if (target == null) yield break;

        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-shakeIntensity, shakeIntensity);
            float y = Random.Range(-shakeIntensity, shakeIntensity);
            target.localPosition = originalPos + new Vector3(x, y, 0);
            yield return null;
        }

        target.localPosition = originalPos;
    }

    #endregion

    #region Level Info

    private void UpdateLevelInfo(int level)
    {
        if (levelText != null)
            levelText.text = $"NIVEL {level}";

        // Intentar obtener config del combatSystem
        LevelConfig config = null;
        if (combatSystem != null)
        {
            config = combatSystem.CurrentLevelConfig;
        }

        if (config != null)
        {
            if (levelNameText != null)
                levelNameText.text = config.levelName;

            if (thresholdText != null)
                thresholdText.text = $"{config.minimumScoreToDamage}";

            if (dealerNameText != null)
                dealerNameText.text = config.isBossLevel ? config.bossName : config.levelName;
        }
        else
        {
            // Valores por defecto si no hay config
            if (levelNameText != null)
                levelNameText.text = $"Nivel {level}";

            if (thresholdText != null)
                thresholdText.text = "Mínimo para dañar: --";
        }

        Debug.Log($"UI Level Info actualizado: Nivel {level}");
    }

    #endregion

    #region Vulnerability

    private void ShowVulnerability()
    {
        if (vulnerabilityIndicator != null)
        {
            vulnerabilityIndicator.SetActive(true);
            StartCoroutine(PulseVulnerability());
        }

        if (vulnerabilityText != null)
        {
            vulnerabilityText.text = "¡VULNERABLE!\nx2 Daño";
            vulnerabilityText.color = vulnerableColor;
        }
    }

    private void HideVulnerability()
    {
        if (vulnerabilityIndicator != null)
            vulnerabilityIndicator.SetActive(false);
    }

    private IEnumerator PulseVulnerability()
    {
        while (vulnerabilityIndicator != null && vulnerabilityIndicator.activeSelf)
        {
            if (vulnerabilityText != null)
            {
                // Pulse alpha
                float alpha = (Mathf.Sin(Time.time * 5f) + 1f) / 2f;
                alpha = Mathf.Lerp(0.5f, 1f, alpha);
                
                Color c = vulnerableColor;
                c.a = alpha;
                vulnerabilityText.color = c;
            }
            yield return null;
        }
    }

    #endregion

    #region Panels

    private void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void ShowVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    private void ShowLevelComplete()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(true);
    }

    #endregion

    #region Button Handlers

    private void OnRestartClicked()
    {
        HideAllPanels();
        if (combatSystem != null)
            combatSystem.RestartGame();
    }

    private void OnNextLevelClicked()
    {
        HideAllPanels();
        if (combatSystem != null)
            combatSystem.AdvanceToNextLevel();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Actualiza toda la UI manualmente
    /// </summary>
    public void RefreshAllUI()
    {
        if (combatSystem == null) 
        {
            Debug.LogWarning("CombatUIManager: CombatSystem no asignado");
            return;
        }

        // Forzar actualización de salud
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = (float)combatSystem.PlayerHealth / combatSystem.PlayerMaxHealth;
        }
        if (playerHealthText != null)
        {
            playerHealthText.text = $"{combatSystem.PlayerHealth}/{combatSystem.PlayerMaxHealth}";
        }
        if (playerHealthFill != null)
        {
            playerHealthFill.color = GetHealthColor((float)combatSystem.PlayerHealth / combatSystem.PlayerMaxHealth);
        }

        if (dealerHealthSlider != null)
        {
            dealerHealthSlider.value = (float)combatSystem.DealerHealth / combatSystem.DealerMaxHealth;
        }
        if (dealerHealthText != null)
        {
            dealerHealthText.text = $"{combatSystem.DealerHealth}/{combatSystem.DealerMaxHealth}";
        }
        if (dealerHealthFill != null)
        {
            dealerHealthFill.color = GetHealthColor((float)combatSystem.DealerHealth / combatSystem.DealerMaxHealth);
        }

        // Actualizar info del nivel
        UpdateLevelInfo(combatSystem.CurrentLevel);

        // Actualizar vulnerabilidad
        if (combatSystem.IsPlayerVulnerable)
            ShowVulnerability();
        else
            HideVulnerability();

        Debug.Log($"UI Actualizada - Nivel: {combatSystem.CurrentLevel}, Jugador: {combatSystem.PlayerHealth}/{combatSystem.PlayerMaxHealth}, Dealer: {combatSystem.DealerHealth}/{combatSystem.DealerMaxHealth}");
    }

    #endregion
}
