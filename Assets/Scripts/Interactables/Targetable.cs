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
    [HideInInspector] public MoveStyle MoveStyle { get { return GetMoveStyle(); } }
    
    public bool IsDead { get; protected set; } = false;

    protected Renderer[] partRenderers;

    private void Awake()
    {
        Pathfinder3D.GetOccupancy.AddListener(DeclareOccupancy);
    }

    void DeclareOccupancy(Vector3 position)
    {
        if (transform.position == position) return;
        Pathfinder3D.SetNodeOccupancy(Vector3Int.RoundToInt(transform.position), true);
    }

    public void SetOutlineColor(Color color)
    {
        foreach (Renderer r in partRenderers)
        {
            foreach (var m in r.materials)
            {
                m.SetColor("_OutlineColor", color);
            }
        }
    }

    public virtual void Die(Vector3 hitSource = default)
    {
        StopAllCoroutines();
        GetComponent<Collider>().isTrigger = false;
        IsDead = true;
    }

    public abstract void ReceiveHit(int damage, Vector3 source, Vector3 hitPoint, bool canBackstab = true);
    public abstract MoveStyle GetMoveStyle();

    protected virtual void ReduceHealth(int damage)
    {
        Stats.Current[StatType.HEALTH] = Math.Clamp(Stats.Current[StatType.HEALTH] - damage, 0, Stats.Max[StatType.HEALTH]);

        if (Stats.Current[StatType.HEALTH] == 0) Die();
    }

    public IEnumerator Fall(Vector3 velocity = default)
    {
        float startHeight = transform.position.z;
        PhysicsBody.isKinematic = false;
        PhysicsBody.velocity = velocity;
        while (true)
        {
            Vector3Int cleanPosition = Vector3Int.RoundToInt(transform.position);
            if (Pathfinder3D.PointIsOffMap(cleanPosition.x, cleanPosition.y, cleanPosition.z))
            {
                Die(transform.position);
                break;
            }
            if (Pathfinder3D.GetLandingPointBy(transform.position, MoveStyle, out Vector3Int coords))
            {
                Land(coords, startHeight);
                break;
            }

            yield return null;
        }
    }

    public void ToggleActiveLayer(bool active)
    {
        if (active) gameObject.layer = 6;
        else gameObject.layer = 0;
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
        ReduceHealth(Mathf.RoundToInt(fallDamage));
    }
}
