using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    /// This class will be attached to the prefab that we auto-generate upon import
    /// It should ensure that the rig always moves relative to the TRS of the Unity hierarchy
    /// This does not manage animations, think of it more as a data container
    /// </summary>
    public class P3D_RigInstance : MonoBehaviour
    {
        public P3D_GeneratedSkeletonAsset data;
    }
}
