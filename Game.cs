using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class Game : Node3D
{
    public readonly string[] worldLayers = { "stone", "stone", "stone", "dirt", "dirt", "grass"};
    public static Block[] worldMap = { };
    public bool loaded = false;

    //build limits
    public static int xBoundMin = -16;
    public static int xBoundMax = 16;
    public static int zBoundMin = -16;
    public static int zBoundMax = 16;

    //highlights
    public Node3D PlaceHl;
    public Node3D BreakHl;
    public Node3D BreakHl2;
    public bool BlockDebug = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { //this fucking shit doesnt even work
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!loaded) {
            CreateWorld();

            //load blocks
            PlaceHl = (Node3D) GetNode("Blocks/block_place").Duplicate();
            BreakHl = (Node3D) GetNode("Blocks/block_break").Duplicate();
            BreakHl2 = (Node3D) GetNode("Blocks/block_highlight").Duplicate();
            GetParent().AddChild(PlaceHl);
            GetParent().AddChild(BreakHl);
            GetParent().AddChild(BreakHl2);

            loaded = true;
        }         

        HighlightBreak();
        if (BlockDebug) {
            HighlightPlace();
        }   
    }

    
    public override void _UnhandledInput(InputEvent @event)
    {
        //this is so you can place and break blocks
        if (@event.IsActionPressed("place"))
        {
            //place block   
            string blockId = Hotbar.GetSelectedItem();
            Vector3? blockPos = GetPlacePos();
            if (blockPos == null) { return; }

            PlaceBlock(blockId, (Vector3)blockPos);            
        }

        if (@event.IsActionPressed("interact"))
        {
            //break block   
            Vector3? blockPos = GetBreakPos();
            if (blockPos == null) { return; }

            BreakBlock((Vector3)blockPos);
        }
    }

    // Functions
    public static bool BreakBlock(Vector3 position) {
        //find block from worldMap

        Block block = null;
        foreach (Block worldBlock in worldMap)
        {
            if (worldBlock.Position == position)
            {
                block = worldBlock;
                break;
            }
        }

        if (block == null) {
            GD.PrintErr("BlockBreakError: No block found at " + position.ToString());
            return false;
        }

        //remove block from worldMap
        worldMap = worldMap.Where(b => b != block).ToArray();

        //destroy node
        block.Node.QueueFree();
        GD.Print("BlockBreak: delete " + block.BlockId + " at " + position.ToString());

        return true;
    }

    public bool PlaceBlock(string blockId, Vector3 position)
    {
        position = new Vector3(
            (float)Math.Round(position.X),
            (float)Math.Round(position.Y),
            (float)Math.Round(position.Z)
        );

        //validate position
        if (position.X < xBoundMin || position.X > xBoundMax || position.Z < zBoundMin || position.Z > zBoundMax)
        {
            GD.PrintErr("BlockPlaceError: Block position " + position.ToString() + " is out of bounds");
            return false;
        }   

        //get block node
        Node3D blockNode = (Node3D)GetNode("Blocks/" + blockId);
        if (blockNode == null)
        {
            GD.PrintErr("BlockPlaceError: Interactable item '" + blockId + "' is not placebale");
            return false;
        }

        //check if block already exists
        foreach (Block worldBlock in worldMap)
        {
            // GD.Print(worldBlock.Position.ToString() + " == " + position.ToString() + " Result:" + (worldBlock.Position == position).ToString());
            if (worldBlock.Position == position)
            {
                GD.PrintErr("BlockPlaceError: Block already exists at " + position.ToString());
                return false;
            }
        }

        //create block
        if (this.loaded) { GD.Print("BlockPlace: create '" + blockId + "' at " + position.ToString()); }

        Node3D block = (Node3D)blockNode.Duplicate();
        Block blockClass = new Block(blockId, block, position);
        worldMap = worldMap.Append(blockClass).ToArray();

        block.Position = position;
        GetParent().AddChild(block);
        return true;
    }

    public void CreateWorld()
    {
        for (int x = xBoundMin; x < xBoundMax; x++)
        {
            for (int z = zBoundMin; z < zBoundMax; z++)
            {
                for (int y = 0; y < worldLayers.Length; y++)
                {
                    //get block node from Blocks
                    PlaceBlock(worldLayers[y], new Vector3(x, y, z));
                }
            }
        }
    }

    public Vector3? GetPlacePos() {
        //raycast to find block position up to 5 blocks away
        Vector3 rayFrom = GetNode<Camera>("Player/TwistPivot/PitchPivot/Camera3D").ProjectRayOrigin(GetViewport().GetMousePosition());
        Vector3 rayTo = rayFrom + GetNode<Camera>("Player/TwistPivot/PitchPivot/Camera3D").ProjectRayNormal(GetViewport().GetMousePosition()) * 5;
        var spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(rayFrom, rayTo));

        // GD.Print(result);
        if (result.Count == 0) { return null; }

        Vector3 blockPos = (Vector3)result["position"];
        // blockPos += (Vector3) result["normal"];

        Vector3 RawPosition = new Vector3(
            Mathf.Round(blockPos.X),
            Mathf.Round(blockPos.Y),
            Mathf.Round(blockPos.Z)
        );

    
        //check if block is already there
        bool blockExists = false;
        foreach (Block worldBlock in worldMap)
        {
            if (worldBlock.Position == RawPosition)
            {
                blockExists = true;
                break;
            }
        }

        if (blockExists) {
            RawPosition += (Vector3) result["normal"];
        }

        return RawPosition;
    }

    public Vector3? GetBreakPos() {
        //raycast to find block position up to 5 blocks away
        var rayFrom = GetNode<Camera>("Player/TwistPivot/PitchPivot/Camera3D").ProjectRayOrigin(GetViewport().GetMousePosition());
        var rayTo = rayFrom + GetNode<Camera>("Player/TwistPivot/PitchPivot/Camera3D").ProjectRayNormal(GetViewport().GetMousePosition()) * 5;
        var spaceState = GetWorld3D().DirectSpaceState;
        var result = spaceState.IntersectRay(PhysicsRayQueryParameters3D.Create(rayFrom, rayTo));

        // GD.Print(result);
        if (result.Count == 0) {
            BreakHl.Visible = false;
            return null;
        }

        Vector3 blockPos = (Vector3)result["position"];
        // blockPos += (Vector3) result["normal"];

        //get nearest block to BlockPos
        Block nearestBlock = null;
        foreach (Block worldBlock in worldMap)
        {
            if (nearestBlock == null) {
                nearestBlock = worldBlock;
                continue;
            }

            if (worldBlock.Position.DistanceTo(blockPos) < nearestBlock.Position.DistanceTo(blockPos)) {
                nearestBlock = worldBlock;
            }
        }

        if (nearestBlock == null) {
            GD.PrintErr("BlockBreakError: No block found at " + blockPos.ToString());
            return null;
        }

        if (nearestBlock.Position.DistanceTo(blockPos) > 1  ) {
            return null;
        }

        //check if block is already there
        bool blockExists = false;
        foreach (Block worldBlock in worldMap)
        {
            if (worldBlock.Position == nearestBlock.Position)
            {
                blockExists = true;
                break;
            }
        }

        if (!blockExists) { return null; }

        return nearestBlock.Position;
    }

    public void HighlightPlace() {
        Vector3? Pos =  GetPlacePos();
        if (Pos == null) {
            PlaceHl.Visible = false;
            return;
        }

        PlaceHl.Visible = true;
        PlaceHl.Position = (Vector3)Pos;
    }

    public void HighlightBreak() {
        Node3D Hl = BreakHl2;
        if (BlockDebug) {
            Hl = BreakHl;
        }

        Vector3? Pos = GetBreakPos();
        if (Pos == null) {
            Hl.Visible = false;
            return;
        }

        Hl.Visible = true;
        Hl.Position = (Vector3)Pos;
    }
}