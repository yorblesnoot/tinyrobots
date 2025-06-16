using PrimeTween;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


public class TinyBot : Targetable
{
    [SerializeField] float deathPushMulti = 1;
    [SerializeField] float landingDuration = .5f;
    [SerializeField] float deathExplodeMaxForce;
    [SerializeField] float hitRecoilTime;
    [SerializeField] float hitReturnTime;
    [SerializeField] float recoilDistancePerDamage;
    
    [SerializeField] GameObject hitSpark;
    public Transform headshotPosition;
    
    readonly float deathExplodeMinForce = .1f;
    
    
    [HideInInspector] public Sprite Portrait;
    
    [HideInInspector] public bool AvailableForTurn;
    [HideInInspector] public bool Summoned;
    [HideInInspector] public List<PartModifier> PartModifiers;

    [HideInInspector] public UnityEvent BeganTurn = new();
    [HideInInspector] public UnityEvent EndedTurn = new();
    [HideInInspector] public UnityEvent ReceivedHit = new();
    [HideInInspector] public UnityEvent AbilitiesChanged = new();
    public static UnityEvent<TinyBot> BotDied = new();

    public static UnityEvent ClearActiveBot = new();
    BotAI botAI;

    [HideInInspector] public BuffController Buffs;
    [HideInInspector] public BotCharacter LinkedCore;
    [HideInInspector] public PrimaryMovement Movement;
    [HideInInspector] public BotCaster Caster;

    public List<Ability> Abilities = new();
    public List<ActiveAbility> ActiveAbilities { get; private set; } = new();
    public List<PassiveAbility> PassiveAbilities { get; private set; } = new();
    public List<MetaAbility> MetaAbilities { get; private set; } = new();

    public override MoveStyle MoveStyle => Movement.Style;

    [HideInInspector] public DamageCalculator DamageCalculator;

    [HideInInspector] public TinyBot BotEcho;

    public void Initialize(List<Ability> abilities, List<PartModifier> parts, PrimaryMovement primaryMovement, bool echo, UnitStats stats, Allegiance allegiance)
    {
        Stats = stats;
        Allegiance = allegiance;
        Movement = primaryMovement;
        Movement.Owner = this;
        Buffs = new BuffController(this);
        if(!echo) SetAbilities(abilities);
        if (echo) return;
        DamageCalculator = GetComponent<DamageCalculator>();
        Caster = GetComponent<BotCaster>();
        PhysicsBody = GetComponent<Rigidbody>();
        PartModifiers = parts;

        ClearActiveBot.AddListener(ClearActiveUnit);
        AbilitiesChanged.AddListener(() => cachedMaterials = CacheMaterials());
        Pathfinder3D.GetOccupancy.AddListener(DeclareOccupancy);
        BotCaster.ResetHighlights.AddListener(() => SetOutlineColor(Color.white));
    }

    private void OnDestroy()
    {
        BeganTurn.RemoveAllListeners();
        EndedTurn.RemoveAllListeners();
        ReceivedHit.RemoveAllListeners();
        AbilitiesChanged.RemoveAllListeners();
        ClearActiveBot.RemoveListener(ClearActiveUnit);
        if (cachedMaterials == null) return;
        // foreach(var material in cachedMaterials) Resources.UnloadAsset(material); //whats going on with this?
    }

    void DeclareOccupancy(Vector3[] positions)
    {
        if (positions.Contains(transform.position)) return;
        Pathfinder3D.SetNodeOccupancy(Vector3Int.RoundToInt(transform.position), true);
    }

    private void SetAbilities(List<Ability> abilities)
    {
        foreach (var ability in abilities)
        {
            ability.Initialize(this);
            ability.ModifyOn(this, true);
        }
    }


    List<Material> cachedMaterials;
    public override void SetOutlineColor(Color color)
    {
        cachedMaterials ??= CacheMaterials();
        foreach (var m in cachedMaterials)
        {
            m.SetColor("_OutlineColor", color);
        }
}

    List<Material> CacheMaterials()
    {
        List<Material> materials = new();
        foreach (PartModifier mod in PartModifiers)
        {
            foreach (Renderer ren in mod.mainRenderers)
            {
                materials.AddRange(ren.materials);
            }
        }
        return materials;
    }

    public void SpendResource(int resource, StatType statType)
    {
        int newTotal = Mathf.Max(Stats.Current[statType] - resource, 0);
        Stats.Current[statType] = newTotal;
    }

    public void BecomeAvailableForTurn()
    {
        BeganTurn?.Invoke();
        Stats.SetToMax(StatType.ACTION);
        Stats.SetToMax(StatType.MOVEMENT);
        AvailableForTurn = true;
    }

    public void Select(bool force = false)
    {
        if (MainCameraControl.CameraAnimating && !force) return;
        if(AvailableForTurn && gameObject.layer != ActiveLayer) ClearActiveBot.Invoke();
        MainCameraControl.FindViewOfPosition(TargetPoint.position, AvailableForTurn ? BeginTurn : null);
    }

    void BeginTurn()
    {
        Pathfinder3D.SetNodeOccupancy(Vector3Int.RoundToInt(transform.position), false);
        ToggleActiveLayer(true);
        if (Allegiance == Allegiance.PLAYER)
        {
            Pathfinder3D.GeneratePathingTree(Movement.Style, transform.position);
            PrimaryCursor.TogglePlayerLockout(false);
            PrimaryCursor.PlayerSelectedBot.Invoke(this);
        }
        else
        {
            botAI ??= new(this);
            StartCoroutine(botAI.TakeTurn());
        }
    }

    void ClearActiveUnit()
    {
        ToggleActiveLayer(false);
    }

    public override void Die(Vector3 hitSource = default)
    {
        

        base.Die(hitSource);
        Pathfinder3D.GetOccupancy.RemoveListener(DeclareOccupancy);
        if (PrimaryCursor.TargetedBot == this) PrimaryCursor.Unsnap();
        Vector3 hitPush = (transform.position - hitSource).normalized * deathPushMulti;
        foreach(var part in PartModifiers)
        {
            part.transform.SetParent(null, true);
            if (!part.TryGetComponent(out Rigidbody rigidPart)) rigidPart = part.gameObject.AddComponent<Rigidbody>();
            Vector3 explodeForce = new(Random.Range(deathExplodeMinForce, deathExplodeMaxForce), 
                Random.Range(deathExplodeMinForce, deathExplodeMaxForce), 
                Random.Range(deathExplodeMinForce, deathExplodeMaxForce));
            rigidPart.velocity = explodeForce + hitPush;
        }
        BotDied.Invoke(this);
        Debug.Log("BotDied invoked for " + this.name + " at " + Time.frameCount);
        Destroy(gameObject);
    }

    public override void ReceiveHit(int baseDamage, TinyBot source, Vector3 hitPoint, bool flinch = true)
    {
        int finalDamage = DamageCalculator.GetDamage(baseDamage, source, this, true);
        Vector3 hitDirection = (source.TargetPoint.position - transform.position).normalized;

        if(flinch) StartCoroutine(Movement.ApplyImpulseToBody(-hitDirection, recoilDistancePerDamage * finalDamage, hitRecoilTime, hitReturnTime));
        GameObject spark = Instantiate(hitSpark, hitPoint, Quaternion.identity);
        spark.transform.LookAt(source.TargetPoint.position);
        Destroy(spark, 1f);
        ReduceHealth(finalDamage);
        ReceivedHit.Invoke();
    }

    public override void ReduceHealth(int damage)
    {
        base.ReduceHealth(damage);
        BattleEnder.IsMissionOver();
    }

    protected override void Land(Vector3Int coords, float startHeight)
    {
        PhysicsBody.isKinematic = true;
        Vector3 surfaceNormal = MoveStyle == MoveStyle.CRAWL ? Pathfinder3D.GetCrawlOrientation(coords) : Vector3.up;
        Vector3 finalPosition = Movement.SanitizePoint(coords);
        Quaternion rotationTarget = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        //StartCoroutine(PrimaryMovement.NeutralStance());
        Tween.Position(transform, endValue: finalPosition, duration: landingDuration).Group(
                Tween.Rotation(transform, endValue: rotationTarget, duration: landingDuration))
                    .OnComplete(() => EndFall(startHeight));
    }

    protected override void EndFall(float startHeight)
    {
        base.EndFall(startHeight);
        Pathfinder3D.EvaluateNodeOccupancy(transform.position);
        Movement.LandingStance();
        Tween.Delay(.5f, () => StartCoroutine(Movement.NeutralStance()));
    }

    public void PlaceAt(Vector3 landingPoint, Vector3 facing = default)
    {
        gameObject.SetActive(true);
        Vector3 cleanPosition = Movement.SanitizePoint(landingPoint);
        transform.position = cleanPosition;
        Movement.PivotToFacePosition(transform.position + facing, true);
    }

    public void DeclareEcho()
    {
        gameObject.name = "Echo";
        Movement.ToggleAnimations(false);
        ToggleActiveLayer(true);
        Collider collider = GetComponent<Collider>();
        Destroy(collider);
        gameObject.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if(UnitControl.PlayerControlledBot != this /*&& ClickableAbility.Active == null*/) PrimaryCursor.SnapToUnit(this);
    }

    private void OnMouseExit()
    {
        if(PrimaryCursor.TargetedBot == this) PrimaryCursor.Unsnap();
    }
}
