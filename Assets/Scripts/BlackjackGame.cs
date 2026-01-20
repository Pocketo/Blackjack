using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controlador principal del juego de Blackjack
/// Maneja el flujo del juego, turnos y determinación del ganador
/// </summary>
public class BlackjackGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Deck deck;
    [SerializeField] private Hand playerHand;
    [SerializeField] private Hand dealerHand;

    [Header("Settings")]
    [SerializeField] private int dealerStandValue = 17; // Dealer se planta en 17+
    [SerializeField] private float dealerDrawDelay = 0.8f; // Delay entre cartas del dealer

    [Header("Events")]
    public UnityEvent<GameState> OnGameStateChanged;
    public UnityEvent<int> OnPlayerScoreChanged;
    public UnityEvent<int> OnDealerScoreChanged;
    public UnityEvent<GameResult> OnGameEnded;
    public UnityEvent OnCardDealt;

    private GameState currentState = GameState.GameOver;
    private GameResult lastResult = GameResult.None;

    public GameState CurrentState => currentState;
    public GameResult LastResult => lastResult;
    public int PlayerScore => playerHand.GetValue();
    public int DealerScore => dealerHand.GetValue();
    public int DealerVisibleScore => dealerHand.GetVisibleValue();

    private void Start()
    {
        // Auto-iniciar si las referencias están asignadas
        if (deck != null && playerHand != null && dealerHand != null)
        {
            StartNewGame();
        }
    }

    /// <summary>
    /// Inicia una nueva partida de Blackjack
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("=== NUEVA PARTIDA DE BLACKJACK ===");

        // Limpiar manos anteriores
        playerHand.Clear();
        dealerHand.Clear();

        // Verificar si hay suficientes cartas, si no, rebarajar
        if (deck.CardsRemaining < 10)
        {
            deck.Reset();
        }

        lastResult = GameResult.None;

        // Repartir cartas iniciales
        StartCoroutine(DealInitialCards());
    }

    /// <summary>
    /// Reparte las cartas iniciales (2 a cada uno)
    /// </summary>
    private IEnumerator DealInitialCards()
    {
        // Primera carta al jugador (boca arriba)
        Card playerCard1 = deck.DrawCard(playerHand.transform, playerHand.transform.position, true);
        playerHand.AddCard(playerCard1);
        OnCardDealt?.Invoke();
        yield return new WaitForSeconds(0.3f);

        // Primera carta al dealer (boca arriba)
        Card dealerCard1 = deck.DrawCard(dealerHand.transform, dealerHand.transform.position, true);
        dealerHand.AddCard(dealerCard1);
        OnCardDealt?.Invoke();
        yield return new WaitForSeconds(0.3f);

        // Segunda carta al jugador (boca arriba)
        Card playerCard2 = deck.DrawCard(playerHand.transform, playerHand.transform.position, true);
        playerHand.AddCard(playerCard2);
        OnCardDealt?.Invoke();
        yield return new WaitForSeconds(0.3f);

        // Segunda carta al dealer (boca ABAJO - hole card)
        Card dealerCard2 = deck.DrawCard(dealerHand.transform, dealerHand.transform.position, false);
        dealerHand.AddCard(dealerCard2);
        OnCardDealt?.Invoke();

        Debug.Log($"Jugador: {playerHand}");
        Debug.Log($"Dealer: {dealerHand.GetFirstCard()} + [oculta]");

        // Actualizar UI
        OnPlayerScoreChanged?.Invoke(playerHand.GetValue());
        OnDealerScoreChanged?.Invoke(dealerHand.GetVisibleValue());

        // Verificar Blackjack natural
        if (playerHand.IsBlackjack() && dealerHand.IsBlackjack())
        {
            // Revelar carta del dealer
            dealerHand.RevealAllCards();
            EndGame(GameResult.Push);
            yield break;
        }
        else if (playerHand.IsBlackjack())
        {
            dealerHand.RevealAllCards();
            EndGame(GameResult.PlayerBlackjack);
            yield break;
        }
        else if (dealerHand.IsBlackjack())
        {
            dealerHand.RevealAllCards();
            EndGame(GameResult.DealerBlackjack);
            yield break;
        }

        // Turno del jugador
        SetGameState(GameState.PlayerTurn);
    }

    /// <summary>
    /// El jugador pide otra carta (Hit)
    /// </summary>
    public void PlayerHit()
    {
        if (currentState != GameState.PlayerTurn) return;

        Card newCard = deck.DrawCard(playerHand.transform, playerHand.transform.position, true);
        playerHand.AddCard(newCard);
        OnCardDealt?.Invoke();

        Debug.Log($"Jugador pide carta: {newCard}");
        Debug.Log($"Mano del jugador: {playerHand}");

        OnPlayerScoreChanged?.Invoke(playerHand.GetValue());

        // Verificar si se pasó
        if (playerHand.IsBust())
        {
            Debug.Log("¡Jugador se pasó!");
            EndGame(GameResult.DealerWins);
        }
        
    }

    /// <summary>
    /// El jugador se planta (Stand)
    /// </summary>
    public void PlayerStand()
    {
        if (currentState != GameState.PlayerTurn) return;

        Debug.Log("Jugador se planta");
        StartCoroutine(DealerTurn());
    }

    /// <summary>
    /// Turno del dealer (automático)
    /// </summary>
    private IEnumerator DealerTurn()
    {
        
        SetGameState(GameState.DealerTurn);

        // Animar el flip de la carta oculta
        Card holeCard = dealerHand.GetHoleCard();
        if (holeCard != null)
        {
            CardAnimator animator = holeCard.GetComponent<CardAnimator>();
            if (animator != null)
            {
                yield return animator.AnimateFlip(() => holeCard.SetFaceUp(true));
            }
            else
            {
                holeCard.SetFaceUp(true);
            }
        }
    
        OnDealerScoreChanged?.Invoke(dealerHand.GetValue());
        // Revelar carta oculta
        dealerHand.RevealAllCards();
        OnDealerScoreChanged?.Invoke(dealerHand.GetValue());
        
        Debug.Log($"Dealer revela: {dealerHand}");

        yield return new WaitForSeconds(dealerDrawDelay);

        // El dealer pide cartas hasta tener 17 o más
        while (dealerHand.GetValue() < dealerStandValue)
        {
            Card newCard = deck.DrawCard(dealerHand.transform, dealerHand.transform.position, true);
            dealerHand.AddCard(newCard);
            OnCardDealt?.Invoke();

            Debug.Log($"Dealer pide carta: {newCard}");
            Debug.Log($"Mano del dealer: {dealerHand}");

            OnDealerScoreChanged?.Invoke(dealerHand.GetValue());

            yield return new WaitForSeconds(dealerDrawDelay);
        }

        // Determinar ganador
        DetermineWinner();
    }

    /// <summary>
    /// Determina el ganador de la partida
    /// </summary>
    private void DetermineWinner()
    {
        int playerValue = playerHand.GetValue();
        int dealerValue = dealerHand.GetValue();

        Debug.Log($"--- RESULTADO FINAL ---");
        Debug.Log($"Jugador: {playerValue} | Dealer: {dealerValue}");

        if (dealerHand.IsBust())
        {
            Debug.Log("¡Dealer se pasó! Jugador gana.");
            EndGame(GameResult.PlayerWins);
        }
        else if (playerValue > dealerValue)
        {
            Debug.Log("¡Jugador gana!");
            EndGame(GameResult.PlayerWins);
        }
        else if (dealerValue > playerValue)
        {
            Debug.Log("Dealer gana.");
            EndGame(GameResult.DealerWins);
        }
        else
        {
            Debug.Log("Empate (Push).");
            EndGame(GameResult.Push);
        }
    }

    /// <summary>
    /// Finaliza el juego con el resultado dado
    /// </summary>
    private void EndGame(GameResult result)
    {
        lastResult = result;
        SetGameState(GameState.GameOver);
        OnGameEnded?.Invoke(result);
    }

    /// <summary>
    /// Cambia el estado del juego y notifica
    /// </summary>
    private void SetGameState(GameState newState)
    {
        currentState = newState;
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"Estado del juego: {newState}");
    }

    /// <summary>
    /// Verifica si el jugador puede realizar acciones
    /// </summary>
    public bool CanPlayerAct()
    {
        return currentState == GameState.PlayerTurn;
    }
}
