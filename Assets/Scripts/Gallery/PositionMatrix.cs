using System;
using System.Collections.Generic;
using System.Linq;

public struct Object
{
    public int Width { get; set; }
    public int Height { get; set; }
    public char Label { get; set; }
}

public class Matrix
{
    private List<List<char?>> matrix;
    private int width;
    private List<int> positions;

    public Matrix(int width)
    {
        this.width = width;
        matrix = new List<List<char?>>();
        positions = new List<int>();
    }

    public void AddObject(Object obj)
    {
        for (int i = 0; i < matrix.Count; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (CanPlaceObject(i, j, obj))
                {
                    PlaceObject(i, j, obj);
                    positions.Add(i * width + j + 1);
                    return;
                }
            }
        }

        // If the object doesn't fit, expand the matrix
        for (int i = 0; i < obj.Height; i++)
        {
            matrix.Add(new List<char?>(new char?[width]));
        }
        PlaceObject(matrix.Count - obj.Height, 0, obj);
        positions.Add((matrix.Count - obj.Height) * width + 1);
    }

    private bool CanPlaceObject(int row, int col, Object obj)
    {
        if (col + obj.Width > width || row + obj.Height > matrix.Count)
        {
            return false;
        }

        for (int i = row; i < row + obj.Height; i++)
        {
            for (int j = col; j < col + obj.Width; j++)
            {
                if (matrix[i][j].HasValue)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void PlaceObject(int row, int col, Object obj)
    {
        for (int i = row; i < row + obj.Height; i++)
        {
            for (int j = col; j < col + obj.Width; j++)
            {
                matrix[i][j] = obj.Label;
            }
        }
    }

    public List<int> GetPositions()
    {
        return positions;
    }
}

public class PositionMatrix
{
    public static Dictionary<int, int> Main(int columnCount, Dictionary<int, (int, int)> input)
    {
        Dictionary<int, int> output = new Dictionary<int, int>();

        var matrix = new Matrix(columnCount);
        char label = 'a';
        foreach (var kvp in input)
        {
            matrix.AddObject(new Object { Width = kvp.Value.Item1, Height = kvp.Value.Item2, Label = label++ });
            output.Add(kvp.Key, matrix.GetPositions().Last());
        }

        return output;
    }
}
/* Example Matrix (4x?):
1 2 3 4
5 6 7 8
9 10 11 12

with grid type of   (2, 1) a
                    (1, 1) b
                    (2, 2) c
                    (1, 2) d
                    (1, 1) e

the position will be 1, 3, 5, 4, 7 or
aacd
bbce
bb e
*/
