public class VoidSphere
{
	Damage Damage = new Damage(Void: 200);

	public void OnHit(IDamageable target)
	{
		target.TakeDamage(Damage);
	}
}