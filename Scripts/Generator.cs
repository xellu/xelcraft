using Godot;

namespace GameTest.Scripts;

public class Generator
{
    public static Block[,,] Generate(Noise noise, Vector2I chunkPosition)
    {
        var dimensions = Chunk.dimensions;
        var blocks = new Block[dimensions.X, dimensions.Y, dimensions.Z];

        for (int x = 0; x < dimensions.X; x++)
        {
            for (int y = 0; y < dimensions.Y; y++)
            {
                for (int z = 0; z < dimensions.Z; z++)
                {
                    Block block;

                    var globalBlockPosition =
                        chunkPosition * new Vector2I(dimensions.X, dimensions.Z) + new Vector2(x, z);
                    var groundHeight = (int)(dimensions.Y *
                                             ((noise.GetNoise2D(globalBlockPosition.X, globalBlockPosition.Y) + 1f) /
                                              2f));

                    if (y < groundHeight / 2)
                    {
                        block = BlockManager.Instance.Stone;
                    }
                    else if (y < groundHeight)
                    {
                        block = BlockManager.Instance.Dirt;
                    }
                    else if (y == groundHeight)
                    {
                        block = BlockManager.Instance.Grass;
                    }
                    else
                    {
                        block = BlockManager.Instance.Air;
                    }

                    blocks[x, y, z] = block;
                }
            }
        }

        return blocks;
    }
}