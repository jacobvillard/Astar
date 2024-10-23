using UnityEngine;
using static Interfaces.Interfaces;

[RequireComponent(typeof(Collider))]
public class Border : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        var damageable = other.GetComponent<IDamageable>();
        damageable?.TakeDamage(100);
    }
}
