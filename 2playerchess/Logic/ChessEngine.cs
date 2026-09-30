using ChessApp.Models;

namespace ChessApp.Logic;

public class ChessEngine
{
    public Board Board { get; private set; }
    public PieceColor CurrentTurn { get; private set; }
    public bool GameStarted { get; private set; }
    public bool IsGameOver { get; private set; }
    public string GameOverReason { get; private set; } = string.Empty;
    public List<Move> MoveHistory { get; } = new();
    
    // History stack of board states for undo feature
    private readonly Stack<(Board board, PieceColor turn)> _historyStack = new();

    public ChessEngine()
    {
        Board = new Board();
        CurrentTurn = PieceColor.White;
        GameStarted = false;
        IsGameOver = false;
    }

    public void StartNewGame()
    {
        Board = new Board();
        Board.SetupStandardBoard();
        CurrentTurn = PieceColor.White;
        GameStarted = true;
        IsGameOver = false;
        GameOverReason = string.Empty;
        MoveHistory.Clear();
        _historyStack.Clear();
    }

    public bool IsKingInCheck(PieceColor color)
    {
        return IsKingInCheck(Board, color);
    }

    public static bool IsKingInCheck(Board board, PieceColor color)
    {
        return IsSquareUnderAttack(board, board.FindKing(color) ?? new Position(-1, -1), OpponentColor(color));
    }

    public static PieceColor OpponentColor(PieceColor color) =>
        color == PieceColor.White ? PieceColor.Black : PieceColor.White;

    public List<Move> GetLegalMovesForPiece(Position pos)
    {
        if (!GameStarted || IsGameOver) return new List<Move>();

        var piece = Board[pos];
        if (piece == null || piece.Color != CurrentTurn) return new List<Move>();

        var pseudoMoves = GeneratePseudoLegalMoves(Board, pos);
        var legalMoves = new List<Move>();

        foreach (var move in pseudoMoves)
        {
            if (IsMoveLegal(Board, move, CurrentTurn))
            {
                legalMoves.Add(move);
            }
        }

        return legalMoves;
    }

    public List<Move> GetAllLegalMoves(PieceColor color)
    {
        var allMoves = new List<Move>();
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                var piece = Board[r, c];
                if (piece != null && piece.Color == color)
                {
                    var pos = new Position(r, c);
                    var pseudoMoves = GeneratePseudoLegalMoves(Board, pos);
                    foreach (var move in pseudoMoves)
                    {
                        if (IsMoveLegal(Board, move, color))
                        {
                            allMoves.Add(move);
                        }
                    }
                }
            }
        }
        return allMoves;
    }

    public (bool success, string warningMessage, Move? executedMove) MakeMove(Position from, Position to, PieceType? promotionChoice = null)
    {
        if (!GameStarted)
        {
            return (false, "Please click 'Start Game' to begin playing!", null);
        }

        if (IsGameOver)
        {
            return (false, "Game is over! Start a new game to play again.", null);
        }

        var piece = Board[from];
        if (piece == null)
        {
            return (false, "No piece selected.", null);
        }

        if (piece.Color != CurrentTurn)
        {
            return (false, $"It's {CurrentTurn}'s turn to play!", null);
        }

        var legalMoves = GetLegalMovesForPiece(from);
        var chosenMove = legalMoves.FirstOrDefault(m => m.To == to);

        if (chosenMove == null)
        {
            return (false, "Invalid move for selected piece.", null);
        }

        // Check if pawn promotion choice is required
        if (chosenMove.MoveType == MoveType.PawnPromotion && !promotionChoice.HasValue)
        {
            return (false, "PROMOTION_REQUIRED", chosenMove);
        }

        if (chosenMove.MoveType == MoveType.PawnPromotion && promotionChoice.HasValue)
        {
            chosenMove = new Move(chosenMove.From, chosenMove.To, chosenMove.PieceMoved, chosenMove.CapturedPiece, MoveType.PawnPromotion, promotionChoice.Value);
        }

        // Save current board for Undo
        _historyStack.Push((Board.Clone(), CurrentTurn));

        // Execute move on active board
        ExecuteMoveOnBoard(Board, chosenMove);

        // Update turn
        CurrentTurn = OpponentColor(CurrentTurn);

        // Check for Check / Checkmate / Stalemate for next player
        bool nextInCheck = IsKingInCheck(CurrentTurn);
        chosenMove.IsCheck = nextInCheck;

        var nextLegalMoves = GetAllLegalMoves(CurrentTurn);
        if (nextLegalMoves.Count == 0)
        {
            IsGameOver = true;
            if (nextInCheck)
            {
                chosenMove.IsCheckmate = true;
                PieceColor winner = OpponentColor(CurrentTurn);
                GameOverReason = $"CHECKMATE! {winner} wins!";
            }
            else
            {
                GameOverReason = "STALEMATE! The game is a draw.";
            }
        }

        MoveHistory.Add(chosenMove);

        string warning = string.Empty;
        if (IsGameOver)
        {
            warning = GameOverReason;
        }
        else if (nextInCheck)
        {
            warning = $"⚠️ CHECK! {CurrentTurn}'s King is under attack!";
        }

        return (true, warning, chosenMove);
    }

    public bool UndoMove()
    {
        if (_historyStack.Count == 0) return false;

        var (prevBoard, prevTurn) = _historyStack.Pop();
        Board = prevBoard;
        CurrentTurn = prevTurn;
        IsGameOver = false;
        GameOverReason = string.Empty;

        if (MoveHistory.Count > 0)
        {
            MoveHistory.RemoveAt(MoveHistory.Count - 1);
        }

        return true;
    }

    private static void ExecuteMoveOnBoard(Board b, Move m)
    {
        b.EnPassantTarget = null;

        var movingPiece = b[m.From];

        // Special handling
        if (m.MoveType == MoveType.EnPassant)
        {
            int captureRow = m.PieceMoved.Color == PieceColor.White ? m.To.Row + 1 : m.To.Row - 1;
            b[captureRow, m.To.Col] = null;
        }
        else if (m.MoveType == MoveType.CastlingKingside)
        {
            int row = m.From.Row;
            // Move rook from h (col 7) to f (col 5)
            var rook = b[row, 7];
            if (rook != null)
            {
                rook.HasMoved = true;
                b[row, 5] = rook;
                b[row, 7] = null;
            }
        }
        else if (m.MoveType == MoveType.CastlingQueenside)
        {
            int row = m.From.Row;
            // Move rook from a (col 0) to d (col 3)
            var rook = b[row, 0];
            if (rook != null)
            {
                rook.HasMoved = true;
                b[row, 3] = rook;
                b[row, 0] = null;
            }
        }
        else if (m.PieceMoved.Type == PieceType.Pawn && Math.Abs(m.To.Row - m.From.Row) == 2)
        {
            // Set En Passant target square
            int epRow = (m.From.Row + m.To.Row) / 2;
            b.EnPassantTarget = new Position(epRow, m.From.Col);
        }

        // Apply piece move on target board b
        if (movingPiece != null)
        {
            movingPiece.HasMoved = true;

            if (m.MoveType == MoveType.PawnPromotion && m.PromotedType.HasValue)
            {
                movingPiece = new Piece(m.PromotedType.Value, movingPiece.Color, true);
            }

            b[m.To] = movingPiece;
            b[m.From] = null;
        }
    }

    private static bool IsMoveLegal(Board b, Move move, PieceColor color)
    {
        var tempBoard = b.Clone();
        ExecuteMoveOnBoard(tempBoard, move);
        var kingPos = tempBoard.FindKing(color);
        if (!kingPos.HasValue) return false;
        return !IsSquareUnderAttack(tempBoard, kingPos.Value, OpponentColor(color));
    }

    public static bool IsSquareUnderAttack(Board b, Position targetPos, PieceColor attackerColor)
    {
        if (!targetPos.IsValid) return false;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                var piece = b[r, c];
                if (piece != null && piece.Color == attackerColor)
                {
                    var moves = GenerateRawAttacks(b, new Position(r, c));
                    if (moves.Any(pos => pos == targetPos))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static List<Move> GeneratePseudoLegalMoves(Board b, Position pos)
    {
        var moves = new List<Move>();
        var piece = b[pos];
        if (piece == null) return moves;

        int row = pos.Row;
        int col = pos.Col;
        PieceColor color = piece.Color;

        switch (piece.Type)
        {
            case PieceType.Pawn:
                int forwardDir = color == PieceColor.White ? -1 : 1;
                int startRow = color == PieceColor.White ? 6 : 1;
                int promotionRow = color == PieceColor.White ? 0 : 7;

                // 1 step forward
                Position f1 = new Position(row + forwardDir, col);
                if (f1.IsValid && b[f1] == null)
                {
                    if (f1.Row == promotionRow)
                    {
                        moves.Add(new Move(pos, f1, piece, null, MoveType.PawnPromotion, PieceType.Queen));
                    }
                    else
                    {
                        moves.Add(new Move(pos, f1, piece));
                    }

                    // 2 steps forward from start rank
                    if (row == startRow)
                    {
                        Position f2 = new Position(row + 2 * forwardDir, col);
                        if (f2.IsValid && b[f2] == null)
                        {
                            moves.Add(new Move(pos, f2, piece));
                        }
                    }
                }

                // Captures
                int[] capCols = { col - 1, col + 1 };
                foreach (int cc in capCols)
                {
                    if (cc >= 0 && cc < 8)
                    {
                        Position capPos = new Position(row + forwardDir, cc);
                        var targetPiece = b[capPos];

                        // Regular capture
                        if (targetPiece != null && targetPiece.Color != color)
                        {
                            if (capPos.Row == promotionRow)
                            {
                                moves.Add(new Move(pos, capPos, piece, targetPiece, MoveType.PawnPromotion, PieceType.Queen));
                            }
                            else
                            {
                                moves.Add(new Move(pos, capPos, piece, targetPiece));
                            }
                        }
                        // En Passant capture
                        else if (targetPiece == null && b.EnPassantTarget.HasValue && b.EnPassantTarget.Value == capPos)
                        {
                            int capturedPawnRow = row; // row of enemy pawn
                            var epCapturedPiece = b[capturedPawnRow, cc];
                            moves.Add(new Move(pos, capPos, piece, epCapturedPiece, MoveType.EnPassant));
                        }
                    }
                }
                break;

            case PieceType.Knight:
                int[,] knightOffsets = { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };
                for (int i = 0; i < 8; i++)
                {
                    Position target = new Position(row + knightOffsets[i, 0], col + knightOffsets[i, 1]);
                    if (target.IsValid)
                    {
                        var targetPiece = b[target];
                        if (targetPiece == null || targetPiece.Color != color)
                        {
                            moves.Add(new Move(pos, target, piece, targetPiece));
                        }
                    }
                }
                break;

            case PieceType.Bishop:
                AddRayMoves(b, pos, piece, moves, new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) });
                break;

            case PieceType.Rook:
                AddRayMoves(b, pos, piece, moves, new[] { (-1, 0), (1, 0), (0, -1), (0, 1) });
                break;

            case PieceType.Queen:
                AddRayMoves(b, pos, piece, moves, new[] { (-1, -1), (-1, 1), (1, -1), (1, 1), (-1, 0), (1, 0), (0, -1), (0, 1) });
                break;

            case PieceType.King:
                int[,] kingOffsets = { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };
                for (int i = 0; i < 8; i++)
                {
                    Position target = new Position(row + kingOffsets[i, 0], col + kingOffsets[i, 1]);
                    if (target.IsValid)
                    {
                        var targetPiece = b[target];
                        if (targetPiece == null || targetPiece.Color != color)
                        {
                            moves.Add(new Move(pos, target, piece, targetPiece));
                        }
                    }
                }

                // Castling
                if (!piece.HasMoved && !IsKingInCheck(b, color))
                {
                    PieceColor enemy = OpponentColor(color);

                    // Kingside (O-O)
                    var rookK = b[row, 7];
                    if (rookK != null && rookK.Type == PieceType.Rook && rookK.Color == color && !rookK.HasMoved)
                    {
                        if (b[row, 5] == null && b[row, 6] == null)
                        {
                            if (!IsSquareUnderAttack(b, new Position(row, 5), enemy) &&
                                !IsSquareUnderAttack(b, new Position(row, 6), enemy))
                            {
                                moves.Add(new Move(pos, new Position(row, 6), piece, null, MoveType.CastlingKingside));
                            }
                        }
                    }

                    // Queenside (O-O-O)
                    var rookQ = b[row, 0];
                    if (rookQ != null && rookQ.Type == PieceType.Rook && rookQ.Color == color && !rookQ.HasMoved)
                    {
                        if (b[row, 1] == null && b[row, 2] == null && b[row, 3] == null)
                        {
                            if (!IsSquareUnderAttack(b, new Position(row, 2), enemy) &&
                                !IsSquareUnderAttack(b, new Position(row, 3), enemy))
                            {
                                moves.Add(new Move(pos, new Position(row, 2), piece, null, MoveType.CastlingQueenside));
                            }
                        }
                    }
                }
                break;
        }

        return moves;
    }

    private static List<Position> GenerateRawAttacks(Board b, Position pos)
    {
        var attacks = new List<Position>();
        var piece = b[pos];
        if (piece == null) return attacks;

        int row = pos.Row;
        int col = pos.Col;

        switch (piece.Type)
        {
            case PieceType.Pawn:
                int forwardDir = piece.Color == PieceColor.White ? -1 : 1;
                Position a1 = new Position(row + forwardDir, col - 1);
                Position a2 = new Position(row + forwardDir, col + 1);
                if (a1.IsValid) attacks.Add(a1);
                if (a2.IsValid) attacks.Add(a2);
                break;

            case PieceType.Knight:
                int[,] knightOffsets = { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };
                for (int i = 0; i < 8; i++)
                {
                    Position target = new Position(row + knightOffsets[i, 0], col + knightOffsets[i, 1]);
                    if (target.IsValid) attacks.Add(target);
                }
                break;

            case PieceType.Bishop:
                AddRayAttacks(b, pos, attacks, new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) });
                break;

            case PieceType.Rook:
                AddRayAttacks(b, pos, attacks, new[] { (-1, 0), (1, 0), (0, -1), (0, 1) });
                break;

            case PieceType.Queen:
                AddRayAttacks(b, pos, attacks, new[] { (-1, -1), (-1, 1), (1, -1), (1, 1), (-1, 0), (1, 0), (0, -1), (0, 1) });
                break;

            case PieceType.King:
                int[,] kingOffsets = { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };
                for (int i = 0; i < 8; i++)
                {
                    Position target = new Position(row + kingOffsets[i, 0], col + kingOffsets[i, 1]);
                    if (target.IsValid) attacks.Add(target);
                }
                break;
        }

        return attacks;
    }

    private static void AddRayMoves(Board b, Position pos, Piece piece, List<Move> moves, (int dr, int dc)[] directions)
    {
        foreach (var (dr, dc) in directions)
        {
            int r = pos.Row + dr;
            int c = pos.Col + dc;

            while (r >= 0 && r < 8 && c >= 0 && c < 8)
            {
                Position target = new Position(r, c);
                var targetPiece = b[target];

                if (targetPiece == null)
                {
                    moves.Add(new Move(pos, target, piece));
                }
                else
                {
                    if (targetPiece.Color != piece.Color)
                    {
                        moves.Add(new Move(pos, target, piece, targetPiece));
                    }
                    break; // Blocked
                }

                r += dr;
                c += dc;
            }
        }
    }

    private static void AddRayAttacks(Board b, Position pos, List<Position> attacks, (int dr, int dc)[] directions)
    {
        foreach (var (dr, dc) in directions)
        {
            int r = pos.Row + dr;
            int c = pos.Col + dc;

            while (r >= 0 && r < 8 && c >= 0 && c < 8)
            {
                Position target = new Position(r, c);
                attacks.Add(target);

                if (b[target] != null)
                {
                    break; // Blocked by piece
                }

                r += dr;
                c += dc;
            }
        }
    }
}
