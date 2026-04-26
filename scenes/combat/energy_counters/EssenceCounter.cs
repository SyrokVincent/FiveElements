using System.Reflection;
using FiveElements.FiveElementsCode.Enums;
using FiveElements.FiveElementsCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Combat;
using BaseLib.Utils;
using FiveElements.FiveElementsCode;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace FiveElements.scenes.combat.energy_counters;
public partial class EssenceCounter : Control//, IOnElementStateChanged
{
	//todo finish this!!!
	//need to do better animation and better image, and manage to display hovertip at a good place
	/*
	public static readonly AddedNode<NEnergyCounter, Control> Node = new((energyCounter) =>
	{
		var scene = ResourceLoader.Load<PackedScene>("res://scenes/combat/energy_counters/elements_counter.tscn");
		var instance = scene.Instantiate<Control>();
		
		// 3. Optionnel : Si tu as besoin de faire un réglage global sur l'instance
		instance.Name = "GlobalEssenceContainer";
		GD.Print($"Injection sur {energyCounter.Name} réussie !");
		
		return instance;
		
	});
	
	*/
	private CardElementTag _myElement;
	private bool _isInitialized = false;
	private bool _hadEcho = false;
	
	private Player? _player;
	private Label? _label;
	private TextureRect _essence;
	private TextureRect _echo;
	private GpuParticles2D _essenceParticles;
	private Control _layersRef;
	
	private NEnergyCounter _parentCounter;
	
	private HoverTip _hoverTip;
	private float _tooltipOffsetY = -50f; // Ajuste cette valeur (négatif pour monter)
	
	private int _lastState = -1; // -1: initial, 0: inactif, 1: actif, 2: essence

// Une seule référence pour toutes les instances
	private static Tween _activeWaveTween;
	private static CardElementTag _currentAnimatingElement = CardElementTag.Neutral;
	
	
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
		_label = GetNodeOrNull<Label>("%EssenceLabel") ?? GetNodeOrNull<Label>("EssenceLabel");
		_essence = GetNodeOrNull<TextureRect>("%Essence") ?? GetNodeOrNull<TextureRect>("Essence");
		_echo = GetNodeOrNull<TextureRect>("%Echo") ?? GetNodeOrNull<TextureRect>("Echo");
		_essenceParticles = GetNodeOrNull<GpuParticles2D>("EssenceParticles");
		
		Node current = GetParent();
		while (current != null && current.Name != "FiveelementsEnergyCounter") // Ton root
		{
			current = current.GetParent();
		}
		_layersRef = current?.GetNodeOrNull<Control>("Layers");
		if (_layersRef?.Material != null)
		{
			// On ne duplique que si ce n'est pas déjà un ShaderMaterial unique
			// (Pour éviter que chaque orbe le fasse)
			if (_layersRef.Material is not ShaderMaterial sm || !sm.ResourceName.Contains("Unique"))
			{
				_layersRef.Material = (ShaderMaterial)_layersRef.Material.Duplicate();
				_layersRef.Material.ResourceName = "UniqueMaterial";
			}
		}
		
		if (_essence != null)
		{
			// On force le scale à presque rien pour éviter le flash "géant"
			_essence.Scale = new Vector2(0.1f, 0.1f);
			_essence.PivotOffset = _essence.Size / 2;
		}
		
		if (_essenceParticles != null)
		{
			_essenceParticles.ProcessMaterial = (ParticleProcessMaterial)_essenceParticles.ProcessMaterial.Duplicate();
	  
			// --- FIX DU BURST AU LANCEMENT ---
			// On force un état "calme" tout de suite avant le premier rendu
			var material = (ParticleProcessMaterial)_essenceParticles.ProcessMaterial;
			_essenceParticles.AmountRatio = 0.2f; // Très peu de particules
			_essenceParticles.Modulate = new Color(1, 1, 1, 0.3f); // Très transparent
			material.ScaleMin = 0.4f;
			material.Gravity = Vector3.Zero;
	  
			_essenceParticles.Emitting = true;
			_essenceParticles.Restart(); // On redémarre pour appliquer les changements proprement
		}
		
		
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
		
		RefreshAll();
	}
	
	private void RefreshAll()
	{
		RefreshLabel();   // Texte + Data
		RefreshVisuals(); // Tweens + Particules
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

		int count = status.GetEssence(_myElement);
		_label.Text = count <= 0 ? "" : count.ToString();
	
		// On met à jour l'objet HoverTip en mémoire sans l'afficher
		bool isActive = _myElement.IsActive(_player.Creature.CombatState);
		bool isEcho = FiveElements.FiveElementsCode.Character.FiveElements.Echo.Contains(_myElement);
		UpdateHoverTip(isActive, isEcho, count);
	}
	
	
	private void RefreshVisuals()
	{

		// On calcule les états : si le player est null, tout sera à false/0 par défaut
		bool isActive = _player != null && _player.Creature.CombatState != null && _myElement.IsActive(_player.Creature.CombatState);
		var status = _player?.Creature?.CombatState?.GetElementalStatus();
		int count = status?.GetEssence(_myElement) ?? 0;
		bool hasEssence = count > 0;
	
		bool isEcho = _player != null && FiveElements.FiveElementsCode.Character.FiveElements.Echo.Contains(_myElement);

		// 1. Gestion Echo (toujours accessible)
		if (_echo != null) _echo.Visible = isEcho;

		if (_layersRef?.Material is ShaderMaterial mat) 
		{
			// 1. DÉTECTION DE L'APPARITION (Ton code existant)
			if (isEcho && !_hadEcho) 
			{
				TriggerGlobalWave();
			}

			// 2. DÉTECTION DU RESET
			// Seul l'élément Fire s'en occupe pour ne pas lancer 5 ondes
			if (_myElement == CardElementTag.Fire)
			{
				var globalEcho = FiveElements.FiveElementsCode.Character.FiveElements.Echo;
				bool isGloballyNeutral = globalEcho.Count == 0 || (globalEcho.Count == 1 && globalEcho.Contains(CardElementTag.Neutral));

				// On regarde si le shader a encore une couleur (Alpha > 0)
				Color currentTarget = (Color)mat.GetShaderParameter("target_color");

				if (isGloballyNeutral && currentTarget.A > 0.01f)
				{
					TriggerNeutralWave();
				}
			}
		}
		
		// On met à jour la mémoire pour le prochain rafraîchissement
		_hadEcho = isEcho;
		
		// 2. Calcul de l'état
		int currentState = hasEssence ? 2 : (isActive ? 1 : 0);
		
		
		// On ne bloque le rafraîchissement que si on a déjà un player ET que l'état n'a pas changé
		if (currentState == _lastState && _player != null) return; 
		_lastState = currentState;

		
	
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
		
		
		if (_essenceParticles != null)
		{
			var tween = CreateTween().SetParallel(true);
			var material = (ParticleProcessMaterial)_essenceParticles.ProcessMaterial;

			float targetScale;
			float targetAlpha;
			float targetRatio;
			float targetGravity;

			if (hasEssence) // État Max : Puissant
			{
				targetScale = 1.1f;
				targetAlpha = 1.0f;
				targetRatio = 1.0f;
				targetGravity = 0f;
			}
			else if (isActive) // État Moyen : Actif
			{
				targetScale = 0.9f;
				targetAlpha = 0.7f;
				targetRatio = 0.8f;
				targetGravity = 0f;
			}
			else // État Mini : Inactif (Petit mais présent)
			{
				targetScale = 0.4f;
				targetAlpha = 0.5f;
				targetRatio = 0.6f;
				targetGravity = 0f;
			}

			// Animation de la transition
			tween.TweenProperty(_essenceParticles, "amount_ratio", targetRatio, 0.5f);
			tween.TweenProperty(_essenceParticles, "modulate:a", targetAlpha, 0.5f);
	
			// Pour tweener les propriétés du matériau, on passe par Set
			// Note : On peut aussi tweener directement l'échelle globale si on veut simplifier
			tween.TweenProperty(material, "scale_min", targetScale, 0.5f);
			tween.TweenProperty(material, "scale_max", targetScale + 0.1f, 0.5f);
			tween.TweenProperty(material, "gravity", new Vector3(0, targetGravity, 0), 0.5f);
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
	
	private void OnEssenceChanged(CardElementTag cardElementTag, int newValue, PlayerChoiceContext? context) => RefreshAll(); 
	private void OnCombatStateChanged(CombatState combatState) => RefreshAll();
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


////////
 

	private void TriggerGlobalWave()
	{
		if (_layersRef?.Material is not ShaderMaterial mat) return;

		// 1. Calcul de la position relative réelle (sans clamp)
		Vector2 myCenterGlobal = GlobalPosition + (Size / 2);
		Vector2 localPos = _layersRef.MakeCanvasPositionLocal(myCenterGlobal);
	
		// 2. On obtient l'UV réel (qui peut être -0.1 ou 1.1)
		Vector2 centerUV = localPos / _layersRef.Size;

		// On envoie le vrai centre, même s'il est un peu en dehors. 
		// Le shader s'en sortira mieux qu'avec un "0" brutal.
		mat.SetShaderParameter("center", centerUV);

		// --- LE RESTE EST IDENTIQUE ---
		Vector2 screenPos = _layersRef.GetGlobalTransformWithCanvas().Origin;
		mat.SetShaderParameter("parent_screen_pos", screenPos);
		mat.SetShaderParameter("parent_size", _layersRef.Size);
	
		Color oldColor = (Color)mat.GetShaderParameter("target_color");
		mat.SetShaderParameter("base_color", oldColor);
		mat.SetShaderParameter("target_color", GetElementColor());
		mat.SetShaderParameter("radius", 0.0f);

		_activeWaveTween?.Kill();
		_activeWaveTween = CreateTween();
		_activeWaveTween.TweenProperty(mat, "shader_parameter/radius", 2.0f, 0.8f) // Radius un peu plus grand
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
	}
/*
	private void TriggerGlobalWave()
{
   if (_layersRef?.Material is not ShaderMaterial mat) return;

   // 1. On arrête l'animation en cours immédiatement
   if (_activeWaveTween != null && _activeWaveTween.IsRunning())
   {
	   _activeWaveTween.Kill();
   }

   // 2. Setup des couleurs
   Color oldColor = (Color)mat.GetShaderParameter("target_color");
   mat.SetShaderParameter("base_color", oldColor);
   
   // 3. Setup du centre propre à CETTE orbe
   Vector2 relativePos = GlobalPosition - _layersRef.GlobalPosition;
   Vector2 size = _layersRef.Size;
   Vector2 centerUV = (relativePos + (Size / 2)) / size;

   mat.SetShaderParameter("center", centerUV);
   mat.SetShaderParameter("target_color", GetElementColor());
   mat.SetShaderParameter("radius", 0.0f);
   mat.SetShaderParameter("feather", 0.15f); // On remet un feather propre

   // 4. Lancement
   _activeWaveTween = CreateTween();
   _activeWaveTween.TweenProperty(mat, "shader_parameter/radius", 1.5f, 0.8f)
	  .SetTrans(Tween.TransitionType.Cubic)
	  .SetEase(Tween.EaseType.Out);
}*/

private void TriggerNeutralWave()
{
   if (_layersRef?.Material is not ShaderMaterial mat) return;

   // Si on est déjà en train d'aller vers le neutre, on ne fait rien
   Color currentTarget = (Color)mat.GetShaderParameter("target_color");
   Color neutralColor = new Color(0.05f, 0.05f, 0.08f, 0.8f);
   if (currentTarget.IsEqualApprox(neutralColor)) return;

   if (_activeWaveTween != null && _activeWaveTween.IsRunning()) _activeWaveTween.Kill();

   Color oldColor = (Color)mat.GetShaderParameter("target_color");
   mat.SetShaderParameter("base_color", oldColor);

   mat.SetShaderParameter("center", new Vector2(0.5f, 0.5f));
   mat.SetShaderParameter("target_color", neutralColor);
   mat.SetShaderParameter("radius", 0.0f);
   mat.SetShaderParameter("feather", 0.3f); // Plus doux pour le noir

   _activeWaveTween = CreateTween();
   _activeWaveTween.TweenProperty(mat, "shader_parameter/radius", 1.5f, 1.2f)
	  .SetTrans(Tween.TransitionType.Cubic)
	  .SetEase(Tween.EaseType.Out);
}
	
	
	private Color GetElementColor()
	{
		return _myElement switch
		{
			CardElementTag.Water => new Color(FiveElementsColor.WaterColor),
			CardElementTag.Wood  => new Color(FiveElementsColor.WoodColor),
			CardElementTag.Fire  => new Color(FiveElementsColor.FireColor),
			CardElementTag.Earth => new Color(FiveElementsColor.EarthColor),
			CardElementTag.Metal => new Color(FiveElementsColor.MetalColor),
			_ => Colors.White
		};
	}



}
