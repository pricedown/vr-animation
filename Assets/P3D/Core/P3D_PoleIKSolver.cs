using NUnit.Framework.Interfaces;
using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    ///     The PoleIK solver is based on the geometric implementation we completed in class, along with the FABRIK solution.
    ///     You'll be able to switch between both to see the differences!
    /// </summary>
    public class P3D_PoleIKSolver : P3D_IKSolver
    {
        [Header("References")] 
        [SerializeField]
        private P3D_RigInstance _rigInstance; // TODO: replace with P3D_RigInstance when ready

        [Header("Pole IK Settings")]
        [SerializeField]
        [Tooltip( "The chain creator will start from this joint and work backwards to the parent joints to build the chain")]
        private string _jointEndAffected; // the last joint effected

        [SerializeField] 
        private Transform _endEffectorTarget;

        [SerializeField] 
        private Transform _poleTargetEffector;

        [SerializeField] [Range(0, 1)] 
        private float _weight;

        [SerializeField] [Range(0, 1)]
        private float _endRotationWeight;

        private P3D_Joint ikChainEnd;
        private P3D_Joint ikChainBase;
        private P3D_Joint ikChainHinge;

        public override void InitializeIK()
        {
            InitializeChain();
        }

        private void InitializeChain()
        {
            // initialize joint chain references
            ikChainEnd = _rigInstance.rig.m_basePose.m_rig.GetJointFromName(_jointEndAffected);
            ikChainHinge = _rigInstance.rig.m_basePose.m_rig.m_joints[ikChainEnd.m_parentIndex];
            ikChainBase = _rigInstance.rig.m_basePose.m_rig.m_joints[ikChainHinge.m_parentIndex];
        }

        public override void SolveIK()
        {
            Vector3 hingeSolved, endSolved;

            // get the hierarchy root object transform relative ot the rig
            Matrix4x4 worldToRigLocal = _rigInstance.deltaPose.m_worldSpaceInverse[0];

            Vector3 effectorRigLocal = _rigInstance.transform.TransformPoint(_endEffectorTarget.transform.position);
            Vector3 poleRigLocal = _rigInstance.transform.TransformPoint(_poleTargetEffector.transform.position);

            // get affected joints relative to world
            Matrix4x4 worldHingeMatrix = _rigInstance.deltaPose.m_worldSpace[ikChainHinge.m_jointIndex];
            Matrix4x4 worldBaseMatrix = _rigInstance.deltaPose.m_worldSpace[ikChainBase.m_jointIndex];
            Matrix4x4 worldEndMatrix = _rigInstance.deltaPose.m_worldSpace[ikChainEnd.m_jointIndex];

            // affected joint positions in rig
            Vector3 worldBasePosition = worldBaseMatrix.MultiplyPoint3x4(Vector3.zero);
            Vector3 worldHingePosition = worldHingeMatrix.MultiplyPoint3x4(Vector3.zero);
            Vector3 worldEndPosition = worldEndMatrix.MultiplyPoint3x4(Vector3.zero);

            // calculate bone lengths
            float upperDist = Vector3.Distance(worldBasePosition, worldHingePosition);
            float lowerDist = Vector3.Distance(worldHingePosition, worldEndPosition);
            float totalDist = upperDist + lowerDist;

            Vector3 baseToEffector = effectorRigLocal - worldBasePosition;
            float effectorDist = baseToEffector.magnitude;
            Vector3 dirBaseToEffector = baseToEffector / effectorDist;

            Vector3 baseToPole = poleRigLocal - worldBasePosition;
            Vector3 normal = Vector3.Cross(dirBaseToEffector, baseToPole).normalized;

            // unreachable case (simple solution) fully stretches towards effector
            if (effectorDist >= totalDist)
            {
                endSolved = worldBasePosition + dirBaseToEffector * totalDist;
                hingeSolved = worldBasePosition + dirBaseToEffector * upperDist;
            }
            else
            {
                // herons formula
                float s = (upperDist + lowerDist + effectorDist) * 0.5f;
                float area = Mathf.Sqrt(s * (s - upperDist) * (s - lowerDist) * (s - effectorDist));
                float height = (2.0f * area) / effectorDist;
                float dist = (upperDist * upperDist - lowerDist * lowerDist + effectorDist * effectorDist) / (2.0f * effectorDist);

                Vector3 offset = worldBasePosition + dirBaseToEffector * dist;
                hingeSolved = offset - normal * height;

                // End effector is target
                endSolved = effectorRigLocal;
            }

            Quaternion baseFK = _rigInstance.deltaPose.m_localPose[ikChainBase.m_jointIndex].m_jointRotation;
            Quaternion hingeFK = _rigInstance.deltaPose.m_localPose[ikChainHinge.m_jointIndex].m_jointRotation;
            Quaternion endFK = _rigInstance.deltaPose.m_localPose[ikChainEnd.m_jointIndex].m_jointRotation;

            Quaternion baseRot = Quaternion.FromToRotation((worldHingePosition - worldBasePosition).normalized, (hingeSolved - worldBasePosition).normalized);
            Quaternion hingeRot = Quaternion.FromToRotation((worldEndPosition - worldHingePosition).normalized, (endSolved - hingeSolved).normalized);

            Quaternion baseWorldRot = baseRot * worldBaseMatrix.rotation;
            Quaternion hingeWorldRot = hingeRot * worldHingeMatrix.rotation;

            // calculate base and hinge rotation in local space
            Quaternion baseLocalRot = Quaternion.Inverse(_rigInstance.deltaPose.m_worldSpace[ikChainBase.m_parentIndex].rotation) * baseWorldRot;
            Quaternion hingeLocalRot = Quaternion.Inverse(baseWorldRot) * hingeWorldRot;

            // calculate end rotation, preserve forward
            Quaternion endLocalRot = Quaternion.Inverse(hingeWorldRot) * _endEffectorTarget.transform.rotation;

            // blend rotations based on weight
            Quaternion blendedBase = Quaternion.Slerp(baseFK, baseLocalRot, _weight);
            Quaternion blendedHinge = Quaternion.Slerp(hingeFK, hingeLocalRot, _weight);
            Quaternion blendedEnd = Quaternion.Slerp(endFK, endLocalRot, _endRotationWeight);

            _rigInstance.deltaPose.m_localPose[ikChainBase.m_jointIndex].m_jointRotation = blendedBase;
            _rigInstance.deltaPose.m_localPose[ikChainHinge.m_jointIndex].m_jointRotation = blendedHinge;
            _rigInstance.deltaPose.m_localPose[ikChainEnd.m_jointIndex].m_jointRotation = blendedEnd;
        }

        public void OnDrawGizmosSelected()
        {
            // show the IK triangle being formed
        }
    }
}