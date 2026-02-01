using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.Util
{
	public static class SortLayer2DUtil
	{
		public static int ConverRatio = 100;
		public static int YAxisConverSortOrderValue (float y)
		{
			return (int) (y * ConverRatio);
		}

		// 根据Tag按y轴排序
		public static void SortOrderGameObjectForTag (string tag)
		{
			var objList = GameObject.FindGameObjectsWithTag(tag);

			foreach ( var obj in objList )
			{
				SpriteRenderer render = obj.GetComponent<SpriteRenderer>();
				if (render == null)
				{
					continue;
				}

				int sortValue = YAxisConverSortOrderValue(obj.transform.position.y);
				render.sortingOrder = sortValue;
			}
		}

	}

}

