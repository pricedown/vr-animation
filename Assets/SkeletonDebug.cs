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

        private void Awake()
        {
            // this skeleton should be created at import instead of now. just putting it here for now   

            CreateRecursiveSkeleton();
        }

        // This is essentially a backwards way of creating a skeleton. We already have the skeleton using GameObjects for testing purposes. We have to replace this later with a custom importer
        public void CreateRecursiveSkeleton()
        {
            skeleton = new P3D_Skeleton((UInt32)GetAllChildrenCount(_sampleHierarchy));

            // add the root
            skeleton.m_joints[0] = CreateJointByGameObject(_sampleHierarchy, -1);
            //RetrieveJointData(_sampleHierarchy);
        }

        // Helper function that gets all children of a hierarchy
        public int GetAllChildrenCount(Transform parentObject)
        {
            int sum = 0;

            for (int i = 0; i < parentObject.childCount; i++)
            {
                Transform child = parentObject.GetChild(i);
                Debug.Log(child.name);
                sum++;

                if (child.childCount > 0)
                {
                    sum += GetAllChildrenCount(child);
                }
            }

            return sum;
        }

        /*
        public P3D_Skeleton AddBones(Transform search)
        {
            for(int i = 0; i < search.childCount; i++)
            {
                
            }
        }*/


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

            Gizmos.color = _boneColor;
            P3D_SkeletonHelpers.DrawBone(Vector3.zero, new Vector3(0, 1, 0));
        }

        private void DrawSkeleton(Transform parent)
        {
        }
    }
}