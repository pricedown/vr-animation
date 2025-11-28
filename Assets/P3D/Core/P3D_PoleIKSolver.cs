using UnityEngine;

namespace pricenerds3D
{
    public class P3D_PoleIKSolver : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] 
        private P3D_RigDebug _rigInstance; // replace with P3D_RigInstance when ready

        [Header("Pole IK Settings")]
        [SerializeField] 
        private int _jointsAffected;
        [SerializeField] 
        private string _jointPoleAffected; 
        [SerializeField] 
        private Transform _poleTargetEffector;
        [SerializeField, Range(0, 1)] 
        private float _weight;

        private P3D_Joint[] jointIKChain;
        private float[] boneLengths;
        private float targetDistance;

        private void Awake()
        {
            CalculateBoneLengths();
        }

        private void CalculateBoneLengths()
        {


            for(int i = 0; i < _jointsAffected; i++)
            {

            }
        }

        private void Update()
        {
            FABRIK_SolveIK();
        }

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
    }
}