using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Frame.GameCamera
{
	public class Camera2DAdapter : MonoBehaviour
	{
		private Camera _camera;

		public float targetWidth;
		public float targetHeight;

		public bool autoAdapter;

		public float perScale;

		// Start is called before the first frame update
		void Start ()
		{
			AdapteScreen(targetWidth, targetHeight);
		}

		void OnValidate ()
		{
#if UNITY_EDITOR
			if (!autoAdapter)
			{
				return;
			}

            AdapteScreen(targetWidth, targetHeight);
#endif
		}

		public void AdapteScreen (float targetWidth, float targetHeight)
		{
			if (_camera == null)
			{
				_camera = GetComponent<Camera>();
			}

			_camera.orthographic = true;

			float ratio = Screen.width / Screen.height;
			float tarRatio = targetWidth / targetHeight;

			if (ratio >= tarRatio)
			{
				float mult = ratio / tarRatio;
				_camera.orthographicSize = (targetHeight / 2 * mult) * perScale;
			}
			else
			{
				_camera.orthographicSize = (targetHeight / 2.0f) * perScale;
			}

			_camera.rect = new Rect(0, 0, 1, 1);
			
		}
	}

}
