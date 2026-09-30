namespace ChessApp.Models;

public class Piece
{
    public PieceType Type { get; set; }
    public PieceColor Color { get; set; }
    public bool HasMoved { get; set; }

    public Piece(PieceType type, PieceColor color, bool hasMoved = false)
    {
        Type = type;
        Color = color;
        HasMoved = hasMoved;
    }

    public Piece Clone()
    {
        return new Piece(Type, Color, HasMoved);
    }

    public char GetSymbolChar()
    {
        return Type switch
        {
            PieceType.Pawn => Color == PieceColor.White ? '♙' : '♟',
            PieceType.Knight => Color == PieceColor.White ? '♘' : '♞',
            PieceType.Bishop => Color == PieceColor.White ? '♗' : '♝',
            PieceType.Rook => Color == PieceColor.White ? '♖' : '♜',
            PieceType.Queen => Color == PieceColor.White ? '♕' : '♛',
            PieceType.King => Color == PieceColor.White ? '♔' : '♚',
            _ => '?'
        };
    }
}
