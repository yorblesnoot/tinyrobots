using System.Collections.Generic;
using UnityEngine;

public class PartModifier : MonoBehaviour
{
    public List<Renderer> mainRenderers;
    [SerializeField] Transform abilityContainer;
    [HideInInspector] public Ability[] Abilities;

    private void Awake()
    {
        Abilities = abilityContainer.GetComponentsInChildren<Ability>();
    }
}
