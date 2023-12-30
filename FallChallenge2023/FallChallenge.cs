using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

enum DroneState
{
    GoingDown,
    Searching,
    Returning,
    Wait
}

class Drone
{
    public int Id {get;}
    public Vector2 Position {get; private set;}
    public int Emergency {get; private set;}
    public int Battery {get; private set;}
    public List<int> Scans {get; set;}
    public List<RadarBlip> RadarBlips {get; set;}

    DroneState state = DroneState.GoingDown;
    int targetLevel = 2;

    public Drone(TextReader textReader)
    {
        var inputs = textReader.ReadLine().Split(' ').Select(int.Parse).ToList();
        Id = inputs[0];
        Update(inputs);
    }

    public void Update(List<int> inputs)
    {
        Position = new Vector2(inputs[1], inputs[2]);
        Emergency = inputs[3];
        Battery = inputs[4];
    }

    public override string ToString()
    {
        return $"Drone(Id={Id}, Position={Position}, Emergency={Emergency}, Battery={Battery})";
    }

    public void StateTransition(Game game)
    {
        switch (state)
        {
            case DroneState.GoingDown:
                if (Position.Y >= game.LevelDepth[targetLevel])
                {
                    state = DroneState.Searching;
                    Console.Error.WriteLine($"Drone{Id}: start searching at level {targetLevel}");
                }
                goto case DroneState.Searching;
            case DroneState.Searching:
                var targetType = targetLevel - 1;
                var fishCount = RadarBlips.Count(x => x.Type.Type == targetType);
                var scanCount = game.My.AllScans.Count(s => game.creatureTypes[s].Type == targetType);
                Console.Error.WriteLine($"Drone{Id}: targetLevel={targetLevel} fishCount={fishCount} scanCount={scanCount}");
                if (scanCount >= fishCount)
                {
                    state = DroneState.Returning;
                    Console.Error.WriteLine($"Drone{Id}: start returning");
                }
                break;
            case DroneState.Returning:
                if (Position.Y <= game.LevelDepth[0])
                {
                    targetLevel++;
                    state = DroneState.GoingDown;
                    if (targetLevel == 4)
                        targetLevel = 1;
                    Console.Error.WriteLine($"Drone{Id}: going down, new targetLevel: {targetLevel}");
                }
                break;
            case DroneState.Wait:
                break;
        }
    }

    public void ComputeAction(Game game)
    {
        var message = "";
        StateTransition(game);
        int x = 0, y = 0, light = 0;
        switch (state)
        {
            case DroneState.GoingDown:
                x = (int)Position.X;
                y = game.LevelDepth[targetLevel];
                light = Battery == game.MaxBattery ? 1 : 0;
                message = "GoingDown";
                break;
            case DroneState.Searching:
                var target = RadarBlips.Where(x => x.Type.Type == targetLevel - 1)
                                .First(x => !game.My.AllScans.Contains(x.CreatureId));
                x = target.Radar.Contains("L") ? 0 : 10000;
                y = game.LevelDepth[targetLevel];
                light = (game.turn % 3) == 0 ? 1 : 0;
                message = "Searching";
                break;
            case DroneState.Returning:
                x = (int)Position.X;
                y = game.LevelDepth[0];
                light = Battery == game.MaxBattery ? 1 : 0;
                message = "Returning";
                break;
            case DroneState.Wait:
                Console.WriteLine($"WAIT 0");
                return;
        }

        var monsters = game.VisibleCreatures.Where(x => x.Type.Type == -1 && Vector2.Distance(x.Position, Position) <= 2500);
        if (monsters.Count() > 0)
        {
            light = 0;
            message += " ..sh";
            var monstersApproaching = monsters.Where(m => Vector2.Dot(Vector2.Normalize(m.Direction) , Vector2.Normalize(Position - m.Position)) > 0.7).ToList();
            if (monstersApproaching.Count() > 0)
            {
                var evasiveDirection = Vector2.Normalize(Position - monstersApproaching[0].Position) * 2000;
                x = (int)(Position.X + evasiveDirection.X);
                x = Math.Clamp(x, 0, 10000);
                y = (int)(Position.Y + evasiveDirection.Y);
                y = Math.Clamp(y, 0, 10000);
                message += " ..Run";
            }
        }

        Console.WriteLine($"MOVE {x} {y} {light} {message}");
    }
}

class CreatureType
{
    public int Id {get;}
    public int Color {get;}
    public int Type {get;}

    public CreatureType(TextReader textReader)
    {
        var inputs = textReader.ReadLine().Split(' ').Select(int.Parse).ToList();
        Id = inputs[0];
        Color = inputs[1];
        Type = inputs[2];
    }
}

class Creature
{
    public int Id {get;}
    public Vector2 Position {get;}
    public Vector2 Direction {get;}

    public CreatureType Type => Game.Instance.creatureTypes[Id];

    public Creature(TextReader textReader)
    {
        var inputs = textReader.ReadLine().Split(' ').Select(int.Parse).ToList();
        Id = inputs[0];
        Position = new Vector2(inputs[1], inputs[2]);
        Direction = new Vector2(inputs[3], inputs[4]);
    }
}

class RadarBlip
{
    public int DroneId {get;}
    public int CreatureId {get;}
    public string Radar {get;}

    public CreatureType Type => Game.Instance.creatureTypes[CreatureId];

    public RadarBlip(TextReader textReader)
    {
        var inputs = textReader.ReadLine().Split(' ').ToList();
        DroneId = int.Parse(inputs[0]);
        CreatureId = int.Parse(inputs[1]);
        Radar = inputs[2];
    }

    public override string ToString()
    {
        return $"RadarBlip(DroneId={DroneId}, CreatureId={CreatureId}, Radar={Radar})";
    }
}

class Player
{
    private string Name;

    public int Score {get; set;}
    public List<int> Scans {get;} = new List<int>();
    public List<Drone> Drones {get;} = new List<Drone>();
    public IEnumerable<int> AllScans
    {
        get
        {
           IEnumerable<int> allScans = Scans;
           foreach (var drone in Drones)
                allScans = allScans.Concat(drone.Scans);
           return allScans.Distinct();
        }
    }

    public Player(string name)
    {
        Name = name;
    }

    public void UpdateScan(int count, TextReader textReader)
    {
        Scans.Clear();
        for (int i = 0; i < count; i++)
            Scans.Add(int.Parse(textReader.ReadLine()));
        Console.Error.WriteLine($"{Name} Scans: " + String.Join(", ", Scans));
    }

    public void UpdateDrone(int count, TextReader textReader)
    {
        if (Drones.Count == 0) // init
        {
            for (int i = 0; i < count; i++)
                Drones.Add(new Drone(textReader));
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                var inputs = textReader.ReadLine().Split(' ').Select(int.Parse).ToList();
                var drone = Drones.First(d => d.Id == inputs[0]);
                drone.Update(inputs);
            }
        }
    }
}

class Game
{
    public static Game Instance;

    public int[] LevelDepth {get;} = {500, 3750, 6250, 8750};
    public int MaxBattery {get;} = 30;
    public int turn = 0;

    public Dictionary<int, CreatureType> creatureTypes {get; private set;}
    public Player My  {get;} = new Player("My");
    public Player Foe {get;} = new Player("Foe");
    public List<Creature> VisibleCreatures {get; private set;}
    public IEnumerable<Drone> AllDrones => My.Drones.Concat(Foe.Drones);

    public Game(TextReader textReader)
    {
        Instance = this;
        int creatureCount = int.Parse(textReader.ReadLine());
        creatureTypes= new Dictionary<int, CreatureType>(creatureCount);
        for (int i = 0; i < creatureCount; i++)
        {
            var creature = new CreatureType(textReader);
            creatureTypes[creature.Id] = creature;
        }
    }

    void Update(TextReader textReader)
    {
        turn++;
        My.Score = int.Parse(textReader.ReadLine());
        Foe.Score = int.Parse(textReader.ReadLine());
        My.UpdateScan(int.Parse(textReader.ReadLine()), textReader);
        Foe.UpdateScan(int.Parse(textReader.ReadLine()), textReader);
        My.UpdateDrone(int.Parse(textReader.ReadLine()), textReader);
        Foe.UpdateDrone(int.Parse(textReader.ReadLine()), textReader);

        // Drone scan
        int droneScanCount = int.Parse(textReader.ReadLine());
        var droneScans = new List<List<int>>();
        for (int i = 0; i < droneScanCount; i++)
            droneScans.Add(textReader.ReadLine().Split(' ').Select(int.Parse).ToList());
        foreach (var drone in My.Drones)
            drone.Scans = droneScans.Where(i => i[0] == drone.Id).Select(i => i[1]).ToList();

        // Visible creatures
        int visibleCreatureCount = int.Parse(textReader.ReadLine());
        VisibleCreatures = new List<Creature>(visibleCreatureCount);
        for (int i = 0; i < visibleCreatureCount; i++)
            VisibleCreatures.Add(new Creature(textReader));
        // RadarBlip
        int radarBlipCount = int.Parse(textReader.ReadLine());
        var radarBlips = new List<RadarBlip>(radarBlipCount);
        for (int i = 0; i < radarBlipCount; i++)
            radarBlips.Add(new RadarBlip(textReader));
        foreach (var drone in My.Drones)
            drone.RadarBlips = radarBlips.Where(d => d.DroneId == drone.Id).ToList();
    }

    public void ComputeAction()
    {
        foreach(var drone in My.Drones)
            drone.ComputeAction(this);
    }

    static void Main(string[] args)
    {
        Game game = new Game(Console.In);
 
        while (true)
        {
            game.Update(Console.In);
            game.ComputeAction();
        }
    }
}
