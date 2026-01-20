using UnityEngine;

/// <summary>
/// Configuración de un nivel de combate
/// Define vida, daños y umbral mínimo para cada nivel
/// </summary>
[System.Serializable]
public class LevelConfig
{
    [Header("Level Info")]
    public string levelName;
    public int levelNumber;

    [Header("Health")]
    public int playerHealth = 100;
    public int dealerHealth = 100;

    [Header("Damage Threshold")]
    [Tooltip("Puntaje mínimo requerido para hacer daño")]
    public int minimumScoreToDamage = 13;

    [Header("Damage Values")]
    public int blackjackDamage = 50;    // Daño crítico
    public int damage21 = 35;           // 21 sin blackjack
    public int damage20 = 30;
    public int damage19 = 25;
    public int damage18 = 20;
    public int damage17 = 15;
    public int damage16 = 12;
    public int damage15 = 10;
    public int damage14 = 8;
    public int damage13 = 6;
    public int damageMinimum = 5;       // Para puntajes menores

    [Header("Boss Settings")]
    public bool isBossLevel = false;
    public string bossName = "";

    /// <summary>
    /// Crea una configuración por defecto basada en el número de nivel
    /// </summary>
    public static LevelConfig CreateDefault(int level)
    {
        LevelConfig config = new LevelConfig();
        config.levelNumber = level;
        
        switch (level)
        {
            case 1:
                config.levelName = "El Aprendiz";
                config.playerHealth = 100;
                config.dealerHealth = 80;
                config.minimumScoreToDamage = 13;
                config.blackjackDamage = 40;
                config.damage21 = 30;
                config.damage20 = 25;
                config.damage19 = 20;
                config.damage18 = 15;
                config.damage17 = 12;
                config.damage16 = 10;
                config.damage15 = 8;
                config.damage14 = 6;
                config.damage13 = 5;
                config.damageMinimum = 4;
                break;

            case 2:
                config.levelName = "El Tahúr";
                config.playerHealth = 100;
                config.dealerHealth = 100;
                config.minimumScoreToDamage = 14;
                config.blackjackDamage = 45;
                config.damage21 = 32;
                config.damage20 = 26;
                config.damage19 = 21;
                config.damage18 = 16;
                config.damage17 = 13;
                config.damage16 = 10;
                config.damage15 = 8;
                config.damage14 = 6;
                config.damage13 = 0;  // Ya no hace daño
                config.damageMinimum = 0;
                break;

            case 3:
                config.levelName = "La Condesa";
                config.playerHealth = 100;
                config.dealerHealth = 120;
                config.minimumScoreToDamage = 15;
                config.blackjackDamage = 50;
                config.damage21 = 35;
                config.damage20 = 28;
                config.damage19 = 22;
                config.damage18 = 17;
                config.damage17 = 13;
                config.damage16 = 10;
                config.damage15 = 7;
                config.damage14 = 0;
                config.damage13 = 0;
                config.damageMinimum = 0;
                break;

            case 4:
                config.levelName = "El Barón";
                config.playerHealth = 100;
                config.dealerHealth = 140;
                config.minimumScoreToDamage = 16;
                config.blackjackDamage = 55;
                config.damage21 = 38;
                config.damage20 = 30;
                config.damage19 = 24;
                config.damage18 = 18;
                config.damage17 = 14;
                config.damage16 = 10;
                config.damage15 = 0;
                config.damage14 = 0;
                config.damage13 = 0;
                config.damageMinimum = 0;
                break;

            case 5:
                config.levelName = "La Reina Negra";
                config.playerHealth = 100;
                config.dealerHealth = 160;
                config.minimumScoreToDamage = 17;
                config.blackjackDamage = 60;
                config.damage21 = 42;
                config.damage20 = 33;
                config.damage19 = 26;
                config.damage18 = 20;
                config.damage17 = 15;
                config.damage16 = 0;
                config.damage15 = 0;
                config.damage14 = 0;
                config.damage13 = 0;
                config.damageMinimum = 0;
                break;

            case 6:
                config.levelName = "El Rey de Espadas";
                config.playerHealth = 100;
                config.dealerHealth = 200;
                config.minimumScoreToDamage = 18;
                config.isBossLevel = true;
                config.bossName = "Rey de Espadas";
                config.blackjackDamage = 70;
                config.damage21 = 50;
                config.damage20 = 38;
                config.damage19 = 28;
                config.damage18 = 20;
                config.damage17 = 0;
                config.damage16 = 0;
                config.damage15 = 0;
                config.damage14 = 0;
                config.damage13 = 0;
                config.damageMinimum = 0;
                break;

            default:
                config.levelName = $"Nivel {level}";
                config.playerHealth = 100;
                config.dealerHealth = 80 + (level * 20);
                config.minimumScoreToDamage = 12 + level;
                break;
        }

        return config;
    }

    /// <summary>
    /// Obtiene el daño correspondiente a un puntaje
    /// </summary>
    public int GetDamageForScore(int score)
    {
        if (score < minimumScoreToDamage) return 0;

        return score switch
        {
            21 => damage21,
            20 => damage20,
            19 => damage19,
            18 => damage18,
            17 => damage17,
            16 => damage16,
            15 => damage15,
            14 => damage14,
            13 => damage13,
            _ => damageMinimum
        };
    }

    /// <summary>
    /// Devuelve información legible del nivel
    /// </summary>
    public override string ToString()
    {
        return $"Nivel {levelNumber}: {levelName}\n" +
               $"HP Jugador: {playerHealth} | HP Dealer: {dealerHealth}\n" +
               $"Umbral mínimo: {minimumScoreToDamage}\n" +
               $"Blackjack: {blackjackDamage} dmg";
    }
}
