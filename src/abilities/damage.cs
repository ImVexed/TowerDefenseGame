public class Damage
{
	public BigRational Fire;
	public BigRational Cold;
	public BigRational Lightning;
	public BigRational Physical;
	public BigRational Chaos;

	public static Damage operator *(Damage a, BigRational scalar) 
	{
		a.Fire *= scalar;
		a.Cold *= scalar;
		a.Lightning *= scalar;
		a.Chaos *= scalar;
		a.Physical *= scalar;

		return a;
	}
}


public interface IHit
{
	void OnHit(IDamageable target);
}

public interface IDamageable
{
	void TakeDamage(Damage dmg);
}