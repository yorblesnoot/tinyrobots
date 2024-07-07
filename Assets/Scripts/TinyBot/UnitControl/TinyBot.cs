using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public enum Allegiance
{
    PLAYER,
    ALLIED,
    ENEMY
}
public class TinyBot : MonoBehaviour
{
    [SerializeField] float landingDuration = .5f;
    [SerializeField] float deathExplodeMaxForce;
    [SerializeField] float hitRecoilTime;
    [SerializeField] float hitReturnTime;
    [SerializeField] float recoilDistancePerDamage;
    [SerializeField] float backstabMultiplier = 1.5f;
    [SerializeField] float fallDamagePerUnit = 2;
    [SerializeField] GameObject selectBrackets;
    [SerializeField] BotStateFeedback feedback;
    [SerializeField] GameObject hitSpark;
    public Transform headshotPosition;
    public Transform ChassisPoint;

    readonly float minForce = .1f;
    [SerializeField] float deathPushMulti = 1;

    [HideInInspector] public BotCore LinkedCore;
    [HideInInspector] public Rigidbody PhysicsBody;
    [HideInInspector] public Sprite Portrait;
    [HideInInspector] public Allegiance Allegiance;
    [HideInInspector] public bool AvailableForTurn;
    [HideInInspector] public PrimaryMovement PrimaryMovement;
    [HideInInspector] public UnityEvent BeganTurn = new();
    [HideInInspector] public UnityEvent EndedTurn = new();
    [HideInInspector] public MoveStyle MoveStyle { get { return PrimaryMovement.Style; } }

    public BotStats Stats = new();
    public static UnityEvent ClearActiveBot = new();

    BotAI botAI;

    public List<Ability> Abilities { get; private set; }
    List<GameObject> Parts;
    Renderer[] partRenderers;
    public void Initialize(List<Ability> abilities, List<GameObject> parts, PrimaryMovement primaryMovement)
    {
        PhysicsBody = GetComponent<Rigidbody>();
        Parts = parts;
        Abilities = abilities;
        PrimaryMovement = primaryMovement;
        PrimaryMovement.Owner = this;
        ClearActiveBot.AddListener(ClearActiveUnit);
        partRenderers = GetComponentsInChildren<Renderer>();
        Pathfinder3D.GetOccupancy.AddListener(DeclareOccupancy);
    }

    void DeclareOccupancy(Vector3 position)
    {
        if(transform.position == position) return;
        Pathfinder3D.SetNodeOccupancy(Vector3Int.RoundToInt(transform.position), true);
    }

    public void SetOutlineColor(Color color)
    {
        foreach (Renderer r in partRenderers)
        {
            foreach(var m in r.materials)
            {
                m.SetColor("_OutlineColor", color);
            }
        }
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

    public void ToggleActiveLayer(bool active)
    {
        if(active) gameObject.layer = 6;
        else gameObject.layer = 0;
    }

    public void ClearActiveUnit()
    {
        UnitControl.PlayerControlledBot = null;
        selectBrackets.SetActive(false);
        ToggleActiveLayer(false);
    }

    
    void Die(Vector3 hitSource = default)
    {
        StopAllCoroutines();
        Vector3 hitPush = (transform.position - hitSource).normalized * deathPushMulti;
        foreach(var part in Parts)
        {
            if(!part.TryGetComponent(out Rigidbody rigidPart)) rigidPart = part.AddComponent<Rigidbody>();
            Vector3 explodeForce = new(Random.Range(minForce, deathExplodeMaxForce), 
                Random.Range(minForce, deathExplodeMaxForce), 
                Random.Range(minForce, deathExplodeMaxForce));
            rigidPart.velocity = explodeForce + hitPush;
        }
        TurnManager.RemoveTurnTaker(this);
        GetComponent<Collider>().isTrigger = false;
        Destroy(gameObject, 5f);
    }

    public void ReceiveHit(int damage, Vector3 source, Vector3 hitPoint, bool canBackstab = true)
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

    private void ReduceHealth(int damage)
    {
        Stats.Current[StatType.HEALTH] = Math.Clamp(Stats.Current[StatType.HEALTH] - damage, 0, Stats.Max[StatType.HEALTH]);
        TurnManager.UpdateHealth(this);
        if (Stats.Current[StatType.HEALTH] == 0) Die();
    }

    public IEnumerator Fall(Vector3 velocity = default)
    {
        float startHeight = transform.position.z;
        PhysicsBody.isKinematic = false;
        PhysicsBody.velocity = velocity;
        while(true)
        {
            Vector3Int cleanPosition = Vector3Int.RoundToInt(transform.position);
            if (Pathfinder3D.PointIsOffMap(cleanPosition.x, cleanPosition.y, cleanPosition.z))
            {
                Die(transform.position);
                break;
            }
            if (Pathfinder3D.GetLandingPointBy(transform.position, PrimaryMovement.Style, out Vector3Int coords))
            {
                PhysicsBody.isKinematic = true;
                //PhysicsBody.velocity = Vector3.zero;
                Vector3 surfaceNormal = PrimaryMovement.Style == MoveStyle.CRAWL ? Pathfinder3D.GetCrawlOrientation(coords) : Vector3.up;
                Quaternion rotationTarget = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
                Tween.Position(transform, endValue: coords, duration: landingDuration).Group(
                Tween.Rotation(transform, endValue: rotationTarget, duration: landingDuration))
                    .OnComplete(() => EndFall());
                break;
            }
            
            yield return null;
        }

        void EndFall()
        {
            StartCoroutine(PrimaryMovement.NeutralStance());
            float heightDifference = transform.position.z - startHeight;
            float fallDamage = Mathf.Clamp(heightDifference * fallDamagePerUnit, 0, float.MaxValue);
            ReduceHealth(Mathf.RoundToInt(fallDamage));
        }
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
