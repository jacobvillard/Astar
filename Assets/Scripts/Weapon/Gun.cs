using UnityEngine;

/// <summary>
/// A script for aiming and shooting
/// </summary>
public class Gun : MonoBehaviour {
    [SerializeField] private BulletPool bulletPool;
    [SerializeField] private GameObject bulletSpawn;
    [SerializeField] private float aimSpeed = 5f; 
    [SerializeField] private float bulletForce = 10f; 
    [SerializeField] private Vector3 aimedPos, defaultPos;

    private void Awake() {
        defaultPos = transform.localPosition;
    }

    private void Update() {
        transform.localPosition = Input.GetMouseButton(1)
            ? Vector3.Lerp(transform.localPosition, aimedPos, aimSpeed * Time.deltaTime)
            : Vector3.Lerp(transform.localPosition, defaultPos, aimSpeed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0)) ShootBullet();
    }

    /// <summary>
    /// Instantiates the bullet and applies force
    /// </summary>
    private void ShootBullet() {
        //Get a bullet from the pool
        var bullet = bulletPool.GetBullet();
        var rb = bullet.GetComponent<Rigidbody>();
        
        bullet.transform.position = bulletSpawn.transform.position;
        bullet.transform.rotation = bulletSpawn.transform.rotation;
        rb.useGravity = false;
        rb.velocity = Vector3.zero; 
        rb.AddForce(bullet.transform.up * bulletForce, ForceMode.Impulse);
    }
}


