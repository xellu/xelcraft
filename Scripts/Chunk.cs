using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameTest.Scripts;
using Godot;

public partial class Chunk : StaticBody3D
{
	[Export] public CollisionShape3D CollisionShape { get; set; }

	[Export] public MeshInstance3D MeshInstance { get; set; }

	public static Vector3I dimensions = new Vector3I(16, 64, 16);

	private static readonly Vector3I[] _vertices =
	{
		new Vector3I(0, 0, 0),
		new Vector3I(1, 0, 0),
		new Vector3I(0, 1, 0),
		new Vector3I(1, 1, 0),
		new Vector3I(0, 0, 1),
		new Vector3I(1, 0, 1),
		new Vector3I(0, 1, 1),
		new Vector3I(1, 1, 1)
	};

	private static readonly int[] _top = { 2, 3, 7, 6 };
	private static readonly int[] _bottom = { 0, 4, 5, 1 };
	private static readonly int[] _left = { 6, 4, 0, 2 };
	private static readonly int[] _right = { 3, 1, 5, 7 };
	private static readonly int[] _back = { 7, 5, 4, 6 };
	private static readonly int[] _front = { 2, 0, 1, 3 };

	private SurfaceTool _surfaceTool = new();

	private Block[,,] _blocks = new Block[dimensions.X, dimensions.Y, dimensions.Z];
	public Vector2I ChunkPosition { get; private set; }
	[Export] public FastNoiseLite Noise { get; set; }
	
	public void SetChunkPosition(Vector2I position)
	{
		_blocks = new Block[dimensions.X, dimensions.Y, dimensions.Z];
		
		ChunkManager.Instance.UpdateChunkPosition(this, position, ChunkPosition);
		ChunkPosition = position;  
		CallDeferred(Node3D.MethodName.SetGlobalPosition,
			new Vector3(ChunkPosition.X * dimensions.X, 0, ChunkPosition.Y * dimensions.Z));
		if (ChunkManager.Instance._oldChunk.TryGetValue(position, out var value))
			_blocks = value;
		else
		{
			Generate();
		}
		//CallDeferred(nameof(Update));
		Task.Run(Update);
	}

	public void Generate()
	{
		_blocks = Generator.Generate(Noise, ChunkPosition);

		ChunkManager.Instance._oldChunk.Remove(ChunkPosition);
		ChunkManager.Instance._oldChunk[ChunkPosition] = _blocks;

		GD.Print($"Generated chunk at X: {ChunkPosition.X} Z: {ChunkPosition.Y}");
	}

	public void Update()
	{
		CallDeferred(Node3D.MethodName.Hide);
		_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
		for (int x = 0; x < dimensions.X; x++)
		{
			for (int y = 0; y < dimensions.Y; y++)
			{
				for (int z = 0; z < dimensions.Z; z++)
				{
					CreateBlockMesh(new Vector3I(x, y, z));
				}
			}
		}

		_surfaceTool.SetMaterial(BlockManager.Instance.ChunkMaterial);
		var mesh = _surfaceTool.Commit();

		CallDeferred(nameof(UpdateCollisionMesh), mesh);
	}
	
	private void UpdateCollisionMesh(ArrayMesh mesh)
	{
		MeshInstance.Mesh = mesh;
		CollisionShape.Shape = mesh.CreateTrimeshShape();
		Show();
	}

	private void CreateBlockMesh(Vector3I blockPosition)
	{
		var block = _blocks[blockPosition.X, blockPosition.Y, blockPosition.Z];

		if (block == BlockManager.Instance.Air) return;
		if(block == null) return;
		
		if (CheckTransparent(blockPosition + Vector3I.Up))
		{
			CreateFaceMesh(_top, blockPosition, block.TopTexture ?? block.Texture);
		}

		if (CheckTransparent(blockPosition + Vector3I.Down))
		{
			CreateFaceMesh(_bottom, blockPosition, block.BottomTexture ?? block.Texture);
		}

		if (CheckTransparent(blockPosition + Vector3I.Left))
		{
			CreateFaceMesh(_left, blockPosition, block.Texture);
		}

		if (CheckTransparent(blockPosition + Vector3I.Right))
		{
			CreateFaceMesh(_right, blockPosition, block.Texture);
		}

		if (CheckTransparent(blockPosition + Vector3I.Forward))
		{
			CreateFaceMesh(_front, blockPosition, block.Texture);
		}

		if (CheckTransparent(blockPosition + Vector3I.Back))
		{
			CreateFaceMesh(_back, blockPosition, block.Texture);
		}
	}

	private void CreateFaceMesh(int[] face, Vector3I blockPosition, Texture2D texture)
	{
		var blockManager = BlockManager.Instance;
		var texturePosition = blockManager.GetTextureAtlasPosition(texture);
		var textureAtlasSize = blockManager.TextureAtlasSize;

		var uvOffset = texturePosition / textureAtlasSize;
		var uvWidth = 1f / textureAtlasSize.X;
		var uvHeight = 1f / textureAtlasSize.Y;

		var blockPositionOffset = blockPosition;

		var uvA = uvOffset + Vector2.Zero;
		var uvB = uvOffset + new Vector2(0, uvHeight);
		var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
		var uvD = uvOffset + new Vector2(uvWidth, 0);

		var vertices = _vertices;
		var a = vertices[face[0]] + blockPositionOffset;
		var b = vertices[face[1]] + blockPositionOffset;
		var c = vertices[face[2]] + blockPositionOffset;
		var d = vertices[face[3]] + blockPositionOffset;

		var uvTriangle1 = new Vector2[] { uvA, uvB, uvC };
		var uvTriangle2 = new Vector2[] { uvA, uvC, uvD };

		var normal = ((Vector3)(c - a)).Cross((b - a)).Normalized();
		var normals = new Vector3[] { normal, normal, normal };

		_surfaceTool.AddTriangleFan(new Vector3[] { a, b, c }, uvTriangle1, normals: normals);
		_surfaceTool.AddTriangleFan(new Vector3[] { a, c, d }, uvTriangle2, normals: normals);
	}

	private bool CheckTransparent(Vector3I blockPosition)
	{
		if (blockPosition.X < 0 || blockPosition.X >= dimensions.X) return true;
		if (blockPosition.Y < 0 || blockPosition.Y >= dimensions.Y) return true;
		if (blockPosition.Z < 0 || blockPosition.Z >= dimensions.Z) return true;

		return _blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] == BlockManager.Instance.Air;
	}

	public void SetBlock(Vector3I blockPosition, Block block)
	{
		_blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] = block;
		Update();
	}


	public Block[,,] GetBlocks()
	{
		return _blocks;
	}

	public void SetBlocks(Block[,,] oldChunk)
	{
		_blocks = oldChunk;
		Update();
	}
}
