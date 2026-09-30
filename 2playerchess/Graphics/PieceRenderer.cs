using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using ChessApp.Models;

namespace ChessApp.Graphics;

public static class PieceRenderer
{
    private static readonly Dictionary<(PieceType type, PieceColor color, int size), Image> _cache = new();

    public static Image GetPieceImage(PieceType type, PieceColor color, int size)
    {
        if (size <= 0) size = 64;

        var key = (type, color, size);
        if (_cache.TryGetValue(key, out var cachedImage))
        {
            return cachedImage;
        }

        Bitmap bitmap = new Bitmap(size, size);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.Clear(Color.Transparent);

            DrawVectorPiece(g, type, color, size);
        }

        _cache[key] = bitmap;
        return bitmap;
    }

    private static void DrawVectorPiece(System.Drawing.Graphics g, PieceType type, PieceColor color, int size)
    {
        float scale = size / 100f;
        g.TranslateTransform(size / 2f, size / 2f);

        // Styling
        Color baseFill = color == PieceColor.White ? Color.FromArgb(245, 245, 240) : Color.FromArgb(35, 38, 42);
        Color accentColor = color == PieceColor.White ? Color.FromArgb(70, 70, 75) : Color.FromArgb(220, 220, 225);
        Color outlineColor = color == PieceColor.White ? Color.FromArgb(40, 40, 45) : Color.FromArgb(200, 205, 210);
        Color shadowColor = Color.FromArgb(60, 0, 0, 0);

        using (Brush fillBrush = new SolidBrush(baseFill))
        using (Brush shadowBrush = new SolidBrush(shadowColor))
        using (Pen outlinePen = new Pen(outlineColor, Math.Max(2f, 3.5f * scale)))
        using (Pen innerPen = new Pen(accentColor, Math.Max(1.5f, 2.5f * scale)))
        {
            outlinePen.LineJoin = LineJoin.Round;
            innerPen.LineJoin = LineJoin.Round;

            // Draw Drop Shadow
            using (GraphicsPath shadowPath = CreatePiecePath(type, scale))
            {
                g.TranslateTransform(0, 3f * scale);
                g.FillPath(shadowBrush, shadowPath);
                g.TranslateTransform(0, -3f * scale);
            }

            // Draw Main Piece Body
            using (GraphicsPath piecePath = CreatePiecePath(type, scale))
            {
                g.FillPath(fillBrush, piecePath);
                g.DrawPath(outlinePen, piecePath);
            }

            // Draw Piece-Specific Details / Inner Features
            DrawPieceDetails(g, type, color, scale, innerPen, fillBrush);
        }

        g.ResetTransform();
    }

    private static GraphicsPath CreatePiecePath(PieceType type, float scale)
    {
        GraphicsPath path = new GraphicsPath();

        switch (type)
        {
            case PieceType.Pawn:
                // Base
                path.AddRectangle(new RectangleF(-28 * scale, 28 * scale, 56 * scale, 12 * scale));
                path.AddArc(-24 * scale, 22 * scale, 48 * scale, 14 * scale, 180, 180);
                // Body
                path.AddBezier(-16 * scale, 22 * scale, -10 * scale, 0, -6 * scale, -10 * scale, -10 * scale, -18 * scale);
                // Head
                path.AddEllipse(-18 * scale, -38 * scale, 36 * scale, 36 * scale);
                break;

            case PieceType.Knight:
                // Base
                path.AddRectangle(new RectangleF(-32 * scale, 28 * scale, 64 * scale, 12 * scale));
                path.AddArc(-28 * scale, 20 * scale, 56 * scale, 16 * scale, 180, 180);
                // Horse Head profile
                PointF[] knightPoints = new PointF[]
                {
                    new PointF(-20 * scale, 22 * scale),
                    new PointF(-22 * scale, 5 * scale),
                    new PointF(-32 * scale, -8 * scale),
                    new PointF(-26 * scale, -28 * scale),
                    new PointF(-10 * scale, -38 * scale),
                    new PointF(10 * scale, -38 * scale),
                    new PointF(26 * scale, -20 * scale),
                    new PointF(22 * scale, 0 * scale),
                    new PointF(14 * scale, 10 * scale),
                    new PointF(20 * scale, 22 * scale)
                };
                path.AddCurve(knightPoints, 0.4f);
                break;

            case PieceType.Bishop:
                // Base
                path.AddRectangle(new RectangleF(-30 * scale, 28 * scale, 60 * scale, 12 * scale));
                path.AddArc(-26 * scale, 20 * scale, 52 * scale, 16 * scale, 180, 180);
                // Body & Mitre
                path.AddBezier(-18 * scale, 20 * scale, -22 * scale, -10 * scale, -14 * scale, -25 * scale, 0 * scale, -38 * scale);
                path.AddBezier(0 * scale, -38 * scale, 14 * scale, -25 * scale, 22 * scale, -10 * scale, 18 * scale, 20 * scale);
                // Top Small Cross / Ball
                path.AddEllipse(-5 * scale, -45 * scale, 10 * scale, 10 * scale);
                break;

            case PieceType.Rook:
                // Base
                path.AddRectangle(new RectangleF(-32 * scale, 28 * scale, 64 * scale, 12 * scale));
                path.AddArc(-28 * scale, 20 * scale, 56 * scale, 16 * scale, 180, 180);
                // Tower Body
                PointF[] castlePoints = new PointF[]
                {
                    new PointF(-20 * scale, 20 * scale),
                    new PointF(-18 * scale, -12 * scale),
                    new PointF(-28 * scale, -18 * scale),
                    new PointF(-28 * scale, -38 * scale), // Left battlement top
                    new PointF(-18 * scale, -38 * scale),
                    new PointF(-18 * scale, -28 * scale),
                    new PointF(-8 * scale, -28 * scale),
                    new PointF(-8 * scale, -38 * scale), // Mid battlement left
                    new PointF(8 * scale, -38 * scale),
                    new PointF(8 * scale, -28 * scale),
                    new PointF(18 * scale, -28 * scale),
                    new PointF(18 * scale, -38 * scale), // Right battlement
                    new PointF(28 * scale, -38 * scale),
                    new PointF(28 * scale, -18 * scale),
                    new PointF(18 * scale, -12 * scale),
                    new PointF(20 * scale, 20 * scale)
                };
                path.AddLines(castlePoints);
                break;

            case PieceType.Queen:
                // Base
                path.AddRectangle(new RectangleF(-34 * scale, 28 * scale, 68 * scale, 12 * scale));
                path.AddArc(-30 * scale, 20 * scale, 60 * scale, 16 * scale, 180, 180);
                // Crown spikes
                PointF[] queenPoints = new PointF[]
                {
                    new PointF(-22 * scale, 20 * scale),
                    new PointF(-26 * scale, -15 * scale),
                    new PointF(-32 * scale, -34 * scale), // Outer left spike
                    new PointF(-18 * scale, -12 * scale),
                    new PointF(-16 * scale, -38 * scale), // Inner left spike
                    new PointF(-8 * scale, -10 * scale),
                    new PointF(0 * scale, -42 * scale),   // Center spike
                    new PointF(8 * scale, -10 * scale),
                    new PointF(16 * scale, -38 * scale),  // Inner right spike
                    new PointF(18 * scale, -12 * scale),
                    new PointF(32 * scale, -34 * scale),  // Outer right spike
                    new PointF(26 * scale, -15 * scale),
                    new PointF(22 * scale, 20 * scale)
                };
                path.AddLines(queenPoints);
                break;

            case PieceType.King:
                // Base
                path.AddRectangle(new RectangleF(-34 * scale, 28 * scale, 68 * scale, 12 * scale));
                path.AddArc(-30 * scale, 20 * scale, 60 * scale, 16 * scale, 180, 180);
                // Body & Crown
                PointF[] kingPoints = new PointF[]
                {
                    new PointF(-22 * scale, 20 * scale),
                    new PointF(-24 * scale, -10 * scale),
                    new PointF(-30 * scale, -28 * scale),
                    new PointF(-15 * scale, -24 * scale),
                    new PointF(0 * scale, -32 * scale), // Crown center top
                    new PointF(15 * scale, -24 * scale),
                    new PointF(30 * scale, -28 * scale),
                    new PointF(24 * scale, -10 * scale),
                    new PointF(22 * scale, 20 * scale)
                };
                path.AddLines(kingPoints);
                // Top Cross
                path.AddRectangle(new RectangleF(-4 * scale, -48 * scale, 8 * scale, 16 * scale));
                path.AddRectangle(new RectangleF(-10 * scale, -44 * scale, 20 * scale, 6 * scale));
                break;
        }

        return path;
    }

    private static void DrawPieceDetails(System.Drawing.Graphics g, PieceType type, PieceColor color, float scale, Pen innerPen, Brush fillBrush)
    {
        switch (type)
        {
            case PieceType.Knight:
                // Eye & Mane detail
                g.FillEllipse(fillBrush, -16 * scale, -24 * scale, 6 * scale, 6 * scale);
                g.DrawEllipse(innerPen, -16 * scale, -24 * scale, 6 * scale, 6 * scale);
                g.DrawLine(innerPen, -6 * scale, -26 * scale, -16 * scale, -12 * scale);
                break;

            case PieceType.Bishop:
                // Slit in Mitre
                g.DrawLine(innerPen, -4 * scale, -16 * scale, 8 * scale, -28 * scale);
                break;

            case PieceType.Queen:
                // Crown Jewels on Spikes
                float[] qSpikeXs = { -32, -16, 0, 16, 32 };
                float[] qSpikeYs = { -34, -38, -42, -38, -34 };
                for (int i = 0; i < 5; i++)
                {
                    g.FillEllipse(fillBrush, (qSpikeXs[i] - 3) * scale, (qSpikeYs[i] - 3) * scale, 6 * scale, 6 * scale);
                    g.DrawEllipse(innerPen, (qSpikeXs[i] - 3) * scale, (qSpikeYs[i] - 3) * scale, 6 * scale, 6 * scale);
                }
                break;

            case PieceType.King:
                // Cross detail
                g.DrawLine(innerPen, 0, -48 * scale, 0, -32 * scale);
                g.DrawLine(innerPen, -7 * scale, -41 * scale, 7 * scale, -41 * scale);
                break;
        }
    }
}
