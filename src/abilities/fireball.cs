using Godot;

public partial class Fireball : Castable
{
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

	public projectile SpawnProjectile(Vector2 position)
	{
		var proj = ResourceManager.NewFireball();
		proj.RemainingForks = 1;
		proj.RemainingChains = 1;
		proj.SpawnProjectileCallback = SpawnProjectile;
		proj.GlobalPosition = position;
		turret.CallDeferred("AddChildAtPosition", proj, position);

		proj.Speed = 600;
		proj.OnHitCallback = (c) =>
		{
			c.TakeDamage(PrimaryHit);

			var e = ResourceManager.NewExplosion();
			e.Scale *= 3;
			e.OnHitCallback = (c) =>
			{
				c.TakeDamage(SecondaryExplosion);
			};

			turret.CallDeferred("AddChildAtPosition", e, c.GlobalPosition);
		};

		return proj;
	}

	public void Cast(creep target)
	{
		var p = SpawnProjectile(turret.GlobalPosition);
		p.SetTarget(target.GlobalPosition, target.Velocity);
	}
}
