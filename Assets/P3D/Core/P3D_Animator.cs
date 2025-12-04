using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D 
{
    public class P3D_Animator : MonoBehaviour
    {
        [SerializeField] 
        private P3D_RigInstance _rigInstance;
        [SerializeField] 
        private P3D_ClipData _debugClip;

        private P3D_KeyframeAnimationController controller;
        private P3D_ClipController clipController;

        private void Start()
        {
            controller = new P3D_KeyframeAnimationController();
            clipController = new P3D_ClipController();
        }

        private void Update()
        {
            
        }
    }
}
