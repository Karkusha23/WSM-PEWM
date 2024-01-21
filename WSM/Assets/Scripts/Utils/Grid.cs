using UnityEngine;

// Base class for floor, room and minimap grids
public abstract class Grid<T>
{
    protected T[,] grid;

    public T this[int i, int j]
    {
        get => grid[i, j];
        set => grid[i, j] = value;
    }

    public T this[Point point]
    {
        get => grid[point.i, point.j];
        set => grid[point.i, point.j] = value;
    }

    public int rows { get => grid.GetLength(0); }

    public int cols { get => grid.GetLength(1); }

    // If point is inside of grid bounds
    public bool hasPoint(Point point)
    {
        return point.i >= 0 && point.i < this.rows && point.j >= 0 && point.j < this.cols;
    }

    public bool hasPoint(int i, int j)
    {
        return i >= 0 && i < this.rows && j >= 0 && j < this.cols;
    }
}

// Base class for floor, room and minimap point
public abstract class Point
{
    public int i { get; set; }
    public int j { get; set; }

    public Point(int row, int col)
    {
        i = row;
        j = col;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !GetType().Equals(obj.GetType()))
        {
            return false;
        }
        Point other = (Point)obj;
        return this.i == other.i && this.j == other.j;
    }

    public override int GetHashCode()
    {
        return (i << 8) | j;
    }

    public override string ToString()
    {
        return string.Format("Point({0}, {1})", i, j);
    }

    public static bool operator ==(Point point1, Point point2)
    {
        return point1.i == point2.i && point1.j == point2.j;
    }

    public static bool operator !=(Point point1, Point point2)
    {
        return !(point1 == point2);
    }

    public static float Distance(Point point1, Point point2)
    {
        return Mathf.Sqrt((point1.i - point2.i) * (point1.i - point2.i) + (point1.j - point2.j) * (point1.j - point2.j));
    }
}