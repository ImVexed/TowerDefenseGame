using Godot;
using System;

public class damage_number : Position2D
{
	Label label;
	Tween tween;

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
		{ DamageType.Physical, Color.ColorN("gray")},
		{ DamageType.Fire, Color.ColorN("red")},
		{ DamageType.Cold, Color.ColorN("blue")},
		{ DamageType.Lightning, Color.ColorN("yellow")},
		{ DamageType.Poison, Color.ColorN("green")},
		{ DamageType.Void, Color.ColorN("purple")}
	};

	public override void _Ready()
	{
		GD.Randomize();

		label = GetNode<Label>("Label");
		tween = GetNode<Tween>("Tween");

		label.Text = Amount.ToString();

		velocity -= new Vector2((float)GD.RandRange(0, 50), 50);

		// Find the highest damage magnitude we surpass
		var mag = Magnitudes.Last(x => Amount > x.Breakpoint);

		label.AddColorOverride("font_color", DamageColors[Type]);

		// Scale up to full size in 0.2 seconds, then after 100ms scale back down to 0.1
		tween.InterpolateProperty(this, "scale", Scale, new Vector2(mag.Scale, mag.Scale), 0.2f, Tween.TransitionType.Linear, Tween.EaseType.Out);
		tween.InterpolateProperty(this, "scale", new Vector2(mag.Scale, mag.Scale), new Vector2(0.1f, 0.1f), 0.7f, Tween.TransitionType.Linear, Tween.EaseType.Out, 0.3f);
		tween.Connect("tween_all_completed", this, "TweenAllCompleted");
		tween.Start();
	}

	public void TweenAllCompleted()
	{
		QueueFree();
	}

	public override void _Process(float delta)
	{
		Position -= velocity * delta;
	}


}

public struct Magnitude
{
	public float Breakpoint;
	public float Scale;
}
