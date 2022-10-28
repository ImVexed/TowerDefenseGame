public class PoisonPool
{
	Damage Damage = new Damage(Poison: 100);

	public void OnHit(IDamageable target)
	{
	    target.TakeDamage(Damage);
	}
}