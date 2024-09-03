using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [TextArea(3, 10)] public string Description;
    public float range;
    public bool ModifiableRange = false;
    public int cooldown = 1;
    public int damage;
    public int cost;
    public Sprite icon;
    protected DurationModule durationModule;

    [HideInInspector] public int currentCooldown;
    [HideInInspector] public TinyBot Owner;
    [HideInInspector] public bool locked;
    

    public virtual void Initialize(TinyBot botUnit)
    {
        Owner = botUnit;
        Owner.BeganTurn.AddListener(LapseCooldown);
    }

    public abstract bool IsActive();

    void LapseCooldown()
    {
        currentCooldown = Mathf.Clamp(currentCooldown - 1, 0, currentCooldown);
    }
}
