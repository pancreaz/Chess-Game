namespace ChessApp.Models;

public readonly struct Position : IEquatable<Position>
{
    public int Row { get; }
    public int Col { get; }

    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public bool IsValid => Row >= 0 && Row < 8 && Col >= 0 && Col < 8;

    public string ToAlgebraic()
    {
        if (!IsValid) return "??";
        char file = (char)('a' + Col);
        int rank = 8 - Row;
        return $"{file}{rank}";
    }

    public static Position FromAlgebraic(string s)
    {
        if (s.Length != 2) return new Position(-1, -1);
        int col = s[0] - 'a';
        int rank = s[1] - '1';
        int row = 7 - rank;
        return new Position(row, col);
    }

    public bool Equals(Position other) => Row == other.Row && Col == other.Col;
    public override bool Equals(object? obj) => obj is Position other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Row, Col);

    public static bool operator ==(Position left, Position right) => left.Equals(right);
    public static bool operator !=(Position left, Position right) => !left.Equals(right);

    public override string ToString() => ToAlgebraic();
}
