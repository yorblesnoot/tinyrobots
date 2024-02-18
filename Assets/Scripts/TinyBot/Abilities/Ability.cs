using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField] protected GameObject emissionPoint;
    public CursorType PreferredCursor;
    public int cost;
    public int range;
    public int cooldown;
    public Sprite icon;

    public string[] blockingLayers;
    protected int blockingLayerMask;

    GameObject trackedTarget;
    [HideInInspector] public TinyBot owner;
    private void Awake()
    {
        blockingLayerMask = LayerMask.GetMask(blockingLayers);
    }

    public IEnumerator Execute()
    {
        PrimaryCursor.actionInProgress = true;
        yield return StartCoroutine(PerformEffects());
        PrimaryCursor.actionInProgress = false;
    }

    protected abstract void AimAt(GameObject target);
    public abstract GameObject GhostAimAt(GameObject target, Vector3 sourcePosition);

    public virtual bool IsUsable(Vector3 sourcePosition)
    {
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
    protected abstract IEnumerator PerformEffects();
    void Update()
    {
        if (trackedTarget == null) return;
        AimAt(trackedTarget);
        owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
    }

} 
