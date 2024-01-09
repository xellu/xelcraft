using Godot;
using System;
using System.Collections.Generic;

public partial class Hotbar : Control
{
    public int _activeSlot = 0;
    private int _slotAmount = 4; //has to be 1 less than the amount of slots in the hotbar
    private int[] slotPositions = {-90, -45, 0, 45, 90};
    private Dictionary<int, InventoryItem> _items = new();
    private TextureRect ActiveSlot;
    private bool Loaded = false; 

    public static Hotbar instance;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (!Loaded) {
            ActiveSlot = GetNode<TextureRect>("Active");
            Loaded = true;
        }

        ActiveSlot.Position = new Vector2(slotPositions[_activeSlot], ActiveSlot.Position.Y);
        for (int i = 0; i < _slotAmount+1; i++) {
            var node = GetNode<TextureRect>("Slot" + i);
            if (_items.ContainsKey(i)) {
                node.Texture = _items[i].Texture2;
            } else {
                node.Texture = null;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("hotbar_left")) {
            if (_activeSlot == 0) {
                _activeSlot = _slotAmount;
            } else {
                _activeSlot--;
            }
        }

        if (@event.IsActionPressed("hotbar_right")) {
            if (_activeSlot == _slotAmount) {
                _activeSlot = 0;
            } else {
                _activeSlot++;
            }
        }
    }

    public InventoryItem GetActiveItem() {
        try {
            return _items[_activeSlot];
        } catch (KeyNotFoundException) {
            return null;
        }
    }

    public void SetSlot(int slot, InventoryItem item) {
        instance._items[slot] = item;
        GD.Print("Hotbar: Set slot " + slot + " to " + item.Name);
    }
}
