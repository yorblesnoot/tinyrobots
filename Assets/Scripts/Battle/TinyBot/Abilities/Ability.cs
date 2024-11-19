using UnityEngine;
using UnityEngine.Serialization;

public abstract class Ability : MonoBehaviour
{
    [TextArea(3, 10)] public string Description;
    public float range;
    public bool ModifiableRange = false;
    public int cooldown = 1;
    [HideInInspector] public float EffectivenessMultiplier = 1;
    public int cost;
    public Sprite icon;
    protected DurationModule DurationModule;

    [HideInInspector] public int CurrentCooldown;
    [HideInInspector] public TinyBot Owner;

    public GameObject emissionPoint;



    public virtual void Initialize(TinyBot botUnit)
    {
        Owner = botUnit;
        Owner.BeganTurn.AddListener(LapseCooldown);
    }

    public abstract bool IsScalable();

    public abstract bool IsActive { get; }

    void LapseCooldown()
    {
        CurrentCooldown = Mathf.Clamp(CurrentCooldown - 1, 0, CurrentCooldown);
    }
}
