using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja la baraja de cartas
/// Se encarga de crear, barajar y repartir cartas
/// </summary>
public class Deck : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject cardPrefab;

    [Header("Card Sprites")]
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private Sprite[] cardSprites; // 52 sprites ordenados
    
    [Header("Settings")]
    [SerializeField] private int numberOfDecks = 1;

    private List<CardData> cards = new List<CardData>();
    private int currentIndex = 0;

    // Estructura interna para datos de carta sin instanciar
    private struct CardData
    {
        public Suit Suit;
        public Rank Rank;
        public int SpriteIndex;
    }

    private void Awake()
    {
        InitializeDeck();
    }

    /// <summary>
    /// Crea todas las cartas de la baraja
    /// </summary>
    public void InitializeDeck()
    {
        cards.Clear();
        currentIndex = 0;

        for (int d = 0; d < numberOfDecks; d++)
        {
            int spriteIndex = 0;
            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new CardData
                    {
                        Suit = suit,
                        Rank = rank,
                        SpriteIndex = spriteIndex
                    });
                    spriteIndex++;
                }
            }
        }

        Shuffle();
    }

    /// <summary>
    /// Baraja las cartas usando Fisher-Yates
    /// </summary>
    public void Shuffle()
    {
        currentIndex = 0;
        
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            CardData temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }

        Debug.Log($"Baraja mezclada: {cards.Count} cartas");
    }

    /// <summary>
    /// Reparte una carta creando el GameObject
    /// </summary>
    public Card DrawCard(Transform parent, Vector3 position, bool faceUp = true)
    {
        if (currentIndex >= cards.Count)
        {
            Debug.LogWarning("¡Baraja agotada! Rebarajando...");
            InitializeDeck();
        }

        CardData data = cards[currentIndex];
        currentIndex++;

        // Crear el GameObject de la carta
        GameObject cardObj = Instantiate(cardPrefab, position, Quaternion.identity, parent);
        Card card = cardObj.GetComponent<Card>();

        if (card == null)
        {
            card = cardObj.AddComponent<Card>();
        }

        // Obtener sprite de la carta
        Sprite faceSprite = GetCardSprite(data.SpriteIndex);
        
        card.Initialize(data.Suit, data.Rank, faceSprite, cardBackSprite);
        card.SetFaceUp(faceUp);

        return card;
    }

    /// <summary>
    /// Obtiene el sprite correspondiente a una carta
    /// </summary>
    private Sprite GetCardSprite(int index)
    {
        if (cardSprites != null && index < cardSprites.Length)
        {
            return cardSprites[index];
        }
        
        // Si no hay sprites, devolver null (se usará color placeholder)
        return null;
    }

    /// <summary>
    /// Devuelve cuántas cartas quedan en la baraja
    /// </summary>
    public int CardsRemaining => cards.Count - currentIndex;

    /// <summary>
    /// Resetea la baraja para un nuevo juego
    /// </summary>
    public void Reset()
    {
        InitializeDeck();
    }
}
