using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    
    public CursorType PreferredCursor;
    public AbilityType Type;
    public int cost;
    public int range;
    public bool ModifiableRange = false;
    public int cooldown = 1;
    public int damage;
    
    public Sprite icon;
    public string[] blockingLayers;
    public GameObject emissionPoint;

    [HideInInspector] public bool locked;    
    [HideInInspector] public int currentCooldown;
    [HideInInspector] public TinyBot Owner;

    
    GameObject trackedTarget;
    protected bool playerTargeting;
    protected int blockingLayerMask;

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
        Vector3Int startPosition = Vector3Int.RoundToInt(Owner.transform.position);
        MainCameraControl.ActionPanTo(GetFinalAimPoint());
        currentCooldown = cooldown;
        PrimaryCursor.actionInProgress = true;
        yield return new WaitForSeconds(skillDelay);
        yield return StartCoroutine(PerformEffects());
        PrimaryCursor.actionInProgress = false;
        if (Vector3Int.RoundToInt(Owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(Owner);
    }

    Vector3 GetFinalAimPoint()
    {
        if(currentTargets != null && currentTargets.Count > 0)
        {
            Vector3 average = Vector3.zero;
            foreach(var target in currentTargets)
            {
                average += target.transform.position;
            }
            average /= currentTargets.Count;
            return average;
        }
        else
        {
            Vector3 offset = PrimaryCursor.Transform.position - Owner.transform.position;
            offset = offset.normalized;
            offset *= Mathf.Min(range, offset.magnitude);
            return offset + Owner.transform.position;
        }
    }

    List<TinyBot> currentTargets = new();
    public abstract List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false);

    public virtual bool IsUsable(Vector3 targetPosition)
    {
        return true;
    }

    public virtual bool IsAvailable()
    {
        if (currentCooldown > 0 || locked) return false;
        return true;
    }
    public virtual void LockOnTo(GameObject target, bool draw)
    {
        trackedTarget = target;
        playerTargeting = draw;
    }
    public virtual void ReleaseLockOn()
    {
        trackedTarget = null;
        StartCoroutine(Owner.PrimaryMovement.NeutralStance());
        LineMaker.HideLine();
        SetTargets(null);
    }

    protected abstract IEnumerator PerformEffects();

    public abstract void NeutralAim();
    void Update()
    {
        if (trackedTarget == null) return;
        List<TinyBot> newTargets = AimAt(trackedTarget, emissionPoint.transform.position);
        if(playerTargeting) SetTargets(newTargets);
        Owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
    }

    private void SetTargets(List<TinyBot> newTargets)
    {
        newTargets ??= new();
        for (int i = 0; i < newTargets.Count; i++)
        {
            TinyBot bot = newTargets[i];
            if (bot == null) newTargets.Remove(bot);
            else if (!currentTargets.Contains(bot)) bot.SetOutlineColor(Color.red);
        }
        foreach(TinyBot bot in currentTargets)
        {
            if(!newTargets.Contains(bot)) bot.SetOutlineColor(Color.white);
        }
        currentTargets = new(newTargets);
    }
}

public enum AbilityType
{
    ATTACK,
    BUFF,
    SHIELD,
    DASH,
    SUMMON
}
