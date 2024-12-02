using System.Collections.Generic;
using UnityEngine;

namespace Weapon {
    public class BulletPool : MonoBehaviour {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private int initialPoolSize = 20;
        private readonly Queue<GameObject> bullets = new();

        private void Start() {
            //Initialize the pool
            for (var i = 0; i < initialPoolSize; i++)CreateBullet();
        }

        /// <summary>
        /// Create a bullet and add it to the pool
        /// </summary>
        private void CreateBullet() {
            var bullet = Instantiate(bulletPrefab, transform);
            bullet.SetActive(false);
            bullet.GetComponent<Bullet>().SetPool(this); //Set the pool reference
            bullets.Enqueue(bullet);
        }

        /// <summary>
        /// Get a bullet from the pool
        /// </summary>
        /// <returns></returns>
        public GameObject GetBullet() {
            // Ensure there's always at least one bullet available
            if (bullets.Count == 0) CreateBullet(); 
        
            var bullet = bullets.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }
    
        /// <summary>
        /// Return a bullet to the pool
        /// </summary>
        /// <param name="bullet"></param>
        public void ReturnBullet(GameObject bullet) {
            bullet.SetActive(false);
            bullets.Enqueue(bullet);
        }
    }
}