using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameTest.Scripts;

public partial class ChunkManager : Node
{
	public static ChunkManager Instance { get; private set; }

	private Dictionary<Chunk, Vector2I> _chunkToPosition = new();
	private Dictionary<Vector2I, Chunk> _positionToChunk = new();
	
	public Dictionary<Vector2I, Block[,,]> _oldChunk = new();


	private List<Chunk> _chunks;

	[Export] public PackedScene ChunkScene;

	private int _width = 5;

	private Vector3 _playerPosition;
	private object _playerPositionLock = new();
	
	public override void _Ready()
	{
		Instance = this;

		_chunks = GetParent().GetChildren().OfType<Chunk>().ToList();

		PreGenerateWorld();
		
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
			new Thread(ThreadProcess).Start();
		}
		
	}

	private void PreGenerateWorld()
	{
		Chunk chunk = ChunkScene.Instantiate<Chunk>();;
		int radius = 20;
		
		for (int x = -radius; x < radius; x++)
		{
			for (int y = -radius; y < radius;y++)
			{
				var chunkPos = new Vector2I(x, y);
				_oldChunk[chunkPos] = Generator.Generate(chunk.Noise, chunkPos);
			}
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
				var halfWidth = _width / 2f;
				var playerChunkXAdjusted = playerChunkX - halfWidth;
				var playerChunkZAdjusted = playerChunkZ - halfWidth;
				
				var newChunkX = (int)(Mathf.PosMod(chunkX - playerChunkX + halfWidth, _width) + playerChunkXAdjusted);
				var newChunkZ = (int)(Mathf.PosMod(chunkZ - playerChunkZ + halfWidth, _width) + playerChunkZAdjusted);
				
				var newPosition = new Vector2I(newChunkX, newChunkZ);

				Task.Run(() => chunk.UpdateBlocks());
				
				if (newChunkX != chunkX || newChunkZ != chunkZ)
				{
					lock (_positionToChunk)
					{
						if (_positionToChunk.ContainsKey(chunkPosition))
						{
							_positionToChunk.Remove(chunkPosition);
						}
						
						chunk.CallDeferred(nameof(Chunk.SetChunkPosition), newPosition);
						Thread.Sleep(70);
					}
				}
			}
			Thread.Sleep(1);
		}
	}
	
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
