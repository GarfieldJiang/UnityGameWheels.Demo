using System.Collections;
using COL.UnityGameWheels.Unity;
using COL.UnityGameWheels.Unity.Ioc;
using UnityEngine.SceneManagement;

namespace COL.UnityGameWheels.Demo
{
    public class Restarter : MonoBehaviourEx
    {
        protected override void Awake()
        {
            base.Awake();
            UnityApp.Instance.Container.OnShutdownComplete += OnShutdownComplete;
            UnityApp.Instance.Container.RequestShutdown();
        }

        private void OnShutdownComplete()
        {
            Destroy(UnityApp.Instance.gameObject);
            StartCoroutine(RestartCo());
        }

        IEnumerator RestartCo()
        {
            yield return null;
            yield return null;
            SceneManager.LoadScene(0);
        }
    }
}