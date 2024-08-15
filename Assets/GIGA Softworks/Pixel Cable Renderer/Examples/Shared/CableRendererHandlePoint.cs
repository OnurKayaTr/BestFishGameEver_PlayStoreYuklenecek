using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GIGA.PixelCableRenderer.Demo
{
	[RequireComponent(typeof(SpriteRenderer),typeof(CircleCollider2D))]
    public class CableRendererHandlePoint : MonoBehaviour
    {
		public enum Point { A,B};
        public CableRenderer cableRenderer;
		public CableRendererHandlePoint otherPoint;
		private Vector3 screenPoint;
		private Vector3 offset;

		public Point point;

		// Flags
		private bool holdStatusBeforeDrag;
		private bool lockDragHoldStatus;


		private void Awake()
		{
			if (this.cableRenderer == null)
				this.enabled = false;
			else
				this.SetToCable();
		}

		private void Update()
		{
			if (this.transform.hasChanged)
			{
				this.SetCableToThis();
				this.transform.hasChanged = false;
			}
		}

		private void SetCableToThis()
		{
			float halfScaleX = cableRenderer.transform.lossyScale.x * 0.5f;
			float halfScaleY = cableRenderer.transform.lossyScale.y * 0.5f;
			
			float length = (this.transform.position - otherPoint.transform.position).Magnitude2D();
			cableRenderer.transform.localScale = new Vector3(Mathf.Max(0, length), cableRenderer.transform.localScale.y, 1);

			float angle = Vector3.Angle(this.point == Point.A ? Vector3.right : Vector3.left, (otherPoint.transform.position - this.transform.position));
			if (this.point == Point.A && this.transform.position.y > otherPoint.transform.position.y || this.point == Point.B && this.transform.position.y < otherPoint.transform.position.y)
				angle = angle * -1;
			cableRenderer.transform.eulerAngles = new Vector3(0, 0, angle);

			if (this.point == Point.A)
			{
				cableRenderer.transform.position = new Vector3(otherPoint.transform.position.x - halfScaleX * Mathf.Cos(cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad), otherPoint.transform.position.y - halfScaleX * Mathf.Sin(cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad), cableRenderer.transform.position.z);
			}
			else
			{
				cableRenderer.transform.position = new Vector3(otherPoint.transform.position.x + halfScaleX * Mathf.Cos(cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad), otherPoint.transform.position.y + halfScaleX * Mathf.Sin(cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad), cableRenderer.transform.position.z);
			}
		}

		public void SetToCable()
		{
			float halfScaleX = cableRenderer.transform.lossyScale.x * 0.5f;
			float halfScaleY = cableRenderer.transform.lossyScale.y * 0.5f;
			
			switch (this.point)
			{
				case Point.A:
					this.transform.position = cableRenderer.transform.position + halfScaleX * (Vector3.left * Mathf.Cos(cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(-cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad)) + Camera.main.transform.forward * -0.01f;
					break;
				case Point.B:
					this.transform.position = cableRenderer.transform.position + halfScaleX * (Vector3.right * Mathf.Cos(cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(cableRenderer.transform.eulerAngles.z * Mathf.Deg2Rad)) + Camera.main.transform.forward * -0.01f; ;
					break;
			}
		}

		void OnMouseDown()
		{
			screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		}

		void OnMouseDrag()
		{
			Vector3 curScreenPoint = new Vector3(Mathf.Clamp(Input.mousePosition.x,20,Screen.width-20), Mathf.Clamp(Input.mousePosition.y,20,Screen.height-20), screenPoint.z);
			Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
			transform.position = curPosition;
		}
	}
}
