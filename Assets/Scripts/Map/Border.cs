using UnityEngine;
using static Interfaces.Interfaces;

namespace Map {
    /// <summary>
    /// Border class that destroys any object that enters it
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Border : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            var damageable = other.GetComponent<IDamageable>();
            damageable?.TakeDamage(100);
        }
    }
}
