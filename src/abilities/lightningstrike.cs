public class LightningStrike
{
	Damage Damage = new Damage(Lightning: 100);

	public void OnHit(IDamageable target)
	{
		target.TakeDamage(Damage);
	}
}