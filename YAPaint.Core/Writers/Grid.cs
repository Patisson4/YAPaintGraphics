using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Avalonia.Controls;

namespace YAPaint.Core.Writers;

public class Grid
{
    static readonly Random Random = new Random();
    readonly Color[,] _data;

    public Grid(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;

        _data = new Color[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                SetRandomColor(i, j);
            }
        }
    }

    public void SetRandomColor(int row, int column)
    {
        _data[row, column] = Color.FromArgb(
            Random.Next(256),
            Random.Next(256),
            Random.Next(256),
            Random.Next(256));
    }

    public int Rows { get; }
    public int Columns { get; }

    public (int row, int column) GetCoordinates(Control target, Point point)
    {
        double wt = target.Width, ht = target.Height;
        double dx = wt / Columns, dy = ht / Rows;

        return ((int)(point.X / dx), (int)(point.Y / dy));
    }

    public Color this[int row, int column]
    {
        get => _data[row, column];
        set => _data[row, column] = value;
    }

    public void Render(Graphics g, Control target)
    {
        double wt = target.Width, ht = target.Height;
        double dx = wt / Columns, dy = ht / Rows;
        using var fill = new SolidBrush(Color.Black);
        for (var i = 0; i < Rows; i++)
        {
            for (var j = 0; j < Columns; j++)
            {
                double x = i * dx, y = j * dy;

                fill.Color = _data[i, j];

                g.FillRectangle(new HatchBrush(HatchStyle.Percent25, Color.Blue, Color.White), (float)x, (float)y,
                    (float)dx, (float)dy);
            }
        }
    }
}