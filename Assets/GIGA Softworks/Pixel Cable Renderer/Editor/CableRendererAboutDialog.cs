#if UNITY_EDITOR
using GIGA.PixelCableRenderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GIGA.PixelCableRenderer.Editor
{
	public class CableRendererAboutDialog : EditorWindow
	{
		public Texture2D starTexture;
		private static float starAnimTime;
		private static float animTime;
		
		public static void Init()
		{
			var window = (CableRendererAboutDialog)EditorWindow.GetWindow(typeof(CableRendererAboutDialog), true, "Soft Pixel Cables LITE");
			window.minSize = new Vector2(400, 500);
			window.maxSize = new Vector2(400, 500);
			window.Show();
			starAnimTime = (float)EditorApplication.timeSinceStartup;
			animTime = 0;
		}

		void OnGUI()
		{
			EditorGUILayout.BeginHorizontal();

			GUILayout.Space(10);
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField(string.Format("Soft Pixel Cables LITE"), EditorStyles.boldLabel);
			EditorGUILayout.LabelField(string.Format("Version: {0}", CableRenderer.VERSION));
			EditorGUILayout.LabelField(string.Format("Copyright \u00A9 GIGA Softworks, 2022 "));

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			GUIStyle linkStyle = new GUIStyle("label");
			linkStyle.normal.textColor = Color.blue;

			EditorGUILayout.LabelField(string.Format("Online Documentation:"), EditorStyles.boldLabel);
			if (GUILayout.Button("Documentation", linkStyle))
				Application.OpenURL("https://www.gigasoftworks.com/products/cablerenderer/docs/overview.html");


			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			EditorGUILayout.LabelField(string.Format("GIGA Softworks Website:"), EditorStyles.boldLabel);
			if (GUILayout.Button("http://www.gigasoftworks.com", linkStyle))
				Application.OpenURL("http://www.gigasoftworks.com");

			EditorGUILayout.LabelField(string.Format("Contact:"), EditorStyles.boldLabel);
			string address = "contact@gigasoftworks.com";
			if (GUILayout.Button(address, linkStyle))
			{
				string subject = "";
				Application.OpenURL(string.Format("mailto:{0}?subject={1}", address, subject));
			}

			EditorGUILayout.LabelField(string.Format("Bug Report:"), EditorStyles.boldLabel);
			address = "bugs@gigasoftworks.com";

			if (GUILayout.Button(address, linkStyle))
			{
				string subject = string.Format("Soft Pixel Cables LITE Bug Report - Ver. {0} Unity {1}", CableRenderer.VERSION, Application.unityVersion);
				Application.OpenURL(string.Format("mailto:{0}?subject={1}", address, subject));
			}


			EditorGUILayout.EndVertical();

			GUILayout.Space(10);
			EditorGUILayout.EndHorizontal();

			float elapsed = (float)EditorApplication.timeSinceStartup - starAnimTime;
			animTime += elapsed;
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			for (int k = 0; k < elapsed % 5; k++)
			{
				GUI.DrawTexture(new Rect(20 + k * 32 + k * 4, 280, 32, 32), this.starTexture);
				//GUILayout.BeginArea();
				//GUILayout.EndArea();
			}
			GUILayout.BeginArea(new Rect(20, 320, 300, 100), "Thank you for downloading this asset!\nIf you found it useful please consider leaving a feedback\non the Asset Store page, it would be of great help :)");
			GUILayout.EndArea();

			if(GUI.Button(new Rect(20,400,300,40),"Leave feedback"))
				Application.OpenURL("https://assetstore.unity.com/packages/slug/235811");

			Repaint();

		}

	}
}
#endif
