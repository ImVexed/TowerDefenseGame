using Godot;

interface Castable
{
    double Cooldown { get; }
    void Cast(creep Target);
}

public delegate void OnHitCallback(creep c);