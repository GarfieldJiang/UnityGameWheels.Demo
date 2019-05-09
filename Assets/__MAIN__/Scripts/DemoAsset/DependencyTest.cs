using COL.UnityGameWheels.Unity;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    public class DependencyTest : MonoBehaviourEx
    {
        #pragma warning disable 414
        [SerializeField] private Object[] m_Dependencies = null;
        #pragma warning restore 414
    }
}