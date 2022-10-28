public class IceSpear
{
	Damage Damage = new Damage(Cold: 100);

	public void OnHit(IDamageable target)
	{
		target.TakeDamage(Damage);
	}
}