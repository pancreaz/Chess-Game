namespace ChessApp.Models;

public class Board
{
    private readonly Piece?[,] _grid = new Piece?[8, 8];

    public Position? EnPassantTarget { get; set; }

    public Piece? this[int row, int col]
    {
        get => IsInBounds(row, col) ? _grid[row, col] : null;
        set { if (IsInBounds(row, col)) _grid[row, col] = value; }
    }

    public Piece? this[Position pos]
    {
        get => this[pos.Row, pos.Col];
        set => this[pos.Row, pos.Col] = value;
    }

    public static bool IsInBounds(int row, int col) => row >= 0 && row < 8 && col >= 0 && col < 8;

    public void SetupStandardBoard()
    {
        // Clear grid
        for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
                _grid[r, c] = null;

        EnPassantTarget = null;

        // Black pieces (Top: row 0 & 1)
        _grid[0, 0] = new Piece(PieceType.Rook, PieceColor.Black);
        _grid[0, 1] = new Piece(PieceType.Knight, PieceColor.Black);
        _grid[0, 2] = new Piece(PieceType.Bishop, PieceColor.Black);
        _grid[0, 3] = new Piece(PieceType.Queen, PieceColor.Black);
        _grid[0, 4] = new Piece(PieceType.King, PieceColor.Black);
        _grid[0, 5] = new Piece(PieceType.Bishop, PieceColor.Black);
        _grid[0, 6] = new Piece(PieceType.Knight, PieceColor.Black);
        _grid[0, 7] = new Piece(PieceType.Rook, PieceColor.Black);

        for (int c = 0; c < 8; c++)
        {
            _grid[1, c] = new Piece(PieceType.Pawn, PieceColor.Black);
        }

        // White pieces (Bottom: row 6 & 7)
        for (int c = 0; c < 8; c++)
        {
            _grid[6, c] = new Piece(PieceType.Pawn, PieceColor.White);
        }

        _grid[7, 0] = new Piece(PieceType.Rook, PieceColor.White);
        _grid[7, 1] = new Piece(PieceType.Knight, PieceColor.White);
        _grid[7, 2] = new Piece(PieceType.Bishop, PieceColor.White);
        _grid[7, 3] = new Piece(PieceType.Queen, PieceColor.White);
        _grid[7, 4] = new Piece(PieceType.King, PieceColor.White);
        _grid[7, 5] = new Piece(PieceType.Bishop, PieceColor.White);
        _grid[7, 6] = new Piece(PieceType.Knight, PieceColor.White);
        _grid[7, 7] = new Piece(PieceType.Rook, PieceColor.White);
    }

    public Position? FindKing(PieceColor color)
    {
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                var piece = _grid[r, c];
                if (piece != null && piece.Type == PieceType.King && piece.Color == color)
                {
                    return new Position(r, c);
                }
            }
        }
        return null;
    }

    public Board Clone()
    {
        var newBoard = new Board();
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (_grid[r, c] != null)
                {
                    newBoard._grid[r, c] = _grid[r, c]!.Clone();
                }
            }
        }
        newBoard.EnPassantTarget = EnPassantTarget;
        return newBoard;
    }
}
