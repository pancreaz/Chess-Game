namespace ChessApp.Models;

public class Move
{
    public Position From { get; }
    public Position To { get; }
    public Piece PieceMoved { get; }
    public Piece? CapturedPiece { get; }
    public MoveType MoveType { get; }
    public PieceType? PromotedType { get; }
    public bool IsCheck { get; set; }
    public bool IsCheckmate { get; set; }

    public Move(Position from, Position to, Piece pieceMoved, Piece? capturedPiece = null, MoveType moveType = MoveType.Normal, PieceType? promotedType = null)
    {
        From = from;
        To = to;
        PieceMoved = pieceMoved;
        CapturedPiece = capturedPiece;
        MoveType = moveType;
        PromotedType = promotedType;
    }

    public string ToAlgebraicNotation()
    {
        if (MoveType == MoveType.CastlingKingside) return "O-O" + (IsCheckmate ? "#" : IsCheck ? "+" : "");
        if (MoveType == MoveType.CastlingQueenside) return "O-O-O" + (IsCheckmate ? "#" : IsCheck ? "+" : "");

        string piecePrefix = PieceMoved.Type switch
        {
            PieceType.Pawn => "",
            PieceType.Knight => "N",
            PieceType.Bishop => "B",
            PieceType.Rook => "R",
            PieceType.Queen => "Q",
            PieceType.King => "K",
            _ => ""
        };

        if (PieceMoved.Type == PieceType.Pawn && CapturedPiece != null)
        {
            piecePrefix = ((char)('a' + From.Col)).ToString();
        }

        string captureStr = CapturedPiece != null ? "x" : "";
        string destStr = To.ToAlgebraic();
        string promoStr = PromotedType.HasValue ? "=" + PromotedType.Value switch
        {
            PieceType.Queen => "Q",
            PieceType.Rook => "R",
            PieceType.Bishop => "B",
            PieceType.Knight => "N",
            _ => ""
        } : "";

        string suffix = IsCheckmate ? "#" : IsCheck ? "+" : "";

        return $"{piecePrefix}{captureStr}{destStr}{promoStr}{suffix}";
    }
}
