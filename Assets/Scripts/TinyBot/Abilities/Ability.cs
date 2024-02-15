using System.Collections;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField] protected GameObject emissionPoint;
    public CursorType PreferredCursor;
    public int cost;
    public int range;
    public Sprite icon;

    GameObject trackedTarget;
    
    [HideInInspector] public TinyBot owner;

    public abstract IEnumerator ExecuteAbility(Vector3 target);
    protected abstract void AimAt(GameObject target);
    public virtual bool ConfirmAbility(Vector3 target, out Vector3 confirmedTarget)
    {
        confirmedTarget = target;
        return true;
    }

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

    void Update()
    {
        if (trackedTarget == null) return;
        AimAt(trackedTarget);
        owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
    }

} 
