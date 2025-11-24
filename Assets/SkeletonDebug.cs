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
                //Debug.Log(child.name);
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
            joint.test = transform;

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
            //P3D_SkeletonHelpers.P3D_DrawSkeletonGizmo(skeleton);

            // test
            for(int i = 0; i < skeleton.m_jointCount; i++)
            {
                sbyte parentIndex = skeleton.m_joints[i].m_parentIndex;

                if(parentIndex == -1) continue;
                Vector3 start = skeleton.m_joints[parentIndex].test.transform.position;
                Vector3 end = skeleton.m_joints[i].test.transform.position;

                P3D_SkeletonHelpers.P3D_DrawBoneGizmo(start, end);
            }
        }
    }
}