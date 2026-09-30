namespace ChessApp.Models;

public enum PieceType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public enum PieceColor
{
    White,
    Black
}

public enum MoveType
{
    Normal,
    CastlingKingside,
    CastlingQueenside,
    EnPassant,
    PawnPromotion
}
