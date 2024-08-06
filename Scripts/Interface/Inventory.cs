using Godot;
using System;
using System.Linq;

public partial class Inventory : Control
{
    public bool IsOpen = false;
    public bool Loaded = false;
    private ItemList _itemList;

    public InventoryItem[] _inventoryItems = {};


    public static Inventory instance;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Visible != IsOpen) {Visible = IsOpen;}

        if (!Loaded) {
            //load inventory items
            _inventoryItems = (InventoryItem[])_inventoryItems.Append<InventoryItem>(new InventoryItem("Grass Block", BlockManager.Instance.Grass, "res://Textures/Blocks/grass_block_side.png", "grass_block")).ToArray();
            _inventoryItems = (InventoryItem[])_inventoryItems.Append<InventoryItem>(new InventoryItem("Dirt", BlockManager.Instance.Dirt, "res://Textures/Blocks/dirt.png")).ToArray();
            _inventoryItems = (InventoryItem[])_inventoryItems.Append<InventoryItem>(new InventoryItem("Cobblestone", BlockManager.Instance.Stone, "res://Textures/Blocks/cobblestone.png")).ToArray();        
            _inventoryItems = (InventoryItem[])_inventoryItems.Append<InventoryItem>(new InventoryItem("Wooden Planks", BlockManager.Instance.Wood, "res://Textures/Blocks/planks.png")).ToArray();        
            

            //load inventory list
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
