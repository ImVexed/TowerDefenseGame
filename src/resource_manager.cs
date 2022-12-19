using Godot;

public partial class ResourceManager
{
    public static PackedScene Fireball = ResourceLoader.Load<PackedScene>("res://scenes/entities/skills/fireball.tscn");
    public static projectile NewFireball() => Fireball.Instantiate<projectile>();

    public static PackedScene Explosion = ResourceLoader.Load<PackedScene>("res://scenes/entities/skills/explosion.tscn");
    public static explosion NewExplosion() => Explosion.Instantiate<explosion>();
}