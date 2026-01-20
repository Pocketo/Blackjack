using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Genera sprites de cartas proceduralmente
/// Útil para testing sin necesidad de assets externos
/// Ejecutar desde el menú: Tools > Blackjack > Generate Card Sprites
/// </summary>
public class CardSpriteGenerator : MonoBehaviour
{
    [Header("Card Settings")]
    [SerializeField] private int cardWidth = 140;
    [SerializeField] private int cardHeight = 190;
    [SerializeField] private int pixelsPerUnit = 100;

    [Header("Colors")]
    [SerializeField] private Color cardBackground = Color.white;
    [SerializeField] private Color redSuitColor = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] private Color blackSuitColor = Color.black;
    [SerializeField] private Color cardBackColor = new Color(0.2f, 0.3f, 0.6f);
    [SerializeField] private Color cardBackPattern = new Color(0.15f, 0.25f, 0.5f);

    private static readonly string[] suitSymbols = { "♥", "♦", "♣", "♠" };
    private static readonly string[] rankSymbols = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

    /// <summary>
    /// Genera todos los sprites de cartas
    /// </summary>
    public Sprite[] GenerateAllCardSprites()
    {
        Sprite[] sprites = new Sprite[52];
        int index = 0;

        for (int suit = 0; suit < 4; suit++)
        {
            for (int rank = 0; rank < 13; rank++)
            {
                sprites[index] = GenerateCardSprite(suit, rank);
                index++;
            }
        }

        return sprites;
    }

    /// <summary>
    /// Genera el sprite del reverso de la carta
    /// </summary>
    public Sprite GenerateCardBackSprite()
    {
        Texture2D texture = new Texture2D(cardWidth, cardHeight);
        
        // Rellenar con color de fondo
        Color[] pixels = new Color[cardWidth * cardHeight];
        
        for (int y = 0; y < cardHeight; y++)
        {
            for (int x = 0; x < cardWidth; x++)
            {
                // Crear patrón de cuadrícula
                bool pattern = ((x / 10) + (y / 10)) % 2 == 0;
                pixels[y * cardWidth + x] = pattern ? cardBackColor : cardBackPattern;
            }
        }
        
        // Borde
        DrawBorder(pixels, cardWidth, cardHeight, Color.black, 2);
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        return Sprite.Create(texture, new Rect(0, 0, cardWidth, cardHeight), 
            new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    /// <summary>
    /// Genera un sprite de carta individual
    /// </summary>
    private Sprite GenerateCardSprite(int suitIndex, int rankIndex)
    {
        Texture2D texture = new Texture2D(cardWidth, cardHeight);
        
        // Rellenar con blanco
        Color[] pixels = new Color[cardWidth * cardHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = cardBackground;
        }
        
        // Borde redondeado (simulado)
        DrawBorder(pixels, cardWidth, cardHeight, Color.black, 2);
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Bilinear;

        return Sprite.Create(texture, new Rect(0, 0, cardWidth, cardHeight), 
            new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    /// <summary>
    /// Dibuja un borde en el array de pixels
    /// </summary>
    private void DrawBorder(Color[] pixels, int width, int height, Color borderColor, int thickness)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < thickness || x >= width - thickness || 
                    y < thickness || y >= height - thickness)
                {
                    pixels[y * width + x] = borderColor;
                }
            }
        }
    }

    /// <summary>
    /// Obtiene el color según el palo
    /// </summary>
    public Color GetSuitColor(Suit suit)
    {
        return (suit == Suit.Hearts || suit == Suit.Diamonds) ? redSuitColor : blackSuitColor;
    }

    /// <summary>
    /// Obtiene el símbolo del palo
    /// </summary>
    public static string GetSuitSymbol(Suit suit)
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

    /// <summary>
    /// Obtiene el símbolo del rango
    /// </summary>
    public static string GetRankSymbol(Rank rank)
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
}

#if UNITY_EDITOR
/// <summary>
/// Editor script para generar cartas desde el menú
/// </summary>
public class CardSpriteGeneratorEditor
{
    [MenuItem("Tools/Blackjack/Generate Placeholder Sprites")]
    public static void GenerateSprites()
    {
        // Crear carpeta si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
        {
            AssetDatabase.CreateFolder("Assets", "Sprites");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Sprites/Cards"))
        {
            AssetDatabase.CreateFolder("Assets/Sprites", "Cards");
        }

        // Crear GameObject temporal con el generador
        GameObject tempObj = new GameObject("TempGenerator");
        CardSpriteGenerator generator = tempObj.AddComponent<CardSpriteGenerator>();

        // Generar sprites
        Sprite[] cardSprites = generator.GenerateAllCardSprites();
        Sprite backSprite = generator.GenerateCardBackSprite();

        // Guardar reverso
        SaveSpriteAsAsset(backSprite, "Assets/Sprites/Cards/card_back.png");

        // Guardar todas las cartas
        string[] suits = { "hearts", "diamonds", "clubs", "spades" };
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        
        int index = 0;
        for (int s = 0; s < 4; s++)
        {
            for (int r = 0; r < 13; r++)
            {
                string filename = $"Assets/Sprites/Cards/card_{suits[s]}_{ranks[r]}.png";
                SaveSpriteAsAsset(cardSprites[index], filename);
                index++;
            }
        }

        // Limpiar
        Object.DestroyImmediate(tempObj);
        AssetDatabase.Refresh();

        Debug.Log("¡Sprites de cartas generados en Assets/Sprites/Cards/!");
        EditorUtility.DisplayDialog("Completado", "Se generaron 52 cartas + 1 reverso", "OK");
    }

    private static void SaveSpriteAsAsset(Sprite sprite, string path)
    {
        Texture2D texture = sprite.texture;
        byte[] pngData = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
    }
}
#endif
