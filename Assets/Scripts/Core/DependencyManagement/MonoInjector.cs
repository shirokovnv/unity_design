using UnityEngine;

namespace Architecture
{
    public class MonoInjector : Injector<MonoBehaviour>
    {
        protected override MonoBehaviour[] CollectTargets()
        {
            return Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID);
        }
    }
}