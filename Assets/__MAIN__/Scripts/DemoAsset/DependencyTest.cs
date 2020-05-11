using COL.UnityGameWheels.Unity;
using UnityEngine;

namespace COL.UnityGameWheels.Demo
{
    public class DependencyTest : MonoBehaviourEx
    {
        [SerializeField]
        private Object[] m_Dependencies = null;

        public Object[] Dependencies => m_Dependencies;

        public void InitDependencies(int count)
        {
            m_Dependencies = new Object[count];
        }
    }
}