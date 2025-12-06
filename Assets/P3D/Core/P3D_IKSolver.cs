using UnityEngine;

namespace pricenerds3D
{
    public abstract class P3D_IKSolver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        protected P3D_RigInstance _rigInstance;

        public abstract void InitializeIK();
        public abstract void SolveIK();
    }
}