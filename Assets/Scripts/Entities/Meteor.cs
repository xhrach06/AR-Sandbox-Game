using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float explosionRadius = 10f;
    public float explosionForce = 1000f;
    public float damage = 50f;

    void Start()
    {
        Destroy(gameObject, 5f); // Destroy after 5 sec
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(damage);
            Debug.Log("💥 Enemy hit by meteor!");
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemy") && col.attachedRigidbody != null)
            {
                col.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        Destroy(gameObject);
    }
}
