using ChessApp.Logic;
using ChessApp.Models;

namespace ChessApp.Tests;

public static class ChessEngineTests
{
    public static void RunAllTests()
    {
        Console.WriteLine("Running Chess Engine Tests...");

        TestInitialSetup();
        TestPawnMoves();
        TestKnightMoves();
        TestIllegalMoveProtection();
        TestCheckmateDetection();
        TestCastling();
        TestEnPassant();

        Console.WriteLine("✅ ALL CHESS ENGINE TESTS PASSED SUCCESSFULLY!");
    }

    private static void TestInitialSetup()
    {
        var engine = new ChessEngine();
        engine.StartNewGame();

        Assert(engine.Board[0, 0]?.Type == PieceType.Rook && engine.Board[0, 0]?.Color == PieceColor.Black, "Black Rook at a8");
        Assert(engine.Board[7, 4]?.Type == PieceType.King && engine.Board[7, 4]?.Color == PieceColor.White, "White King at e1");
        Assert(engine.CurrentTurn == PieceColor.White, "White starts");
    }

    private static void TestPawnMoves()
    {
        var engine = new ChessEngine();
        engine.StartNewGame();

        Position e2 = Position.FromAlgebraic("e2");
        Position e4 = Position.FromAlgebraic("e4");

        var moves = engine.GetLegalMovesForPiece(e2);
        Assert(moves.Any(m => m.To == e4), "e2-e4 should be legal move for White pawn");

        var (success, _, _) = engine.MakeMove(e2, e4);
        Assert(success, "Make move e2-e4 succeeded");
        Assert(engine.CurrentTurn == PieceColor.Black, "Turn changed to Black");
    }

    private static void TestKnightMoves()
    {
        var engine = new ChessEngine();
        engine.StartNewGame();

        Position g1 = Position.FromAlgebraic("g1");
        Position f3 = Position.FromAlgebraic("f3");

        var moves = engine.GetLegalMovesForPiece(g1);
        Assert(moves.Any(m => m.To == f3), "g1-f3 (Nf3) should be legal");
    }

    private static void TestIllegalMoveProtection()
    {
        var engine = new ChessEngine();
        engine.StartNewGame();

        engine.MakeMove(Position.FromAlgebraic("e2"), Position.FromAlgebraic("e4"));

        var (success, warning, _) = engine.MakeMove(Position.FromAlgebraic("d2"), Position.FromAlgebraic("d4"));
        Assert(!success, "Black cannot move White piece out of turn");
        Assert(warning.Contains("turn"), "Warning should notify about wrong turn");
    }

    private static void TestCheckmateDetection()
    {
        var engine = new ChessEngine();
        engine.StartNewGame();

        engine.MakeMove(Position.FromAlgebraic("e2"), Position.FromAlgebraic("e4"));
        engine.MakeMove(Position.FromAlgebraic("e7"), Position.FromAlgebraic("e5"));
        engine.MakeMove(Position.FromAlgebraic("f1"), Position.FromAlgebraic("c4"));
        engine.MakeMove(Position.FromAlgebraic("b8"), Position.FromAlgebraic("c6"));
        engine.MakeMove(Position.FromAlgebraic("d1"), Position.FromAlgebraic("h5"));
        engine.MakeMove(Position.FromAlgebraic("g8"), Position.FromAlgebraic("f6"));
        
        var (success, warning, move) = engine.MakeMove(Position.FromAlgebraic("h5"), Position.FromAlgebraic("f7"));

        Assert(success, "Scholar's mate move Qxf7 succeeded");
        Assert(engine.IsGameOver, "Game should be over after Scholar's mate");
        Assert(move?.IsCheckmate == true, "Move should be checkmate");
        Assert(warning.Contains("CHECKMATE"), "Warning banner should alert CHECKMATE");
    }

    private static void TestCastling()
    {
        var engine = new ChessEngine();
        engine.StartNewGame();

        engine.MakeMove(Position.FromAlgebraic("e2"), Position.FromAlgebraic("e4"));
        engine.MakeMove(Position.FromAlgebraic("e7"), Position.FromAlgebraic("e5"));
        engine.MakeMove(Position.FromAlgebraic("g1"), Position.FromAlgebraic("f3"));
        engine.MakeMove(Position.FromAlgebraic("b8"), Position.FromAlgebraic("c6"));
        engine.MakeMove(Position.FromAlgebraic("f1"), Position.FromAlgebraic("c4"));
        engine.MakeMove(Position.FromAlgebraic("h7"), Position.FromAlgebraic("h6"));

        Position e1 = Position.FromAlgebraic("e1");
        Position g1 = Position.FromAlgebraic("g1");

        var moves = engine.GetLegalMovesForPiece(e1);
        Assert(moves.Any(m => m.To == g1 && m.MoveType == MoveType.CastlingKingside), "Kingside castling O-O should be legal");

        var (success, _, _) = engine.MakeMove(e1, g1);
        Assert(success, "Castling e1-g1 executed");
        Assert(engine.Board[Position.FromAlgebraic("f1")]?.Type == PieceType.Rook, "Rook automatically moved to f1 during castling");
    }

    private static void TestEnPassant()
    {
        var engine = new ChessEngine();
        engine.StartNewGame();

        engine.MakeMove(Position.FromAlgebraic("e2"), Position.FromAlgebraic("e4"));
        engine.MakeMove(Position.FromAlgebraic("a7"), Position.FromAlgebraic("a6"));
        engine.MakeMove(Position.FromAlgebraic("e4"), Position.FromAlgebraic("e5"));
        engine.MakeMove(Position.FromAlgebraic("d7"), Position.FromAlgebraic("d5"));

        Position e5 = Position.FromAlgebraic("e5");
        Position d6 = Position.FromAlgebraic("d6");

        var moves = engine.GetLegalMovesForPiece(e5);
        Assert(moves.Any(m => m.To == d6 && m.MoveType == MoveType.EnPassant), "En passant capture on d6 should be legal");

        var (success, _, _) = engine.MakeMove(e5, d6);
        Assert(success, "En passant capture executed");
        Assert(engine.Board[Position.FromAlgebraic("d5")] == null, "Black pawn on d5 captured via en passant");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Test Failed: {message}");
        }
    }
}
