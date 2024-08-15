#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GIGA.PixelCableRenderer.Editor
{
	[CustomEditor(typeof(CableRenderer))]
	public class CableRendererEditor : UnityEditor.Editor
	{
		private Tool exTool;
		
		// Styles
		GUIStyle style_header;

		// Serialized Properties
		SerializedProperty s_softness;
		SerializedProperty s_shapeModifier;
		SerializedProperty s_rippleModifier;
		SerializedProperty s_pixelSize;
		SerializedProperty s_smoothness;

		SerializedProperty s_windAmount;
		SerializedProperty s_windSpeed;
		SerializedProperty s_windRippleEffect;

		SerializedProperty s_colorMode;
		SerializedProperty s_color1;
		SerializedProperty s_color2;
		SerializedProperty s_colorIndex;

		SerializedProperty s_texture;
		SerializedProperty s_staticParameters;
		SerializedProperty s_simulateMovementInEditor;

		SerializedProperty s_useBatching;
		SerializedProperty s_batchingGroup;


		private void OnEnable()
		{
			this.exTool = Tools.current;
			Tools.current = Tool.Rect;
			
			s_softness = serializedObject.FindProperty("softness");
			s_shapeModifier = serializedObject.FindProperty("shapeModifier");
			s_rippleModifier = serializedObject.FindProperty("rippleModifier");
			s_pixelSize = serializedObject.FindProperty("pixelSize");
			s_smoothness = serializedObject.FindProperty("edgeSmoothness");

			s_windAmount = serializedObject.FindProperty("windAmount");
			s_windSpeed = serializedObject.FindProperty("windSpeed");
			s_windRippleEffect = serializedObject.FindProperty("windRippleEffect");

			s_colorMode = serializedObject.FindProperty("colorMode");
			s_color1 = serializedObject.FindProperty("color1");
			s_color2 = serializedObject.FindProperty("color2");
			s_colorIndex = serializedObject.FindProperty("colorIndex");
			s_texture = serializedObject.FindProperty("texture");

			s_staticParameters = serializedObject.FindProperty("staticParameters");
			s_simulateMovementInEditor = serializedObject.FindProperty("simulateMovementInEditor");

			s_useBatching = serializedObject.FindProperty("useBatching");
			s_batchingGroup = serializedObject.FindProperty("batchingGroup");
		}

		private void OnDisable()
		{
			Tools.current = exTool;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var t = target as CableRenderer;

			// Creating styles if null
			if (this.style_header == null)
			{
				this.style_header = new GUIStyle("box");
				style_header.stretchWidth = true;
			}

			EditorGUILayout.Space(10);

			// Canvas renderer info
			if (t.GetComponent<RectTransform>() != null)
				EditorGUILayout.HelpBox("RectTransform properties are driven by the CableRenderer script.\nPlease use the editor handles to move and rotate the cable instead of modyfing the RectTransform values.", MessageType.Info);

			// Checking if parent has uneven scale
			if (t.transform.parent != null && t.transform.parent.lossyScale.x != t.transform.parent.lossyScale.y)
				EditorGUILayout.HelpBox("Uneven scaling found in parent, this renderer will not work properly!", MessageType.Error);

			if (!Application.isPlaying || !t.staticParameters)
			{
				// Cable settings
				EditorGUILayout.BeginHorizontal(style_header);
				EditorGUILayout.LabelField("Cable Settings");
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel++;
				s_softness.floatValue = (float)Math.Round(EditorGUILayout.Slider(new GUIContent("Softness", "Cable softness"), s_softness.floatValue, 0, 1), 2);
				s_pixelSize.intValue = EditorGUILayout.IntSlider(new GUIContent("Thickness", "Cable Thickness"), s_pixelSize.intValue, 1, CableRenderer.MAX_PIXEL_SIZE);
				s_smoothness.floatValue = EditorGUILayout.Slider(new GUIContent("Edge Smoothness", "Softens cable edges"), s_smoothness.floatValue, 0, 1);
				EditorGUI.BeginDisabledGroup(t.windRippleEffect);
				s_shapeModifier.floatValue = (float)Math.Round(EditorGUILayout.Slider(new GUIContent("Shape Modifier", "Changes the shape of the cable to better adapt to its orientation. Disabled if wind ripple effect is on"), s_shapeModifier.floatValue, 0, 1), 2);
				EditorGUI.EndDisabledGroup();
				EditorGUI.indentLevel--;

				EditorGUILayout.Space(10);

				// Wind settings
				EditorGUILayout.BeginHorizontal(style_header);
				EditorGUILayout.LabelField("Wind Settings");
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel++;
				s_windAmount.floatValue = (float)Math.Round(EditorGUILayout.Slider(new GUIContent("Wind Amount", "Strength of the wind animation"), s_windAmount.floatValue, 0, 1), 2);
				s_windSpeed.floatValue = (float)Math.Round(EditorGUILayout.Slider(new GUIContent("Wind Speed", "Speed of the wind animation"), s_windSpeed.floatValue, 0, 1), 2);
				if (s_windAmount.floatValue > 0 && s_windSpeed.floatValue == 0)
					EditorGUILayout.HelpBox("Wind amount is on but wind speed is zero, wind will not be applied.", MessageType.Warning);
				else if (s_windSpeed.floatValue > 0 && s_windAmount.floatValue == 0)
					EditorGUILayout.HelpBox("Wind speed is on but wind amount is zero, wind will not be applied.", MessageType.Warning);
				s_windRippleEffect.boolValue = EditorGUILayout.Toggle(new GUIContent("Ripple Effect", "Use ripple effect in wind animation"), s_windRippleEffect.boolValue);
				if(s_windRippleEffect.boolValue)
					s_rippleModifier.floatValue = (float)Math.Round(EditorGUILayout.Slider(new GUIContent("Ripple Modifier", "Ripple effect modifier, mainly used in wind animations"), s_rippleModifier.floatValue, 0, 1), 2);
				EditorGUI.indentLevel--;

				EditorGUILayout.Space(10);

				// Color settings
				EditorGUILayout.BeginHorizontal(style_header);
				EditorGUILayout.LabelField("Color Settings");
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel++;

				s_colorMode.enumValueIndex = (int)(CableRenderer.ColorMode)EditorGUILayout.EnumPopup(new GUIContent("Color Mode", "Cable Coloring Mode"), (CableRenderer.ColorMode)s_colorMode.enumValueIndex);
				if (s_colorMode.enumValueIndex != (int)CableRenderer.ColorMode.PixelTexture && s_colorMode.enumValueIndex != (int)CableRenderer.ColorMode.FullTexture && s_colorMode.enumValueIndex != (int)CableRenderer.ColorMode.RepeatedTexture && s_colorMode.enumValueIndex != (int)CableRenderer.ColorMode.IndexedColor)
					s_color1.colorValue = EditorGUILayout.ColorField(new GUIContent(s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.SingleColor ? "Color" : "Color 1", "Main color"), s_color1.colorValue);
				if (s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.AlternatePixels || s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.LinearGradient || s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.GradientFromCenter)
					s_color2.colorValue = EditorGUILayout.ColorField(new GUIContent("Color 2", "Secondary color, used for gradients and alternate pixel modes"), s_color2.colorValue);
				if (s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.PixelTexture || s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.FullTexture || s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.RepeatedTexture)
				{
					// Texture warnings
					EditorGUILayout.HelpBox("Textures are not supported in LITE package.", MessageType.Info);
				}
				if (s_colorMode.enumValueIndex == (int)CableRenderer.ColorMode.IndexedColor)
				{
					s_colorIndex.intValue = EditorGUILayout.IntSlider(new GUIContent("Color Index", "Use indexed colors to prevent breaking batching when drawing cables with different colors"),s_colorIndex.intValue,0,t.indexedColorsData.indexedColors.Length-1);
					EditorGUILayout.ColorField(GUIContent.none,t.indexedColorsData.indexedColors[s_colorIndex.intValue]);
				}
				
				EditorGUI.indentLevel--;

				// Batching Settings
				if (!Application.isPlaying)
				{
					if (t.GetComponent<CanvasRenderer>() != null)
					{
						EditorGUILayout.BeginHorizontal(style_header);
						EditorGUILayout.LabelField("Canvas Batching Settings");
						EditorGUILayout.EndHorizontal();
						EditorGUI.indentLevel++;
						s_useBatching.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable Batching", "Enables batching for canvas renderers to reduce draw calls"), s_useBatching.boolValue);
						s_batchingGroup.intValue = Math.Max(1, EditorGUILayout.IntField(new GUIContent("Batch Group", "Renderers with the same group will share material"), s_batchingGroup.intValue));

						EditorGUI.indentLevel--;
					}
				}

				// Other settings
				if (!Application.isPlaying)
				{
					EditorGUILayout.BeginHorizontal(style_header);
					EditorGUILayout.LabelField("Other Settings");
					EditorGUILayout.EndHorizontal();
					EditorGUI.indentLevel++;
					s_staticParameters.boolValue = EditorGUILayout.Toggle(new GUIContent("Static Parameters", "You can turn this on if you don't plan to change parameters at runtime to improve performance"), s_staticParameters.boolValue);
					s_simulateMovementInEditor.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable Animation in Editor", "Turn this on to enable animations of the selected cable inside the editor window"), s_simulateMovementInEditor.boolValue);
					EditorGUI.indentLevel--;
				}
			}
			else
			{
				// Static parameters info box
				EditorGUILayout.HelpBox("This cable has static parameters and cannot be edited at runtime.", MessageType.Info);
			}

			// Checking if SpriteRenderer or UI.Image is missing
			if (t.GetComponent<SpriteRenderer>() == null && t.GetComponent<Image>() == null)
			{
				EditorGUILayout.HelpBox("CableRenderer requires either SpriteRenderer or UI.Image component", MessageType.Error);

				// Buttons to add required components
				if (GUILayout.Button("Add SpriteRenderer"))
				{
					SpriteRenderer spriteRenderer = t.gameObject.AddComponent<SpriteRenderer>();
					spriteRenderer.sharedMaterial = t.defaultCableMaterial;
					t.Init();
				}
				if (GUILayout.Button("Add CanvasRenderer"))
				{
					if (t.GetComponent<RectTransform>() == null)
						t.gameObject.AddComponent<RectTransform>();
					if (t.GetComponent<CableRendererMeshEffect>() == null)
						t.gameObject.AddComponent<CableRendererMeshEffect>();
					Image canvasRenderer = t.gameObject.AddComponent<Image>();
					canvasRenderer.material = t.defaultCableMaterial;
					t.Init();
				}
			}

			serializedObject.ApplyModifiedProperties();

			// Version label
			EditorGUILayout.Space(10);
			GUIStyle rightcentederLabel = new GUIStyle(GUI.skin.label);
			rightcentederLabel.fontStyle = FontStyle.Italic;
			rightcentederLabel.fontSize = Mathf.RoundToInt(GUI.skin.label.fontSize * 0.75f);
			rightcentederLabel.alignment = TextAnchor.UpperRight;
			rightcentederLabel.normal.textColor = new Color(0.25f, 0.25f, 0.25f, 1f);
			if (GUILayout.Button("GIGA Softworks - Pixel Cable Renderer ver. " + CableRenderer.VERSION, rightcentederLabel))
				CableRendererAboutDialog.Init();

		}

		public void OnSceneGUI()
		{
			GUIStyle redLabel = new GUIStyle("label");
			redLabel.normal.textColor = Color.red;

			var t = target as CableRenderer;
			var tr = t.transform;
			var pos = tr.position;

			string errorLabel = "";

			// Edges Handles
			this.DrawHandles(t);

			// Error checking

			// Checking if parent has uneven scale
			if (t.transform.parent != null && t.transform.parent.lossyScale.x != t.transform.parent.lossyScale.y)
				errorLabel = "Uneven scale in parents!";

			float angle = (tr.eulerAngles.z > 180) ? tr.eulerAngles.z - 360 : tr.eulerAngles.z;
			if (Mathf.Abs(angle) > 90)
				errorLabel = "Cable is upside down!";

			// Error Labels
			if (!string.IsNullOrEmpty(errorLabel))
			{
				Handles.Label(pos, errorLabel,redLabel);
			}
		}

		private void DrawHandles(CableRenderer renderer)
		{
			// Checking if parent has uneven scale
			if (renderer.transform.parent != null && renderer.transform.parent.lossyScale.x != renderer.transform.parent.lossyScale.y)
				return;

			Color exColor = Handles.color;

			// Drawing resizing handles
			DrawResizingHandles(renderer, true);

			// Drawing modifier handle
			Handles.color = Color.yellow;

			float size = EditorPrefs.GetFloat(CableRendererSettings.SETTING_STRING_HANDLESIZE, CableRendererSettings.DEFAULT_HANDLESIZE);
			if (renderer.GetComponent<RectTransform>() != null)
				size *= CableRenderer.CANVAS_RESIZE_FACTOR * 2;

			float halfScaleX = renderer.transform.localScale.x * 0.5f;
			float halfScaleFromSoftness = renderer.VerticalScaleFromSoftness() * 0.5f;

			if (renderer.transform.parent != null)
			{
				halfScaleFromSoftness *= renderer.transform.parent.lossyScale.y;
				halfScaleX *= renderer.transform.parent.lossyScale.x;
			}

			Vector3 bottomCenter = renderer.transform.position + halfScaleFromSoftness * (Vector3.left * (Mathf.Sin(-renderer.transform.eulerAngles.z * Mathf.Deg2Rad)) + Vector3.down * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad));
			Vector3 bottomLeft = renderer.transform.position + halfScaleFromSoftness * (Vector3.left * (Mathf.Sin(-renderer.transform.eulerAngles.z * Mathf.Deg2Rad)) + Vector3.down * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad)) + halfScaleX * (Vector3.left * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(-renderer.transform.eulerAngles.z * Mathf.Deg2Rad));
			Vector3 bottomRight = renderer.transform.position + halfScaleFromSoftness * (Vector3.left * (Mathf.Sin(-renderer.transform.eulerAngles.z * Mathf.Deg2Rad)) + Vector3.down * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad)) - halfScaleX * (Vector3.left * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(-renderer.transform.eulerAngles.z * Mathf.Deg2Rad));

			Vector3 prevModifierPos = bottomLeft - (halfScaleX * 2) * (renderer.windRippleEffect ? 0.5f : renderer.shapeModifier) * (Vector3.left * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(-renderer.transform.eulerAngles.z * Mathf.Deg2Rad));

			var fmh_294_69_638592805376454249 = Quaternion.identity; Vector3 newModifierPos = Handles.FreeMoveHandle(prevModifierPos, size, Vector3.zero, CableRendererSettings.GetUnityHandle((CableRendererSettings.SupportedHandleType)EditorPrefs.GetInt(CableRendererSettings.SETTING_STRING_HANDLETYPE, 0)));

			// Clamping handle
			if (newModifierPos != prevModifierPos && Vector3.Dot(newModifierPos - bottomLeft, bottomRight - bottomLeft) < 0)
				newModifierPos = bottomLeft;


			Undo.RecordObject(renderer, "Cable Properties Change");

			Vector3 l_v = renderer.transform.position - newModifierPos;
			Vector3 b_v = bottomCenter - newModifierPos;
			float l = l_v.Magnitude2D();
			float b = b_v.Magnitude2D();
			float p = l * l - b * b;
			float h = p > 0 ? Mathf.Sqrt(p) : 0;
			if (renderer.transform.parent != null)
				h /= renderer.transform.parent.lossyScale.y;

			if (h * 2 < CableRenderer.MIN_VERTICAL_SCALE)
				h = CableRenderer.MIN_VERTICAL_SCALE;
			renderer.softness = Mathf.Clamp((float)Math.Round(renderer.SoftnessFromVerticalScale(h * 2), 2), 0, 1);
			if(!renderer.windRippleEffect)
				renderer.shapeModifier = Mathf.Clamp((float)Math.Round(((newModifierPos - bottomLeft).Magnitude2D() / (halfScaleX * 2)), 2), 0, 1);

			Handles.color = exColor;
		}

		#region Shared Editor Functions

		public static void DrawResizingHandles(CableRenderer renderer,bool hasDirection = false)
		{

			float size = EditorPrefs.GetFloat(CableRendererSettings.SETTING_STRING_HANDLESIZE, CableRendererSettings.DEFAULT_HANDLESIZE);
			if (renderer.GetComponent<RectTransform>() != null)
				size *= CableRenderer.CANVAS_RESIZE_FACTOR * 2;

			Vector2 settingsSnapSize = EditorPrefs.GetBool(CableRendererSettings.SETTING_STRING_ENABLESNAP, CableRendererSettings.DEFAULT_ENABLESNAP) ? new Vector2(EditorPrefs.GetFloat(CableRendererSettings.SETTING_STRING_SNAPSIZE_X), EditorPrefs.GetFloat(CableRendererSettings.SETTING_STRING_SNAPSIZE_Y)) : Vector2.zero;
			Vector3 snap = new Vector3(settingsSnapSize.x, settingsSnapSize.y, 0);
			Color exColor = Handles.color;

			float halfScaleX = renderer.transform.lossyScale.x * 0.5f;
			float halfScaleY = renderer.transform.lossyScale.y * 0.5f;

			Vector3 prevLeftEdge = renderer.transform.position + halfScaleX * (Vector3.left * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(-renderer.transform.eulerAngles.z * Mathf.Deg2Rad));
			Vector3 prevRightEdge = renderer.transform.position + halfScaleX * (Vector3.right * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(renderer.transform.eulerAngles.z * Mathf.Deg2Rad));

			// Drawing edges handles
			Handles.color = Color.green;
			var fmh_342_60_638592805376471839 = Quaternion.identity; Vector3 leftEdge = Handles.FreeMoveHandle(prevLeftEdge, size, snap, CableRendererSettings.GetUnityHandle((CableRendererSettings.SupportedHandleType)EditorPrefs.GetInt(CableRendererSettings.SETTING_STRING_HANDLETYPE, 0)));
			Handles.color = hasDirection ? Color.red : Color.green;
			var fmh_344_62_638592805376473542 = Quaternion.identity; Vector3 rightEdge = Handles.FreeMoveHandle(prevRightEdge, size, snap, CableRendererSettings.GetUnityHandle((CableRendererSettings.SupportedHandleType)EditorPrefs.GetInt(CableRendererSettings.SETTING_STRING_HANDLETYPE, 0)));

			// Snapping to grid if enabled
			if (EditorPrefs.GetBool(CableRendererSettings.SETTING_STRING_ENABLESNAP, CableRendererSettings.DEFAULT_ENABLESNAP) && EditorPrefs.GetBool(CableRendererSettings.SETTING_STRING_SNAPTOGRID, CableRendererSettings.DEFAULT_SNAPTOGRID))
			{
				if (leftEdge != prevLeftEdge) // Moved left
					leftEdge = Handles.SnapValue(leftEdge, snap);
				else if (rightEdge != prevRightEdge) // Moved right
					rightEdge = Handles.SnapValue(rightEdge, snap);
			}

			Undo.RecordObject(renderer.transform, "Cable Repositioning");
			if (renderer.transform.parent == null)
			{
				float length = (rightEdge - leftEdge).Magnitude2D();
				renderer.transform.localScale = new Vector3(Mathf.Max(0, length), renderer.transform.localScale.y, 1);
			}
			else
			{
				float length = new Vector3((rightEdge.x - leftEdge.x) / renderer.transform.parent.transform.lossyScale.x, (rightEdge.y - leftEdge.y) / renderer.transform.parent.transform.lossyScale.y, (rightEdge.z - leftEdge.z) / renderer.transform.parent.transform.lossyScale.z).Magnitude2D();
				renderer.transform.localScale = new Vector3(Mathf.Max(0, length), renderer.transform.localScale.y, 1);
			}

			float angle = Vector3.Angle(Vector3.right, (rightEdge - leftEdge));
			if (leftEdge.y > rightEdge.y)
				angle = angle * -1;
			renderer.transform.eulerAngles = new Vector3(0, 0, angle);

			halfScaleX = renderer.transform.lossyScale.x * 0.5f;
			halfScaleY = renderer.transform.lossyScale.y * 0.5f;

			if (leftEdge != prevLeftEdge)
			{
				// Moved left edge
				renderer.transform.position = new Vector3(prevRightEdge.x - halfScaleX * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad), prevRightEdge.y - halfScaleX * Mathf.Sin(renderer.transform.eulerAngles.z * Mathf.Deg2Rad), renderer.transform.position.z);
			}
			else if (rightEdge != prevRightEdge)
			{
				// Moved right edge
				renderer.transform.position = new Vector3(prevLeftEdge.x + halfScaleX * Mathf.Cos(renderer.transform.eulerAngles.z * Mathf.Deg2Rad), prevLeftEdge.y + halfScaleX * Mathf.Sin(renderer.transform.eulerAngles.z * Mathf.Deg2Rad), renderer.transform.position.z);
			}

			Handles.color = exColor;
		}

		#endregion

		
	}
}

#endif
