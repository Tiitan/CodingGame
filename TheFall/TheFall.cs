using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheFall
{
    public struct Vector2i
    {
        public int X;
        public int Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X + b.X, a.Y + b.Y);
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }

        public static bool operator==(Vector2i a, Vector2i b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return !(a == b);
        }

        public static Vector2i NullVector = new Vector2i(int.MinValue, int.MinValue);

        public override bool Equals(object? obj)
        {
            return obj is Vector2i i && this == i;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }

    public struct CellType
    {
        public string Name; // debug name
        public Vector2i[] NextCell; // 0: top, 1: left, 3: right
        public int[] Rotation; // 0 left, 1 right
    }

    public class Actor // rock and Indy
    {
        Vector2i Position;
        int Origin; // 0: top, 1: left, 3: right

        public Actor(TextReader input)
        {
            var line = input.ReadLine()!.Split(' ');
            Position = new Vector2i(int.Parse(line[0]), int.Parse(line[1]));
            Origin = GetDirection(line[2]);
        }

        static int GetDirection(string direction)
        {
            switch (direction)
            {
                case "TOP": return 0;
                case "LEFT": return 1;
                case "RIGHT": return 2;
            }
            return -1;
        }
    }

    public class World // representation of the board, duplicated on each node
    {
        public int[,] Map;
        public Actor Indy;
        public Actor[] Rocks;
        public int Exit;

        public int Width => Map.GetLength(0);
        public int Height => Map.GetLength(1);

        public World(int[,] map, Actor indy, Actor[] rocks, int exit)
        {
            Map = map;
            Indy = indy;
            Rocks = rocks;
            Exit = exit;
        }

        public World(TextReader input)
        {
            var line = input.ReadLine()!.Split(' ');
            Map = new int[int.Parse(line[0]), int.Parse(line[1])];
            for (int i = 0; i < Map.GetLength(1); i++)
            {
                line = input.ReadLine()!.Split(' ');
                for (int j = 0; j < Map.GetLength(0); j++)
                    Map[j, i] = int.Parse(line[j]);
            }
            Exit = int.Parse(input.ReadLine()!);

            Indy = new Actor(input);
            Rocks = new Actor[int.Parse(input.ReadLine()!)];
            for (int i = 0; i < Rocks.Length; i++)
                Rocks[i] = new Actor(input);
        }

        public World Clone()
        {
            return new World(Map, Indy, (Actor[])Rocks.Clone(), Exit);
        }
    }

    public class Game
    {
        static CellType[] cell_types = new CellType[14]
        {
            new CellType() {Name = "Wall"}, // wall
            new CellType() // Type 1: cross
            {
                Name = "Type 1: cross",
                NextCell = new Vector2i[3]{
                    new Vector2i(0, 1),
                    new Vector2i(0, 1),
                    new Vector2i(0, 1)
                }
            },
            new CellType() // Type 2: horizontal
            {
                Name = "Type 2: horizontal",
                NextCell = new Vector2i[3]{
                    Vector2i.NullVector,
                    new Vector2i(1, 0),
                    new Vector2i(-1, 0)
                },
                Rotation = new int[2] {3, 3}
            },
            new CellType() // Type 3: vertical
            {
                Name = "Type 3: vertical",
                NextCell = new Vector2i[3]{
                    new Vector2i(0, 1),
                    Vector2i.NullVector,
                    Vector2i.NullVector
                },
                Rotation = new int[2] {2, 2}
            },
            new CellType() // type 4: double slide left
            {
                Name = "type 4: double slide left",
                NextCell = new Vector2i[3]{
                    new Vector2i(-1, 0),
                    Vector2i.NullVector,
                    new Vector2i(0, 1)
                },
                Rotation = new int[2] {5, 5}
            },
            new CellType() // type 5: double slide right
            {
                Name = "type 5: double slide right",
                NextCell = new Vector2i[3]{
                    new Vector2i(1, 0),
                    new Vector2i(0, 1),
                    Vector2i.NullVector
                },
                Rotation = new int[2] {4, 4}
            },
            new CellType() // type 6: T top
            {
                Name = "type 6: T top",
                NextCell = new Vector2i[3]{
                    Vector2i.NullVector,
                    new Vector2i(1, 0),
                    new Vector2i(-1, 0)
                },
                Rotation = new int[2] {9, 7}
            },
            new CellType() // type 7: T right
            {
                Name = "type 7: T right",
                NextCell = new Vector2i[3]{
                    new Vector2i(0, 1),
                    Vector2i.NullVector,
                    new Vector2i(0, 1)
                },
                Rotation = new int[2] {6, 8}
            },
            new CellType() // type 8: T down
            {
                Name = "type 8: T down",
                NextCell = new Vector2i[3]{
                    Vector2i.NullVector,
                    new Vector2i(0, 1),
                    new Vector2i(0, 1)
                },
                Rotation = new int[2] {7, 9}
            },
            new CellType() // type 9: T left
            {
                Name = "type 9: T left",
                NextCell = new Vector2i[3]{
                    new Vector2i(0, 1),
                    new Vector2i(0, 1),
                    Vector2i.NullVector
                },
                Rotation = new int[2] {8, 6}
            },
            new CellType() // type 10: slide left
            {
                Name = "type 10: slide left",
                NextCell = new Vector2i[3]{
                    new Vector2i(-1, 0),
                    Vector2i.NullVector,
                    Vector2i.NullVector
                },
                Rotation = new int[2] {13, 11}
            },
            new CellType() // type 11: slide right
            {
                Name = "type 11: slide right",
                NextCell = new Vector2i[3]{
                    new Vector2i(1, 0),
                    Vector2i.NullVector,
                    Vector2i.NullVector
                },
                Rotation = new int[2] {10, 12}
            },
            new CellType() // type 12: slide left down
            {
                Name = "type 12: slide left down",
                NextCell = new Vector2i[3]{
                    Vector2i.NullVector,
                    Vector2i.NullVector,
                    new Vector2i(0, 1)
                },
                Rotation = new int[2] {11, 13}
            },
            new CellType() // type 13: slide right down
            {
                Name = "type 13: slide right down",
                NextCell = new Vector2i[3]{
                    Vector2i.NullVector,
                    new Vector2i(0, 1),
                    Vector2i.NullVector
                },
                Rotation = new int[2] {12, 10}
            },
        };

        private TextReader _input;
        private World _world;

        public World World => _world;

        public Game(TextReader input)
        {
            _input = input;
            _world = new World(_input);
        }

        public string Update()
        {
            return "WAIT";
        }
    }

    /*
    // Coding game's Main
    class MainClass
    {
        static void Main(string[] args)
        {
            TheFall game = new TheFall(Console.In);

            while (true)
            {
                Console.WriteLine(game.Update());
            }
        }
    }
    */
}
