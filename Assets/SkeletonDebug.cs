using System;
using UnityEngine;

namespace pricenerds3D
{
    public class P3D_SkeletonDebug : MonoBehaviour
    {
        [SerializeField]
        private Transform _sampleHierarchy;

        [Header("Gizmos")]
        [SerializeField]
        private bool _gizmosEnabled = true;
        [SerializeField]
        private Color _boneColor = Color.blue;

        P3D_Skeleton skeleton;
        P3D_SkeletonPose basePose;

        private void OnEnable()
        {
            // this skeleton should be created at import instead of now. just putting it here for now   
            InitializeSkeleton();
            basePose = new P3D_SkeletonPose(skeleton);
        }

        // This is essentially a backwards way of creating a skeleton. We already have the skeleton using GameObjects for testing purposes. We have to replace this later with a custom importer
        public void InitializeSkeleton()
        {
            skeleton = new P3D_Skeleton((UInt32)GetAllChildrenCount(_sampleHierarchy));

            sbyte currentIndex = 0;
            CreateSkeletonFromHierarchy(_sampleHierarchy.GetChild(0), ref currentIndex, -1, skeleton);
        }
        
        // This function is responsible for recursively creating a skeleton from a Transform hierarchy
        public void CreateSkeletonFromHierarchy(Transform current, ref sbyte currentIndex, sbyte parentIndex, P3D_Skeleton skeleton)
        {
            // keep track of the current index to send to children to use as the parentIndex
            sbyte self = currentIndex;
            skeleton.m_joints[self] = CreateJointByGameObject(current, parentIndex);

            // keeps track of where we are in the array
            currentIndex++;

            // Recursive step to continue filling out the skeleton
            for(int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                CreateSkeletonFromHierarchy(child, ref currentIndex, self, skeleton);
            }
        }

        // Helper function that gets all children of a hierarchy
        public int GetAllChildrenCount(Transform parentObject)
        {
            int sum = 0;

            for (int i = 0; i < parentObject.childCount; i++)
            {
                Transform child = parentObject.GetChild(i);
                sum++;

                if (child.childCount > 0)
                {
                    sum += GetAllChildrenCount(child);
                }
            }

            return sum;
        }

        // Helper debug function that creates a joint based on a passed GameObject
        public P3D_Joint CreateJointByGameObject(Transform transform, sbyte parentIndex)
        {
            P3D_Joint joint = new P3D_Joint();

            joint.m_name = transform.name;
            joint.m_parentIndex = parentIndex;
            joint.m_localPosition = transform.transform.localPosition;
            joint.m_localRotation = transform.transform.localRotation;
            joint.m_localScale = transform.transform.localScale;

            return joint;
        }

        public void OnDrawGizmos()
        {
            if (!_gizmosEnabled) return;
            if (skeleton == null)
            {
                Debug.LogWarning("Failed to draw skeleton gizmos! Skeleton is null.");
                return;
            }

            Gizmos.color = _boneColor;

            // test
            for(int i = 0; i < skeleton.m_jointCount; i++)
            {
                sbyte parentIndex = basePose.m_skeleton.m_joints[i].m_parentIndex;

                if(parentIndex == -1) continue;

                // NOTES for joe!!
                // MultiplyPoint3x4 is a cool function, it's optimized for what we want!!!
                // we will only ever make affine transformations, which is what MultiplyPoint3x4 accels at
                // the function wipes out the last row which is always constant [0 0 0 1]
                // MultiplyPoint3x4 also can be used to just quickly extract the translation component because you can just zero out the rotation and scale properties to get translation back

                Vector3 start = basePose.m_worldSpace[parentIndex].MultiplyPoint3x4(Vector3.zero);
                Vector3 end = basePose.m_worldSpace[i].MultiplyPoint3x4(Vector3.zero);

                P3D_SkeletonHelpers.P3D_DrawBoneGizmo(start, end);
            }
        }
    }
}