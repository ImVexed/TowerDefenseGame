
using Godot;

public enum Diffuculty
{
    Easy,
    Medium,
    Hard
}

public enum WaveState
{
    NotStarted,
    Started,
    Finished,
    Died
}
public class WaveManager
{
    private Diffuculty diffuculty;

    public WaveState State = WaveState.NotStarted;
    public DateTime NextWaveTime = default(DateTime);

    public BigRational Gold;
    public double Health;
    public double DifficultyScalar;
    public double WaveNumber = 0;

    private Node2D parent;
    private Vector2 start;
    private Vector2 end;

    public WaveManager(Diffuculty diffuculty, Node2D parent, Vector2 start, Vector2 end)
    {
        this.diffuculty = diffuculty;
        this.parent = parent;
        this.start = start;
        this.end = end;

        switch (diffuculty)
        {
            case Diffuculty.Easy:
                Gold = 300;
                Health = 100;
                DifficultyScalar = 1;
                break;
            case Diffuculty.Medium:
                Gold = 200;
                Health = 50;
                DifficultyScalar = 0.8;
                break;
            case Diffuculty.Hard:
                Gold = 100;
                Health = 10;
                DifficultyScalar = 0.5;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(diffuculty), diffuculty, null);
        }
    }

    public void Start()
    {
        State = WaveState.Started;
    }

    public void Pause()
    {
        State = WaveState.NotStarted;
    }
    public void Died()
    {
        State = WaveState.Died;
    }


    public DateTime LastSpawnTime = DateTime.Now;
    public void Update()
    {
        if (State == WaveState.Died)
            return;

        // If we've just finished a wave, wait to start the next one
        if (State == WaveState.Finished && NextWaveTime < DateTime.Now)
        {
            State = WaveState.Started;
            WaveNumber++;
            NextWaveTime = DateTime.Now.AddSeconds(20); // TODO: back to 60s?
        }

        if (State != WaveState.Started)
            return;

        // If we're currently in a wave, wait to finish it
        if (NextWaveTime < DateTime.Now)
        {
            State = WaveState.Finished;
            NextWaveTime = DateTime.Now.AddSeconds(5); // TODO: back to 10s?
            return;
        }

        // Exponential decay function, with a minimum of 200ms between spawns
        // At wave 100 Easy is ~400ms, Medium is ~300ms, Hard is ~200ms
        var spawnDelay = 900 * Math.Pow(1 - (0.01 / DifficultyScalar), WaveNumber) + 100;
        GD.Print(spawnDelay);
        if (LastSpawnTime.AddMilliseconds(spawnDelay) < DateTime.Now)
        {
            LastSpawnTime = DateTime.Now;
            var creep = ResourceManager.NewCreep();
            // Exponential growth function, with a minimum of 1000 health
            // At wave 100 Easy is ~13m, Medium is ~130m, Hard is ~82b
            creep.MaxHealth = 1000 * Math.Pow(1 + (0.1 / DifficultyScalar), WaveNumber);
            creep.Leaked += CreepLeaked;
            creep.Killed += CreepKilled;

            parent.AddChild(creep);

            creep.GlobalPosition = start;
            creep.SetNavigationPosition(end);
        }
    }

    private void CreepLeaked(creep creep)
    {
        // Todo: Base this on creep type, ex a boss?
        Health -= 1;
        if (Health <= 0)
        {
            State = WaveState.Finished;
            NextWaveTime = default(DateTime);
        }
    }

    private void CreepKilled(creep creep)
    {
        Gold += WaveNumber *Math.Pow(1 - (0.01 / DifficultyScalar), WaveNumber);
    }


}