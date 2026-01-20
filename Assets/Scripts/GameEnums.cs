using UnityEngine;

/// <summary>
/// Palos de la baraja francesa
/// </summary>
public enum Suit
{
    Hearts,     // Corazones ♥
    Diamonds,   // Diamantes ♦
    Clubs,      // Tréboles ♣
    Spades      // Picas ♠
}

/// <summary>
/// Rangos de las cartas
/// </summary>
public enum Rank
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13
}

/// <summary>
/// Estados posibles del juego
/// </summary>
public enum GameState
{
    Betting,        // Esperando apuesta (futuro)
    PlayerTurn,     // Turno del jugador
    DealerTurn,     // Turno del dealer
    GameOver        // Juego terminado
}

/// <summary>
/// Resultados posibles de la partida
/// </summary>
public enum GameResult
{
    None,
    PlayerWins,
    DealerWins,
    Push,           // Empate
    PlayerBlackjack,
    DealerBlackjack
}
