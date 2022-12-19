using Godot;

public partial class ResourceManager
{
    public static PackedScene Fireball = ResourceLoader.Load<PackedScene>("res://scenes/entities/skills/fireball.tscn");
    public static projectile NewFireball() => Fireball.Instantiate<projectile>();

    public static PackedScene Explosion = ResourceLoader.Load<PackedScene>("res://scenes/entities/skills/explosion.tscn");
    public static explosion NewExplosion() => Explosion.Instantiate<explosion>();

    public static PackedScene GameOverScene = ResourceLoader.Load<PackedScene>("res://scenes/gui/game_over.tscn");
    public static Label NewGameOverLabel() => GameOverScene.Instantiate<Label>();

    public static PackedScene CreepBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/creep.tscn");
    public static creep NewCreep() => CreepBase.Instantiate<creep>();

    public static PackedScene TowerBase = ResourceLoader.Load<PackedScene>("res://scenes/entities/turret.tscn");
    public static Turret NewTower() => TowerBase.Instantiate<Turret>();

    public static Texture2D PauseTexture = ResourceLoader.Load<Texture2D>("res://assets/pause.png");
    public static Texture2D PlayTexture = ResourceLoader.Load<Texture2D>("res://assets/next.png");
}