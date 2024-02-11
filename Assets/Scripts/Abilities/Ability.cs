using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    public Sprite icon;
    protected bool targeting;

    [SerializeField] protected int maxRange;
    [SerializeField] protected GameObject emissionPoint;
    public CursorType PreferredCursor;
    [HideInInspector] public TinyBot owner;

    public int cost;


    public abstract void ExecuteAbility(Vector3 target);
    public abstract void ControlTargetLine();

    public virtual void ToggleSkillTargeting(bool on)
    {
        targeting = on;
        if (on == false)
        {
            StartCoroutine(owner.PrimaryMovement.NeutralStance());
            LineMaker.HideLine();
        }
    }

    private void Update()
    {
        if (!targeting) return;
        ControlTargetLine();
        owner.PrimaryMovement.TrackEntity(PrimaryCursor.Transform.gameObject);
    }

    

} 
