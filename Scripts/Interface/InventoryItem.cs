using Godot;

public class InventoryItem {
    public string Name { get; set; }
    public Texture2D Texture2 { get; set; }
    public Texture Texture3 { get; set; }
    public bool IsBlock { get; set; }
    public int Amount { get; set; }
    public object ItemInstance { get; set; }

    public InventoryItem(string name, object item, string texturePath, string iconPath=null, bool is2D=true,
                        bool isBlock = true, int amount = 1) {
        Name = name;
        ItemInstance = item;
        IsBlock = isBlock;
        Amount = amount;

        if (is2D) { //load 2d texture
            Texture2 = (Texture2D)GD.Load(texturePath);
        } else {
            Texture2 = (Texture2D)GD.Load(iconPath);
            Texture3 = (Texture)GD.Load(texturePath);
        }
    }
}