using Godot;
using System;

public partial class Inventory : Control
{
    public bool IsOpen = false;
    public bool Loaded = false;
    private ItemList _itemList;

    public InventoryItem[] _inventoryItems = {
        new InventoryItem("Grass Block", BlockManager.Instance.Grass, "res://Textures/Blocks/grass_block_side.png", "grass_block"),
        new InventoryItem("Dirt", BlockManager.Instance.Dirt, "res://Textures/Blocks/dirt.png"),
        new InventoryItem("Cobblestone", BlockManager.Instance.Stone, "res://Textures/Blocks/cobblestone.png")
        
    };

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Visible != IsOpen) {Visible = IsOpen;}

        if (!Loaded) {
            //load inventory items
            _itemList = GetNode<ItemList>("ItemList");
            _itemList.Clear();
            foreach (InventoryItem item in _inventoryItems) {
                _itemList.AddItem(item.Name, item.Texture2);
            }

            GD.Print("Inventory: Loaded inventory items");
            Loaded = true;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("container_toggle")) {
            IsOpen = !IsOpen;
            InterfaceManager.instance.SetContainer(IsOpen);
        }
    }
}
