using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpatialTargeter : MonoBehaviour
{
    List<TinyBot> intersectingBots = new();
    public void ResetIntersecting()
    {
        intersectingBots = new();
    }

    public List<TinyBot> GetIntersectingBots()
    {
        return new(intersectingBots.Where(bot => bot != null));
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
