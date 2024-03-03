using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialTargeter : MonoBehaviour
{
    [HideInInspector] public List<TinyBot> intersectingBots = new();
    public void ResetIntersecting()
    {
        intersectingBots = new();
    }

    public List<TinyBot> GetIntersectingBots()
    {
        return new(intersectingBots);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out TinyBot bot))
        {
            intersectingBots.Add(bot);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out TinyBot bot))
        {
            intersectingBots.Remove(bot);
        }

    }
}
