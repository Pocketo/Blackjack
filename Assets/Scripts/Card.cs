using UnityEngine;

/// <summary>
/// Representa una carta individual en el juego
/// Maneja la visualización y el valor de la carta
/// </summary>
public class Card : MonoBehaviour
{
    [Header("Card Data")]
    [SerializeField] private Suit suit;
    [SerializeField] private Rank rank;
    [SerializeField] private bool isFaceUp = true;

    [Header("Sprites")]
    [SerializeField] private Sprite cardBackSprite;
    
    private SpriteRenderer spriteRenderer;
    private Sprite cardFaceSprite;

    public Suit Suit => suit;
    public Rank Rank => rank;
    public bool IsFaceUp => isFaceUp;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Inicializa la carta con su palo y rango
    /// </summary>
    public void Initialize(Suit suit, Rank rank, Sprite faceSprite, Sprite backSprite)
    {
        this.suit = suit;
        this.rank = rank;
        this.cardFaceSprite = faceSprite;
        this.cardBackSprite = backSprite;
        
        UpdateVisual();
    }

    /// <summary>
    /// Calcula el valor de la carta en Blackjack
    /// El As puede valer 1 u 11, esto se maneja en Hand
    /// </summary>
    public int GetValue()
    {
        switch (rank)
        {
            case Rank.Ace:
                return 11; // El valor de 1 se ajusta en Hand si es necesario
            case Rank.Jack:
            case Rank.Queen:
            case Rank.King:
                return 10;
            default:
                return (int)rank;
        }
    }

    /// <summary>
    /// Voltea la carta (boca arriba/abajo)
    /// </summary>
    public void Flip()
    {
        isFaceUp = !isFaceUp;
        UpdateVisual();
    }

    /// <summary>
    /// Establece si la carta está boca arriba o abajo
    /// </summary>
    public void SetFaceUp(bool faceUp)
    {
        isFaceUp = faceUp;
        UpdateVisual();
    }

    /// <summary>
    /// Actualiza el sprite según el estado de la carta
    /// </summary>
    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;
        
        spriteRenderer.sprite = isFaceUp ? cardFaceSprite : cardBackSprite;
    }

    /// <summary>
    /// Devuelve una representación en texto de la carta
    /// </summary>
    public override string ToString()
    {
        string rankStr = rank switch
        {
            Rank.Ace => "A",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            _ => ((int)rank).ToString()
        };

        string suitStr = suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };

        return $"{rankStr}{suitStr}";
    }
}
