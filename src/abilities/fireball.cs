using Godot;

public partial class Fireball : Castable
{
	static PackedScene fireballscene = ResourceLoader.Load<PackedScene>("res://scenes/entities/skills/fireball.tscn");
	static PackedScene explosionscene = ResourceLoader.Load<PackedScene>("res://scenes/entities/skills/explosion.tscn");
	Damage PrimaryHit = new Damage(Fire: 100);
	Damage SecondaryExplosion = new Damage(Fire: 50);
	public double Cooldown
	{
		get { return 500; }
	}

	Turret turret;

	public Fireball(Turret turret)
	{
		this.turret = turret;
	}

	public void SpawnProjectile(Turret turret, creep target)
	{
		var proj = fireballscene.Instantiate<projectile>();
		turret.AddChild(proj);

		proj.Speed = 600;
		proj.OnHitCallback = OnProjectileHit;
		proj.SetTarget(target);
	}

	public void OnProjectileHit(creep c)
	{
		c.TakeDamage(PrimaryHit);

		var e = explosionscene.Instantiate<explosion>();
		e.Scale *= 3;
		e.OnHitCallback = OnAOEHit;
		turret.AddChild(e);
		e.GlobalPosition = c.GlobalPosition;
	}

	public void OnAOEHit(creep c)
	{
		c.TakeDamage(SecondaryExplosion);
	}

	public void Cast(creep target)
	{
		SpawnProjectile(turret, target);
	}
}
