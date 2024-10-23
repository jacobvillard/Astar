using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour {
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialPoolSize = 20;
    private readonly Queue<GameObject> bullets = new();

    private void Start() {
        //Initialize the pool
        for (var i = 0; i < initialPoolSize; i++)CreateBullet();
    }

    private void CreateBullet() {
        var bullet = Instantiate(bulletPrefab, transform);
        bullet.SetActive(false);
        bullet.GetComponent<Bullet>().SetPool(this); //Set the pool reference
        bullets.Enqueue(bullet);
    }

    public GameObject GetBullet() {
        // Ensure there's always at least one bullet available
        if (bullets.Count == 0) CreateBullet(); 
        
        var bullet = bullets.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }
    
    public void ReturnBullet(GameObject bullet) {
        bullet.SetActive(false);
        bullets.Enqueue(bullet);
    }
}