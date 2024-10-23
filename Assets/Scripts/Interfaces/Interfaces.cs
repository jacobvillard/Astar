using UnityEngine;

namespace Interfaces
{
    public class Interfaces : MonoBehaviour
    {
        public interface IDamageable
        {
            void TakeDamage(float amount);
        }
    }
}
