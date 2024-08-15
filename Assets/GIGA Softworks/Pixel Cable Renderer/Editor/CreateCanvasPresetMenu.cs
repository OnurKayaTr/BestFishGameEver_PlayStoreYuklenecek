using GIGA.PixelCableRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CreateCanvasPresetMenu : MonoBehaviour
{
    [MenuItem("Assets/Create/Cable Renderer/Canvas Preset Variant", false, 1)]
    private static void CreateCanvasPresetVariant()
    {
        if (Selection.activeObject is GameObject && ((GameObject)Selection.activeObject).GetComponent<CableRenderer>())
        {
            GameObject originalPreset = (GameObject)Selection.activeObject;
            string newPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            newPath = newPath.Replace(".prefab", "_canvas.prefab");

            // Checking if already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(newPath) != null)
            {
                EditorUtility.DisplayDialog("Error", $"Asset {newPath} already exists.", "OK");
                return;
            }

            // Getting the prefab parent from the variation
            GameObject originalPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource<GameObject>((GameObject)Selection.activeObject);

            // Making sure it is not alraeady a canvas prefab
            if (originalPrefab.GetComponent<RectTransform>() != null)
            {
                EditorUtility.DisplayDialog("Error", "This is already a canvas prefab.", "OK");
                return;
            }

            // Loading the canvas version
            string originalPrefabPath = AssetDatabase.GetAssetPath(originalPrefab);
            string canvasPrefabPath = originalPrefabPath.Replace(".prefab", "_canvas.prefab");
            GameObject canvasBasePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(canvasPrefabPath);

            // Creating prefab variant from canvas base prefab
            GameObject objSource = (GameObject)PrefabUtility.InstantiatePrefab(canvasBasePrefab);
            GameObject newAsset = PrefabUtility.SaveAsPrefabAsset(objSource, newPath);

            // Making sure the new prefab has the required components
            if (newAsset.GetComponent<Image>() == null)
                newAsset.gameObject.AddComponent<Image>();
            newAsset.GetComponent<Image>().material = newAsset.GetComponent<CableRenderer>().defaultCableMaterial;
            if (newAsset.GetComponent<CableRendererMeshEffect>() == null)
                newAsset.gameObject.AddComponent<CableRendererMeshEffect>();

            // Setting canvas scaling
            newAsset.GetComponent<RectTransform>().localScale = new Vector2(originalPreset.transform.localScale.x * CableRenderer.CANVAS_RESIZE_FACTOR, originalPreset.transform.localScale.y * CableRenderer.CANVAS_RESIZE_FACTOR);

            // Copying values from origina preset to the new one
            if (originalPreset.GetComponent<CableRenderer>() != null)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(originalPreset.GetComponent<CableRenderer>());
                UnityEditorInternal.ComponentUtility.PasteComponentValues(newAsset.GetComponent<CableRenderer>());
            }

            GameObject.DestroyImmediate(objSource);
            AssetDatabase.SaveAssets();

        }
        else
        {
            EditorUtility.DisplayDialog("Error","This function can be used on CableRenderer objects only.","OK");
        }

    }

}
