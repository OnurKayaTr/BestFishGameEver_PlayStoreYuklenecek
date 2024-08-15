#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GIGA.PixelCableRenderer.Editor
{
	public class CableRendererSettings : EditorWindow
	{
		public enum SupportedHandleType {Circle,Dot,Sphere};

		public const string SETTING_STRING_HANDLESIZE = "GigaSoftworks.CableRenderer.HandleSize";
		public const string SETTING_STRING_HANDLETYPE = "GigaSoftworks.CableRenderer.HandleType";
		public const string SETTING_STRING_ENABLESNAP = "GigaSoftworks.CableRenderer.EnableSnap";
		public const string SETTING_STRING_SNAPTOGRID = "GigaSoftworks.CableRenderer.SnapToGrid";
		public const string SETTING_STRING_SNAPSIZE_X = "GigaSoftworks.CableRenderer.SnapSizeX";
		public const string SETTING_STRING_SNAPSIZE_Y = "GigaSoftworks.CableRenderer.SnapSizeY";

		public const float DEFAULT_HANDLESIZE = 0.25f;
		public const bool DEFAULT_ENABLESNAP = false;
		public const bool DEFAULT_SNAPTOGRID = false;
		public const float DEFAULT_SNAPSIZE = 1;

		private const float HANDLESIZE_MIN = 0.1f;
		private const float HANDLESIZE_MAX = 0.6f;

		// Settings
		private static float handleSize;								// Size of the cable handles in the editor 
		private static SupportedHandleType handleType;                  // Type of the handle used in the editor
		private static bool enableSnap,snapToGrid;
		private static Vector2 snappingSize;

		// Styles
		GUIStyle style_header;

		public static void Init()
		{
			CableRendererSettings window = (CableRendererSettings)EditorWindow.GetWindow(typeof(CableRendererSettings), false, "CableRenderer Settings");

			// Loading settings
			LoadSettings();

			window.Show();
		}

		private void OnDestroy()
		{

		}

		void OnGUI()
		{
			// Creating styles if null
			if (this.style_header == null)
			{
				this.style_header = new GUIStyle("box");
				style_header.stretchWidth = true;
			}

			// Editor settings header
			EditorGUILayout.BeginHorizontal(style_header);
			EditorGUILayout.LabelField("Editor Settings");
			EditorGUILayout.EndHorizontal();

			// Editor settings
			handleSize = EditorGUILayout.Slider(new GUIContent("Handle Size","Size of the cable handles in the editor."),handleSize, HANDLESIZE_MIN, HANDLESIZE_MAX);
			handleType = (SupportedHandleType)EditorGUILayout.EnumPopup(new GUIContent("Handle Type", "Type of the cable handles in the editor."), handleType);
			enableSnap = EditorGUILayout.Toggle(new GUIContent("Enable Snap", "Enable snapping (Hold CTRL while dragging handles to activate."), enableSnap);
			GUI.enabled = enableSnap;
			snapToGrid = EditorGUILayout.Toggle(new GUIContent("Snap to Grid", "Enable Unity grid snapping"), snapToGrid);
			snappingSize = EditorGUILayout.Vector2Field(new GUIContent("Snap Size", ""), snappingSize);
			GUI.enabled = true;

			// Buttons
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Ok"))
			{
				SaveSettings();
				CableRendererSettings window = (CableRendererSettings)EditorWindow.GetWindow(typeof(CableRendererSettings), false, "CableRenderer Settings");
				window.Close();
			}
			if (GUILayout.Button("Apply"))
			{
				SaveSettings();
			}
			if (GUILayout.Button("Cancel"))
			{
				CableRendererSettings window = (CableRendererSettings)EditorWindow.GetWindow(typeof(CableRendererSettings), false, "CableRenderer Settings");
				window.Close();
			}

			GUILayout.EndHorizontal();

			if (GUILayout.Button("Reset To Default"))
			{
				if (EditorUtility.DisplayDialog("Confirm", "Reset all settings to default?", "Yes", "No"))
				{
					ClearSettings();
					Init();
				}
			}

		}

		private static void LoadSettings()
		{
			handleSize = EditorPrefs.GetFloat(SETTING_STRING_HANDLESIZE, DEFAULT_HANDLESIZE);
			handleType = (SupportedHandleType)EditorPrefs.GetInt(SETTING_STRING_HANDLETYPE, 0);
			enableSnap = EditorPrefs.GetBool(SETTING_STRING_ENABLESNAP,DEFAULT_ENABLESNAP);
			snapToGrid = EditorPrefs.GetBool(SETTING_STRING_SNAPTOGRID, DEFAULT_SNAPTOGRID);
			snappingSize = new Vector2(EditorPrefs.GetFloat(SETTING_STRING_SNAPSIZE_X,DEFAULT_SNAPSIZE), EditorPrefs.GetFloat(SETTING_STRING_SNAPSIZE_Y,DEFAULT_SNAPSIZE));
		}

		private static void SaveSettings()
		{
			EditorPrefs.SetFloat(SETTING_STRING_HANDLESIZE, Mathf.Clamp(handleSize,HANDLESIZE_MIN,HANDLESIZE_MAX));
			EditorPrefs.SetInt(SETTING_STRING_HANDLETYPE, Mathf.Clamp((int)handleType, 0, 2));
			EditorPrefs.SetBool(SETTING_STRING_ENABLESNAP, enableSnap);
			EditorPrefs.SetBool(SETTING_STRING_SNAPTOGRID, snapToGrid);
			EditorPrefs.SetFloat(SETTING_STRING_SNAPSIZE_X, snappingSize.x);
			EditorPrefs.SetFloat(SETTING_STRING_SNAPSIZE_Y, snappingSize.y);
		}

		private static void ClearSettings()
		{
			EditorPrefs.DeleteKey(SETTING_STRING_HANDLESIZE);
			EditorPrefs.DeleteKey(SETTING_STRING_HANDLETYPE);
			EditorPrefs.DeleteKey(SETTING_STRING_ENABLESNAP);
			EditorPrefs.DeleteKey(SETTING_STRING_SNAPTOGRID);
			EditorPrefs.DeleteKey(SETTING_STRING_SNAPSIZE_X);
			EditorPrefs.DeleteKey(SETTING_STRING_SNAPSIZE_Y);
		}

		public static Handles.CapFunction GetUnityHandle(SupportedHandleType handleType)
		{
			switch (handleType)
			{
				case SupportedHandleType.Dot:
					return Handles.DotHandleCap;
				case SupportedHandleType.Sphere:
					return Handles.SphereHandleCap;
				default:
					return Handles.CircleHandleCap;
			}
		}


	}
}
#endif
