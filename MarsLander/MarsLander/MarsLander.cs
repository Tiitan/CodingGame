// This is the file running in coding game (with main uncommented)

using System.Numerics;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

// Grid coordinate vector
public struct Vector2i : IEquatable<Vector2i>
{
    Vector2 _v;

    public int X => (int)_v.X;
    public int Y => (int)_v.Y;

    public Vector2i(int x, int y)
    {
        _v = new Vector2(x, y);
    }

    // get the 4 neighbors arround this vector. no diagonal, out of range unchecked, buffer param for speed (no gc)
    public Vector2i[] GetNeighbors(Vector2i[] buffer)
    {
        buffer[0].Set(X - 1, Y);
        buffer[1].Set(X + 1, Y);
        buffer[2].Set(X, Y + 1);
        buffer[3].Set(X, Y - 1);
        return buffer;
    }

    // Vector2i is usually readonly, set only used for buffer update
    private void Set(int x, int y)
    {
        _v.X = x;
        _v.Y = y;
    }

    public static implicit operator Vector2(Vector2i d) => d._v;

    public bool Equals(Vector2i o)
    {
        return (int)_v.X == (int)o._v.X && (int)_v.Y == (int)o._v.Y;
    }

    public static bool operator ==(Vector2i? a, Vector2i? b)
    {
        if (Vector2i.ReferenceEquals(a, b))
            return true;
        return a?.Equals(b) ?? false;
    }

    public static bool operator !=(Vector2i? a, Vector2i? b)
    {
        return !(a == b);
    }
}

// Cell contains the A* pathfinding values
public struct Cell : IComparable<Cell> 
{
    public int Type; // 0=empty (used uninitialized), other= blocker
    public float G; // G score: steps to origin
    public float H; // H score: distance to destination
    public float F => G + H; // F score (G + H) to sort the list of open cells
    public Vector2i? From; // previous cell to reconstruct path

    public int CompareTo(Cell o)
    {
        return F < o.F ? -1 : (F > o.F ? 1 : 0);
    }
}

// Game contains the terrain definition
public class Game
{
    public int GridResolution { get; }
    public int surfaceN { get; }
    public Vector2[] vectorTerrain { get; }
    public Cell[,] gridMap { get; }
    public Vector2 Finish { get; }

    public Game(TextReader input, int gridResolution = 100)
    {
        GridResolution = gridResolution;
        surfaceN = int.Parse(input.ReadLine());
        vectorTerrain = new Vector2[surfaceN];
        for (int i = 0; i < surfaceN; i++)
        {
            string[] inputs = input.ReadLine().Split(' ');
            vectorTerrain[i] = new Vector2(int.Parse(inputs[0]), int.Parse(inputs[1]));
            if (i > 0 && vectorTerrain[i - 1].Y == vectorTerrain[i].Y)
            {
                var finish = Vector2.Lerp(vectorTerrain[i - 1], vectorTerrain[i], 0.5f);
                finish.Y += gridResolution * 2; // raise finish 2 cell, because path don't compute if finish is inside a wall 
                Finish = finish;
            }
        }

        gridMap = new Cell[7000 / gridResolution, 3000 / gridResolution];
        Vector2i[] neighborBuffer = new Vector2i[4];
        for (int i = 0; i < surfaceN - 1; i++)
        {
            foreach (var p in RasterizeVector(vectorTerrain[i], vectorTerrain[i + 1]))
            {
                gridMap[p.X, p.Y].Type = 2;

                // propagate blocking cell 1 block for navigation safety
                foreach (var n in p.GetNeighbors(neighborBuffer))
                {
                    if (IsInMapRange(n) && gridMap[n.X, n.Y].Type != 2)
                        gridMap[n.X, n.Y].Type = 1;
                }
            }
        }
    }

    // project a segment onto the grid and retrieves a list of grid coordinate
    private Vector2i[] RasterizeVector(Vector2 a, Vector2 b)
    {
        float dist = Vector2.Distance(a, b);
        int n = (int)(dist / GridResolution) + 1;
        Vector2i[] list = new Vector2i[n];
        for (int i = 0; i < n; i++)
            list[i] = FromWorldCoordinate(Vector2.Lerp(a, b, (float)i / n));
        return list;
    }

    // Build a path (using A*) from an origin (player spawn), return a list of waypoint in world coordinate
    public List<Vector2> BuildPath(Vector2 origin)
    {
        var path = new List<Vector2>();

        var gridOrigin = FromWorldCoordinate(origin);
        var gridFinish = FromWorldCoordinate(Finish);

        List<Vector2i> openList = new List<Vector2i>() { gridOrigin };
        List<Vector2i> closedList = new List<Vector2i>();
        Vector2i[] neighborBuffer = new Vector2i[4];

        while (openList.Count != 0)
        {
            // Sort by F score then pop lowest
            openList.Sort((a, b) => gridMap[a.X, a.Y].CompareTo(gridMap[b.X, b.Y]));
            var coord = openList.First();
            openList.RemoveAt(0);
            closedList.Add(coord);

            if (coord == gridFinish)
                break; // Path found

            // Initialize neighbours and push them to the open list
            float g = gridMap[coord.X, coord.Y].G + 1; /// neighbors g score
            foreach (var neighbor in coord.GetNeighbors(neighborBuffer))
            {
                if (!IsInMapRange(neighbor) || closedList.Contains(neighbor))
                    continue;
                Cell neighborCell = gridMap[neighbor.X, neighbor.Y];
                if (neighborCell.Type != 0) 
                    continue;

                bool inOpenList = openList.Contains(neighbor);
                if (!inOpenList || g < neighborCell.G)
                {
                    neighborCell.From = coord;
                    neighborCell.G = g;
                    neighborCell.H = Vector2.Distance(neighbor, gridFinish);
                    if (!inOpenList)
                        openList.Add(neighbor);
                    gridMap[neighbor.X, neighbor.Y] = neighborCell;
                }
            }

        }
        // construct path: create the list from finish to origin with the cells "from" value and revert
        Vector2i? pathCoord = gridFinish;
        while (pathCoord != gridOrigin)
        {
            if (pathCoord is null)
                throw new Exception("path not found");
            path.Add(ToWorldCoordinate(pathCoord.Value));
            pathCoord = gridMap[pathCoord.Value.X, pathCoord.Value.Y].From;
        }
        path.Reverse();

        // cleanup path: remove extra steps by checking line of sight
        for (int i = 0; i != path.Count - 1;)
        {
            int j = i + 2;
            while (j < path.Count && HasLineOfSight(path[i], path[j]))
                j++;
            int n = j - i - 2;
            if (n > 1)
            {
                path.RemoveRange(i + 1, n);
                j -= n;
            }
            i = j - 1;
        }

        return path;
    }

    // Check if there is a blocking cell between 2 world position. used for path cleanup.
    private bool HasLineOfSight(Vector2 a, Vector2 b)
    {
        foreach (var p in RasterizeVector(a, b))
        {
            if (gridMap[p.X, p.Y].Type != 0)
                return false;
        }
        return true;
    }

    // Check if a grid cell coordinate exist on the grid map.
    private bool IsInMapRange(Vector2i v)
    {
        return v.X > 0 && v.Y > 0 && v.X < gridMap.GetLength(0) && v.Y < gridMap.GetLength(1);
    }

    // Convert from world coordinate to Grid coordinate 
    public Vector2i FromWorldCoordinate(Vector2 v)
    {
        return new Vector2i((int)(v.X / GridResolution), (int)((v.Y) / GridResolution));
    }

    // Convert from grid coordinate to world coordinate (center of cell)
    public Vector2 ToWorldCoordinate(Vector2i v)
    {
        return new Vector2i(v.X * GridResolution + GridResolution / 2, v.Y * GridResolution + GridResolution / 2);
    }
}

// Lander definition, updated each tick
public class Lander
{
    public Vector2 position;
    public Vector2 velocity;
    public int fuel; // the quantity of remaining fuel in liters.
    public int rotate; // the rotation angle in degrees (-90 to 90).
    public int power; // the thrust power (0 to 4).

    public void Update(TextReader input)
    {
        var playerData = input.ReadLine().Split(' ');
        position = new Vector2(int.Parse(playerData[0]), int.Parse(playerData[1]));
        velocity = new Vector2(int.Parse(playerData[2]), int.Parse(playerData[3]));
        fuel = int.Parse(playerData[4]);
        rotate = int.Parse(playerData[5]);
        power = int.Parse(playerData[6]);
    }

    public Lander(TextReader input)
    {
        Update(input);
    }
}

// Control the lander during game loop, tries to stay as close to the path as possible.
class Controller
{
    Game Game { get; }
    Lander Lander { get; }
    List<Vector2> Path { get; }
    int currentSegment = 0;

    public Controller(Game game, Lander lander, List<Vector2> path)
    {
        Game = game;
        Lander = lander;
        Path = path;
    }

    // Final landing called when the lander reached the end of the path to control touchdown.
    string Landing()
    {
        var rotation = Lander.velocity.X < -8 ? -15 : (Lander.velocity.X > 8 ? 15 : 0);
        return $"{rotation} {((Lander.velocity.Y > -35 && rotation == 0) ? 2 : 4)}";
    }

    // Make the lander follow the path with ad-hoc rules
    private string GoToNextWaypoint(Vector2 positionOnPath, Vector2 nextWaypoint)
    {
        bool goDown = (Lander.position.Y > positionOnPath.Y || nextWaypoint.Y < Lander.position.Y) &&
            Lander.velocity.Y > -20;

        //boost altitude if way too low, ignoring horizontal inertia
        if (Lander.position.Y - positionOnPath.Y < -80 && Lander.velocity.Y < 0)
            return "0 4";

        // Maintain horizontal movement
        if (Lander.velocity.X > 30 && nextWaypoint.X > Lander.position.X ||
            Lander.velocity.X < -30 && nextWaypoint.X < Lander.position.X)

        {
            return $"0 {(goDown ? 2 : 4)}";
        }

        // Accelerate or slow down horizontal movement
        return $"{(nextWaypoint.X > Lander.position.X ? -20 : 20)} {(goDown ? 2 : 4)}";
    }

    // project a point onto the vector AB, returns progress ([0, 1] and position on path.
    public static (float, Vector2) GetPositionOnSegment(Vector2 a, Vector2 b, Vector2 position)
    {
        Vector2 u = position - a;
        Vector2 v = b - a;
        float progress = Vector2.Dot(u, v) / Vector2.DistanceSquared(a, b);
        if (progress < 0)
            return (progress, a);
        if (progress > 1)
            return (progress, b);
        Vector2 positionOnPath = v * progress + a;
        return (progress, positionOnPath);
    }

    // Update called on each step, return the new control string, rotation: [-90 90] thrust: [0, 4]. (ex: "-45 3").
    public string Update()
    {
        if (currentSegment == Path.Count - 1)
            return Landing();

        (float progress, Vector2 positionOnPath) = GetPositionOnSegment(Path[currentSegment], Path[currentSegment + 1], Lander.position);
        // We don't wait for 100% progress before targeting next waypoint because of huge inertia.
        if (Vector2.Distance(Path[currentSegment + 1], positionOnPath) < 500) 
            ++currentSegment;

        if (currentSegment == Path.Count - 1)
            return Landing();
        return GoToNextWaypoint(positionOnPath, Path[currentSegment + 1]);
    }
}

/*
// Coding game's Main
class MarsLanderGame
{
    static void Main(string[] args)
    {
        Game game = new Game(Console.In, 150);
        Lander lander = new Lander(Console.In);
        var path = game.BuildPath(lander.position);
        Controller controller = new Controller(game, lander, path);

        while (true)
        {
            Console.WriteLine(controller.Update());
            lander.Update(Console.In);
        }
    }
}
*/
