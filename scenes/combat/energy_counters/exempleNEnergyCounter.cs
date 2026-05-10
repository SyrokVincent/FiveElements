namespace FiveElements.scenes.combat.energy_counters;



/*
public partial class exempleNEnergyCounter : Control
{
    private const string DarkenedMatPath = "res://materials/ui/energy_orb_dark.tres";
    private Player _player;
    private MegaLabel _label;
    private Control _layers;
    private Control _rotationLayers;
    private NParticlesContainer? _backVfx;
    private NParticlesContainer? _frontVfx;
    private HoverTip _hoverTip;
    
    private Tween? _animInTween;
    private Tween? _animOutTween;
    
    private static readonly Vector2 ShowPosition = Vector2.Zero;
    private static readonly Vector2 HidePosition = new Vector2(-480f, 128f);

    public override void _Ready()
    {
        // Initialisation des références aux Nodes du .tscn
        _label = GetNode<MegaLabel>("Label");
        _layers = GetNode<Control>("%Layers");
        _rotationLayers = GetNode<Control>("%RotationLayers");
        _backVfx = GetNodeOrNull<NParticlesContainer>("%EnergyVfxBack");
        _frontVfx = GetNodeOrNull<NParticlesContainer>("%EnergyVfxFront");

        // Configuration de l'infobulle (Tooltip)
        LocString description = new LocString("static_hover_tips", "ENERGY_COUNT.description");
        description.Add("energyPrefix", EnergyIconHelper.GetPrefix(_player.Character.CardPool));
        _hoverTip = new HoverTip(new LocString("static_hover_tips", "ENERGY_COUNT.title"), description);

        // Connexion des événements de survol
        MouseEntered += OnHovered;
        MouseExited += OnUnhovered;

        RefreshLabel();
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        // S'abonner aux changements d'énergie et d'état de combat
        CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
        _player.PlayerCombatState.EnergyChanged += OnEnergyChanged;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
        _player.PlayerCombatState.EnergyChanged -= OnEnergyChanged;
    }

    private void RefreshLabel()
    {
        var state = _player.PlayerCombatState;
        
        // Texte : "Actuel / Max"
        _label.SetTextAutoSize($"{state.Energy}/{state.MaxEnergy}");

        // Couleurs : Rouge si 0, sinon crème
        Color fontColor = state.Energy == 0 ? StsColors.red : StsColors.cream;
        Color outlineColor = state.Energy == 0 ? StsColors.unplayableEnergyCostOutline : _player.Character.EnergyLabelOutlineColor;
        
        _label.AddThemeColorOverride("font_color", fontColor);
        _label.AddThemeColorOverride("font_outline_color", outlineColor);

        // Si 0 énergie, on applique un matériau sombre (grisé) sur les orbes
        Material? orbMat = state.Energy == 0 ? PreloadManager.Cache.GetMaterial(DarkenedMatPath) : null;
        
        foreach (var child in _layers.GetChildren().OfType<Control>()) child.Material = orbMat;
        foreach (var child in _rotationLayers.GetChildren().OfType<Control>()) child.Material = orbMat;
        
        _layers.Modulate = state.Energy == 0 ? Colors.DarkGray : Colors.White;
    }

    private void OnEnergyChanged(int oldEnergy, int newEnergy)
    {
        RefreshLabel();
        // Si on gagne de l'énergie, on joue les particules
        if (newEnergy > oldEnergy)
        {
            _backVfx?.Restart();
            _frontVfx?.Restart();
        }
    }

    public override void _Process(double delta)
    {
        // Rotation des couches (plus lent si 0 énergie)
        float speed = _player.PlayerCombatState.Energy == 0 ? 5f : 30f;
        for (int i = 0; i < _rotationLayers.GetChildCount(); i++)
        {
            var layer = _rotationLayers.GetChild<Control>(i);
            layer.RotationDegrees += (float)delta * speed * (i + 1);
        }
    }

    // --- Animations de transition (Apparition/Disparition de l'interface) ---
    public void AnimIn()
    {
        _animOutTween?.Kill();
        Position = HidePosition;
        _animInTween = CreateTween();
        _animInTween.TweenProperty(this, "position", ShowPosition, 0.6f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
    }

    public void AnimOut()
    {
        _animInTween?.Kill();
        _animOutTween = CreateTween();
        _animOutTween.TweenProperty(this, "position", HidePosition, 0.6f)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Back);
    }

    private void OnHovered() => NHoverTipSet.CreateAndShow(this, _hoverTip);
    private void OnUnhovered() => NHoverTipSet.Remove(this);
    private void OnCombatStateChanged(CombatState combatState) => RefreshLabel();
}


/// exemple starcounter
///


public partial class exempleNStarCounter : Control
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