using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	// player rotation
	public const float sensitivity = 4f;
	public static float yaw = 0f;
	public static float pitch = 0f;
	
	// player movement
	public const float Speed = 5.0f;
	public const float SprintFactor = 1.5f;
	public const float JumpVelocity = 4.5f;
	public static float X = 0f;
	public static float Y = 0f;
	public static float Z = 0f;
	public static bool teleport = false;

	public static Player Instance { get; private set; }
	
	[Export] public RayCast3D RayCast;

	[Export] public MeshInstance3D BlockHighlight;
	

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		Instance = this;
		Input.MouseMode = Input.MouseModeEnum.Captured; 
	}

	public override void _Process(double delta)
	{
		if (teleport) {
			this.Position = new Vector3(X, Y, Z);
			teleport = false;
		}

		if (RayCast.IsColliding() && RayCast.GetCollider() is Chunk chunk)
		{
			BlockHighlight.Visible = true;

			var blockPosition = RayCast.GetCollisionPoint() - 0.5f * RayCast.GetCollisionNormal();
			var intBlockPosition = new Vector3I(Mathf.FloorToInt(blockPosition.X), Mathf.FloorToInt(blockPosition.Y),
				Mathf.FloorToInt(blockPosition.Z));

			BlockHighlight.GlobalPosition = intBlockPosition + new Vector3(0.5f, 0.5f, 0.5f);
            BlockHighlight.Rotation =  -this.Rotation;

			if (Input.IsActionJustPressed("interact"))
			{
				ChunkManager.Instance.SetBlock((Vector3I)(intBlockPosition), BlockManager.Instance.Air);
			}

			if (Input.IsActionJustPressed("place"))
			{
				ChunkManager.Instance.SetBlock((Vector3I)(intBlockPosition + RayCast.GetCollisionNormal()), BlockManager.Instance.Stone);

			}
		}
		else
		{
			BlockHighlight.Visible = false;
		}
		
		
		X = this.Position.X;
		Y = this.Position.Y;
		Z = this.Position.Z;
	}

	public override void _PhysicsProcess(double delta)
	{
		//tp to spawn
		if (this.Position.Y < -10) {
			this.Position = new Vector3(0, 10, 0);
			yaw = 0f;
			pitch = 0f;
		}

		// exit to menu
		if (Input.IsActionJustPressed("ui_cancel")) {
			if (Input.MouseMode == Input.MouseModeEnum.Captured) {
				Input.MouseMode = Input.MouseModeEnum.Visible;
				return;
			}
			
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		//exit if in menu
		if (Input.MouseMode != Input.MouseModeEnum.Captured) {return;} 
		
		// process movements
		
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
			velocity.Y = JumpVelocity;


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		// Handle sprint.
		if (Input.IsActionPressed("move_sprint") && !Input.IsActionPressed("sneak"))
		{
			velocity.X *= SprintFactor;
			velocity.Z *= SprintFactor;
		}

		if (Input.IsActionPressed("sneak"))
		{
			velocity.X /= SprintFactor;
			velocity.Z /= SprintFactor;
			Scale = new Vector3(1, 0.5f, 1);
		} else {
			Scale = new Vector3(1, 1, 1);
		}

		Velocity = velocity;        
		MoveAndSlide();
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		//exit if on menu 
		if (Input.MouseMode != Input.MouseModeEnum.Captured) {return;}

		// process rotation
		if (@event is InputEventMouseMotion mouseEvent)
		{
			yaw -= mouseEvent.Relative.X * (sensitivity/1000);
			pitch -= mouseEvent.Relative.Y * (sensitivity/1000);
			
			if (pitch > 1.5f) {pitch = 1.5f;}
			if (pitch < -1.5f) {pitch = -1.5f;}

			Vector3 PlayerRotation = new Vector3(0, yaw, 0);
			this.Rotation = PlayerRotation;            
		}

		//reset
		if (@event.IsActionPressed("ui_filedialog_refresh")) {
			this.Position = new Vector3(0, 100, 0);
			yaw = 0f;
			pitch = 0f;
		}
	}
}
