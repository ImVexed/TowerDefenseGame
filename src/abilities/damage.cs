public enum DamageType
{
    Physical,
    Fire,
    Cold,
    Lightning,
    Chaos,
}


public class Damage
{
    public Damage(
     BigRational Fire = default(BigRational),
     BigRational Cold = default(BigRational),
     BigRational Lightning = default(BigRational),
     BigRational Physical = default(BigRational),
     BigRational Chaos = default(BigRational)
     )
    {
        this.Fire = Fire;
        this.Cold = Cold;
        this.Lightning = Lightning;
        this.Physical = Physical;
        this.Chaos = Chaos;
    }

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

    public static Damage operator /(Damage a, Resistances r)
    {
        a.Fire *= ((100 - r.Fire) / 100);
        a.Cold *= ((100 - r.Cold) / 100);
        a.Lightning *= ((100 - r.Lightning) / 100);
        a.Chaos *= ((100 - r.Chaos) / 100);
        a.Physical *= ((100 - r.Physical) / 100);

        return a;
    }


    // God Bless Copilot
    public DamageType PrimaryDamageType()
    {
        if (Fire > Cold && Fire > Lightning && Fire > Physical && Fire > Chaos)
        {
            return DamageType.Fire;
        }
        else if (Cold > Fire && Cold > Lightning && Cold > Physical && Cold > Chaos)
        {
            return DamageType.Cold;
        }
        else if (Lightning > Fire && Lightning > Cold && Lightning > Physical && Lightning > Chaos)
        {
            return DamageType.Lightning;
        }
        else if (Physical > Fire && Physical > Cold && Physical > Lightning && Physical > Chaos)
        {
            return DamageType.Physical;
        }
        else if (Chaos > Fire && Chaos > Cold && Chaos > Lightning && Chaos > Physical)
        {
            return DamageType.Chaos;
        }
        else // If it did literally no damage it's probably phys lmao
        {
            return DamageType.Physical;
        }
    }


    public static implicit operator BigRational(Damage d) => d.Fire + d.Cold + d.Lightning + d.Chaos + d.Physical;
}

public class Resistances
{
    public Resistances(
     BigRational Fire = default(BigRational),
     BigRational Cold = default(BigRational),
     BigRational Lightning = default(BigRational),
     BigRational Physical = default(BigRational),
     BigRational Chaos = default(BigRational)
     )
    {
        this.Fire = Fire;
        this.Cold = Cold;
        this.Lightning = Lightning;
        this.Physical = Physical;
        this.Chaos = Chaos;
    }

    public BigRational Fire;
    public BigRational Cold;
    public BigRational Lightning;
    public BigRational Physical;
    public BigRational Chaos;

    public static Resistances operator *(Resistances a, BigRational scalar)
    {
        a.Fire *= scalar;
        a.Cold *= scalar;
        a.Lightning *= scalar;
        a.Chaos *= scalar;
        a.Physical *= scalar;

        return a;
    }
}