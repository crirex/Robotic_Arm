using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    #region Private members
    private static bool shuttingDown = false; // Check to see if we're about to be destroyed.
    private static readonly object locker = new object(); // Protection against multithreading
    private static T instance;
    #endregion

    #region Methods
    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (shuttingDown)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed. Returning null.");
                return null;
            }

            lock (locker)
            {
                if (instance == null)
                {
                    // Search for existing instance.
                    instance = (T)FindObjectOfType(typeof(T));

                    // Create new instance if one doesn't already exist.
                    if (instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        // Make instance persistent.
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return instance;
            }
        }
    }


    private void OnApplicationQuit()
    {
        shuttingDown = true;
    }


    private void OnDestroy()
    {
        shuttingDown = true;
    }
    #endregion
}
