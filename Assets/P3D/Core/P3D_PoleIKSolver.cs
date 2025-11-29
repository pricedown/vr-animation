using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    /// The PoleIK solver is based on the geometric implementation we completed in class, along with the FABRIK solution.
    /// You'll be able to switch between both to see the differences!
    /// </summary>
    public class P3D_PoleIKSolver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private P3D_RigDebug _rigInstance; // replace with P3D_RigInstance when ready

        [Header("Pole IK Settings")]
        [SerializeField] 
        private int _jointsAffected;
        [SerializeField, Tooltip("The chain creator will start from this joint and work backwards to the parent joints to build the chain")] 
        private string _jointEndAffected;
        [SerializeField]
        private Transform _endEffectorTarget;
        [SerializeField] 
        private Transform _poleTargetEffector;
        [SerializeField, Range(0, 1)] 
        private float _weight;

        private P3D_Joint[] jointIKChain;
        private Vector3[] solverPositions;
        private float[] boneLengths;
        private float targetDistance;
        private float totalChainLength;

        private void Start()
        {
            solverPositions = new Vector3[_jointsAffected];

            // its important that we do this in the start method because the rig is built in the Awake() method
            InitializeChain();
            CalculateBoneLengths();
        }
        
        /// <summary>
        /// This function initializes the chain, starting from the specified end affector and moving up the chain by the specified _jointsAffected
        /// (FUTURE) - we need some kind of error checking to ensure we dont choose too many bones to affect
        /// </summary>
        private void InitializeChain()
        {
            // initialize chain
            jointIKChain = new P3D_Joint[_jointsAffected];
            jointIKChain[0] = _rigInstance.rig.m_basePose.m_rig.GetJointFromName(_jointEndAffected); // set end affector first

            // add all joints to the chain, working up the hierarchy from the end affector
            for (int i = 1; i < _jointsAffected; i++)
            {
                int parentIndex = jointIKChain[i - 1].m_parentIndex;

                // IMPORTANT: not sure if we use base pose here !! may change later
                jointIKChain[i] = _rigInstance.rig.m_basePose.m_rig.m_joints[parentIndex];
            }
        }

        /// <summary>
        /// Takes the jointIKChain array we initialized and calculates the distance between them
        /// </summary>
        private void CalculateBoneLengths()
        {
            boneLengths = new float[_jointsAffected];

            for(int i = 0; i < _jointsAffected; i++)
            {
                // get self to end positions in world space
                Vector3 start = _rigInstance.rig.m_basePose.m_worldSpace[jointIKChain[i].m_jointIndex].MultiplyPoint3x4(Vector3.zero);
                Vector3 end = _rigInstance.rig.m_basePose.m_worldSpace[jointIKChain[i].m_parentIndex].MultiplyPoint3x4(Vector3.zero);

                // calculate length
                float length = Vector3.Distance(start, end);
                boneLengths[i] = length;
                totalChainLength += length;
            }
        }

        private void Update()
        {
            FABRIK_SolveIK();
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
            if (_weight == 0.0f) return;

            // get initial world space positions
            for (int i = 0; i < _jointsAffected; i++)
            {
                solverPositions[i] = _rigInstance.rig.m_basePose.m_worldSpace[jointIKChain[i].m_jointIndex].MultiplyPoint3x4(Vector3.zero);
            }

            FABRIK_UpdateForward();
            FABRIK_UpdateBackward();
        }
        #endregion
    }
}