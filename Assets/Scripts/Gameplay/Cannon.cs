using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour
{
    public new Rigidbody rigidbody;
    public new Collider collider;
    public GameObject deathParticles;

    public TankController Owner { get; private set; }
    public float Damage { get; private set; }


    public void Initialize(TankController owner, float damage)
    {
        Owner = owner;
        Damage = damage;
    }

    private void OnCollisionEnter(Collision col)
    {
        TankController tank = col.collider.GetComponentInParent<TankController>();

        if (tank != null && tank != Owner)
        {
            tank.ApplyDamage(Damage);
        }

        var instance = Instantiate(deathParticles, transform.position, Quaternion.identity);
        Destroy(instance, 3f);

        Destroy(gameObject);
    }
}
