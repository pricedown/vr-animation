using UnityEngine;

namespace pricenerds3D
{
    public abstract class P3D_IKSolver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        protected P3D_RigInstance _rigInstance;

        public abstract void SolveIK();
    }

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

        private void Start()
        {
            jointAffected = _rigInstance.rig.GetJointFromName(_lookAtAffectedName);
            jointAffectedIndex = jointAffected.m_jointIndex;
        }

        private void Update()
        {
            SolveIK();  
        }

        public override void SolveIK()
        {
            Quaternion worldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].rotation;
            Quaternion parentWorldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffected.m_parentIndex].rotation;

            Vector3 jointWorldPosition = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].MultiplyPoint3x4(Vector3.zero);
            Vector3 dirToTargetWorld = (_lookAtEffector.position - jointWorldPosition).normalized;

            Quaternion lookRot = Quaternion.LookRotation(dirToTargetWorld, Vector3.up);
            _rigInstance.deltaPose.m_localPose[jointAffectedIndex].m_jointRotation = lookRot;

            /*
            // get world rotations
            Quaternion worldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].rotation;
            Quaternion parentWorldRotation = _rigInstance.deltaPose.m_worldSpace[jointAffected.m_parentIndex].rotation;

            // HELPER CUBE
            Vector3 relative = _lookAtEffector.localPosition - _helperCube.transform.localPosition;
            Quaternion rot = Quaternion.LookRotation(relative, Vector3.up);
            _helperCube.transform.rotation = rot;

            // direction in world space
            //Vector3 jointWorldPosition = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].MultiplyPoint3x4(Vector3.zero);
            
            Matrix4x4 effectorJointLocalSpace = Matrix4x4.TRS(_lookAtEffector.transform.position, _lookAtEffector.transform.rotation, _lookAtEffector.transform.localScale) * Matrix4x4.Inverse(_rigInstance.deltaPose.m_worldSpace[jointAffectedIndex]);
            //Vector3 dirToTargetWorld = (_lookAtEffector.position - jointWorldPosition).normalized;
            Vector3 dirToTargetLocal = (effectorJointLocalSpace.MultiplyPoint3x4(Vector3.zero) - _rigInstance.deltaPose.m_localPose[jointAffectedIndex].m_jointTranslation).normalized;


            //Debug.DrawRay(jointWorldPosition, dirToTargetWorld.normalized, Color.red);
            //Debug.DrawRay(jointWorldPosition, Vector3.Cross(dirToTargetWorld.normalized, Vector3.up).normalized, Color.green);

            // calculate rotations
            //Vector3 forward = worldRotation * Vector3.forward;
            Quaternion lookRotLocal = Quaternion.LookRotation(dirToTargetLocal, Vector3.up);
            //Quaternion fromToRot = Quaternion.FromToRotation(forward, dirToTarget);
            //Quaternion lookRotLocal = Quaternion.Inverse(parentWorldRotation) * lookRotWorld * worldRotation;

            // apply in joint local space
            //_rigInstance.deltaPose.m_localPose[jointAffectedIndex].m_jointRotation = lookRotLocal;
            //_rigInstance.deltaPose.m_localPose[jointAffectedIndex].m_jointRotation = _helperCube.transform.rotation;

            // i think this needs to be different :p
            //_rigInstance.deltaPose.SolveFK();

            /*
             *          Matrix4x4 localPoseMatrix = Matrix4x4.TRS(
                            m_localPose[i].m_jointTranslation,
                            m_localPose[i].m_jointRotation,
                            m_localPose[i].m_jointScale);

                        if (joint.m_parentIndex == -1) m_worldSpace[i] = localPoseMatrix;
                        else m_worldSpace[i] = m_worldSpace[joint.m_parentIndex] * localPoseMatrix;*/

            /*
            Matrix4x4 localPoseMatrix = Matrix4x4.TRS(
               _helperCube.transform.localPosition,
               _helperCube.transform.localRotation,
               _helperCube.transform.localScale);

            _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex] = localPoseMatrix;
            //_rigInstance.deltaPose.m_worldSpace[jointAffectedIndex]

            Vector3 forward = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].GetColumn(2);
Vector3 upwards = _rigInstance.deltaPose.m_worldSpace[jointAffectedIndex].GetColumn(1);

            _helperCube2.transform.rotation = Quaternion.LookRotation(forward, upwards);*/

            // get world rotations
            /*
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
            _rigInstance.deltaPose.m_localPose[jointAffectedIndex].m_jointRotation = localRotation;*/

            // i think this needs to be different :p
            //_rigInstance.deltaPose.SolveFK();
        }
    }
}