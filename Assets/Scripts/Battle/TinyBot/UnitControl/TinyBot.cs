using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;


public class TinyBot : Targetable
{
    [SerializeField] float landingDuration = .5f;
    [SerializeField] float deathExplodeMaxForce;
    [SerializeField] float hitRecoilTime;
    [SerializeField] float hitReturnTime;
    [SerializeField] float recoilDistancePerDamage;
    [SerializeField] float backstabMultiplier = 1.5f;
    [SerializeField] GameObject selectBrackets;
    [SerializeField] BotStateFeedback feedback;
    [SerializeField] GameObject hitSpark;
    public Transform headshotPosition;
    

    readonly float deathExplodeMinForce = .1f;
    [SerializeField] float deathPushMulti = 1;

    [HideInInspector] public BotCore LinkedCore;
    
    [HideInInspector] public Sprite Portrait;
    
    [HideInInspector] public bool AvailableForTurn;
    [HideInInspector] public PrimaryMovement PrimaryMovement;
    [HideInInspector] public UnityEvent BeganTurn = new();
    [HideInInspector] public UnityEvent EndedTurn = new();

    public static UnityEvent ClearActiveBot = new();
    BotAI botAI;

    public List<ActiveAbility> ActiveAbilities { get; private set; }
    public List<PassiveAbility> PassiveAbilities { get; private set;}
    List<GameObject> Parts;
    
    public void Initialize(List<Ability> abilities, List<GameObject> parts, PrimaryMovement primaryMovement)
    {
        partRenderers = GetComponentsInChildren<Renderer>();
        PhysicsBody = GetComponent<Rigidbody>();
        Parts = parts;
        SetAbilities(abilities);

        PrimaryMovement = primaryMovement;
        PrimaryMovement.Owner = this;
        ClearActiveBot.AddListener(ClearActiveUnit);
    }

    private void SetAbilities(List<Ability> abilities)
    {
        ActiveAbilities = new();
        PassiveAbilities = new();
        foreach (var ability in abilities)
        {
            ActiveAbility active = ability as ActiveAbility;
            if (active != null) ActiveAbilities.Add(active);
            else PassiveAbilities.Add(ability as PassiveAbility);
        }
    }

    public override MoveStyle GetMoveStyle()
    {
        return PrimaryMovement.Style;
    }

    public void SpendResource(int resource, StatType statType)
    {
        Stats.Current[statType] -= resource;
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
            MainCameraControl.CutToUnit(this);
            StartCoroutine(botAI.TakeTurn());
        }
    }

    public void BecomeActiveUnit()
    {
        selectBrackets.SetActive(true);
        UnitControl.PlayerControlledBot = this;
        ToggleActiveLayer(true);
    }

    

    public void ClearActiveUnit()
    {
        UnitControl.PlayerControlledBot = null;
        selectBrackets.SetActive(false);
        ToggleActiveLayer(false);
    }

    public override void Die(Vector3 hitSource = default)
    {
        base.Die(hitSource);
        Vector3 hitPush = (transform.position - hitSource).normalized * deathPushMulti;
        foreach(var part in Parts)
        {
            if(!part.TryGetComponent(out Rigidbody rigidPart)) rigidPart = part.AddComponent<Rigidbody>();
            Vector3 explodeForce = new(Random.Range(deathExplodeMinForce, deathExplodeMaxForce), 
                Random.Range(deathExplodeMinForce, deathExplodeMaxForce), 
                Random.Range(deathExplodeMinForce, deathExplodeMaxForce));
            rigidPart.velocity = explodeForce + hitPush;
        }
        TurnManager.RemoveTurnTaker(this);
        
        Destroy(gameObject, 5f);
    }

    public override void ReceiveHit(int damage, Vector3 source, Vector3 hitPoint, bool canBackstab = true)
    {
        //check for a backstab
        Vector3 hitDirection = (source - transform.position).normalized;
        float dot = Vector3.Dot(hitDirection, transform.forward);
        bool backstabbed = canBackstab && dot < 0;

        feedback.QueuePopup(damage, backstabbed);
        StartCoroutine(PrimaryMovement.ApplyImpulseToBody(source, recoilDistancePerDamage * damage, hitRecoilTime, hitReturnTime));
        GameObject spark = Instantiate(hitSpark, hitPoint, Quaternion.identity);
        spark.transform.LookAt(source);
        Destroy(spark, 1f);
        ReduceHealth(backstabbed ? Mathf.RoundToInt(backstabMultiplier * damage) : damage);
    }

    protected override void ReduceHealth(int damage)
    {
        base.ReduceHealth(damage);
        TurnManager.UpdateHealth(this);
    }

    protected override void Land(Vector3Int coords, float startHeight)
    {
        PhysicsBody.isKinematic = true;
        Vector3 surfaceNormal = MoveStyle == MoveStyle.CRAWL ? Pathfinder3D.GetCrawlOrientation(coords) : Vector3.up;
        Quaternion rotationTarget = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        Tween.Position(transform, endValue: coords, duration: landingDuration).Group(
                Tween.Rotation(transform, endValue: rotationTarget, duration: landingDuration))
                    .OnComplete(() => EndFall(startHeight));
    }

    protected override void EndFall(float startHeight)
    {
        base.EndFall(startHeight);
        if(IsDead) return;
        StartCoroutine(PrimaryMovement.NeutralStance());
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
