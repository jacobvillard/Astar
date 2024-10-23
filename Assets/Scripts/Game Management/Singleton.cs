using UnityEngine;

/// <summary>
/// Singleton Generic
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour{
    private static T _instance;
    public static T Instance {
        get {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<T>();
            if (_instance == null) {
                var obj = new GameObject(typeof(T).Name);
                _instance = obj.AddComponent<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = this as T;
        }
        else if (_instance != this) {
            Destroy(gameObject);
        }
    }
}