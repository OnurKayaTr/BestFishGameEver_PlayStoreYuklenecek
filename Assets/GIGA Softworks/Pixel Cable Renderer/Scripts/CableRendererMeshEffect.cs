using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GIGA.PixelCableRenderer
{

	public class CableRendererMeshEffect : BaseMeshEffect
	{
		public override void ModifyMesh(VertexHelper vh)
		{
			if (!IsActive()) return;

			int vertCount = vh.currentVertCount;

			var vert = new UIVertex();
			for (int v = 0; v < 4; v++)
			{
				vh.PopulateUIVertex(ref vert, v);

				switch (v)
				{
					case 0:
						vert.position = new Vector3(-0.5f, -0.5f, 0);
						break;
					case 1:
						vert.position = new Vector3(-0.5f, 0.5f, 0);
						break;
					case 2:
						vert.position = new Vector3(0.5f, 0.5f, 0) ;
						break;
					case 3:
						vert.position = new Vector3(0.5f, -0.5f, 0) ;
						break;
				}

				vh.SetUIVertex(vert, v);
			}
		}

		public void Update()
		{
			var graphic = GetComponent<Graphic>();
			graphic.SetVerticesDirty();
		}

	}
}
