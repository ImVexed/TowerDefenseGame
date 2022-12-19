using Godot;

interface Castable
{
    double Cooldown { get; }
    void Cast(creep Target);
}

public delegate void OnHitCallback(creep c);
public delegate projectile SpawnProjectileCallback(Vector2 position);