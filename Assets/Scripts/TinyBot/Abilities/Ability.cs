using UnityEngine;

public class Ability : MonoBehaviour
{
    public int range;
    public bool ModifiableRange = false;
    public int cooldown = 1;
    public int damage;
    public Sprite icon;

    [HideInInspector] public int currentCooldown;
    [HideInInspector] public TinyBot Owner;
    [HideInInspector] public bool locked;

    public virtual void Initialize(TinyBot botUnit)
    {
        Owner = botUnit;
        Owner.BeganTurn.AddListener(LapseCooldown);
    }

    void LapseCooldown()
    {
        currentCooldown = Mathf.Clamp(currentCooldown - 1, 0, currentCooldown);
    }
}
