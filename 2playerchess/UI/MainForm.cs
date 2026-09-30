using System.Drawing;
using System.Windows.Forms;
using ChessApp.Graphics;
using ChessApp.Logic;
using ChessApp.Models;

namespace ChessApp.UI;

public class MainForm : Form
{
    private readonly ChessEngine _engine;
    private readonly ChessBoardControl _boardControl;

    // UI Controls
    private readonly Button _btnStart;
    private readonly Button _btnUndo;
    private readonly Label _lblTurnBadge;
    private readonly Label _lblWarningBanner;
    private readonly ListBox _lstMoveHistory;
    private readonly FlowLayoutPanel _pnlWhiteCaptured;
    private readonly FlowLayoutPanel _pnlBlackCaptured;
    private readonly Label _lblScoreDiff;
    private System.Windows.Forms.Timer _warningTimer;

    public MainForm()
    {
        _engine = new ChessEngine();

        // Window Settings
        Text = "2D Chess - 2 Player Local Game";
        Size = new Size(980, 720);
        MinimumSize = new Size(880, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 26, 32);

        var icon = CreateAppIcon();
        if (icon != null) Icon = icon;

        // --- TOP HEADER PANEL ---
        Panel headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.FromArgb(32, 36, 45),
            Padding = new Padding(12)
        };
        Controls.Add(headerPanel);

        Label lblTitle = new Label
        {
            Text = "♔ 2D CHESS",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Left = 15,
            Top = 16
        };
        headerPanel.Controls.Add(lblTitle);

        _btnStart = new Button
        {
            Text = "▶ Start Game",
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(46, 139, 87),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(130, 38),
            Left = 180,
            Top = 13,
            Cursor = Cursors.Hand
        };
        _btnStart.FlatAppearance.BorderSize = 0;
        _btnStart.Click += BtnStart_Click;
        headerPanel.Controls.Add(_btnStart);

        _btnUndo = new Button
        {
            Text = "↶ Undo Move",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(70, 80, 95),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(110, 38),
            Left = 320,
            Top = 13,
            Cursor = Cursors.Hand,
            Enabled = false
        };
        _btnUndo.FlatAppearance.BorderSize = 0;
        _btnUndo.Click += BtnUndo_Click;
        headerPanel.Controls.Add(_btnUndo);

        _lblTurnBadge = new Label
        {
            Text = "CLICK START TO PLAY",
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(180, 185, 195),
            BackColor = Color.FromArgb(45, 52, 65),
            Size = new Size(200, 38),
            Left = 445,
            Top = 13,
            TextAlign = ContentAlignment.MiddleCenter
        };
        headerPanel.Controls.Add(_lblTurnBadge);

        // --- WARNING & ALERT STATUS BANNER ---
        _lblWarningBanner = new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(40, 44, 55),
            Text = "Welcome! Click 'Start Game' to begin a 2-Player local chess match.",
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(_lblWarningBanner);

        // Timer to reset temporary warnings
        _warningTimer = new System.Windows.Forms.Timer
        {
            Interval = 3500
        };
        _warningTimer.Tick += (s, e) =>
        {
            _warningTimer.Stop();
            UpdateWarningBannerDefault();
        };

        // --- RIGHT SIDE PANEL ---
        Panel rightPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 320,
            BackColor = Color.FromArgb(30, 34, 42),
            Padding = new Padding(12)
        };
        Controls.Add(rightPanel);

        // Move History Title
        Label lblHistoryTitle = new Label
        {
            Text = "📜 Move History",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(220, 225, 235),
            Dock = DockStyle.Top,
            Height = 28
        };
        rightPanel.Controls.Add(lblHistoryTitle);

        _lstMoveHistory = new ListBox
        {
            Dock = DockStyle.Top,
            Height = 260,
            BackColor = Color.FromArgb(20, 22, 28),
            ForeColor = Color.FromArgb(220, 225, 230),
            Font = new Font("Consolas", 10.5f, FontStyle.Regular),
            BorderStyle = BorderStyle.FixedSingle,
            SelectionMode = SelectionMode.None
        };
        rightPanel.Controls.Add(_lstMoveHistory);

        // Captured Pieces Section
        Label lblCapturedTitle = new Label
        {
            Text = "♟ Captured Pieces",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(220, 225, 235),
            Dock = DockStyle.Top,
            Height = 32
        };
        rightPanel.Controls.Add(lblCapturedTitle);

        Label lblWhiteCap = new Label
        {
            Text = "Captured by White:",
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            ForeColor = Color.FromArgb(170, 175, 185),
            Dock = DockStyle.Top,
            Height = 20
        };
        rightPanel.Controls.Add(lblWhiteCap);

        _pnlWhiteCaptured = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = Color.FromArgb(22, 25, 32),
            AutoScroll = true
        };
        rightPanel.Controls.Add(_pnlWhiteCaptured);

        Label lblBlackCap = new Label
        {
            Text = "Captured by Black:",
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            ForeColor = Color.FromArgb(170, 175, 185),
            Dock = DockStyle.Top,
            Height = 20
        };
        rightPanel.Controls.Add(lblBlackCap);

        _pnlBlackCaptured = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = Color.FromArgb(22, 25, 32),
            AutoScroll = true
        };
        rightPanel.Controls.Add(_pnlBlackCaptured);

        _lblScoreDiff = new Label
        {
            Dock = DockStyle.Top,
            Height = 30,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.Gold,
            Text = "Material: Equal",
            TextAlign = ContentAlignment.MiddleCenter
        };
        rightPanel.Controls.Add(_lblScoreDiff);

        // Instructions Footer
        Label lblHelp = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 85,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
            ForeColor = Color.FromArgb(140, 145, 160),
            Text = "💡 How to play:\n" +
                   "• Click piece on your turn to view legal moves.\n" +
                   "• Green dots = Empty target squares.\n" +
                   "• Red rings = Enemy piece captures.\n" +
                   "• Check & illegal moves give warnings.",
            TextAlign = ContentAlignment.MiddleLeft
        };
        rightPanel.Controls.Add(lblHelp);

        // --- MAIN BOARD CONTROL ---
        _boardControl = new ChessBoardControl
        {
            Dock = DockStyle.Fill,
            Engine = _engine
        };
        _boardControl.OnWarningNotification += BoardControl_OnWarningNotification;
        _boardControl.OnMoveExecuted += BoardControl_OnMoveExecuted;
        _boardControl.OnPromotionRequested += BoardControl_OnPromotionRequested;

        Controls.Add(_boardControl);
        _boardControl.BringToFront();
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        _engine.StartNewGame();
        _boardControl.SetLastMove(null);
        _btnStart.Text = "🔄 Restart Game";
        _btnUndo.Enabled = false;
        _lstMoveHistory.Items.Clear();
        _pnlWhiteCaptured.Controls.Clear();
        _pnlBlackCaptured.Controls.Clear();
        _lblScoreDiff.Text = "Material: Equal";

        UpdateTurnDisplay();
        SetWarningBanner("Game started! White moves first.", Color.FromArgb(46, 139, 87));
        _boardControl.Invalidate();
    }

    private void BtnUndo_Click(object? sender, EventArgs e)
    {
        if (_engine.UndoMove())
        {
            _boardControl.SetLastMove(_engine.MoveHistory.LastOrDefault());
            _btnUndo.Enabled = _engine.MoveHistory.Count > 0;
            RefreshMoveHistoryList();
            UpdateCapturedPiecesDisplay();
            UpdateTurnDisplay();
            UpdateWarningBannerDefault();
            _boardControl.Invalidate();
        }
    }

    private void BoardControl_OnMoveExecuted(Move move)
    {
        _btnUndo.Enabled = true;
        RefreshMoveHistoryList();
        UpdateCapturedPiecesDisplay();
        UpdateTurnDisplay();
        _boardControl.Invalidate();
    }

    private PieceType? BoardControl_OnPromotionRequested(PieceColor color)
    {
        using (PromotionDialog dlg = new PromotionDialog(color))
        {
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                return dlg.SelectedType;
            }
        }
        return PieceType.Queen;
    }

    private void BoardControl_OnWarningNotification(string msg, bool isError)
    {
        if (isError)
        {
            System.Media.SystemSounds.Exclamation.Play();
            SetWarningBanner($"⚠️ WARNING: {msg}", Color.FromArgb(180, 60, 50));
        }
        else
        {
            SetWarningBanner(msg, Color.FromArgb(200, 120, 20));
        }

        _warningTimer.Stop();
        _warningTimer.Start();
    }

    private void UpdateTurnDisplay()
    {
        if (!_engine.GameStarted)
        {
            _lblTurnBadge.Text = "CLICK START TO PLAY";
            _lblTurnBadge.BackColor = Color.FromArgb(45, 52, 65);
            _lblTurnBadge.ForeColor = Color.FromArgb(180, 185, 195);
            return;
        }

        if (_engine.IsGameOver)
        {
            _lblTurnBadge.Text = "GAME OVER";
            _lblTurnBadge.BackColor = Color.FromArgb(150, 40, 40);
            _lblTurnBadge.ForeColor = Color.White;
            return;
        }

        if (_engine.CurrentTurn == PieceColor.White)
        {
            _lblTurnBadge.Text = "♔ WHITE'S TURN";
            _lblTurnBadge.BackColor = Color.FromArgb(240, 240, 245);
            _lblTurnBadge.ForeColor = Color.FromArgb(20, 20, 25);
        }
        else
        {
            _lblTurnBadge.Text = "♚ BLACK'S TURN";
            _lblTurnBadge.BackColor = Color.FromArgb(35, 38, 45);
            _lblTurnBadge.ForeColor = Color.FromArgb(235, 235, 240);
        }
    }

    private void SetWarningBanner(string text, Color backColor)
    {
        _lblWarningBanner.Text = text;
        _lblWarningBanner.BackColor = backColor;
    }

    private void UpdateWarningBannerDefault()
    {
        if (!_engine.GameStarted)
        {
            SetWarningBanner("Click 'Start Game' button to begin.", Color.FromArgb(40, 44, 55));
        }
        else if (_engine.IsGameOver)
        {
            SetWarningBanner($"🏆 {_engine.GameOverReason}", Color.FromArgb(160, 40, 40));
        }
        else if (_engine.IsKingInCheck(_engine.CurrentTurn))
        {
            SetWarningBanner($"⚠️ CHECK! {_engine.CurrentTurn}'s King is under attack!", Color.FromArgb(200, 70, 30));
        }
        else
        {
            SetWarningBanner($"{_engine.CurrentTurn}'s turn - Click piece to view legal moves.", Color.FromArgb(40, 50, 65));
        }
    }

    private void RefreshMoveHistoryList()
    {
        _lstMoveHistory.BeginUpdate();
        _lstMoveHistory.Items.Clear();

        var history = _engine.MoveHistory;
        for (int i = 0; i < history.Count; i += 2)
        {
            int moveNum = (i / 2) + 1;
            string whiteMove = history[i].ToAlgebraicNotation();
            string blackMove = (i + 1 < history.Count) ? history[i + 1].ToAlgebraicNotation() : "";

            string line = $"{moveNum,2}.  {whiteMove,-9} {blackMove}";
            _lstMoveHistory.Items.Add(line);
        }

        if (_lstMoveHistory.Items.Count > 0)
        {
            _lstMoveHistory.TopIndex = _lstMoveHistory.Items.Count - 1;
        }

        _lstMoveHistory.EndUpdate();
    }

    private void UpdateCapturedPiecesDisplay()
    {
        _pnlWhiteCaptured.Controls.Clear();
        _pnlBlackCaptured.Controls.Clear();

        int whiteScore = 0;
        int blackScore = 0;

        foreach (var move in _engine.MoveHistory)
        {
            if (move.CapturedPiece != null)
            {
                var piece = move.CapturedPiece;
                int val = GetPieceValue(piece.Type);

                PictureBox pb = new PictureBox
                {
                    Width = 26,
                    Height = 26,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = PieceRenderer.GetPieceImage(piece.Type, piece.Color, 26),
                    Margin = new Padding(1)
                };

                if (move.PieceMoved.Color == PieceColor.White)
                {
                    _pnlWhiteCaptured.Controls.Add(pb);
                    whiteScore += val;
                }
                else
                {
                    _pnlBlackCaptured.Controls.Add(pb);
                    blackScore += val;
                }
            }
        }

        int diff = whiteScore - blackScore;
        if (diff > 0)
        {
            _lblScoreDiff.Text = $"Material: White +{diff}";
            _lblScoreDiff.ForeColor = Color.LightSkyBlue;
        }
        else if (diff < 0)
        {
            _lblScoreDiff.Text = $"Material: Black +{-diff}";
            _lblScoreDiff.ForeColor = Color.Orange;
        }
        else
        {
            _lblScoreDiff.Text = "Material: Equal";
            _lblScoreDiff.ForeColor = Color.Gold;
        }
    }

    private static int GetPieceValue(PieceType type) => type switch
    {
        PieceType.Pawn => 1,
        PieceType.Knight => 3,
        PieceType.Bishop => 3,
        PieceType.Rook => 5,
        PieceType.Queen => 9,
        _ => 0
    };

    private static Icon? CreateAppIcon()
    {
        try
        {
            using (Bitmap bmp = new Bitmap(32, 32))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(40, 45, 55));
                    g.DrawImage(PieceRenderer.GetPieceImage(PieceType.King, PieceColor.White, 28), 2, 2);
                }
                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }
        catch
        {
            return null;
        }
    }
}
