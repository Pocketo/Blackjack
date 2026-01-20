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
    
        // Actualizar sorting order
        SpriteRenderer sr = card.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = cards.Count - 1;
    }

    private Vector3 CalculateCardPosition(int index)
    {
        float totalWidth = index * cardOverlap;
        float startX = -totalWidth / 2f;
        return transform.position + new Vector3(startX + (index * cardOverlap), 0, -index * 0.01f);
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
    private void RepositionCards()
    {
        float totalWidth = (cards.Count - 1) * cardOverlap;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cards.Count; i++)
        {
            Vector3 targetPos = transform.position + new Vector3(startX + (i * cardOverlap), 0, -i * 0.01f);
            cards[i].transform.position = targetPos;
            
            // Asegurar que las cartas más recientes estén encima
            SpriteRenderer sr = cards[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = i;
            }
        }
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
}
