using PrimeTween;
using System;
using System.Collections;
using UnityEngine;
public enum Allegiance
{
    PLAYER,
    ALLIED,
    ENEMY
}
public abstract class Targetable : MonoBehaviour
{
    [SerializeField] float fallDamagePerUnit = 2;
    public Transform TargetPoint;
    public UnitStats Stats;
    [HideInInspector] public Rigidbody PhysicsBody;
    [HideInInspector] public Allegiance Allegiance;
    [HideInInspector] public abstract MoveStyle MoveStyle { get; }
    
    public bool IsDead { get; protected set; } = false;

    protected Renderer[] PartRenderers;
    protected Collider Collider;
    int terrainMask;
    private void Awake()
    {
        terrainMask = LayerMask.GetMask("Terrain");
        Collider = GetComponent<Collider>();
    }

    public virtual void SetOutlineColor(Color color)
    {
        
    }

    public virtual void Die(Vector3 hitSource = default)
    {
        StopAllCoroutines();
        Collider.isTrigger = false;
        IsDead = true;
    }

    public abstract void ReceiveHit(int damage, TinyBot source, Vector3 hitPoint, bool canBackstab = true);

    protected virtual void ReduceHealth(int damage)
    {
        Stats.Current[StatType.HEALTH] = Math.Clamp(Stats.Current[StatType.HEALTH] - damage, 0, Stats.Max[StatType.HEALTH]);

        if (Stats.Current[StatType.HEALTH] == 0) Die();
    }

    readonly int maxFallDuration = 5;
    public IEnumerator Fall(Vector3 velocity = default)
    {
        yield return Depenetrate();
        float startHeight = transform.position.z;
        PhysicsBody.isKinematic = false;
        PhysicsBody.velocity = velocity;
        float elapsedTime = 0;
        while (elapsedTime < maxFallDuration)
        {
            Vector3Int cleanPosition = Vector3Int.RoundToInt(transform.position);
            if (Pathfinder3D.PointIsOffMap(cleanPosition.x, cleanPosition.y, cleanPosition.z))
            {
                Die(transform.position);
                yield break;
            }
            if (Pathfinder3D.GetLandingPointBy(transform.position, MoveStyle, out Vector3Int coords))
            {
                Land(coords, startHeight);
                yield break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Die(transform.position);
    }

    readonly float depenetrationRadius = 1f;
    readonly float depenetrationDuration = .3f;
    IEnumerator Depenetrate()
    {
        if (!Physics.CheckSphere(TargetPoint.position, depenetrationRadius, terrainMask)) yield break;
        Pathfinder3D.GetLandingPointBy(transform.position, MoveStyle, out Vector3Int cleanPosition, false);
        Vector3 direction = Pathfinder3D.GetCrawlOrientation(cleanPosition);
        Vector3 finalPosition = transform.position + direction * depenetrationRadius;
        yield return Tween.Position(transform, finalPosition, depenetrationDuration).ToYieldInstruction();


        /*Collider[] colliders = Physics.OverlapSphere(TargetPoint.position, depenetrationRadius, terrainMask);
        if (colliders == null || colliders.Length == 0) yield break;
       
        foreach (Collider contactCollider in colliders)
        {
            if(Physics.ComputePenetration(Collider, transform.position, transform.rotation, contactCollider, 
                contactCollider.transform.position, contactCollider.transform.rotation, out Vector3 direction, out float distance))
            {
                Vector3 position = transform.position + direction * distance;
                Debug.Log(direction * distance);
                yield return Tween.Position(transform, position, depenetrationDuration).ToYieldInstruction();
                yield break;
            }
        }*/
    }

    protected readonly int ActiveLayer = 6;
    public void ToggleActiveLayer(bool active)
    {
        gameObject.layer = active ? ActiveLayer : 0;
    }

    protected virtual void Land(Vector3Int coords, float startHeight)
    {
        PhysicsBody.isKinematic = true;
        Vector3 surfaceNormal = MoveStyle == MoveStyle.CRAWL ? Pathfinder3D.GetCrawlOrientation(coords) : Vector3.up;
        Quaternion rotationTarget = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
        Tween.Position(transform, endValue: coords, duration: .5f).OnComplete(() => EndFall(startHeight));
    }

    protected virtual void EndFall(float startHeight)
    {
        float heightDifference = transform.position.z - startHeight;
        float fallDamage = Mathf.Clamp(heightDifference * fallDamagePerUnit, 0, float.MaxValue);
        ReduceHealth(Mathf.FloorToInt(fallDamage));
    }
}
