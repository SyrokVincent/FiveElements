using Godot;

namespace FiveElements.scenes.combat.energy_counters;
public partial class EssenceCounter : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}


/*
// star counter model 
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

public class NStarCounter : Control
{
	private Player? _player;
	private MegaRichTextLabel _label;
	private Control _rotationLayers;
	private ShaderMaterial _hsv;
	
	// Pour l'animation fluide du chiffre
	private float _lerpingCount;
	private float _velocity;
	private int _displayedCount;
	
	private HoverTip _hoverTip;

	public override void _Ready()
	{
		// Récupération des nodes via le nom unique (%)
		_label = GetNode<MegaRichTextLabel>("%CountLabel");
		_rotationLayers = GetNode<Control>("%RotationLayers");
		_hsv = (ShaderMaterial)GetNode<Control>("Icon").Material;

		// Configuration de l'infobulle
		LocString desc = new LocString("static_hover_tips", "STAR_COUNT.description");
		_hoverTip = new HoverTip(new LocString("static_hover_tips", "STAR_COUNT.title"), desc);

		// Signaux de survol
		MouseEntered += OnHovered;
		MouseExited += OnUnhovered;
		
		Visible = false;
	}

	public void Initialize(Player player)
	{
		_player = player;
		// On s'abonne aux changements de stats du combat
		_player.PlayerCombatState.StarsChanged += OnStarsChanged;
		RefreshVisibility();
	}

	public override void _Process(double delta)
	{
		if (_player == null) return;

		// 1. Animation de rotation des couches de l'icône
		float speed = _player.PlayerCombatState.Stars == 0 ? 5f : 30f;
		for (int i = 0; i < _rotationLayers.GetChildCount(); i++)
		{
			_rotationLayers.GetChild<Control>(i).RotationDegrees += (float)delta * speed * (i + 1);
		}

		// 2. Lissage du nombre (SmoothDamp)
		_lerpingCount = MathHelper.SmoothDamp(_lerpingCount, _player.PlayerCombatState.Stars, ref _velocity, 0.1f, (float)delta);
		UpdateText(Mathf.RoundToInt(_lerpingCount));
	}

	private void UpdateText(int count)
	{
		if (_displayedCount == count) return;
		_displayedCount = count;
		
		// Change la couleur si on est à 0
		_label.AddThemeColorOverride("font_color", count == 0 ? StsColors.red : StsColors.cream);
		_label.Text = $"[center]{count}[/center]";
	}

	private void OnStarsChanged(int oldS, int newS) => RefreshVisibility();

	private void RefreshVisibility()
	{
		if (_player == null) return;
		// Visible si > 0 ou si le perso doit TOUJOURS l'afficher
		Visible = _player.Character.ShouldAlwaysShowStarCounter || _player.PlayerCombatState.Stars > 0;
	}

	private void OnHovered() => NHoverTipSet.CreateAndShow(this, _hoverTip);
	private void OnUnhovered() => NHoverTipSet.Remove(this);
}





*/
