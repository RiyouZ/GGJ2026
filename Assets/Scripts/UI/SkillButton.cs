using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private float dropDistance = 0.2f;
		[SerializeField] private float duration = 0.2f;
		[SerializeField] private Ease ease = Ease.InOutSine;

		private Tween _tween;
		private Vector3 originalLocalPos;

		private void Awake()
		{
			originalLocalPos = transform.localPosition;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_tween?.Kill();
			_tween = transform.DOLocalMoveY(originalLocalPos.y - dropDistance, duration).SetEase(ease);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_tween?.Kill();
			_tween = transform.DOLocalMoveY(originalLocalPos.y, duration).SetEase(ease);
		}
	}

}
