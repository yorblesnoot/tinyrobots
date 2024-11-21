using PrimeTween;
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
    [SerializeField] float backstabMultiplier = 1.5f;
    [SerializeField] BotStateFeedback feedback;
    [SerializeField] GameObject hitSpark;
    public Transform headshotPosition;
    
    readonly float deathExplodeMinForce = .1f;
    
    
    [HideInInspector] public Sprite Portrait;
    
    [HideInInspector] public bool AvailableForTurn;
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
    [HideInInspector] public PrimaryMovement PrimaryMovement;
    public List<ActiveAbility> ActiveAbilities { get; private set; }
    public List<PassiveAbility> PassiveAbilities { get; private set;}

    [HideInInspector] public DamageCalculator DamageCalculator;

    public void Initialize(List<Ability> abilities, List<PartModifier> parts, PrimaryMovement primaryMovement)
    {
        DamageCalculator = GetComponent<DamageCalculator>();
        Buffs = new BuffController(this);
        PartRenderers = GetComponentsInChildren<Renderer>();
        PhysicsBody = GetComponent<Rigidbody>();
        PartModifiers = parts;
        SetAbilities(abilities);

        
        PrimaryMovement = primaryMovement;
        PrimaryMovement.Owner = this;
        PrimaryCursor.PlayerSelectedBot.AddListener(TryToBecomeActive);
        ClearActiveBot.AddListener(ClearActiveUnit);
    }

    private void SetAbilities(List<Ability> abilities)
    {
        ActiveAbilities = new();
        PassiveAbilities = new();
        foreach (var ability in abilities)
        {
            AddAbility(ability);
        }
    }

    public void AddAbility(Ability ability)
    {
        ability.Initialize(this);
        ActiveAbility active = ability as ActiveAbility;
        if (active != null) ActiveAbilities.Add(active);
        else PassiveAbilities.Add(ability as PassiveAbility);
        AbilitiesChanged.Invoke();
    }

    public override void SetOutlineColor(Color color)
    {
        foreach (PartModifier mod in PartModifiers)
        {
            foreach(Renderer ren in mod.mainRenderers)
            {
                foreach (var m in ren.materials)
                {
                    m.SetColor("_OutlineColor", color);
                }
            }
        }
    }

    public override MoveStyle GetMoveStyle()
    {
        return PrimaryMovement.Style;
    }

    public void SpendResource(int resource, StatType statType)
    {
        Stats.Current[statType] -= resource;
        if (Stats.Current[statType] < 0) Stats.Current[statType] = 0;
    }

    public void BeginTurn()
    {
        BeganTurn?.Invoke();
        Pathfinder3D.SetNodeOccupancy(Vector3Int.RoundToInt(transform.position), false);
        Stats.SetToMax(StatType.ACTION);
        Stats.SetToMax(StatType.MOVEMENT);
        if (Allegiance == Allegiance.PLAYER) AvailableForTurn = true;
        else
        {
            botAI ??= new(this);
            StartCoroutine(botAI.TakeTurn());
        }
    }

    void TryToBecomeActive(TinyBot target)
    {
        if(target == this) ToggleActiveLayer(true);
    }

    public void ClearActiveUnit()
    {
        UnitControl.PlayerControlledBot = null;
        ToggleActiveLayer(false);
    }

    public override void Die(Vector3 hitSource = default)
    {
        base.Die(hitSource);
        
        if(PrimaryCursor.TargetedBot == this) PrimaryCursor.Unsnap();
        Vector3 hitPush = (transform.position - hitSource).normalized * deathPushMulti;
        foreach(var part in PartModifiers)
        {
            if(!part.TryGetComponent(out Rigidbody rigidPart)) rigidPart = part.gameObject.AddComponent<Rigidbody>();
            Vector3 explodeForce = new(Random.Range(deathExplodeMinForce, deathExplodeMaxForce), 
                Random.Range(deathExplodeMinForce, deathExplodeMaxForce), 
                Random.Range(deathExplodeMinForce, deathExplodeMaxForce));
            rigidPart.velocity = explodeForce + hitPush;
        }
        TurnManager.RemoveTurnTaker(this);
        if(LinkedCore != null) LinkedCore.HealthRatio = 0;
        BotDied.Invoke(this);
        Destroy(gameObject, 5f);
    }

    public override void ReceiveHit(int baseDamage, TinyBot source, Vector3 hitPoint, bool canBackstab = true)
    {
        int finalDamage = DamageCalculator.GetDamage(baseDamage, source, this, true);
        Vector3 hitDirection = (source.TargetPoint.position - transform.position).normalized;

        feedback.QueuePopup(finalDamage, finalDamage > 1.5f * baseDamage);
        StartCoroutine(PrimaryMovement.ApplyImpulseToBody(-hitDirection, recoilDistancePerDamage * finalDamage, hitRecoilTime, hitReturnTime));
        GameObject spark = Instantiate(hitSpark, hitPoint, Quaternion.identity);
        spark.transform.LookAt(source.TargetPoint.position);
        Destroy(spark, 1f);
        ReduceHealth(finalDamage);
        ReceivedHit.Invoke();
    }

    protected override void ReduceHealth(int damage)
    {
        base.ReduceHealth(damage);
        BattleEnder.IsMissionOver();
    }

    public void Heal(int amount)
    {
        ReduceHealth(-amount);
        //heal vfx
    }

    protected override void Land(Vector3Int coords, float startHeight)
    {
        PhysicsBody.isKinematic = true;
        Vector3 surfaceNormal = MoveStyle == MoveStyle.CRAWL ? Pathfinder3D.GetCrawlOrientation(coords) : Vector3.up;
        Vector3 finalPosition = PrimaryMovement.SanitizePoint(coords);
        Quaternion rotationTarget = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        StartCoroutine(PrimaryMovement.NeutralStance());
        Tween.Position(transform, endValue: finalPosition, duration: landingDuration).Group(
                Tween.Rotation(transform, endValue: rotationTarget, duration: landingDuration))
                    .OnComplete(() => EndFall(startHeight));
    }

    protected override void EndFall(float startHeight)
    {
        base.EndFall(startHeight);
        PrimaryMovement.LandingStance();
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
