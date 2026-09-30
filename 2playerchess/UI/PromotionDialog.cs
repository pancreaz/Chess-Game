using System.Drawing;
using System.Windows.Forms;
using ChessApp.Graphics;
using ChessApp.Models;

namespace ChessApp.UI;

public class PromotionDialog : Form
{
    public PieceType SelectedType { get; private set; } = PieceType.Queen;

    public PromotionDialog(PieceColor color)
    {
        Text = "Pawn Promotion";
        Width = 420;
        Height = 180;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(245, 245, 248);

        Label label = new Label
        {
            Text = "Choose piece for Pawn Promotion:",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(40, 40, 50),
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(label);

        Panel btnPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        Controls.Add(btnPanel);

        PieceType[] choices = { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight };

        int btnWidth = 85;
        int btnHeight = 70;
        int startX = 20;

        for (int i = 0; i < choices.Length; i++)
        {
            PieceType pType = choices[i];
            Button btn = new Button
            {
                Width = btnWidth,
                Height = btnHeight,
                Left = startX + i * (btnWidth + 10),
                Top = 10,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = Color.White
            };

            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 210);
            btn.FlatAppearance.BorderSize = 2;
            btn.Image = PieceRenderer.GetPieceImage(pType, color, 52);
            btn.ImageAlign = ContentAlignment.MiddleCenter;

            btn.Click += (s, e) =>
            {
                SelectedType = pType;
                DialogResult = DialogResult.OK;
                Close();
            };

            btnPanel.Controls.Add(btn);
        }
    }
}
