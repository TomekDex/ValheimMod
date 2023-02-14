using UnityEngine;

namespace TomekDexValheimMod
{
    public class ContainerOnDestroy : MonoBehaviour
    {
        private Container _container;

        public void Awake()
        {
            _container = GetComponent<Container>();
        }

        public void OnDestroy()
        {
            ContainerQuickAccess.RemoveContainer(_container);
        }
    }
}
