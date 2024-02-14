using System.Collections;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public Sprite icon;

    [SerializeField] protected GameObject emissionPoint;
    public CursorType PreferredCursor;
    [HideInInspector] public TinyBot owner;

    public int cost;
    public int range;

    GameObject trackedTarget;


    public abstract IEnumerator ExecuteAbility(Vector3 target);
    protected abstract void TargetEntity(GameObject target);

    public virtual void LockOnTo(GameObject target)
    {
        trackedTarget = target;
    }

    public virtual void ReleaseLock()
    {
        trackedTarget = null;
        StartCoroutine(owner.PrimaryMovement.NeutralStance());
        LineMaker.HideLine();
    }

    private void Update()
    {
        if (trackedTarget == null) return;
        TargetEntity(trackedTarget);
        owner.PrimaryMovement.TrackEntity(trackedTarget);
    }

    

} 
