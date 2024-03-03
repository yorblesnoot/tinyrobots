using System;
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
    public int cooldown = 1;
    public int damage;
    public Sprite icon;
    

    [HideInInspector] public int currentCooldown;

    public string[] blockingLayers;
    protected int blockingLayerMask;

    GameObject trackedTarget;
    [HideInInspector] public TinyBot owner;
    private void Awake()
    {
        blockingLayerMask = LayerMask.GetMask(blockingLayers);
    }

    public void Initialize(TinyBot botUnit)
    {
        owner = botUnit;
        owner.beganTurn.AddListener(LapseCooldown);
    }

    void LapseCooldown()
    {
        currentCooldown = Mathf.Clamp(currentCooldown - 1, 0, currentCooldown); 
    }

    readonly float skillDelay = .5f;
    public IEnumerator Execute()
    {
        currentCooldown = cooldown;
        PrimaryCursor.actionInProgress = true;
        yield return new WaitForSeconds(skillDelay);
        yield return StartCoroutine(PerformEffects());
        PrimaryCursor.actionInProgress = false;
    }
    List<TinyBot> currentTargets = new();
    protected abstract List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool drawLine);
    public virtual List<TinyBot> GhostAimAt(GameObject target, Vector3 sourcePosition)
    {
        return AimAt(target, sourcePosition, false);
    }

    public virtual bool IsUsable(Vector3 sourcePosition)
    {
        if (currentCooldown == 0) return true;
        return false;
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
        HighlightAffectedTargets(null);
    }
    protected abstract IEnumerator PerformEffects();
    void Update()
    {
        if (trackedTarget == null) return;
        List<TinyBot> newTargets = AimAt(trackedTarget, emissionPoint.transform.position, true);
        Debug.Log(newTargets.Count);
        HighlightAffectedTargets(newTargets);
        owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
    }

    private void HighlightAffectedTargets(List<TinyBot> newTargets)
    {
        newTargets ??= new();
        foreach(TinyBot bot in newTargets)
        {
            if(!currentTargets.Contains(bot)) bot.SetOutlineColor(Color.red);
        }
        foreach(TinyBot bot in currentTargets)
        {
            if(!newTargets.Contains(bot)) bot.SetOutlineColor(Color.white);
        }
        currentTargets = new(newTargets);
    }
} 
