using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChessApp.Graphics;
using ChessApp.Logic;
using ChessApp.Models;

namespace ChessApp.UI;

public class ChessBoardControl : Control
{
    private ChessEngine? _engine;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ChessEngine? Engine
    {
        get => _engine;
        set
        {
            _engine = value;
            ClearSelection();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Position? SelectedPosition { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<Move> ValidMovesForSelected { get; private set; } = new();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Move? LastMove { get; private set; }

    // Events
    public event Action<string, bool>? OnWarningNotification; // (message, isError)
    public event Action<Move>? OnMoveExecuted;
    public event Func<PieceColor, PieceType?>? OnPromotionRequested;

    // Board Colors
    private readonly Color _lightSquareColor = Color.FromArgb(238, 238, 210); // Standard tournament light
    private readonly Color _darkSquareColor = Color.FromArgb(118, 150, 86);   // Standard tournament green
    private readonly Color _selectedSquareColor = Color.FromArgb(120, 186, 202, 27); // Soft yellow/green highlight
    private readonly Color _lastMoveSquareColor = Color.FromArgb(100, 246, 224, 105);
    private readonly Color _checkSquareColor = Color.FromArgb(160, 235, 60, 60);

    public ChessBoardControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Cursor = Cursors.Hand;
    }

    public void ClearSelection()
    {
        SelectedPosition = null;
        ValidMovesForSelected.Clear();
        Invalidate();
    }

    public void SetLastMove(Move? move)
    {
        LastMove = move;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left || _engine == null) return;

        int tileSize = Math.Min(Width, Height) / 8;
        int boardOffsetX = (Width - tileSize * 8) / 2;
        int boardOffsetY = (Height - tileSize * 8) / 2;

        int col = (e.X - boardOffsetX) / tileSize;
        int row = (e.Y - boardOffsetY) / tileSize;

        Position clickedPos = new Position(row, col);

        if (!clickedPos.IsValid) return;

        if (!_engine.GameStarted)
        {
            OnWarningNotification?.Invoke("Click 'Start Game' button to start playing!", true);
            return;
        }

        if (_engine.IsGameOver)
        {
            OnWarningNotification?.Invoke("Game is over! Click 'Start Game' to restart.", true);
            return;
        }

        // If piece already selected, check if clicked square is a valid legal move target
        if (SelectedPosition.HasValue)
        {
            var targetMove = ValidMovesForSelected.FirstOrDefault(m => m.To == clickedPos);
            if (targetMove != null)
            {
                // Attempt to execute move
                PieceType? promoChoice = null;
                if (targetMove.MoveType == MoveType.PawnPromotion)
                {
                    promoChoice = OnPromotionRequested?.Invoke(_engine.CurrentTurn) ?? PieceType.Queen;
                }

                var (success, warningMsg, executedMove) = _engine.MakeMove(SelectedPosition.Value, clickedPos, promoChoice);

                if (success && executedMove != null)
                {
                    LastMove = executedMove;
                    ClearSelection();
                    OnMoveExecuted?.Invoke(executedMove);

                    if (!string.IsNullOrEmpty(warningMsg))
                    {
                        OnWarningNotification?.Invoke(warningMsg, false);
                    }
                }
                else if (warningMsg == "PROMOTION_REQUIRED")
                {
                    // Handled above
                }
                else
                {
                    OnWarningNotification?.Invoke(warningMsg, true);
                }
                return;
            }
        }

        // Clicked piece selection
        var clickedPiece = _engine.Board[clickedPos];

        if (clickedPiece != null)
        {
            if (clickedPiece.Color == _engine.CurrentTurn)
            {
                SelectedPosition = clickedPos;
                ValidMovesForSelected = _engine.GetLegalMovesForPiece(clickedPos);
                Invalidate();
            }
            else
            {
                // Wrong turn attempt
                OnWarningNotification?.Invoke($"It's {_engine.CurrentTurn}'s turn! You cannot select {clickedPiece.Color}'s piece.", true);
                ClearSelection();
            }
        }
        else
        {
            // Clicked empty square that was not a move target
            if (SelectedPosition.HasValue)
            {
                OnWarningNotification?.Invoke("Invalid square for selected piece.", true);
            }
            ClearSelection();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        System.Drawing.Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int boardSize = Math.Min(Width, Height);
        int tileSize = boardSize / 8;
        int boardOffsetX = (Width - tileSize * 8) / 2;
        int boardOffsetY = (Height - tileSize * 8) / 2;

        // Draw Background
        g.Clear(Color.FromArgb(30, 32, 38));

        if (_engine == null) return;

        // Find King position in check (if any)
        Position? checkedKingPos = null;
        if (_engine.IsKingInCheck(_engine.CurrentTurn))
        {
            checkedKingPos = _engine.Board.FindKing(_engine.CurrentTurn);
        }

        using (Brush lightBrush = new SolidBrush(_lightSquareColor))
        using (Brush darkBrush = new SolidBrush(_darkSquareColor))
        using (Brush selectBrush = new SolidBrush(_selectedSquareColor))
        using (Brush lastMoveBrush = new SolidBrush(_lastMoveSquareColor))
        using (Brush checkBrush = new SolidBrush(_checkSquareColor))
        using (Font labelFont = new Font("Segoe UI", 9f, FontStyle.Bold))
        using (Brush labelLightBrush = new SolidBrush(_lightSquareColor))
        using (Brush labelDarkBrush = new SolidBrush(_darkSquareColor))
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    int x = boardOffsetX + c * tileSize;
                    int y = boardOffsetY + r * tileSize;
                    Rectangle rect = new Rectangle(x, y, tileSize, tileSize);

                    bool isLight = (r + c) % 2 == 0;
                    g.FillRectangle(isLight ? lightBrush : darkBrush, rect);

                    Position currentPos = new Position(r, c);

                    // Highlight Last Move
                    if (LastMove != null && (LastMove.From == currentPos || LastMove.To == currentPos))
                    {
                        g.FillRectangle(lastMoveBrush, rect);
                    }

                    // Highlight Selected Square
                    if (SelectedPosition.HasValue && SelectedPosition.Value == currentPos)
                    {
                        g.FillRectangle(selectBrush, rect);
                        using (Pen selectPen = new Pen(Color.FromArgb(200, 70, 130, 180), 3f))
                        {
                            g.DrawRectangle(selectPen, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                        }
                    }

                    // Highlight King in Check
                    if (checkedKingPos.HasValue && checkedKingPos.Value == currentPos)
                    {
                        g.FillRectangle(checkBrush, rect);
                        using (Pen checkPen = new Pen(Color.Red, 4f))
                        {
                            g.DrawRectangle(checkPen, rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
                        }
                    }

                    // Draw Rank/File Notation Labels
                    if (c == 0) // Rank label (8 to 1)
                    {
                        string rankStr = (8 - r).ToString();
                        g.DrawString(rankStr, labelFont, isLight ? labelDarkBrush : labelLightBrush, x + 3, y + 3);
                    }
                    if (r == 7) // File label (a to h)
                    {
                        string fileStr = ((char)('a' + c)).ToString();
                        SizeF labelSize = g.MeasureString(fileStr, labelFont);
                        g.DrawString(fileStr, labelFont, isLight ? labelDarkBrush : labelLightBrush, x + tileSize - labelSize.Width - 3, y + tileSize - labelSize.Height - 3);
                    }

                    // Draw Piece
                    var piece = _engine.Board[r, c];
                    if (piece != null)
                    {
                        Image pieceImg = PieceRenderer.GetPieceImage(piece.Type, piece.Color, tileSize);
                        g.DrawImage(pieceImg, rect);
                    }

                    // Draw Valid Move Indicators
                    if (SelectedPosition.HasValue)
                    {
                        var move = ValidMovesForSelected.FirstOrDefault(m => m.To == currentPos);
                        if (move != null)
                        {
                            if (piece == null && move.MoveType != MoveType.EnPassant)
                            {
                                // Green dot in center for empty destination
                                int dotRadius = tileSize / 6;
                                Rectangle dotRect = new Rectangle(x + tileSize / 2 - dotRadius, y + tileSize / 2 - dotRadius, dotRadius * 2, dotRadius * 2);
                                using (Brush dotBrush = new SolidBrush(Color.FromArgb(160, 40, 180, 70)))
                                {
                                    g.FillEllipse(dotBrush, dotRect);
                                }
                            }
                            else
                            {
                                // Red capture ring around targeted piece
                                using (Pen capturePen = new Pen(Color.FromArgb(200, 220, 50, 50), 4f))
                                {
                                    g.DrawEllipse(capturePen, x + 4, y + 4, tileSize - 8, tileSize - 8);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
