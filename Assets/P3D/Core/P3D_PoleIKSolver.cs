using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

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
        //[SerializeField]
        //private Transform _rigTest2;

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

        private float angle;

        private void Start()
        {
            solverPositions = new Vector3[_jointsAffected];

            // its important that we do this in the start method because the rig is built in the Awake() method
            InitializeChain();
            CalculateBoneLengths();

            //StartCoroutine(ITest());
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
            //FABRIK_SolveIK();

            // Test
        }

        /*
        private IEnumerator ITest()
        {
            yield return null;

            List<Transform> helper = _rigTest2.GetAllChildren();
            float duration = 0.5f;

            for (int i = 0; i < _rigInstance.deltaPose.m_rig.m_jointCount; i++)
            {
                DOVirtual.Float(0.0f, 45.0f, 3.0f, (x) => angle = x);
                //Vector3 localAcis = _rigInstance.deltaPose.m_localPose[i].m_jointRotation.

                float elapsed = 0.0f;
                while (elapsed < duration)
                {
                    Vector3 forward = _rigInstance.deltaPose.m_worldSpace[i].GetColumn(2);
                    Vector3 upwards = _rigInstance.deltaPose.m_worldSpace[i].GetColumn(1);

                    _rigInstance.deltaPose.m_localPose[i].m_jointRotation = Quaternion.AngleAxis(angle, forward);
                    _rigInstance.deltaPose.SolveFK();

                    forward = _rigInstance.deltaPose.m_worldSpace[i].GetColumn(2);
                    upwards = _rigInstance.deltaPose.m_worldSpace[i].GetColumn(1);

                    helper[i].transform.rotation = Quaternion.LookRotation(forward, upwards);

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            StartCoroutine(ITest());
        }*/

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