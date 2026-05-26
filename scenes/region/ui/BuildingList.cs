using System;
using System.Linq;
using System.Text;
using Godot;
using resources.game;
using resources.game.building_types;
using scenes.region;
using scenes.region.ui;
using scenes.ui;
using static Building;

namespace scenes.region.ui;

public partial class BuildingList : TabMenu {

	static readonly BuildingType[] ORDER = [
		(BuildingType)Registry.BuildingsS.GrainField,
		(BuildingType)Registry.BuildingsS.LogCabin,
		(BuildingType)Registry.BuildingsS.Marketplace,
		(BuildingType)Registry.BuildingsS.Sawmill,
		(BuildingType)Registry.BuildingsS.Housing,
		(BuildingType)Registry.BuildingsS.Quarry,
		(BuildingType)Registry.BuildingsS.Well,
		(BuildingType)Registry.BuildingsS.Windmill,
		(BuildingType)Registry.BuildingsS.Carpentry,
		(BuildingType)Registry.BuildingsS.MudPit,
		(BuildingType)Registry.BuildingsS.Kiln,
		(BuildingType)Registry.BuildingsS.BrickHousing,
		(BuildingType)Registry.BuildingsS.Bakery,
		(BuildingType)Registry.BuildingsS.CharcoalPit,
		(BuildingType)Registry.BuildingsS.Bloomeries,
		(BuildingType)Registry.BuildingsS.Smithy,
		(BuildingType)Registry.BuildingsS.Tower,
		(BuildingType)Registry.BuildingsS.Barracks,
	];

	[Export] ItemList itemList;
	[Export] RichTextLabel descriptionText;
	[Export] RichTextLabel resourceListText;

	long selectedBuildThingId = -1;
	MapObjectView selectedBuildingScene = null;
	IBuildingType _selectedBuildingType = null;
	public IBuildingType SelectedBuildingType {
		get {
			//Debug.PrintWithStack("GET sel build type: ", _selectedBuildingType);
			return _selectedBuildingType;
		}
		set {
			_selectedBuildingType = value;
			//Debug.PrintWithStack("SET sel build type: ", _selectedBuildingType);
		}
	}


	public override void _Ready() {
		base._Ready();
		itemList.ItemActivated += OnBuildThingConfirmed;
		itemList.ItemSelected += OnBuildThingSelected;
	}

	public override void _GuiInput(InputEvent evt) {
		if (evt is InputEventMouseButton) {
			GetViewport().SetInputAsHandled();
		}
	}

	void OnBuildThingSelected(long which) {
		selectedBuildThingId = which;
		var btype = (BuildingType)itemList.GetItemMetadata((int)which).Obj;
		var desc = new StringBuilder();
		if (btype.GetPopulationCapacity() > 0) desc.Append($"+ {btype.GetPopulationCapacity()} population cap\n");
		if (btype.GetMilitaryBoost() > 0) desc.Append($"+ {btype.GetMilitaryBoost()} military\n");
		desc.Append(btype.GetDescription());
		descriptionText.Text = desc.ToString();
		resourceListText.Text = "";
		var resources = ui.GetResources();
		bool requiresMaterials = false;
		foreach (var r in btype.GetConstructionResources()) {
			requiresMaterials = true;
			var str = r.ToString();
			if (!resources.HasEnough(r)) {
				str = $"[color={Palette.BrownRust.ToHtml()}]" + str + "[/color]";
			}
			resourceListText.AppendText(str + '\n');
		}
		if (!requiresMaterials) resourceListText.Text = "this building requires\nno materials to build";

		OnBuildThingConfirmed(selectedBuildThingId);
	}

	void OnBuildThingConfirmed() {
		// pressed button, didnt doubleclick
		OnBuildThingConfirmed(selectedBuildThingId);
	}

	void OnBuildThingConfirmed(long which) {
		var btype = (BuildingType)itemList.GetItemMetadata((int)which).Obj;
		if (!ui.GetHasBuildingMaterials(btype)) return;
		SetBuildCursor(btype);
		//selectedBuildThingId = -1;
	}

	public void SetBuildCursor(IBuildingType buildingType) {
		if (buildingType == null) {
			if (selectedBuildingScene != null) {
				selectedBuildingScene.QueueFree();
				SelectedBuildingType = null;
			}
			SelectedBuildingType = null;
			selectedBuildingScene = null;
			return;
		} else {
			if (IsInstanceValid(selectedBuildingScene) && !selectedBuildingScene.IsQueuedForDeletion()) selectedBuildingScene.QueueFree();
			selectedBuildingScene = null;
		}
		Debug.Assert(buildingType != null, "Buuldint type canät be null here....w aht...");
		Debug.Assert(ui.GetHasBuildingMaterials(buildingType), "can't build this...");
		var scene = MapObjectView.MakeDisplay(DataStorage.GetScenePath(buildingType), buildingType);
		Debug.Assert(scene != null, "building display scene canät be null here....w aht...");

		ui.Camera.Cursor.AddChild(scene);
		selectedBuildingScene = scene;
		SelectedBuildingType = buildingType;
		ui.state = UI.State.PlacingBuild;
		selectedBuildingScene.Modulate = new Color(selectedBuildingScene.Modulate, 0.67f);
	}

	public void UpdateCursorWhilePlacing(Vector2I regpos) {
		bool buildable = ui.GetHasBuildingMaterials(SelectedBuildingType);
		buildable = buildable && ui.GetCanBuild(SelectedBuildingType, regpos);
		Color modColor;
		if (!buildable) modColor = Palette.BrownRust;
		else modColor = Palette.WhiteSmoke;

		selectedBuildingScene.Modulate = new(modColor, 0.67f);
	}

	public void Update() {
		itemList.Clear();
		var items = ORDER.ToList();
		var canmake = items.Where(j => ui.GetHasBuildingMaterials(j));
		var cantmake = items.Where(j => !ui.GetHasBuildingMaterials(j));
		foreach (var buildingType in canmake) {
			int ix = itemList.AddItem(buildingType.AssetName);
			// storing buildingtype references locally so if we happen to update the buildingtypes list
			// in between calls here, we should still get the correct buildings that the visual
			// ItemList was set up with
			itemList.SetItemMetadata(ix, Variant.CreateFrom(buildingType));
			itemList.SetItemCustomFgColor(ix, Palette.WhiteSmoke);
			itemList.SetItemCustomBgColor(ix, new(Palette.LunarGreen, 0.75f));
		}
		foreach (var buildingType in cantmake) {
			int ix = itemList.AddItem(buildingType.AssetName);
			itemList.SetItemMetadata(ix, Variant.CreateFrom(buildingType));
			itemList.SetItemCustomFgColor(ix, Palette.StormDust);
		}
	}

	public void Reset() {
		selectedBuildThingId = -1;
		resourceListText.Text = "";
		descriptionText.Text = "";
		itemList.Clear();
	}

}
