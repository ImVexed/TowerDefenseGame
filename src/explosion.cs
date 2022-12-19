using Godot;
using System;

public partial class explosion : Area2D
{

	public OnHitCallback? OnHitCallback;
	public Delegate? OnFreeCallback;

	bool hasPhysicsProcessed = false;
	bool hasAnimationEnded = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Connect("body_entered", new Callable(this, "BodyEntered"));
		GetNode("AnimatedSprite2D").Connect("animation_finished", new Callable(this, "OnAnimationFinished"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (hasPhysicsProcessed && hasAnimationEnded)
		{	
			CallDeferred("queue_free");
			OnFreeCallback?.DynamicInvoke();
		}
	}

	public void OnAnimationFinished()
	{
		hasAnimationEnded = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		hasPhysicsProcessed = true;
	}

	public new void BodyEntered(Node body)
	{
		if (body is not creep)
			return;

		var c = body as creep;

		OnHitCallback?.Invoke(c!);
	}
}
