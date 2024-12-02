using UnityEngine;

namespace Interfaces {
    /// <summary>
    /// Contains all interfaces used in the game
    /// </summary>
    public class Interfaces : MonoBehaviour {
        /// <summary>
        /// Interface for any entity that can take damage
        /// </summary>
        public interface IDamageable {
            void TakeDamage(float amount);
        }
    }
}
