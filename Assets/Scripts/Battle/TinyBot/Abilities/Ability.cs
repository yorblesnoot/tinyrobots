using UnityEngine;
using UnityEngine.Serialization;

public abstract class Ability : MonoBehaviour
{
    [TextArea(3, 10)] public string Description;
    public float range;
    public bool ModifiableRange = false;
    public int cooldown = 1;
    [FormerlySerializedAs("damage")] public int EffectMagnitude;
    public int cost;
    public Sprite icon;
    public string EffectDescription = " Damage";
    protected DurationModule durationModule;

    [HideInInspector] public int CurrentCooldown;
    [HideInInspector] public TinyBot Owner;
    
    

    public virtual void Initialize(TinyBot botUnit)
    {
        Owner = botUnit;
        Owner.BeganTurn.AddListener(LapseCooldown);
    }

    public abstract bool IsActive { get; }

    void LapseCooldown()
    {
        CurrentCooldown = Mathf.Clamp(CurrentCooldown - 1, 0, CurrentCooldown);
    }
}
