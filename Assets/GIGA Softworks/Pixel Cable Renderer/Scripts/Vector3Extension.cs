using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGA.PixelCableRenderer
{
    public static class Vector3Extension
    {
        public static float Magnitude2D(this Vector3 vec)
        {
            return Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y);
        }
    }
}
