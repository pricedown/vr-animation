using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    /// The PoleIK solver is based on the geometric implementation we completed in class, along with the FABRIK solution.
    /// You'll be able to switch between both to see the differences!
    /// </summary>
    public class P3D_PoleIKSolver : MonoBehaviour
    {
        public enum EPoleIKSolution
        {
            Geometric,
            FABRIK
        }

        [Header("References")]
        [SerializeField] 
        private P3D_RigDebug _rigInstance; // replace with P3D_RigInstance when ready

        [Header("Pole IK Settings")]
        [SerializeField]
        private EPoleIKSolution _poleSolution;
        [SerializeField] 
        private int _jointsAffected;
        [SerializeField, Tooltip("The chain creator will start from this joint and work backwards to the parent joints to build the chain")] 
        private string _jointPoleEndAffector;
        [SerializeField] 
        private Transform _poleTargetEffector;
        [SerializeField, Range(0, 1)] 
        private float _weight;

        private P3D_Joint[] jointIKChain;
        private float[] boneLengths;
        private float targetDistance;
        private float totalChainLength;

        private void Start()
        {
            InitializeChain();
            CalculateBoneLengths();
        }

        private void InitializeChain()
        {
            // initialize chain
            jointIKChain = new P3D_Joint[_jointsAffected];
            jointIKChain[0] = _rigInstance.basePose.m_rig.GetJointFromName(_jointPoleEndAffector); // end effector

            // add all joints to the chain, working up the hierarchy from the end affector
            for (int i = 1; i < _jointsAffected; i++)
            {
                sbyte parentIndex = jointIKChain[i - 1].m_parentIndex;

                // IMPORTANT: not sure if we use base pose here !! may change later
                jointIKChain[i] = _rigInstance.basePose.m_rig.m_joints[parentIndex];
            }
        }

        private void CalculateBoneLengths()
        {
            boneLengths = new float[_jointsAffected];

            for(int i = 0; i < _jointsAffected; i++)
            {
                // get self to end positions in world space
                Vector3 start = _rigInstance.basePose.m_worldSpace[jointIKChain[i].m_jointIndex].MultiplyPoint3x4(Vector3.zero);
                Vector3 end = _rigInstance.basePose.m_worldSpace[jointIKChain[i].m_parentIndex].MultiplyPoint3x4(Vector3.zero);

                // calculate length
                float length = Vector3.Distance(start, end);
                boneLengths[i] = length;
                totalChainLength += length;
            }
        }

        private void Update()
        {
            switch (_poleSolution)
            {
                case EPoleIKSolution.FABRIK:
                    FABRIK_SolveIK();
                    break;
                default:
                    Debug.LogWarning($"No IK solution implemented for {_poleSolution}");
                    break;
            }
        }

        #region FABRIK
        private void FABRIK_UpdateBackward()
        {

        }

        private void FABRIK_UpdateForward()
        {

        }

        private void FABRIK_SolveIK()
        {
            // 1. get joint positions

            // 2. calculate distance from the root to the IK target

            // 3. check if the target is reachable

            // 4. apply pole constraint

            // 
        }
        #endregion
    }
}