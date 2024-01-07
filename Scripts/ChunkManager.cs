using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class ChunkManager : Node
{
	public static ChunkManager Instance { get; private set; }

	private Dictionary<Chunk, Vector2I> _chunkToPosition = new();
	private Dictionary<Vector2I, Chunk> _positionToChunk = new();
	
	public Dictionary<Vector2I, Block[,,]> _oldChunk = new();


	private List<Chunk> _chunks;

	[Export] public PackedScene ChunkScene;

	private int _width = 10;

	private Vector3 _playerPosition;
	private object _playerPositionLock = new();
	
	public override void _Ready()
	{
		Instance = this;

		_chunks = GetParent().GetChildren().OfType<Chunk>().ToList();

		for (int i = _chunks.Count; i < _width * _width; i++)
		{
			var chunk = ChunkScene.Instantiate<Chunk>();
			GetParent().CallDeferred(Node.MethodName.AddChild, chunk);
			_chunks.Add(chunk);
		}
		
		for(int x = 0; x< _width; x++)
		{
		
			for(int y = 0; y< _width; y++)
			{
				var index = (y * _width) + x;
				var halfWidth = Mathf.FloorToInt(_width / 2f);
				_chunks[index].SetChunkPosition(new Vector2I(x - halfWidth, y - halfWidth));
			}	
		}

		if (!Engine.IsEditorHint())
		{
			new Thread(new ThreadStart(ThreadProcess)).Start();
		}
		
	}

	public void UpdateChunkPosition(Chunk chunk, Vector2I currentPosition, Vector2I previousPosition)
	{
		if (_positionToChunk.TryGetValue(previousPosition, out var chunkAtPosition) && chunkAtPosition == chunk)
		{
			_positionToChunk.Remove(previousPosition);
		}

		_chunkToPosition[chunk] = currentPosition;
		_positionToChunk[currentPosition] = chunk;
	}

	public void SetBlock(Vector3I globalPosition, Block block)
	{
		var chunkTilePosition = new Vector2I(Mathf.FloorToInt(globalPosition.X / (float)Chunk.dimensions.X),
			Mathf.FloorToInt(globalPosition.Z / (float)Chunk.dimensions.Z));

		lock (_positionToChunk)
		{
			if (_positionToChunk.TryGetValue(chunkTilePosition, out var chunk))
			{
				chunk.SetBlock((Vector3I)(globalPosition - chunk.GlobalPosition), block);
			}
		}
	}

	private void ThreadProcess()
	{
		while (IsInstanceValid(this))
		{
			int playerChunkX, playerChunkZ;
			lock (_playerPositionLock)
			{
				playerChunkX = Mathf.FloorToInt(_playerPosition.X / Chunk.dimensions.X);
				playerChunkZ = Mathf.FloorToInt(_playerPosition.Z / Chunk.dimensions.Z);
			}

			foreach (var chunk in _chunks)
			{
				var chunkPosition = _chunkToPosition[chunk];

				var chunkX = chunkPosition.X;
				var chunkZ = chunkPosition.Y;

				var newChunkX = (int)(Mathf.PosMod(chunkX - playerChunkX + _width / 2f, _width) + playerChunkX -
									  _width / 2f);
				var newChunkZ = (int)(Mathf.PosMod(chunkZ - playerChunkZ + _width / 2f, _width) + playerChunkZ -
									  _width / 2f);

				if (newChunkX != chunkX || newChunkZ != chunkZ)
				{
					lock (_positionToChunk)
					{
						if (_positionToChunk.ContainsKey(chunkPosition))
						{
							_positionToChunk.Remove(chunkPosition);

							GD.Print($"Unloading chunk at {newChunkX} {newChunkZ}");
						}

						var newPosition = new Vector2I(newChunkX, newChunkZ);
						_chunkToPosition[chunk] = newPosition;
						_positionToChunk[newPosition] = chunk;

						chunk.CallDeferred(nameof(Chunk.SetChunkPosition), newPosition);
					}
					Thread.Sleep(100);
				}
			}
			Thread.Sleep(1);

		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (!Engine.IsEditorHint())
		{
			lock (_playerPositionLock)
			{
				_playerPosition = Player.Instance.GlobalPosition;
			}
		}
	}
}
