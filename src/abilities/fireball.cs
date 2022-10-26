public class Fireball
{
	Damage Damage = new Damage(Fire: 100);

	public void OnHit(IDamageable target)
	{
		target.TakeDamage(Damage);
	}
}