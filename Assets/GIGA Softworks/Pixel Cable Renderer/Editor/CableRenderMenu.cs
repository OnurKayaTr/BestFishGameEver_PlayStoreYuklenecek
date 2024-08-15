#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using GIGA.PixelCableRenderer.Editor;
using GIGA.PixelCableRenderer;

public class CableRendererMenu : MonoBehaviour
{

	#region Prefab Creation
	
	private const string prefabManagerPath = "Assets/GIGA Softworks/Pixel Cable Renderer/Data/CableRendererPrefabsManager.asset"; // Change this if you move the assets in a different folder
	private static CableRendererPrefabsManager LocatePrefabManager() => AssetDatabase.LoadAssetAtPath<CableRendererPrefabsManager>(prefabManagerPath);
	
	// Create functions
	[MenuItem("GameObject/2D Object/Cable Renderer/SpriteRenderer/Cable", priority = 10)]
	static void CreateCable()
	{
		CreatePrefab(typeof(CableRenderer));
	}

	[MenuItem("GameObject/2D Object/Cable Renderer/CanvasRenderer/Cable", priority = 10)]
	static void CreateCanvasCable()
	{
		CreatePrefab(typeof(CableRenderer),true);
	}

	private static void CreatePrefab(Type prefabType,bool canvasVersion = false)
	{
		CableRendererPrefabsManager prefabsManager = LocatePrefabManager();
		GameObject instance = null;
		if (prefabsManager != null)
		{
			switch (prefabType.Name)
			{
				case "CableRenderer":
					if(!canvasVersion)
						instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabsManager.prefab_cable.gameObject, Selection.activeTransform);
					else
						instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabsManager.prefab_cable_canvas.gameObject, Selection.activeTransform);
					break;
				default:
					Debug.LogError("Invalid prefab type");
					break;
			}

			if (instance != null)
			{
				Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");
				Selection.activeObject = instance;
			}
			else
				EditorUtility.DisplayDialog("Error", "Couldn't find the prefabs " + prefabType.ToString() + ", if you moved the assets from the default folder please update CableRendererMenu.prefabManagerPath", "Ok");
		}
		else
			EditorUtility.DisplayDialog("Error", "Couldn't find the prefabsManager at " + prefabManagerPath + ", if you moved the assets from the default folder please update CableRendererMenu.prefabManagerPath","Ok");
	}


	#endregion


	[MenuItem("GameObject/2D Object/Cable Renderer/Settings", priority = 20)]
	static void OpenSettingsWindow()
	{
		CableRendererSettings.Init();
	}

	[MenuItem("GameObject/2D Object/Cable Renderer/About", priority = 100)]
	static void OpenAboutDialog()
	{
		CableRendererAboutDialog.Init();
	}

}
#endif
