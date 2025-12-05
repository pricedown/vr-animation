using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    /// The LookAtIK solver is based on the animal3D implementation we completed in class
    /// </summary>
    public class P3D_LookAtIKSolver : P3D_IKSolver
    {
        [Header("LookAt Settings")]
        [SerializeField]
        private Transform _lookAtEffector;
        [SerializeField] 
        private string _lookAtAffectedName;

        P3D_Joint jointAffected;
        int jointAffectedIndex;

        public override void InitializeIK()
        {
            jointAffected = _rigInstance.rig.GetJointFromName(_lookAtAffectedName);
            jointAffectedIndex = jointAffected.m_jointIndex;
        }

        public override void SolveIK(float weight)
        {
            Quaternion worldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].rotation;
            Quaternion parentWorldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffected.m_parentIndex].rotation;

            Vector3 jointWorldPosition = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].MultiplyPoint3x4(Vector3.zero);
            Vector3 dirToTargetWorld = (_lookAtEffector.position - jointWorldPosition).normalized;

            Quaternion lookRot = Quaternion.LookRotation(dirToTargetWorld, Vector3.up);
            _rigInstance.deltaPose.m_localPose[jointAffectedIndex].m_jointRotation = lookRot;
        }
    }
}