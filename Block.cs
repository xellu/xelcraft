using Godot;

public class Block {
    public string BlockId;
    public Node3D Node;

    public Vector3 Position;

    public Block(string blockId, Node3D node, Vector3 position) {
        this.BlockId = blockId;
        this.Node = node;
        this.Position = position;
    }    
}