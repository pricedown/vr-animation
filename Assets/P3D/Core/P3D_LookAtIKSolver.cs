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
        [SerializeField]
        private GameObject _helperCube;

        P3D_Joint jointAffected;
        int jointAffectedIndex;

        private void Start()
        {
            jointAffected = _rigInstance.rig.GetJointFromName(_lookAtAffectedName);
            jointAffectedIndex = jointAffected.m_jointIndex;
        }

        private void Update()
        {
            SolveIK();  
        }

        private void SolveIK()
        {
            // get world rotations
            Quaternion worldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].rotation;
            Quaternion parentWorldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffected.m_parentIndex].rotation;

            // direction in world space
            Vector3 jointWorldPosition = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].MultiplyPoint3x4(Vector3.zero);
            Vector3 dirToTarget = (_lookAtEffector.position - jointWorldPosition).normalized;

            // calculate rotations
            Vector3 forward = worldRotation * Vector3.forward;
            Quaternion fromToRot = Quaternion.FromToRotation(forward, dirToTarget);
            Quaternion localRotation = Quaternion.Inverse(parentWorldRotation) * fromToRot * worldRotation;

            // apply in joint local space
            _rigInstance.deltaPose.m_localPose[jointAffectedIndex].m_jointRotation = localRotation;

            // i think this needs to be different :p
            _rigInstance.deltaPose.SolveFK();
        }
    }
}