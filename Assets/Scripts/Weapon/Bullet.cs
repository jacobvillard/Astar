using System.Collections;
using UnityEngine;
using static Interfaces.Interfaces;

/// <summary>
/// A script that allows bullets to be pooled and deals damage on collision with relevant objects
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour {
    private BulletPool pool;
    private float returnAfterSeconds = 10f;
    private float bulletDamage = 50f;
    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public void SetPool(BulletPool bulletPool) {
        pool = bulletPool;
    }

    private void OnEnable() {
        StartCoroutine(ReturnToPoolAfterDelay(returnAfterSeconds));
    }

    private IEnumerator ReturnToPoolAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        pool.ReturnBullet(gameObject);
    }
    
    private void OnCollisionEnter(Collision other) {
        var damageable = other.gameObject.GetComponent<IDamageable>();
        if (damageable != null) {
            damageable.TakeDamage(bulletDamage);
            pool.ReturnBullet(gameObject);
        }
        else {
            rb.useGravity = true;
            pool.ReturnBullet(gameObject);
        }
        
        
    }
}
