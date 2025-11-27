using System;
using UnityEngine;

namespace pricenerds3D
{
    [Serializable]
    public class P3D_ClipData : ScriptableObject
    {
        public readonly string clipName;

        public P3D_ClipData(string clipName)
        {
            this.clipName = clipName;
        }
    }
}
