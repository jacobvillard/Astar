using System.Collections;
using UnityEngine;
using static Interfaces.Interfaces;

namespace Weapon {
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

        /// <summary>
        /// Set the pool reference
        /// </summary>
        /// <param name="bulletPool"></param>
        public void SetPool(BulletPool bulletPool) {
            pool = bulletPool;
        }

        /// <summary>
        /// Start coroutine to return to pool after delay on enable       
        /// </summary>
        private void OnEnable() {
            StartCoroutine(ReturnToPoolAfterDelay(returnAfterSeconds));
        }

        /// <summary>
        /// Return to the pool after delay
        /// </summary>
        /// <param name="delay">delay before returning to the pool</param>
        /// <returns></returns>
        private IEnumerator ReturnToPoolAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            pool.ReturnBullet(gameObject);
        }
    
        /// <summary>
        /// Manages collisions of bullet
        /// </summary>
        /// <param name="other"></param>
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
}
