using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public struct Vector2i
{
    public int X;
    public int Y;

    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Vector2i operator+(Vector2i a, Vector2i b)
    {
        return new Vector2i(a.X + b.X, a.Y + b.Y);
    }

    public override string ToString()
   {
      return $"{X} {Y}";
   }
}

public class CellType
{
    public string Name; // debug name
    public Vector2i?[] NextCell; // 0: top, 1: left, 3: right
}

class Player
{
    static CellType[] cell_types = new CellType[14]
    {
        new CellType() {Name = "Wall"}, // wall
        new CellType() // Type 1: cross
        {
            Name = "Type 1: cross",
            NextCell = new Vector2i?[3]{
                new Vector2i(0, 1),
                new Vector2i(0, 1),
                new Vector2i(0, 1)
            }
        },
        new CellType() // Type 2: horizontal
        {
            Name = "Type 2: horizontal",
            NextCell = new Vector2i?[3]{
                null,
                new Vector2i(1, 0),
                new Vector2i(-1, 0)
            }
        },
        new CellType() // Type 3: vertical
        {
            Name = "Type 3: vertical",
            NextCell = new Vector2i?[3]{
                new Vector2i(0, 1),
                null,
                null
            }
        },
        new CellType() // type 4: double slide left
        {
            Name = "type 4: double slide left",
            NextCell = new Vector2i?[3]{
                new Vector2i(-1, 0),
                null,
                new Vector2i(0, 1)
            }
        },
        new CellType() // type 5: double slide right
        {
            Name = "type 5: double slide right",
            NextCell = new Vector2i?[3]{
                new Vector2i(1, 0),
                new Vector2i(0, 1),
                null
            }
        },
        new CellType() // type 6: T top
        {
            Name = "type 6: T top",
            NextCell = new Vector2i?[3]{
                null,
                new Vector2i(1, 0),
                new Vector2i(-1, 0)
            }
        },
        new CellType() // type 7: T right
        {
            Name = "type 7: T right",
            NextCell = new Vector2i?[3]{
                new Vector2i(0, 1),
                null,
                new Vector2i(0, 1)
            }
        },
        new CellType() // type 8: T down
        {
            Name = "type 8: T down",
            NextCell = new Vector2i?[3]{
                null,
                new Vector2i(0, 1),
                new Vector2i(0, 1)
            }
        },
        new CellType() // type 9: T left
        {
            Name = "type 9: T left",
            NextCell = new Vector2i?[3]{
                new Vector2i(0, 1),
                new Vector2i(0, 1),
                null
            }
        },
        new CellType() // type 10: slide left
        {
            Name = "type 10: slide left",
            NextCell = new Vector2i?[3]{
                new Vector2i(-1, 0),
                null,
                null
            }
        },
        new CellType() // type 11: slide right
        {
            Name = "type 11: slide right",
            NextCell = new Vector2i?[3]{
                new Vector2i(1, 0),
                null,
                null
            }
        },
        new CellType() // type 12: slide left down
        {
            Name = "type 12: slide left down",
            NextCell = new Vector2i?[3]{
                null,
                null,
                new Vector2i(0, 1)
            }
        },
        new CellType() // type 13: slide right down
        {
            Name = "type 13: slide right down",
            NextCell = new Vector2i?[3]{
                null,
                new Vector2i(0, 1),
                null
            }
        },
    };

    static int GetDirection(string direction)
    {
        switch(direction)
        {
            case "TOP": return 0;
            case "LEFT": return 1;
            case "RIGHT": return 2;
        }
        return -1;
    }

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int[,] map =  new int[int.Parse(inputs[0]), int.Parse(inputs[1])];
        for (int i = 0; i < map.GetLength(1); i++)
        {
            var line = Console.ReadLine().Split(' ');
            for (int j = 0; j < map.GetLength(0); j++)
                map[j, i] = int.Parse(line[j]);
        }
        int exit = int.Parse(Console.ReadLine());

        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            var position = new Vector2i(int.Parse(inputs[0]), int.Parse(inputs[1]));
            int direction = GetDirection(inputs[2]);
            Console.Error.WriteLine($"current cell={cell_types[map[position.X, position.Y]].Name}, direction={direction}");
            var nextCell = position + cell_types[map[position.X, position.Y]].NextCell[direction];
            Console.WriteLine(nextCell.ToString());
        }
    }
}