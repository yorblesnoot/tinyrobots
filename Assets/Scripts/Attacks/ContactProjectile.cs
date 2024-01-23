using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactProjectile : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] bool pierce;
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.transform.TryGetComponent<TinyBot>(out var bot)) bot.ReceiveDamage(damage);
        if (!pierce) Destroy(gameObject);

    }
}
