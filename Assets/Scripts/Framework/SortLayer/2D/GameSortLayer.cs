using System.Collections;
using System.Collections.Generic;
using RuGameFramework.Util;
using UnityEngine;

namespace RuGameFramework.SortLayer2D
{
	public class GameSortLayer : MonoBehaviour
	{
		public GameObject BindObj => this.gameObject;

		[SerializeField] private string _sortLayerName = string.Empty;
		public string SortLayerName
		{
			get
			{
				return _sortLayerName;
			}
			set
			{
				_sortLayerName = value;

				if (_spriteRenderer != null)
				{
					_spriteRenderer.sortingLayerName = _sortLayerName;
				}

				if (_meshRenderer != null)
				{
					_meshRenderer.sortingLayerName = _sortLayerName;
				}
			}
		}

		private SpriteRenderer _spriteRenderer;
		private MeshRenderer _meshRenderer;

		void Start()
		{
			InitComponent();
		}

		void LateUpdate()
		{
			UpdateSpriteSortLayer();
			UpdateMeshRendererSortLayer();
		}

		public void UpdateSpriteSortLayer()
		{
			if (_spriteRenderer == null)
			{
				return;
			}

			int sortOrder = SortLayer2DUtil.YAxisConverSortOrderValue(transform.position.y);
			_spriteRenderer.sortingOrder = -sortOrder;
		}

		public void UpdateMeshRendererSortLayer()
		{
			if (_meshRenderer == null)
			{
				return;
			}

			int sortOrder = SortLayer2DUtil.YAxisConverSortOrderValue(transform.position.y);
			_meshRenderer.sortingOrder = -sortOrder;
		}

		public void InitComponent()
		{
			_meshRenderer = GetComponent<MeshRenderer>();
			if (_meshRenderer != null)
			{
				_meshRenderer.sortingLayerName = SortLayerName;
				return;
			}

			_spriteRenderer = GetComponent<SpriteRenderer>();
			if (_spriteRenderer != null)
			{
				_spriteRenderer.sortingLayerName = SortLayerName;
				return;
			}
		}

		public void Dispose()
		{

		}

		public void Init()
		{

		}
	}

}

