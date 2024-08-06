using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

	private ConcurrentDictionary<Vector3, Block> _blockQueue = new();

	public void SetChunkPosition(Vector2I position)
	{
		CallThreadSafe(Node3D.MethodName.Hide); //Hide the chunk while we update it
		_blocks = new Block[dimensions.X, dimensions.Y, dimensions.Z]; //Clear the blocks
		
		ChunkManager.Instance.UpdateChunkPosition(this, position, ChunkPosition); //Update the chunk position in the chunk manager
		ChunkPosition = position; //Set the chunk position
		CallDeferred(Node3D.MethodName.SetGlobalPosition,
			new Vector3(ChunkPosition.X * dimensions.X, 0, ChunkPosition.Y * dimensions.Z));
		if (ChunkManager.Instance._oldChunk.TryGetValue(position, out var value))
			_blocks = value;
		else
		{
			Generate();
		}
		Task.Run(() => Update(true)); //Update the chunk in a thread
	}

	public void Generate()
	{
		_blocks = Generator.Generate(Noise, ChunkPosition);

		ChunkManager.Instance._oldChunk.Remove(ChunkPosition);
		ChunkManager.Instance._oldChunk[ChunkPosition] = _blocks;

		GD.Print($"Generated chunk at X: {ChunkPosition.X} Z: {ChunkPosition.Y}");
	}

	public void Update(bool threadSafe = false)
	{
		//This is really stupid, but it works
		if (!threadSafe)
		{
			_surfaceTool = new();
			_surfaceTool.Clear();
		}
		
		_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
		lock (_blocks)
		{
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
		}
		BuildMesh(threadSafe);
	}

	private void BuildMesh(bool threadSafe)
	{
		_surfaceTool.SetMaterial(BlockManager.Instance.ChunkMaterial);
		var mesh = _surfaceTool.Commit();
		
		//Count the faces, this is really slowing the game down :(
		int faces = 0;
		Godot.Collections.Array a = mesh.SurfaceGetArrays((int) Mesh.ArrayType.Vertex);
		foreach(Godot.Collections.Array array in a)
		{
				faces += array.Count;
		}

		if(threadSafe) //Calling this from a thread causes a crash
			CallThreadSafe(nameof(UpdateCollisionMesh), mesh, true, faces);
		else
			UpdateCollisionMesh(mesh, faces:faces);
	}

	private void UpdateCollisionMesh(Mesh mesh, bool threadSafe = false, int faces = 0)
	{
		if (faces % 3 != 0) //How does this even happen??
		{
			GD.PrintErr("Updating collision mesh failed, mesh rebuild required");
			if (threadSafe)
			{
				Update();
				return;
			}

			Task.Run((() => Update(true)));
			return;
		}
		

		MeshInstance.Mesh = mesh;
		CollisionShape.Shape = mesh.CreateTrimeshShape();

		if (!Visible)
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
		var texturePosition = BlockManager.Instance.GetTextureAtlasPosition(texture);
		var textureAtlasSize = BlockManager.Instance.TextureAtlasSize;

		var uvOffset = texturePosition / textureAtlasSize;
		var uvWidth = 1f / textureAtlasSize.X;
		var uvHeight = 1f / textureAtlasSize.Y;

		var uvA = uvOffset + new Vector2(0, 0);
		var uvB = uvOffset + new Vector2(0, uvHeight);
		var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
		var uvD = uvOffset + new Vector2(uvWidth, 0);

		var a = _vertices[face[0]] + blockPosition;
		var b = _vertices[face[1]] + blockPosition;
		var c = _vertices[face[2]] + blockPosition;
		var d = _vertices[face[3]] + blockPosition;

		var uvTriangle1 = new[] { uvA, uvB, uvC };
		var uvTriangle2 = new[] { uvA, uvC, uvD };

		var triangle1 = new Vector3[] { a, b, c };
		var triangle2 = new Vector3[] { a, c, d };

		var normal = ((Vector3)(c - a)).Cross(b - a).Normalized();
		var normals = new[] { normal, normal, normal };

		AddTriangles(triangle1, uvTriangle1, normals);
		AddTriangles(triangle2, uvTriangle2, normals);
	}

	public void UpdateBlocks()
	{
		if(_blockQueue.Count < 1)
			return;
		
		foreach(Vector3 vec in _blockQueue.Keys)
		{
			lock (_blocks)
			{
				_blocks[(int)vec.X, (int)vec.Y, (int)vec.Z] = _blockQueue[vec];
			}
		}

		_blockQueue.Clear();
		
		Task.Run((() => Update(true)));
		//CallThreadSafe(nameof(Update), true);
	}
	
	private void AddTriangles(Vector3[] vertices, Vector2[] uvs, Vector3[] normals)
	{
		_surfaceTool.AddTriangleFan(vertices, uvs, normals: normals);
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
		_blockQueue[blockPosition] = block;
		//UpdateBlocks();
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
