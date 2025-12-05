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

        private P3D_Joint ikChainEnd;
        private P3D_Joint ikChainBase;
        private P3D_Joint ikChainHinge;

        private float targetDistance;
        private float totalChainLength;

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

        public override void SolveIK(float weight)
        {
            // get the hierarchy root object transform relative ot the rig
            Matrix4x4 worldToRigLocal = _rigInstance.deltaPose.m_worldSpaceInverse[0];

            // 1. get affected joints relative to world
            Matrix4x4 worldEndMatrix = _rigInstance.deltaPose.m_worldSpace[ikChainEnd.m_jointIndex];
            Matrix4x4 worldHingeMatrix = _rigInstance.deltaPose.m_worldSpace[ikChainHinge.m_jointIndex];
            Matrix4x4 worldBaseMatrix = _rigInstance.deltaPose.m_worldSpace[ikChainBase.m_jointIndex];

            // 2. affected joint positions in rig
            Vector3 endWorldPosition = worldEndMatrix.MultiplyPoint3x4(Vector3.zero);
            Vector3 baseWorldPosition = worldBaseMatrix.MultiplyPoint3x4(Vector3.zero);
            Vector3 hingeWorldPosition = worldHingeMatrix.MultiplyPoint3x4(Vector3.zero);

            // 3. get effector and constraint positions relative to rig
            Matrix4x4 j2RigEndAffected = worldToRigLocal * worldEndMatrix;
            Matrix4x4 j2RigHingeAffected = worldToRigLocal * worldHingeMatrix;
            Matrix4x4 j2RigBaseAffected = worldToRigLocal * worldBaseMatrix;

            Vector3 endAffectedRigLocalPosition = j2RigEndAffected.MultiplyPoint3x4(Vector3.zero);
            Vector3 hingeAffectedRigLocalPosition = j2RigHingeAffected.MultiplyPoint3x4(Vector3.zero);
            Vector3 baseAffectedRigLocalPosition = j2RigBaseAffected.MultiplyPoint3x4(Vector3.zero);

            //Vector3 effectorRigLocalSpacePosition = worldToRigLocal.MultiplyPoint3x4(_endEffectorTarget.transform.position);
            //Vector3 poleRigLocalSpacePosition = worldToRigLocal.MultiplyPoint3x4(_poleTargetEffector.transform.position);

            Vector3 effectorRigLocalSpacePosition = _rigInstance.transform.TransformPoint(_endEffectorTarget.transform.position);
            Vector3 poleRigLocalSpacePosition = _rigInstance.transform.TransformPoint(_poleTargetEffector.transform.position);

            Vector3 baseToEffector = effectorRigLocalSpacePosition - baseAffectedRigLocalPosition;
            Vector3 baseToPole = poleRigLocalSpacePosition - baseAffectedRigLocalPosition;

            float upperDist, lowerDist, effectorDist, maxDist;
            upperDist = Vector3.Distance(baseAffectedRigLocalPosition, hingeAffectedRigLocalPosition);
            lowerDist = Vector3.Distance(hingeAffectedRigLocalPosition, endAffectedRigLocalPosition);

            Vector3 normal = Vector3.Cross(baseToPole, baseToEffector).normalized;
            effectorDist = baseToEffector.magnitude;
            maxDist = upperDist + lowerDist;

            // effector dist >= max dist? 
            //    end goes to farthest possible point, hinge also easy to solve
            if (effectorDist >= maxDist)
            {
                endAffectedRigLocalPosition = baseToEffector * maxDist;
                baseAffectedRigLocalPosition += endAffectedRigLocalPosition;

                hingeAffectedRigLocalPosition = baseToEffector * upperDist;
                baseAffectedRigLocalPosition += hingeAffectedRigLocalPosition;
            }
            else
            {

            }
            // otherwise, 

            // Heron's formula
            // A = sqrt(s(s - B)(s - L1)(s - L2))
            // s = 1/2(B + L1 + L2)
            // A = 1/2(BH) -> H = 2A/B

            // c = e(pole) - e(base) // pole displacement
            // d = e(end) - e(base) // effector displacement
            // n = d x c


            // check if base pose is all zeros
            // apply correction, use the first key frame as the base pose instead
            // copy first keyrame to the base, subtrack base from all of those (ON LOAD you correct all of the keyframes)
            // make sure you're not adding the base pose twice
        }

        public void OnDrawGizmosSelected()
        {
            // show the IK triangle being formed
        }
    }
}