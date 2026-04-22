using System.Reflection;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using BaseLib;
using BaseLib.Utils;
using FiveElements.FiveElementsCode.Interfaces;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace FiveElements.scenes.combat.energy_counters;
public partial class EssenceCounter : Control//, IOnElementStateChanged
{
	//todo finish this!!!
	//need to do better animation and better image, and manage to display hovertip at a good place
	
	public static readonly AddedNode<NEnergyCounter, Control> Node = new((energyCounter) =>
	{
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/combat/energy_counters/elements_counter.tscn");
		var instance = scene.Instantiate<Control>();
		
		// 3. Optionnel : Si tu as besoin de faire un réglage global sur l'instance
		instance.Name = "GlobalEssenceContainer";
		GD.Print($"Injection sur {energyCounter.Name} réussie !");
		
		return instance;
		
	});
	
	private CardElementTag _myElement;
	private bool _isInitialized = false;
	
	private Player? _player;
	private Label? _label;
	private TextureRect _essence;
	private TextureRect _echo;
	private NEnergyCounter _parentCounter;
	
	private HoverTip _hoverTip;
	private float _tooltipOffsetY = -50f; // Ajuste cette valeur (négatif pour monter)



	
	
	public override void _Ready()
	{
		/*
		GD.Print($"--- Inventaire des enfants de {Name} ---");
		foreach (Node child in GetChildren())
		{
			GD.Print($"Nom: {child.Name} | Type: {child.GetType()}");
		}
		*/
		
		// On récupère les nodes
		// Le % ne fonctionne que si "Access as Unique Name" est coché dans l'éditeur Godot
		// On ajoute donc une recherche par nom direct au cas où
		_label = GetNodeOrNull<Label>("%Label") ?? GetNodeOrNull<Label>("Label");
		_essence = GetNodeOrNull<TextureRect>("%Essence") ?? GetNodeOrNull<TextureRect>("Essence");
		_echo = GetNodeOrNull<TextureRect>("%Echo") ?? GetNodeOrNull<TextureRect>("Echo");

		if (_essence == null) {
			GD.PrintErr($"[FiveElements] ERREUR CRITIQUE : _essence est null pour {Name} !");
		}
	
		// Déterminer l'élément en fonction du nom du parent dans la hiérarchie
		string parentName = GetParent().Name;
	
		_myElement = parentName switch
		{
			"WaterLayer"  => CardElementTag.Water,
			"WoodLayer" => CardElementTag.Wood,
			"FireLayer" => CardElementTag.Fire,
			"EarthLayer" => CardElementTag.Earth,
			"MetalLayer" => CardElementTag.Metal,
		};
		
		if (_label != null) _label.Text = "0";
		
		// On initialise une HoverTip par défaut très simple pour éviter qu'elle soit null
		// On utilise la clé du Feu par défaut, elle sera écrasée au premier Refresh
		_hoverTip = new HoverTip(
			new LocString("static_hover_tips", "FIVEELEMENTS-TITLE_FIRE"), 
			""
		);
		// On connecte les signaux de la souris
		MouseEntered += OnHovered;
		MouseExited += OnUnhovered;
		
		RefreshLabel();
	}
	
	
	public override void _EnterTree()
	{
		base._EnterTree();
		// On ne touche pas au label ici, car il n'est pas encore assigné via GetNode
	
		// On vérifie si l'instance du combat existe avant de s'abonner
		if (CombatManager.Instance?.StateTracker != null)
		{
			CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
		}
	}
	
	
	public override void _ExitTree()
	{
		base._ExitTree();
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
	
		// Désabonnement sécurisé
		if (_player?.Creature?.CombatState != null)
		{
			var elementalStatus = _player.Creature.CombatState.GetElementalStatus();
			if (elementalStatus != null)
				elementalStatus.EssenceChanged -= OnEssenceChanged;
		}
	}
	public override void _Process(double delta)
	{
		if (_player != null) return;

		// On cherche le parent NEnergyCounter pour récupérer le joueur
		Node current = GetParent();
		while (current != null && current is not NEnergyCounter)
		{
			current = current.GetParent();
		}

		if (current is NEnergyCounter energyCounter)
		{
			var field = energyCounter.GetType().GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);
			if (field?.GetValue(energyCounter) is Player p)
			{
				InitializeWithPlayer(p);
			}
		}
	}
	

	private void InitializeWithPlayer(Player p)
	{
		if (_player != null) return; // Sécurité supplémentaire
		_player = p;
	
		GD.Print("[FiveElements] Player trouvé et initialisé ");
		// On s'abonne MAINTENANT que le player est trouvé
		var elementalStatus = _player.Creature.CombatState?.GetElementalStatus();
		if (elementalStatus != null)
		{
			elementalStatus.EssenceChanged += OnEssenceChanged;
		}
	
		RefreshLabel();
	}
	
	
	private void RefreshLabel()
	{
		
		
		if (_label == null || _player?.Creature?.CombatState == null) return;

		var status = _player.Creature.CombatState.GetElementalStatus();
		if (status == null) return;

		// --- UTILISATION DE TA LOGIQUE DE CARTES ---
		// On utilise l'extension que tu as définie pour savoir si l'élément est "Actif"
		bool isActive = _myElement.IsActive(_player.Creature.CombatState);
		bool isEcho = FiveElements.FiveElementsCode.Character.FiveElements.Echo.Contains(_myElement);
		int count = status.GetEssence(_myElement);

		// --- MISE À JOUR DE L'INFOBULLE ---
		UpdateHoverTip(isActive, isEcho, count);
		
		// 1. Texte (Rien si 0)
		_label.Text = count <= 0 ? "" : count.ToString();

		// 2. Visuel de l'icône Essence
		if (_essence != null)
		{
			var tween = CreateTween();
			
			//I need that for the scaling to not move it
			_essence.PivotOffset = _essence.Size / 2;
			// Si actif : Taille normale (1.0), sinon réduit (0.5)
			Vector2 targetScale = isActive ? Vector2.One : new Vector2(0.5f, 0.5f);
			tween.TweenProperty(_essence, "scale", targetScale, 0.25f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Back);
			//
			// Si actif : Lumineux (White), sinon assombri/transparent
			//Color targetColor = isActive ? Colors.White : new Color(0.3f, 0.3f, 0.3f, 0.6f);
			//tween.Parallel().TweenProperty(_essence, "modulate", targetColor, 0.25f);
			//
		}
		
		// 3. Gestion de l'Echo (Optionnel)
		// Si l'élément est actif à cause de l'Echo, tu peux allumer un effet spécial
		if (_echo != null)
		{
			_echo.Visible = isEcho;
		}
		
	}
	private void UpdateHoverTip(bool active, bool echo, int essenceCount)
	{
		if (_player == null) return;

		// 1. Titre
		string titleKey = $"FIVEELEMENTS-TITLE_{_myElement.ToString().ToUpper()}";
		LocString title = new LocString("static_hover_tips", titleKey);

		// 2. Description Maîtresse
		LocString desc = new LocString("static_hover_tips", "FIVEELEMENTS-DESC_MASTER");

		// 3. Injection des variables (On initialise tout à vide pour éviter l'erreur SmartFormat)
		string activeVal = "";
		string echoVal = "";
		string essenceVal = "";
		string inactiveVal = "";

		if (active) 
		{
			activeVal = new LocString("static_hover_tips", "FIVEELEMENTS-LINE_ACTIVE").GetFormattedText();
		}

		if (echo)
		{
			echoVal = new LocString("static_hover_tips", "FIVEELEMENTS-LINE_ECHO").GetFormattedText();
		}

		if (essenceCount > 0)
		{
			LocString essenceLine = new LocString("static_hover_tips", "FIVEELEMENTS-LINE_ESSENCE");
			essenceLine.Add("count", essenceCount);
			essenceVal = essenceLine.GetFormattedText();
		}
		else if (!active)
		{
			inactiveVal = new LocString("static_hover_tips", "FIVEELEMENTS-LINE_INACTIVE").GetFormattedText();
		}

		// On remplit OBLIGATOIREMENT toutes les balises du master desc
		desc.Add("activeDesc", activeVal);
		desc.Add("echoDesc", echoVal);
		desc.Add("essenceDesc", essenceVal);
		desc.Add("inactiveDesc", inactiveVal);

		_hoverTip = new HoverTip(title, desc);
	}
	
	// --- MÉTHODES DE SURVOL ---
	
	
	private void OnHovered() 
	{
		if (_player == null || _hoverTip == null) return;
		RefreshLabel();
		
		NHoverTipSet.Remove(this);
		var tooltip = NHoverTipSet.CreateAndShow(this, _hoverTip, HoverTipAlignment.Right);
	}
	
	/*
		// 3. Ajustement manuel de la position
		if (tooltip is Control tooltipControl)
		{
			tooltipControl.GlobalPosition = GlobalPosition + new Vector2(Size.X + 10f, _tooltipOffsetY);
		}*/
	
	
	
	/*
	private void OnHovered() 
	{
		if (_player == null || _hoverTip == null) return;
		RefreshLabel();

		// 1. Nettoyage
		NHoverTipSet.Remove(this);

		// 2. Récupération de tes coordonnées réelles
		Vector2 myGlobalPos = GlobalPosition;
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 mySize = GetGlobalRect().Size;

		// 3. Affichage de l'infobulle
		var tooltip = NHoverTipSet.CreateAndShow(this, _hoverTip, HoverTipAlignment.Right);

		if (tooltip is Control tooltipControl)
		{
			// 4. Calcul de la nouvelle position (30px à droite de l'icône, et aligné en hauteur)
			Vector2 targetPos = new Vector2(myGlobalPos.X + mySize.X + 10, myGlobalPos.Y - 20);
        
			tooltipControl.GlobalPosition = targetPos;

			// 5. PRINT DE DEBUG
			GD.Print($"--- DEBUG HOVER [{_myElement}] ---");
			GD.Print($"Position Icône (Global): {myGlobalPos}");
			GD.Print($"Position Souris: {mousePos}");
			GD.Print($"Position Cible Tooltip: {targetPos}");
			GD.Print($"Taille du Control: {mySize}");
			GD.Print("-----------------------------------");
		}
		else 
		{
			GD.PrintErr($"[FiveElements] Échec: Le tooltip créé pour {_myElement} n'est pas un Control !");
		}
	}*/
	

	
	
	private void OnUnhovered() 
	{
		NHoverTipSet.Remove(this);
	}
	
	private void OnEssenceChanged(CardElementTag cardElementTag, int newValue, PlayerChoiceContext? context) => RefreshLabel(); 
	private void OnCombatStateChanged(CombatState combatState) => RefreshLabel();
/*
	public async Task OnElementStateChanged(CardElementTag element, bool isActive)
	{
		// On ne réagit que si le changement concerne NOTRE élément
		if (element == _myElement)
		{
			// On rafraîchit l'UI (RefreshLabel s'occupera du visuel On/Off)
			RefreshLabel();
		}
		await Task.CompletedTask;
	}*/
}
