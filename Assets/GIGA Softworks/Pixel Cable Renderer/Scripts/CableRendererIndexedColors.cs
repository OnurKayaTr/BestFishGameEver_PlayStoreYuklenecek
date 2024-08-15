using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGA.PixelCableRenderer
{
    [CreateAssetMenu(fileName = "CableIndexedColors", menuName = "Cable Renderer/Indexed Colors Data", order = 1)]
    public class CableRendererIndexedColors : ScriptableObject
    {
        public Color[] indexedColors = new Color[16];       // If you want to use a different color count, you must modify the indexedColors array size inside the shader
    }

}
