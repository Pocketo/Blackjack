using UnityEngine;
using TMPro;

/// <summary>
/// Versión visual de carta usando UI en lugar de sprites
/// Útil para prototipos rápidos sin necesidad de assets gráficos
/// </summary>
public class CardVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private TextMeshPro rankText;
    [SerializeField] private TextMeshPro suitText;
    [SerializeField] private TextMeshPro centerText;

    [Header("Card Colors")]
    [SerializeField] private Color cardFaceColor = Color.white;
    [SerializeField] private Color cardBackColor = new Color(0.2f, 0.3f, 0.6f);
    [SerializeField] private Color redColor = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color blackColor = Color.black;

    private Suit suit;
    private Rank rank;
    private bool isFaceUp = true;

    public void Setup(Suit suit, Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
        UpdateVisuals();
    }

    public void SetFaceUp(bool faceUp)
    {
        isFaceUp = faceUp;
        UpdateVisuals();
    }

    public void Flip()
    {
        isFaceUp = !isFaceUp;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (backgroundRenderer == null) return;

        if (isFaceUp)
        {
            // Mostrar cara de la carta
            backgroundRenderer.color = cardFaceColor;
            
            Color textColor = (suit == Suit.Hearts || suit == Suit.Diamonds) ? redColor : blackColor;
            string suitSymbol = GetSuitSymbol();
            string rankSymbol = GetRankSymbol();

            if (rankText != null)
            {
                rankText.text = rankSymbol;
                rankText.color = textColor;
                rankText.gameObject.SetActive(true);
            }

            if (suitText != null)
            {
                suitText.text = suitSymbol;
                suitText.color = textColor;
                suitText.gameObject.SetActive(true);
            }

            if (centerText != null)
            {
                centerText.text = suitSymbol;
                centerText.color = textColor;
                centerText.fontSize = 80;
                centerText.gameObject.SetActive(true);
            }
        }
        else
        {
            // Mostrar reverso
            backgroundRenderer.color = cardBackColor;

            if (rankText != null)
                rankText.gameObject.SetActive(false);
            
            if (suitText != null)
                suitText.gameObject.SetActive(false);
            
            if (centerText != null)
            {
                centerText.text = "🂠";
                centerText.color = Color.white;
                centerText.gameObject.SetActive(true);
            }
        }
    }

    private string GetSuitSymbol()
    {
        return suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };
    }

    private string GetRankSymbol()
    {
        return rank switch
        {
            Rank.Ace => "A",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            _ => ((int)rank).ToString()
        };
    }

    /// <summary>
    /// Crea una carta visual completa con todos los componentes
    /// </summary>
    public static GameObject CreateCardObject(string name = "Card")
    {
        // GameObject principal
        GameObject cardObj = new GameObject(name);
        
        // Fondo de la carta
        GameObject background = new GameObject("Background");
        background.transform.SetParent(cardObj.transform);
        background.transform.localPosition = Vector3.zero;
        
        SpriteRenderer bgRenderer = background.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateCardSprite();
        bgRenderer.sortingOrder = 0;

        // Texto del rango (esquina superior izquierda)
        GameObject rankObj = new GameObject("RankText");
        rankObj.transform.SetParent(cardObj.transform);
        rankObj.transform.localPosition = new Vector3(-0.5f, 0.7f, 0);
        
        TextMeshPro rankTMP = rankObj.AddComponent<TextMeshPro>();
        rankTMP.fontSize = 3;
        rankTMP.alignment = TextAlignmentOptions.Center;
        rankTMP.sortingOrder = 1;

        // Texto del palo (debajo del rango)
        GameObject suitObj = new GameObject("SuitText");
        suitObj.transform.SetParent(cardObj.transform);
        suitObj.transform.localPosition = new Vector3(-0.5f, 0.4f, 0);
        
        TextMeshPro suitTMP = suitObj.AddComponent<TextMeshPro>();
        suitTMP.fontSize = 2;
        suitTMP.alignment = TextAlignmentOptions.Center;
        suitTMP.sortingOrder = 1;

        // Texto central grande
        GameObject centerObj = new GameObject("CenterText");
        centerObj.transform.SetParent(cardObj.transform);
        centerObj.transform.localPosition = Vector3.zero;
        
        TextMeshPro centerTMP = centerObj.AddComponent<TextMeshPro>();
        centerTMP.fontSize = 8;
        centerTMP.alignment = TextAlignmentOptions.Center;
        centerTMP.sortingOrder = 1;

        // Añadir componente CardVisual
        CardVisual visual = cardObj.AddComponent<CardVisual>();
        
        // Asignar referencias via reflection (o hacerlo manual en el inspector)
        // En producción, esto se haría desde el prefab

        return cardObj;
    }

    /// <summary>
    /// Crea un sprite rectangular simple para el fondo de la carta
    /// </summary>
    private static Sprite CreateCardSprite()
    {
        int width = 140;
        int height = 190;
        
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Borde negro
                if (x < 3 || x >= width - 3 || y < 3 || y >= height - 3)
                {
                    pixels[y * width + x] = Color.black;
                }
                else
                {
                    pixels[y * width + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), 
            new Vector2(0.5f, 0.5f), 100);
    }
}
