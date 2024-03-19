using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public GameObject emissionPoint;
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
    [HideInInspector] public TinyBot Owner;
    protected bool playerTargeting;
    private void Awake()
    {
        blockingLayerMask = LayerMask.GetMask(blockingLayers);
    }

    public void Initialize(TinyBot botUnit)
    {
        Owner = botUnit;
        Owner.beganTurn.AddListener(LapseCooldown);
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
    public abstract List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false);

    public virtual bool IsUsable(Vector3 sourcePosition)
    {
        if (currentCooldown == 0) return true;
        return false;
    }
    public virtual void LockOnTo(GameObject target, bool draw)
    {
        trackedTarget = target;
        playerTargeting = draw;
    }
    public virtual void ReleaseLock()
    {
        trackedTarget = null;
        StartCoroutine(Owner.PrimaryMovement.NeutralStance());
        LineMaker.HideLine();
        HighlightAffectedTargets(null);
    }

    protected abstract IEnumerator PerformEffects();
    void Update()
    {
        if (trackedTarget == null) return;
        List<TinyBot> newTargets = AimAt(trackedTarget, emissionPoint.transform.position);
        if(playerTargeting) HighlightAffectedTargets(newTargets);
        Owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
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
