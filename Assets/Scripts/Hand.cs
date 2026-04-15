using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa una mano de cartas (del jugador o del dealer)
/// Maneja la colección de cartas y calcula el puntaje
/// </summary>
public class Hand : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private float cardSpacing = 1.5f;
    [SerializeField] private float cardOverlap = 0.3f; // Solapamiento horizontal
    [SerializeField] private bool isPlayerHand = false; // true = izquierda, false = derecha
    
    [Header("Rendering Settings")]
    [SerializeField] private string sortingLayerName = "Cards"; // Capa para evitar superposición con UI
    [SerializeField] private int baseSortingOrder = 0;
    
    private List<Card> cards = new List<Card>();

    public List<Card> Cards => cards;
    public int CardCount => cards.Count;

    /// <summary>
    /// Añade una carta a la mano y la posiciona
    /// </summary>
    public void AddCard(Card card)
    {
        cards.Add(card);
    
        // Calcular posición final
        Vector3 targetPos = CalculateCardPosition(cards.Count - 1);
    
        // Intentar animar
        CardAnimator animator = card.GetComponent<CardAnimator>();
        if (animator != null)
        {
            animator.AnimateDeal(targetPos);
        }
        else
        {
            card.transform.position = targetPos;
        }
    
        // Actualizar sorting order y layer
        ConfigureCardRendering(card, cards.Count - 1);
    }

    /// <summary>
    /// Calcula la posición de una carta según el índice y la dirección de la mano
    /// </summary>
    private Vector3 CalculateCardPosition(int index)
    {
        float totalWidth = index * cardOverlap;
        
        // Determinar dirección según si es jugador o dealer
        float direction = isPlayerHand ? -1f : 1f; // Jugador: izquierda, Dealer: derecha
        float startX = isPlayerHand ? totalWidth / 2f : -totalWidth / 2f;
        
        return transform.position + new Vector3(
            startX + (index * cardOverlap * direction), 
            0, 
            -index * 0.01f
        );
    }

    /// <summary>
    /// Configura el Sorting Layer y Order para evitar superposición con UI
    /// </summary>
    private void ConfigureCardRendering(Card card, int index)
    {
        SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Asignar sorting layer para que las cartas no se superpongan a UI
            sr.sortingLayerName = sortingLayerName;
            
            // Las cartas más recientes van encima
            sr.sortingOrder = baseSortingOrder + index;
        }
        
        // También configurar sprites hijos si existen
        SpriteRenderer[] childRenderers = card.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = baseSortingOrder + index;
        }
    }

    /// <summary>
    /// Calcula el valor total de la mano
    /// Ajusta automáticamente los Ases de 11 a 1 si es necesario
    /// </summary>
    public int GetValue()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in cards)
        {
            total += card.GetValue();
            if (card.Rank == Rank.Ace)
            {
                aceCount++;
            }
        }

        // Convertir Ases de 11 a 1 si nos pasamos de 21
        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    /// <summary>
    /// Calcula el valor visible (solo cartas boca arriba)
    /// Útil para mostrar el puntaje del dealer durante el juego
    /// </summary>
    public int GetVisibleValue()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in cards)
        {
            if (card.IsFaceUp)
            {
                total += card.GetValue();
                if (card.Rank == Rank.Ace)
                {
                    aceCount++;
                }
            }
        }

        while (total > 21 && aceCount > 0)
        {
            total -= 10;
            aceCount--;
        }

        return total;
    }

    /// <summary>
    /// Verifica si la mano es un Blackjack (21 con 2 cartas)
    /// </summary>
    public bool IsBlackjack()
    {
        return cards.Count == 2 && GetValue() == 21;
    }

    /// <summary>
    /// Verifica si la mano se pasó de 21
    /// </summary>
    public bool IsBust()
    {
        return GetValue() > 21;
    }

    /// <summary>
    /// Verifica si la mano tiene un "soft" total (As contando como 11)
    /// </summary>
    public bool IsSoft()
    {
        int total = 0;
        int aceCount = 0;

        foreach (Card card in cards)
        {
            total += card.GetValue();
            if (card.Rank == Rank.Ace)
            {
                aceCount++;
            }
        }

        // Es soft si hay al menos un As y el total sin ajustar no supera 21
        return aceCount > 0 && total <= 21;
    }

    /// <summary>
    /// Reposiciona todas las cartas en la mano
    /// </summary>
    public void RepositionCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 targetPos = CalculateCardPosition(i);
            cards[i].transform.position = targetPos;
            
            // Re-configurar rendering
            ConfigureCardRendering(cards[i], i);
        }
    }

    /// <summary>
    /// Cambia la dirección de la mano en runtime
    /// </summary>
    public void SetPlayerHand(bool isPlayer)
    {
        isPlayerHand = isPlayer;
        RepositionCards();
    }

    /// <summary>
    /// Limpia todas las cartas de la mano
    /// </summary>
    public void Clear()
    {
        foreach (Card card in cards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        cards.Clear();
    }

    /// <summary>
    /// Voltea todas las cartas boca arriba
    /// </summary>
    public void RevealAllCards()
    {
        foreach (Card card in cards)
        {
            card.SetFaceUp(true);
        }
    }

    /// <summary>
    /// Obtiene la primera carta (útil para verificar carta visible del dealer)
    /// </summary>
    public Card GetFirstCard()
    {
        return cards.Count > 0 ? cards[0] : null;
    }

    /// <summary>
    /// Obtiene la carta oculta del dealer (segunda carta)
    /// </summary>
    public Card GetHoleCard()
    {
        return cards.Count > 1 ? cards[1] : null;
    }

    /// <summary>
    /// Devuelve representación en texto de la mano
    /// </summary>
    public override string ToString()
    {
        string result = "";
        foreach (Card card in cards)
        {
            result += card.ToString() + " ";
        }
        return result.Trim() + $" (Total: {GetValue()})";
    }

#if UNITY_EDITOR
    /// <summary>
    /// Visualización en el Editor para debugging
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        // Color según tipo de mano
        Gizmos.color = isPlayerHand ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Dibujar flecha de dirección
        float direction = isPlayerHand ? -1f : 1f;
        Vector3 arrowEnd = transform.position + new Vector3(direction * 0.6f, 0f, 0f);
        Gizmos.DrawLine(transform.position, arrowEnd);
        
        // Dibujar posiciones de cartas
        Gizmos.color = Color.yellow;
        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 cardPos = CalculateCardPosition(i);
            Gizmos.DrawWireCube(cardPos, new Vector3(0.2f, 0.3f, 0.01f));
        }
    }
#endif
}