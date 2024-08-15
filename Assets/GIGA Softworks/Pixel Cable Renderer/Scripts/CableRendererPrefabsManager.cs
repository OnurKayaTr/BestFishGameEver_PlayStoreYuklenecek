using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GIGA.PixelCableRenderer
{
    public class CableRendererPrefabsManager : ScriptableObject
    {
#if UNITY_EDITOR
        public CableRenderer prefab_cable;
        public CableRenderer prefab_cable_canvas;
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CableRendererPrefabsManager))]
    public class CableRendererPrefabsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This scriptable contains reference to instantiate cable renderers from the context menu. If you move the assets from the default folder, change the prefabManagerPath path in CableRendererMenu.cs ", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
#endif
}