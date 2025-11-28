using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    /// The LookAtIK solver is based on the animal3D implementation we completed in class
    /// </summary>
    public class P3D_LookAtIKSolver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private P3D_RigDebug _rigInstance; // replace with P3D_RigInstance when ready

        [Header("LookAt Settings")]
        [SerializeField]
        private Transform _lookAtEffector;
        [SerializeField] 
        private string _lookAtAffectedName;

        private void Update()
        {
            SolveIK();  
        }

        private void SolveIK()
        {
            // 1. world rotation of joint
            // 2. direction in world space
            // 3. find rotation delta
            // apply in joint local space
        }
    }
}