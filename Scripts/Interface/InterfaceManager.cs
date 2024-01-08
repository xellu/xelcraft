using System.ComponentModel;
using System.Runtime.CompilerServices;
using Godot;

public partial class InterfaceManager : Control  {
    public static bool ContainerOpen = false;
    public static InterfaceManager instance = new InterfaceManager();

    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    //hadle container toggles
    public void OpenContainer() {
        if (ContainerOpen) return;

        Input.MouseMode = Input.MouseModeEnum.Visible;
        ContainerOpen = true;
        GD.Print("InterfaceManager: Container opened");
    }

    public void CloseContainer() {
        if (!ContainerOpen) return;

        Input.MouseMode = Input.MouseModeEnum.Captured;
        ContainerOpen = false;
        GD.Print("InterfaceManager: Container closed");
    }

    public void SetContainer(bool state) {
        if (state) {
            OpenContainer();
            return;
        }

        CloseContainer();
    }

    

    //getters

    public bool IsContainerOpen() {
        return ContainerOpen;
    }
}