using System;
using Godot;

namespace scenes.region.ui {

	public partial class JobSlider : VBoxContainer {

		static readonly PackedScene Packed = GD.Load<PackedScene>("res://scenes/region/ui/job_slider.tscn");

		[ExportGroup("Nodes")]
		[Export] Label NameLabel;
		[Export] Label MoneyLabel;
		[Export] Slider Slider;

		[Export] Container AddRemoveHotkeys;
		[Export] Button AddHotkey;
		[Export] Button RemoveHotkey;

		public bool Editable {
			get => Slider.Editable;
			set => Slider.Editable = value;
		}

		string unitSymbol;
		bool round;
		bool enableHotkeys;
		int fastAddCount = 1;
		bool _ready;
		Action<int, float> valueChangedCallback;
		int jobIx;


		public static JobSlider Instantiate() => Packed.Instantiate<JobSlider>();

		public override void _Ready() {
			Slider.ValueChanged += ValueChanged;
			Slider.DragEnded += DragEnded;
			AddHotkey.Pressed += OnPlusOne;
			RemoveHotkey.Pressed += OnMinusOne;
			_ready = true;
			//GD.Print("JobSlider::_Ready : ready");
		}

		public override void _UnhandledKeyInput(InputEvent ev) {
			if (ev is not InputEventKey iev) return;
			if (!enableHotkeys) return;
			if (iev.Keycode == Key.Shift && iev.Pressed) fastAddCount = 10;
			if (iev.Keycode == Key.Shift && !iev.Pressed) fastAddCount = 1;
			UpdateButtonsDisplay();
		}

		public override void _Notification(int what) {
			if (what == NotificationPredelete) {
				_ready = false;
				Slider.ValueChanged -= ValueChanged;
				Slider.DragEnded -= DragEnded;
				AddHotkey.Pressed -= OnPlusOne;
				RemoveHotkey.Pressed -= OnMinusOne;
			}
		}

		public void Setup(Action<int, float> valueChangedCallback, int jobIx, float currentValue, string name, float valueMax, string unitSymbol, bool round = true, float minValue = 0f, bool enableHotkeys = false) {
			Debug.Assert(_ready, "dont anything before we're ready");
			this.jobIx = jobIx;
			this.round = round;
			this.enableHotkeys = enableHotkeys;
			this.valueChangedCallback = valueChangedCallback;
			NameLabel.Text = name;
			Slider.MinValue = minValue;
			Slider.MaxValue = valueMax;
			Slider.Value = currentValue;
			Slider.Editable = valueMax != 0;
			lastValue = round ? (int)Slider.Value : (float)Slider.Value;
			this.unitSymbol = unitSymbol;
			MoneyLabel.Text = "" + lastValue + unitSymbol;
			if (!enableHotkeys) {
				AddRemoveHotkeys.Hide();
			}
		}

		//public int GetValue() {
		//	Debug.Assert(_ready, "dont anything before we're ready");
		//
		//	// Slider.Rounded needs to be set in the editor
		//	return (int)Slider.Value;
		//}

		void ValueChanged(double to) {
			float val = (float)to;
			if (round) val = (float)Mathf.Round(Slider.Value);

			MoneyLabel.Text = "" + (round ? (int)val : val) + unitSymbol;
			AddHotkey.Disabled = val >= Slider.MaxValue;
			RemoveHotkey.Disabled = val <= Slider.MinValue;
			Slider.SetValueNoSignal(val);
			//ValueChangedCallback(jobIx, val);
		}

		float lastValue = 0;
		void DragEnded(bool valueChanged) {
			if (!valueChanged) return;
			float val = (float)Slider.Value;
			if (round) val = Mathf.RoundToInt(Slider.Value);
			GD.Print($"JobSlider::DragEnded : val {val} last {lastValue}");
			ValueChangedCallback(jobIx, val - lastValue);
			lastValue = val;
		}

		void ValueChangedCallback(int ix, float to) => valueChangedCallback(ix, to);

		public void Disable() {
			Slider.Editable = false;
		}

		void OnPlusOne() {
			Debug.Assert(Slider.Value < Slider.MaxValue);
			Slider.Value = Mathf.Min(Slider.Value + fastAddCount, Slider.MaxValue);
			DragEnded(true);
		}

		void OnMinusOne() {
			Debug.Assert(Slider.Value > Slider.MinValue);
			Slider.Value = Mathf.Max(Slider.Value - fastAddCount, Slider.MinValue);
			DragEnded(true);
		}

		void UpdateButtonsDisplay() {
			AddHotkey.Text = $"+{fastAddCount} (E)";
			RemoveHotkey.Text = $"-{fastAddCount} (Q)";
		}

	}

}

