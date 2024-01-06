using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
public partial class Hotbar : Node2D
{
    public static int selectedSlot = 0;
    public const int selectedSlotMax = 4;
    public static readonly Vector2[] selectedPos = { new Vector2(-52, 0), new Vector2(-26, 0), new Vector2(0, 0), new Vector2(26, 0), new Vector2(52, 0) };
    public static readonly string[] items = {"grass", "dirt", "glass", "rocks", "light_bulb"};

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //center crosshair
        this.Position = new Vector2(GetWindow().Size.X / 2, GetWindow().Size.Y-50);
        
        //update selected slot
        Sprite2D sprite = (Sprite2D)GetChild(1);
        
        sprite.Position = selectedPos[selectedSlot];

        //render items
        for (int i = 0; i <= 7; i++) { //no idea how it works
            if (GetChildCount() < 3 + selectedSlotMax) {
                //add item
                //create a node
                Node2D itemNode = new Node2D {
                    Name = items[i],
                    Position = selectedPos[i]
                };
                Sprite2D itemSprite = new Sprite2D {
                    Texture = (Texture2D) ResourceLoader.Load("res://assets/" + items[i] + ".png"),
                    Position = new Vector2(0, 0),
                    Scale = new Vector2(0.1f, 0.1f)
                };

                itemNode.AddChild(itemSprite);
                AddChild(itemNode);
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        //scroll in hotbar
        if (@event.IsActionPressed("hotbar_left")) {
            if (selectedSlot > 0) {
                selectedSlot--;
            } else {
                selectedSlot = selectedSlotMax;
            }
        } else if (@event.IsActionPressed("hotbar_right")) {
            if (selectedSlot < selectedSlotMax) {
                selectedSlot++;
            } else {
                selectedSlot = 0;
            }
        }

    }

    public static string GetSelectedItem() {
        return items[selectedSlot];
    }

}
