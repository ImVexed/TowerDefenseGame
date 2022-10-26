using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
 
 
 public unsafe struct BigRational : IComparable<BigRational>, IEquatable<BigRational>
  {
    int sign; uint[] bits;
    public override int GetHashCode()
    {
      var h = (uint)sign;
      if (bits != null) for (int i = bits.Length; i-- != 0; h = ((h << 7) | (bits[i] >> 25)) ^ bits[i]) ;
      return (int)h;
    }
    public override bool Equals(object obj)
    {
      return obj is BigRational && Equals((BigRational)obj);
    }
    public bool Equals(BigRational b)
    {
      if (sign != b.sign) return false;
      if (bits == null && b.bits == null) return true;
      if (bits == null || b.bits == null || bits.Length != b.bits.Length) return false;
      for (int i = 0, n = bits.Length; i < n; i++) if (bits[i] != b.bits[i]) return false;
      return true;
    }
    public int CompareTo(BigRational b)
    {
      return cmp(this, b);
    }
    public string ToString(int digits)
    {
      fixed (uint* p = bits)
      {
        rat c; fix(&c, p); var nn = (c.num.n > c.den.n ? c.num.n : c.den.n) + 2;
        var p1 = stackalloc uint[nn * 3]; var p2 = p1 + nn; var p3 = p2 + nn;
        for (int i = 0; i < c.num.n; i++) p1[i] = c.num.p[i];
        var n1 = mod(p1, c.num.n, c.den.p, c.den.n, p2); var n = n1 >> 16;
        var ss = stackalloc char[2 + n * 10 + digits]; var ns = 0; uint ten = 10; big bten; bten.p = &ten; bten.n = 1;
        if (sign < 0) ss[ns++] = '-'; if (n == 1 && p2[0] == 0) ss[ns++] = '0'; var ab = ns;
        while (n > 1 || p2[0] != 0) { n = mod(p2, n, bten.p, 1, p3) >> 16; ss[ns++] = (char)('0' + *p2); var t = p2; p2 = p3; p3 = t; }
        for (int i = ab, k = ns - 1; i < k; i++, k--) { var t = ss[i]; ss[i] = ss[k]; ss[k] = t; } n = n1 & 0xffff;
        for (int x = 0; x < digits && (n > 1 || p1[0] != 0); x++)
        {
          if (x == 0) ss[ns++] = '.'; for (int i = 0; i < n + 2; i++) p2[i] = 0; Debug.Assert(n + 2 <= nn);
          var n2 = mul(new big { p = p1, n = n }, bten, p2); Debug.Assert(n2.n < nn);
          n = mod(n2.p, n2.n, c.den.p, c.den.n, p1) & 0xffff; ss[ns++] = (char)('0' + *p1);
          var t = p1; p1 = p2; p2 = t;
        }
        return new string(ss, 0, ns);
      }
    }

	public override string ToString()
    {
      return ToString(128);
    }
    public static BigRational Parse(string s)
    {
      BigRational a = 0, b = a, e = 1, f = 10;
      for (int i = s.Length - 1, c; i >= 0; i--)
      {
        if ((c = s[i]) >= '0' && c <= '9') { a += (c - '0') * e; e *= f; continue; }
        if (c == '.') b = e; else if (c == '-') a = -a;
      }
      if (b.sign != 0) a /= b; return a;
    }
    public int Sign
    {
      get { return (sign >> 31) - (-sign >> 31); }
    }
    public BigRational Num
    {
      get { fixed (uint* p = bits) { rat c; fix(&c, p); c.den.p = (uint*)&c.den.n; if (c.sign < 0) c.sign = 1; c.den.n = 1; Tmp t; t.p = &c; return t; } }
    }
    public BigRational Den
    {
      get { fixed (uint* p = bits) { rat c; fix(&c, p); c.num = c.den; c.den.p = (uint*)&c.den.n; c.den.n = 1; if (c.sign < 0) c.sign = 1; Tmp t; t.p = &c; return t; } }
    }
    public static bool operator ==(BigRational a, BigRational b)
    {
      return a.Equals(b);
    }
    public static bool operator !=(BigRational a, BigRational b)
    {
      return !a.Equals(b);
    }
    public static bool operator <=(BigRational a, BigRational b)
    {
      return cmp(a, b) <= 0;
    }
    public static bool operator >=(BigRational a, BigRational b)
    {
      return cmp(a, b) >= 0;
    }
    public static bool operator <(BigRational a, BigRational b)
    {
      return cmp(a, b) < 0;
    }
    public static bool operator >(BigRational a, BigRational b)
    {
      return cmp(a, b) > 0;
    }
    public static BigRational operator -(BigRational v)
    {
      if (v.sign != 0) v.sign ^= 1 << 31; return v;
    }
    public static implicit operator BigRational(int v)
    {
      var a = (uint)(v < 0 ? -v : v);
      if (a <= 0x7fff) { BigRational r; r.sign = (int)(a << 16) | (v & (1<<31)); r.bits = null; return r; }
      return (Tmp)v;
    }
    public static implicit operator BigRational(float v)
    {
      return (Tmp)v;
    }
    public static implicit operator BigRational(double v)
    {
      return (Tmp)v;
    }
    public static implicit operator BigRational(decimal v)
    {
      return (Tmp)v;
    }
    public static explicit operator double(BigRational v)
    {
      double r;
      if (v.bits == null) r = (double)((v.sign & 0x7fff0000) >> 16) / ((v.sign & 0xffff) + 1);
      else
      {
        var na = v.sign & 0x7fffffff; var nb = v.bits.Length - na;
        fixed (uint* p = v.bits) { r = dbl(p, na); if (nb != 0) r /= dbl(p + na, nb); }
      }
      return v.sign < 0 ? -r : r;
    }
    public static explicit operator float(BigRational v)
    {
      return (float)(double)v;
    }
    public static explicit operator long(BigRational v)
    {
      long r;
      if (v.bits == null) r = ((v.sign & 0x7fff0000) >> 16) / ((v.sign & 0xffff) + 1);
      else
        fixed (uint* p = v.bits)
        {
          rat c; v.fix(&c, p);
          var p1 = stackalloc uint[(c.num.n << 1) + 1]; var p2 = p1 + c.num.n;
          for (int i = 0; i < c.num.n; i++) p1[i] = c.num.p[i];
          var n = mod(p1, c.num.n, c.den.p, c.den.n, p2) >> 16;
          r = checked((long)*(ulong*)p2); if (n > 2) throw new OverflowException();
        }
      return v.sign < 0 ? -r : r;
    }
    public static Tmp operator +(BigRational a, BigRational b)
    {
      fixed (uint* pa = a.bits, pb = b.bits) { rat ca, cb; a.fix(&ca, pa); b.fix(&cb, pb); return add(&ca, &cb); }
    }
    public static Tmp operator +(BigRational a, Tmp b)
    {
      fixed (uint* p = a.bits) { rat c; a.fix(&c, p); return add(&c, b.p); }
    }
    public static Tmp operator +(Tmp a, BigRational b)
    {
      fixed (uint* p = b.bits) { rat c; b.fix(&c, p); return add(a.p, &c); }
    }
    public static Tmp operator -(BigRational a, BigRational b)
    {
      fixed (uint* pa = a.bits, pb = b.bits) { rat ca, cb; a.fix(&ca, pa); b.fix(&cb, pb); cb.sign = -cb.sign; return add(&ca, &cb); }
    }
    public static Tmp operator -(BigRational a, Tmp b)
    {
      fixed (uint* p = a.bits) { rat c; a.fix(&c, p); b.p->sign = -b.p->sign; return add(&c, b.p); }
    }
    public static Tmp operator -(Tmp a, BigRational b)
    {
      fixed (uint* p = b.bits) { rat c; b.fix(&c, p); c.sign = -c.sign; return add(a.p, &c); }
    }
    public static Tmp operator *(BigRational a, BigRational b)
    {
      fixed (uint* pa = a.bits, pb = b.bits) { rat ca, cb; a.fix(&ca, pa); b.fix(&cb, pb); return mul(&ca, &cb); }
    }
    public static Tmp operator *(BigRational a, Tmp b)
    {
      fixed (uint* p = a.bits) { rat c; a.fix(&c, p); return mul(&c, b.p); }
    }
    public static Tmp operator *(Tmp a, BigRational b)
    {
      fixed (uint* p = b.bits) { rat c; b.fix(&c, p); return mul(a.p, &c); }
    }
    public static Tmp operator /(BigRational a, BigRational b)
    {
      fixed (uint* pa = a.bits, pb = b.bits) { rat ca, cb; a.fix(&ca, pa); b.fix(&cb, pb); return div(&ca, &cb); }
    }
    public static Tmp operator /(BigRational a, Tmp b)
    {
      fixed (uint* p = a.bits) { rat c; a.fix(&c, p); return div(&c, b.p); }
    }
    public static Tmp operator /(Tmp a, BigRational b)
    {
      fixed (uint* p = b.bits) { rat c; b.fix(&c, p); return div(a.p, &c); }
    }
    public struct Tmp
    {
      internal rat* p;
      public override int GetHashCode() { return 0; }
      public override bool Equals(object obj) { throw new NotImplementedException(); }
      public override string ToString() { return ((BigRational)this).ToString(); }
      public int Sign { get { return p->sign; } }
      internal Tmp(uint* a, int na, uint* b, int nb, int sign)
      {
        for (; na > 1 && a[na - 1] == 0; na--) ;
        for (; nb > 1 && b[nb - 1] == 0; nb--) ; Debug.Assert(sign >= -1 && sign <= 1 && (sign != 0 || (na == 1 && nb == 1 && a[0] == 0)));
        p = (rat*)alloc(((sizeof(rat) >> 2) - 2) + na + nb);
        p->num.p = (uint*)&p->tmp; p->den.p = p->num.p + na;
        p->num.n = na; for (int i = 0; i < na; i++) p->num.p[i] = a[i];
        p->den.n = nb; for (int i = 0; i < nb; i++) p->den.p[i] = b[i]; p->sign = sign;
      }
      public static implicit operator Tmp(int v)
      {
        var p = (rat*)alloc(sizeof(rat) >> 2);
        p->den.p = (p->num.p = (uint*)&p->tmp) + (p->num.n = p->den.n = 1);
        p->num.p[0] = (uint)(v < 0 ? -v : v); p->den.p[0] = 1;
        p->sign = v == 0 ? 0 : v > 0 ? +1 : -1;
        Tmp t; t.p = p; return t;
      }
      public static implicit operator Tmp(float v)
      {
        var bits = *(uint*)&v; if ((bits & 0x7FFFFFFF) == 0) return 0;
        var man = bits & 0x7FFFFF;
        var exp = (int)(bits >> 23) & 0xFF; Debug.Assert(exp != 0); //NaN
        man |= 0x800000; exp -= 150; 
        int n1 = 1, n2 = 1; if (exp > 0) n1 += exp >> 5; else n2 += -exp >> 5;
        var p = (rat*)alloc(((sizeof(rat) >> 2) - 2) + n1 + n2);
        p->num.n = 1; *(p->num.p = (uint*)&p->tmp) = man;
        p->den.n = 1; *(p->den.p = p->num.p + n1) = 1;
        if (exp > 0) shl(&p->num, exp); else if (exp < 0) shl(&p->den, -exp);
        p->sign = (bits >> 31) == 0 ? +1 : -1; Tmp t; t.p = p; return t;
      }
      public static implicit operator Tmp(double v)
      {
        var bits = *(ulong*)&v; if ((bits & 0x7FFFFFFFFFFFFFFF) == 0) return 0;
        var man = bits & 0x000FFFFFFFFFFFFF;
        var exp = (int)(bits >> 52) & 0x7FF; Debug.Assert(exp != 0 && exp != 0x7FF); //NaN
        man |= 0x0010000000000000; exp -= 1075;
        int n1 = (man >> 32) != 0 ? 2 : 1, n2 = 1, s1 = n1, s2 = n2;
        if (exp > 0) n1 += exp >> 5; else n2 += -exp >> 5;
        var p = (rat*)alloc(((sizeof(rat) >> 2) - 2) + n1 + n2);
        p->num.n = s1; *(ulong*)(p->num.p = (uint*)&p->tmp) = man;
        p->den.n = s2; *(p->den.p = p->num.p + n1) = 1;
        if (exp > 0) shl(&p->num, exp); else if (exp < 0) shl(&p->den, -exp);
        p->sign = (bits >> 63) == 0 ? +1 : -1; Tmp t; t.p = p; return t;
      }
      public static implicit operator Tmp(decimal v)
      {
        if (v == 0) return 0;
        var d = 1M; big a, b; a.p = (uint*)&v; b.p = (uint*)&d;
        var s = a.p[0]; var e = (s >> 16) & 0xff; //0-28
        for (var t = 10UL; ; t = t * t) { if ((e & 1) != 0) d *= t; if ((e >>= 1) == 0) break; } //pow(10, e)
        a.p[0] = a.p[2]; a.p[2] = a.p[1]; a.p[1] = a.p[3];
        b.p[0] = b.p[2]; b.p[2] = b.p[1]; b.p[1] = b.p[3];
        return new Tmp(a.p, 3, b.p, 3, (s >> 31) == 0 ? +1 : -1);
      }
      public static Tmp operator -(Tmp v)
      {
        v.p->sign = -v.p->sign; return v;
      }
      public static Tmp operator +(Tmp a, Tmp b) { return add(a.p, b.p); }
      public static Tmp operator -(Tmp a, Tmp b) { b.p->sign = -b.p->sign; var t = add(a.p, b.p); b.p->sign = -b.p->sign; return t; }
      public static Tmp operator *(Tmp a, Tmp b) { return mul(a.p, b.p); }
      public static Tmp operator /(Tmp a, Tmp b) { return div(a.p, b.p); }
      public static bool operator ==(Tmp a, Tmp b) { return cmp(a, b) == 0; }
      public static bool operator !=(Tmp a, Tmp b) { return cmp(a, b) != 0; }
      public static bool operator <=(Tmp a, Tmp b) { return cmp(a, b) <= 0; }
      public static bool operator >=(Tmp a, Tmp b) { return cmp(a, b) >= 0; }
      public static bool operator <(Tmp a, Tmp b) { return cmp(a, b) < 0; }
      public static bool operator >(Tmp a, Tmp b) { return cmp(a, b) > 0; }
      public static implicit operator BigRational(Tmp v)
      {
        BigRational r; var a = &v.p->num; var b = &v.p->den;
        r.sign = 0; r.bits = null; if (a->n == 1 && a->p[0] == 0) return r; //0
        simplify(a, b);
        if (a->n == 1 && b->n == 1 && a->p[0] <= 0x7fff && (b->p[0] - 1) <= 0xffff) { r.sign = ((int)a->p[0] << 16) | ((int)b->p[0] - 1) | (v.p->sign & (1 << 31)); return r; }
        if (b->n == 1 && b->p[0] == 1) b->n = 0;
        r.sign = a->n; r.bits = new uint[a->n + b->n]; //the one and only new on the GC heap (except ToString)
        for (int i = 0; i < a->n; i++) r.bits[i] = a->p[i];
        for (int i = 0; i < b->n; i++) r.bits[a->n + i] = b->p[i];
        r.sign |= v.p->sign & (1 << 31); return r;
      }
      public static explicit operator double(Tmp v)
      {
        var f = dbl(v.p->num.p, v.p->num.n) / dbl(v.p->den.p, v.p->den.n);
        if (v.p->sign < 0) f = -f; return f;
      }
    }

    [ThreadStatic]
    static int imem;
    [ThreadStatic]
    static uint* pmem;
    static uint* alloc(int n)
    {
      if (pmem == null) pmem = (uint*)Marshal.AllocCoTaskMem(4096 << 2).ToPointer();
      if (imem + n > 4096) { imem = 0; if (n > 4096) throw new OutOfMemoryException(); }
      var p = pmem + imem; imem += n; return p;
    }

    static Tmp add(rat* a, rat* b)
    {
      if (a->sign == 0) return new Tmp(b->num.p, b->num.n, b->den.p, b->den.n, b->sign);
      if (b->sign == 0) return new Tmp(a->num.p, a->num.n, a->den.p, a->den.n, a->sign);
      var m1 = a->num.n + b->den.n + 1;
      var m2 = a->den.n + b->num.n + 1;
      var m3 = a->den.n + b->den.n + 1; var m4 = m1 + m2 + m3;
      Tmp t; t.p = (rat*)alloc(((sizeof(rat) >> 2) - 2) + m4);
      var p1 = (uint*)&t.p->tmp; var p2 = p1 + m1; var p3 = p2 + m2;
      for (int i = 0; i < m4; i++) p1[i] = 0;
      var n1 = mul(a->num, b->den, p1);
      var n2 = mul(a->den, b->num, p2);
      int si; big s1;
      if (a->sign > 0 == b->sign > 0)
      {
        s1 = add(n1, n2, p1); si = a->sign;
      }
      else
      {
        si = cmp(n1, n2); if (si == 0) return 0;
        s1 = si > 0 ? sub(n1, n2, p1) : sub(n2, n1, p1); si = a->sign == si ? +1 : -1;
      }
      t.p->den = mul(a->den, b->den, p3);
      t.p->num = s1; t.p->sign = si;
      return t;
    }
    static Tmp mul(rat* a, rat* b)
    {
      if (a->sign == 0) return 0;
      if (b->sign == 0) return 0;
      var m1 = a->num.n + b->num.n + 1;
      var m2 = a->den.n + b->den.n + 1; var m3 = m1 + m2;
      Tmp t; t.p = (rat*)alloc(((sizeof(rat) >> 2) - 2) + m3);
      var p1 = (uint*)&t.p->tmp; var p2 = p1 + m1;
      for (int i = 0; i < m3; i++) p1[i] = 0;
      t.p->num = mul(a->num, b->num, p1);
      t.p->den = mul(a->den, b->den, p2);
      t.p->sign = a->sign == b->sign ? +1 : -1;
      return t;
    }
    static Tmp div(rat* a, rat* b)
    {
      if (b->sign == 0) throw new DivideByZeroException();
      var t = b->num; b->num = b->den; b->den = t; return mul(a, b);
    }
    static int cmp(Tmp a, Tmp b) 
    { 
      b.p->sign = -b.p->sign; var s = add(a.p, b.p).p->sign; b.p->sign = -b.p->sign; return s; 
    }

    static void simplify(big* a, big* b)
    {
      int na; if ((na = a->n) == 1 && a->p[0] == 1) return;
      int nb; if ((nb = b->n) == 1 && b->p[0] == 1) return;
      Debug.Assert(!(a->n == 1 && a->p[0] == 0));
      Debug.Assert(!(b->n == 1 && b->p[0] == 0));
      if (na <= 2 && nb <= 2)
      {
        big ca; var ua = (ca.n = na) == 2 ? *(ulong*)a->p : a->p[0]; ca.p = (uint*)&ua; var ra = ua;
        big cb; var ub = (cb.n = nb) == 2 ? *(ulong*)b->p : b->p[0]; cb.p = (uint*)&ub; var rb = ub;
        var cc = gcd(ca, cb); var d = cc.n != 1 ? *(ulong*)cc.p : cc.p[0]; if (d == 1) return;
        ra /= d; if ((ra >> 32) != 0) *(ulong*)a->p = ra; else { a->p[0] = (uint)ra; a->n = 1; }
        rb /= d; if ((rb >> 32) != 0) *(ulong*)b->p = rb; else { b->p[0] = (uint)rb; b->n = 1; }
        return;
      }
      var p1 = stackalloc uint[(na + nb) << 1]; var p2 = p1 + na;
      big ba; ba.p = p1; ba.n = na; for (int i = 0; i < na; i++) ba.p[i] = a->p[i];
      big bb; bb.p = p2; bb.n = nb; for (int i = 0; i < nb; i++) bb.p[i] = b->p[i];
      var nr = gcd(ba, bb); if (nr.n == 1 && nr.p[0] == 1) return;
      var t1 = p2 + nb; var t2 = t1 + na;
      for (int i = 0; i < na; i++) t1[i] = a->p[i];
      for (int i = 0; i < nb; i++) t2[i] = b->p[i];
      a->n = mod(t1, na, nr.p, nr.n, a->p) >> 16;
      b->n = mod(t2, nb, nr.p, nr.n, b->p) >> 16;
    }
    static big gcd(big a, big b)
    {
      int shift = 0;
      if (a.p[0] == 0 || b.p[0] == 0)
      {
        int i1 = 0; for (; a.p[i1] == 0; i1++) ; i1 = clz(a.p[i1]) + (i1 << 5); if (i1 != 0) shr(&a, i1);
        int i2 = 0; for (; b.p[i2] == 0; i2++) ; i2 = clz(b.p[i2]) + (i2 << 5); if (i2 != 0) shr(&b, i2); shift = i1 < i2 ? i1 : i2;
      }
      for (; ; )
      {
        if (a.n < b.n) { var t = a; a = b; b = t; }
        int max = a.n, min = b.n;
        if (min == 1)
        {
          if (max != 1)
          {
            if (b.p[0] == 0) break;
            ulong u = 0; for (int i = a.n; i-- != 0; u = (u << 32) | a.p[i], u %= b.p[0]) ;
            a.n = 1; if (u == 0) { a.p[0] = b.p[0]; break; } a.p[0] = (uint)u;
          }
          uint xa = a.p[0], xb = b.p[0]; for (; (xa > xb ? xa %= xb : xb %= xa) != 0; ) ; a.p[0] = xa | xb; break;
        }
        if (max == 2)
        {
          var xa = a.n == 2 ? *(ulong*)a.p : a.p[0];
          var xb = b.n == 2 ? *(ulong*)b.p : b.p[0];
          for (; (xa > xb ? xa %= xb : xb %= xa) != 0; ) ;
          *(ulong*)a.p = xa | xb; a.n = a.p[1] != 0 ? 2 : 1; break;
        }
        if (min <= max - 2) { a.n = mod(a.p, a.n, b.p, b.n, null); continue; }
        ulong uu1 = a.n >= max ? ((ulong)a.p[max - 1] << 32) | a.p[max - 2] : a.n == max - 1 ? a.p[max - 2] : 0;
        ulong uu2 = b.n >= max ? ((ulong)b.p[max - 1] << 32) | b.p[max - 2] : b.n == max - 1 ? b.p[max - 2] : 0;
        int cbit = chz(uu1 | uu2);
        if (cbit > 0)
        {
          uu1 = (uu1 << cbit) | (a.p[max - 3] >> (32 - cbit));
          uu2 = (uu2 << cbit) | (b.p[max - 3] >> (32 - cbit));
        }
        if (uu1 < uu2) { var t1 = uu1; uu1 = uu2; uu2 = t1; var t2 = a; a = b; b = t2; }
        if (uu1 == 0xffffffffffffffff || uu2 == 0xffffffffffffffff) { uu1 >>= 1; uu2 >>= 1; }
        if (uu1 == uu2) { a = sub(a, b, a.p); continue; }
        if ((uu2 >> 32) == 0) { a.n = mod(a.p, a.n, b.p, b.n, null); continue; }
        uint ma = 1, mb = 0, mc = 0, md = 1;
        for (; ; )
        {
          uint uQuo = 1; ulong uuNew = uu1 - uu2;
          for (; uuNew >= uu2 && uQuo < 32; uuNew -= uu2, uQuo++) ;
          if (uuNew >= uu2)
          {
            ulong uuQuo = uu1 / uu2; if (uuQuo > 0xffffffff) break;
            uQuo = (uint)uuQuo; uuNew = uu1 - uQuo * uu2;
          }
          ulong uuAdNew = ma + (ulong)uQuo * mc;
          ulong uuBcNew = mb + (ulong)uQuo * md;
          if (uuAdNew > 0x7FFFFFFF || uuBcNew > 0x7FFFFFFF) break;
          if (uuNew < uuBcNew || uuNew + uuAdNew > uu2 - mc) break;
          ma = (uint)uuAdNew; mb = (uint)uuBcNew;
          uu1 = uuNew; if (uu1 <= mb) break;
          uQuo = 1; uuNew = uu2 - uu1;
          for (; uuNew >= uu1 && uQuo < 32; uuNew -= uu1, uQuo++) ;
          if (uuNew >= uu1)
          {
            ulong uuQuo = uu2 / uu1; if (uuQuo > 0xffffffff) break;
            uQuo = (uint)uuQuo; uuNew = uu2 - uQuo * uu1;
          }
          uuAdNew = md + (ulong)uQuo * mb;
          uuBcNew = mc + (ulong)uQuo * ma;
          if (uuAdNew > 0x7FFFFFFF || uuBcNew > 0x7FFFFFFF) break;
          if (uuNew < uuBcNew || uuNew + uuAdNew > uu1 - mb) break;
          md = (uint)uuAdNew; mc = (uint)uuBcNew;
          uu2 = uuNew; if (uu2 <= mc) break;
        }
        if (mb == 0) { if (uu1 / 2 >= uu2) a.n = mod(a.p, a.n, b.p, b.n, null); else a = sub(a, b, a.p); continue; }
        int c1 = 0, c2 = 0; b.n = a.n = min;
        for (int iu = 0; iu < min; iu++)
        {
          uint u1 = a.p[iu], u2 = b.p[iu];
          long nn1 = (long)u1 * ma - (long)u2 * mb + c1; a.p[iu] = (uint)nn1; c1 = (int)(nn1 >> 32);
          long nn2 = (long)u2 * md - (long)u1 * mc + c2; b.p[iu] = (uint)nn2; c2 = (int)(nn2 >> 32);
        }
        while (a.n > 1 && a.p[a.n - 1] == 0) a.n--;
        while (b.n > 1 && b.p[b.n - 1] == 0) b.n--;
      }
      if (shift != 0) shl(&a, shift); return a;
    }
    static void shr(big* p, int c)
    {
      var s = c & 31; var d = c >> 5;
      if (s == 0) { p->n -= d; for (int i = 0; i < p->n; i++) p->p[i] = p->p[i + d]; return; }
      var r = 32 - s; p->n -= d + 1;
      for (int i = 0; i < p->n; i++) p->p[i] = (p->p[i + d] >> s) | (p->p[i + d + 1] << r);
      var t = p->p[p->n + d] >> s; if (t != 0) p->p[p->n++] = t;
    }
    static void shl(big* p, int c)
    {
      var s = c & 31; var d = c >> 5;
      if (s == 0) { p->n += d; for (int i = p->n; i-- > d; ) p->p[i] = p->p[i - d]; }
      else
      {
        var r = 32 - s; var l = p->n; var t = p->p[l - 1] >> r;
        if (t != 0) { p->p[l + d] = t; p->n = l + d + 1; } else p->n = l + d;
        for (int i = l - 1; i > 0; i--) p->p[i + d] = (p->p[i] << s) | (p->p[i - 1] >> r);
        p->p[d] = p->p[0] << s;
      }
      for (int i = 0; i < d; i++) p->p[i] = 0;
    }
    static int chz(uint u)
    {
      int c = 0;
      if ((u & 0xFFFF0000) == 0) { c += 16; u <<= 16; }
      if ((u & 0xFF000000) == 0) { c += 8; u <<= 8; }
      if ((u & 0xF0000000) == 0) { c += 4; u <<= 4; }
      if ((u & 0xC0000000) == 0) { c += 2; u <<= 2; }
      if ((u & 0x80000000) == 0) c += 1; return c;
    }
    static int chz(ulong uu)
    {
      return (uu & 0xFFFFFFFF00000000) == 0 ? 32 + chz((uint)uu) : chz((uint)(uu >> 32));
    }
    static int clz(uint u)
    {
      int c = 0;
      if ((u & 0x0000FFFF) == 0) { c += 16; u >>= 16; }
      if ((u & 0x000000FF) == 0) { c += 8; u >>= 8; }
      if ((u & 0x0000000F) == 0) { c += 4; u >>= 4; }
      if ((u & 0x00000003) == 0) { c += 2; u >>= 2; }
      if ((u & 0x00000001) == 0) c += 1; return c;
    }
    static big add(big a, big b, uint* p)
    {
      if (a.n < b.n) { var t = a; a = b; b = t; }
      big r; r.p = p; r.n = a.n; uint c = 0; int i = 0;
      for (; i < b.n; i++) { var u = (ulong)a.p[i] + b.p[i] + c; r.p[i] = ((uint*)&u)[0]; c = ((uint*)&u)[1]; }
      for (; i < a.n; i++) { var u = (ulong)a.p[i] + c; /*    */ r.p[i] = ((uint*)&u)[0]; c = ((uint*)&u)[1]; }
      if (c != 0) r.p[r.n++] = c; return r;
    }
    static big sub(big a, big b, uint* p)
    {
      big r; r.p = p; uint c = 0; int i = 0; Debug.Assert(a.n >= b.n);
      for (; i < b.n; i++) { var u = (ulong)a.p[i] - b.p[i] - c; r.p[i] = ((uint*)&u)[0]; c = (uint)-((int*)&u)[1]; }
      for (; i < a.n; i++) { var u = (ulong)a.p[i] /*    */ - c; r.p[i] = ((uint*)&u)[0]; c = (uint)-((int*)&u)[1]; }
      for (r.n = i; r.n > 1 && r.p[r.n - 1] == 0; r.n--) ;
      Debug.Assert(c == 0); return r;
    }
    static big mul(big a, big b, uint* p)
    {
      big r; r.p = p; r.n = a.n + b.n - 1;
      if (a.n == 1 && a.p[0] == 0) { *r.p = 0; r.n = 1; return r; }
      if (b.n == 1 && b.p[0] == 0) { *r.p = 0; r.n = 1; return r; }
      for (int i = a.n, k; i-- != 0; )
      {
        uint c = 0;
        for (k = 0; k < b.n; k++) { var t = (ulong)b.p[k] * a.p[i] + r.p[i + k] + c; r.p[i + k] = (uint)t; c = (uint)(t >> 32); }
        if (c == 0) continue;
        for (k = i + b.n; c != 0 && k < r.n; k++) { var t = (ulong)r.p[k] + c; r.p[k] = (uint)t; c = (uint)(t >> 32); }
        if (c == 0) continue; r.p[r.n++] = c;
      }
      return r;
    }
    static int mod(uint* a, int na, uint* b, int nb, uint* p)
    {
      if (na < nb) { if (p == null) return na; *p = 0; return na | (1 << 16); }
      if (nb == 1)
      {
        ulong uu = 0, ub = b[0];
        for (int i = na; i-- != 0; ) { uu = ((ulong)(uint)uu << 32) | a[i]; if (p != null) p[i] = (uint)(uu / ub); uu %= ub; }
        a[0] = (uint)uu; if (p == null) return 1;
        for (; na > 1 && p[na - 1] == 0; na--) ; return 1 | (na << 16);
      }
      if (nb == 2 && na == 2)
      {
        if (p != null) *(ulong*)p = *(ulong*)a / *(ulong*)b; *(ulong*)a %= *(ulong*)b;
        if (a[na - 1] == 0) na = 1; if (p == null) return na;
        if (p[nb - 1] == 0) nb = 1; return (nb << 16) | na;
      }

      int diff = na - nb, nc = diff;
      for (int i = na - 1; ; i--)
      {
        if (i < diff) { nc++; break; }
        if (b[i - diff] != a[i]) { if (b[i - diff] < a[i]) nc++; break; }
      }
      if (nc == 0) { if (p == null) return na; *p = 0; return na | (1 << 16); }
      uint uden = b[nb - 1], unex = nb > 1 ? b[nb - 2] : 0;
      int shl = chz(uden), shr = 32 - shl;
      if (shl > 0)
      {
        uden = (uden << shl) | (unex >> shr); unex <<= shl;
        if (nb > 2) unex |= b[nb - 3] >> shr;
      }
      for (int i = nc; --i >= 0; )
      {
        uint hi = i + nb < na ? a[i + nb] : 0;
        ulong uu = ((ulong)hi << 32) | a[i + nb - 1];
        uint un = i + nb - 2 >= 0 ? a[i + nb - 2] : 0;
        if (shl > 0)
        {
          uu = (uu << shl) | (un >> shr); un <<= shl;
          if (i + nb >= 3) un |= a[i + nb - 3] >> shr;
        }
        ulong quo = uu / uden, rem = (uint)(uu % uden);
        if (quo > 0xffffffff) { rem += uden * (quo - 0xffffffff); quo = 0xffffffff; }
        while (rem <= 0xffffffff && quo * unex > (((ulong)(uint)rem << 32) | un)) { quo--; rem += uden; }
        if (quo > 0)
        {
          ulong bor = 0;
          for (int k = 0; k < nb; k++)
          {
            bor += b[k] * quo; uint sub = (uint)bor;
            bor >>= 32; if (a[i + k] < sub) bor++;
            a[i + k] -= sub;
          }
          if (hi < bor)
          {
            uint c = 0;
            for (int k = 0; k < nb; k++)
            {
              ulong t = (ulong)a[i + k] + b[k] + c;
              a[i + k] = (uint)t; c = (uint)(t >> 32);
            }
            quo--;
          }
          na = i + nb;
        }
        if (p != null) p[i] = (uint)quo;
      }
      for (; na > 1 && a[na - 1] == 0; na--) ; if (p == null) return na;
      for (; nc > 1 && p[nc - 1] == 0; nc--) ; return (nc << 16) | na;
    }
    static int cmp(big a, big b)
    {
      if (a.n != b.n) return a.n > b.n ? +1 : -1;
      for (var i = a.n; i-- != 0; ) if (a.p[i] != b.p[i]) return a.p[i] > b.p[i] ? +1 : -1; return 0;
    }
    static int cmp(BigRational a, BigRational b)
    {
      var sa = (a.sign >> 31) - (-a.sign >> 31);
      var sb = (b.sign >> 31) - (-b.sign >> 31);
      if (sa != sb) return sa > sb ? +1 : -1;
      if (sa == 0) return 0;
      fixed (uint* pa = a.bits, pb = b.bits)
      {
        rat aa, bb; a.fix(&aa, pa); b.fix(&bb, pb);
        var sn = cmp(aa.num, bb.num);
        var sd = cmp(aa.den, bb.den);
        if (sd == 0) return +sn * sa;
        if (sn == 0) return -sd * sa;
        if (sn > 0 && sd < 0) return +sa;
        if (sd < 0 && sn > 0) return -sa;
        var m1 = aa.num.n + bb.den.n + 1;
        var m2 = aa.den.n + bb.num.n + 1;
        var p1 = stackalloc uint[m1 + m2]; var p2 = p1 + m1;
        var n1 = mul(aa.num, bb.den, p1);
        var n2 = mul(aa.den, bb.num, p2);
        return cmp(n1, n2) * sa;
      }
    }

    static double dbl(uint* p, int n)
    {
      if (n == 1) return p[0];
      if (n == 2) return *(ulong*)p;
      var man = ((ulong)p[n - 1] << 32) | p[n - 2];
      var exp = (n - 2) * 32; int cbit;
      if ((cbit = chz(p[n - 1])) > 0) { man = (man << cbit) | (p[n - 3] >> (32 - cbit)); exp -= cbit; }
      if (man == 0) return 0; ulong uu; int csh = chz(man) - 11;
      if (csh < 0) man >>= -csh; else man <<= csh; exp -= csh; exp += 1075;
      if (exp >= 0x7FF) uu = 0x7FF0000000000000;
      else if (exp <= 0) { exp--; if (exp < -52) uu = 0; else uu = man >> -exp; }
      else uu = (man & 0x000FFFFFFFFFFFFF) | ((ulong)exp << 52);
      return *(double*)&uu;
    }

    internal struct big
    {
      internal uint* p; internal int n;
      public override string ToString()
      {
        rat c; c.sign = 1; c.num = this; c.den.p = (uint*)&c.den.n; c.den.n = 1;
        Tmp t; t.p = &c; return ((BigRational)t).ToString();
      }
    }
    internal struct rat
    {
      internal int sign; internal big num, den; internal ulong tmp;
      public override string ToString()
      {
        rat c = this; Tmp t; t.p = &c;
        var p1 = stackalloc uint[num.n + den.n]; var p2 = p1 + num.n;
        for (int i = 0; i < num.n; i++) p1[i] = num.p[i]; c.num.p = p1;
        for (int i = 0; i < den.n; i++) p2[i] = den.p[i]; c.den.p = p2;
        return ((BigRational)t).ToString();
      }
    }

    void fix(rat* p, uint* t)
    {
      p->sign = (sign >> 31) - (-sign >> 31);
      if ((p->num.p = t) == null)
      {
        p->num.p = (p->den.p = (uint*)&p->tmp) + (p->num.n = p->den.n = 1);
        p->num.p[0] = ((uint)sign >> 16) & 0x7fff; p->den.p[0] = ((uint)sign & 0xffff) + 1; return;
      }
      p->num.n = sign & 0x7fffffff; p->den.n = bits.Length - p->num.n;
      if (p->den.n != 0) p->den.p = p->num.p + p->num.n; else { *(p->den.p = (uint*)&p->tmp) = 1; p->den.n = 1; }
    }
  }