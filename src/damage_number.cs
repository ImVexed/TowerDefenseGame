using Godot;
using System;

public partial class damage_number : Marker2D
{
	Label? label;

	[Export] public float Amount = 0;
	[Export] public DamageType Type;

	Vector2 velocity = Vector2.Zero;

	static Magnitude[] Magnitudes = {
		new Magnitude{Breakpoint=0,Scale=1},
		new Magnitude{Breakpoint=50,Scale=1.2f},
		new Magnitude{Breakpoint=100,Scale=1.5f},
		new Magnitude{Breakpoint=200,Scale=1.7f},
		new Magnitude{Breakpoint=400,Scale=2.0f},
		new Magnitude{Breakpoint=800,Scale=2.5f}
	};

	public enum DamageType
	{
		Physical,
		Fire,
		Cold,
		Lightning,
		Poison,
		Void
	}

	static Dictionary<DamageType, Color> DamageColors = new()
	{
		{ DamageType.Physical, Colors.Gray},
		{ DamageType.Fire, Colors.Red},
		{ DamageType.Cold, Colors.Blue},
		{ DamageType.Lightning, Colors.Yellow},
		{ DamageType.Poison, Colors.Green},
		{ DamageType.Void, Colors.Purple}
	};

	public override void _Ready()
	{
		GD.Randomize();

		label = GetNode<Label>("Label");
		var tween = new Tween();

		label.Text = Amount.ToString();

		velocity -= new Vector2((float)GD.RandRange(0, 50), 50);

		// Find the highest damage magnitude we surpass
		var mag = Magnitudes.Last(x => Amount > x.Breakpoint);

		label.AddThemeColorOverride("font_color", DamageColors[Type]);

		// Scale up to full size in 0.2 seconds, then after 100ms scale back down to 0.1
		tween.SetTrans(Tween.TransitionType.Linear);
		tween.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(this, new NodePath("scale"), new Vector2(mag.Scale, mag.Scale), 0.2f);
		tween.TweenProperty(this, new NodePath("scale"), new Vector2(0.1f, 0.1f), 0.7f);
		tween.TweenInterval(0.3f);
		tween.TweenCallback(new Callable(this,"TweenAllCompleted"));
		tween.Play();
	}

	public void TweenAllCompleted()
	{
		QueueFree();
	}

	public override void _Process(double delta)
	{
		//TODO delta is a double so we had to cast it to a float to multiply it by velocity which is a Vector2. 
		var rDelta = (float) delta;
		Position -= velocity * rDelta;
	}

}

public struct Magnitude
{
	public float Breakpoint;
	public float Scale;
}
