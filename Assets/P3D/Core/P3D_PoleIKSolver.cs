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

        private float angle;
        private float[] boneLengths;

        private P3D_Joint ikChainEnd;
        private P3D_Joint ikChainBase;
        private P3D_Joint ikChainHinge;

        private Vector3[] solverPositions;
        private float targetDistance;
        private float totalChainLength;

        public override void InitializeIK()
        {
            solverPositions = new Vector3[3];

            // its important that we do this in the start method because the rig is built in the Awake() method
            InitializeChain();
            CalculateBoneLengths();
        }

        /// <summary>
        ///     Initializes the chain from the end affector and moving up the chain by the _jointsAffected
        ///     (FUTURE) - we need some kind of error checking to ensure we dont choose too many bones to affect
        /// </summary>
        private void InitializeChain()
        {
            // initialize chain

            /*
            jointIKChain = new P3D_Joint[3];
            jointIKChain[0] =
                _rigInstance.rig.m_basePose.m_rig.GetJointFromName(_jointEndAffected); // set end affector first

            // add all joints to the chain, working up the hierarchy from the end affector
            for (var i = 1; i < 3; i++)
            {
                var parentIndex = jointIKChain[i - 1].m_parentIndex;

                // IMPORTANT: not sure if we use base pose here !! may change later
                jointIKChain[i] = _rigInstance.rig.m_basePose.m_rig.m_joints[parentIndex];
            }*/
        }

        /// <summary>
        ///    Calculates every distance between the bones in the jointIKChain we initialized
        /// </summary>
        private void CalculateBoneLengths()
        {
            boneLengths = new float[2];
            totalChainLength = 0.0f;

            for (var i = 0; i < 2; i++)
            {
                /*
                // get self to end positions in world space
                var start = _rigInstance.rig.m_basePose.m_worldSpace[jointIKChain[i].m_jointIndex]
                    .MultiplyPoint3x4(Vector3.zero);
                var end = _rigInstance.rig.m_basePose.m_worldSpace[jointIKChain[i + 1].m_jointIndex]
                    .MultiplyPoint3x4(Vector3.zero);

                // calculate length
                var length = Vector3.Distance(start, end);
                boneLengths[i] = length;
                totalChainLength += length;*/
            }
        }

        public override void SolveIK()
        {
            // geometric solution using Heron's formula

            // 1. get affected joints relative to rig
            // 2. affected joint positions in rig
            // 3. get effector and constraint positions relative to rig
            // effector dist >= max dist? 
            //    end goes to farthest possible point, hinge also easy to solve
            // otherwise, 

            // Heron's formula
            // A = sqrt(s(s - B)(s - L1)(s - L2))
            // s = 1/2(B + L1 + L2)
            // A = 1/2(BH) -> H = 2A/B

            // c = e(pole) - e(base) // pole displacement
            // d = e(end) - e(base) // effector displacement
            // n = d x c


        }

        public void OnDrawGizmosSelected()
        {
            // show the IK triangle being formed
        }
    }
}