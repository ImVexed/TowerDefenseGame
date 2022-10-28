public class Baptism
{
	Damage Damage = new Damage(Holy: 1000);

	public void OnHit(IDamageable target)
	{
		target.TakeDamage(Damage);
	}
}