using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody3D
{
	// player rotation
	
	[Export]
	public float Speed = 5f;
	[Export]
	public float SprintFactor = 1.5f;
	[Export]
	public float MouseSensitivity = 20f;

	[Export] public Camera3D _camera;
	
	// player movement
	public const float JumpVelocity = 4.5f;

	public static Player Instance { get; private set; }
	
	[Export] public RayCast3D RayCast;

	[Export] public MeshInstance3D BlockHighlight;

	private Vector3 _direction = new Vector3();
	

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		Instance = this;
	}

	public override void _Process(double delta)
	{

		if (RayCast.IsColliding() && RayCast.GetCollider() is Chunk chunk)
		{
			BlockHighlight.Visible = true;

			var blockPosition = RayCast.GetCollisionPoint() - 0.5f * RayCast.GetCollisionNormal();
			var intBlockPosition = new Vector3I(Mathf.FloorToInt(blockPosition.X), Mathf.FloorToInt(blockPosition.Y),
				Mathf.FloorToInt(blockPosition.Z));

			BlockHighlight.GlobalPosition = intBlockPosition + new Vector3(0.5f, 0.5f, 0.5f);
			BlockHighlight.Rotation =  -this.Rotation;

            if (Input.MouseMode == Input.MouseModeEnum.Captured) { //disable interacting if in a container
                if (Input.IsActionJustPressed("destroy"))
                {
                    ChunkManager.Instance.SetBlock((Vector3I)(intBlockPosition), BlockManager.Instance.Air);
                }

                if (Input.IsActionJustPressed("interact"))
                {
                    var item = Hotbar.instance.GetActiveItem();
                    if (item == null) return;
                    ChunkManager.Instance.SetBlock((Vector3I)(intBlockPosition + RayCast.GetCollisionNormal()), (Block)item.ItemInstance);

                }
            }
		}
		else
		{
			BlockHighlight.Visible = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		//tp to spawn
		if (this.Position.Y < -10) {
			this.Position = new Vector3(0, 100, 0);
		}
		
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
        _direction = _direction.Lerp((Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(), 0.5f);
        
        if (_direction != Vector3.Zero)
        {
            velocity.X = _direction.X * Speed;
            velocity.Z = _direction.Z * Speed;
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
	
	public override void _Input(InputEvent @event)
	{
		//exit if on menu 
		if (Input.MouseMode != Input.MouseModeEnum.Captured) {return;}

		// process rotation
		if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;
			_camera.RotateX(Mathf.DegToRad(-mouseEvent.Relative.Y * (MouseSensitivity / 100f)));
			RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * (MouseSensitivity / 100f)));

			Vector3 cameraRot = _camera.RotationDegrees;
			cameraRot.X = Mathf.Clamp(cameraRot.X, -89, 89);
			_camera.RotationDegrees = cameraRot;
		}

		//reset
		if (@event.IsActionPressed("ui_filedialog_refresh")) {
			this.Position = new Vector3(0, 100, 0);
		}
	}
}
