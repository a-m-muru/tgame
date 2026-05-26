using Godot;

namespace scenes.region.ui;

public partial class TabMenu : Control {

	[Export] Button CloseButton;
	[Export] protected UI ui;


	public override void _Ready() {
		Debug.Assert(CloseButton != null, "need closebutton");
		Debug.Assert(ui != null, "need ui");
		CloseButton.Pressed += Close;
	}

	public void Close() {
		ui.SelectTab(UI.Tab.None);
        ui.state = UI.State.Idle;
	}

}