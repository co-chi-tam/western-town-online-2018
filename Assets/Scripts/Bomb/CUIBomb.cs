using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CUIBomb : MonoBehaviour {

	[SerializeField]	protected Text m_Timer;
	[SerializeField]	protected float m_Speed = 2f;
	[SerializeField]	protected float m_NewIndex = 0f;
	[SerializeField]	protected float m_CurrentIndex = 0f;
	[SerializeField]	protected Animator m_ExplosionAnimator;

	protected RectTransform m_RectTransform;

	protected WaitForFixedUpdate m_WaitFor = new WaitForFixedUpdate();

	protected virtual void Awake() {
		this.m_RectTransform = this.transform as RectTransform;
	}

	public virtual void SetFitBomb(int timer, int index, int maxLength, float radius) {
		this.m_Timer.text = timer.ToString ();	
		var segment = 2f * Mathf.PI / maxLength;
		var x = Mathf.Sin (segment * index) * radius;
		var y = Mathf.Cos (segment * index) * radius;
		this.SetPosition (new Vector2(x, y));
		this.m_NewIndex = index;
		this.m_CurrentIndex = index;
	}

	public virtual void SetBomb(int timer, int index, int maxLength, float radius) {
		this.m_Timer.text = timer.ToString ();	
		var segment = 2f * Mathf.PI / maxLength;
		this.m_NewIndex = index;
		StopAllCoroutines ();
		StartCoroutine (this.HandleMoveBomb(segment, maxLength, radius));
	}

	protected IEnumerator HandleMoveBomb(float segment, int maxLength, float radius) {
		while (this.m_CurrentIndex < this.m_NewIndex) {
			var x = Mathf.Sin (segment * this.m_CurrentIndex) * radius;
			var y = Mathf.Cos (segment * this.m_CurrentIndex) * radius;
			this.SetPosition (new Vector2(x, y));
			yield return this.m_WaitFor;
			this.m_CurrentIndex += (Time.fixedDeltaTime * this.m_Speed);
		}
	}

	public void SetExplosion(UnityAction callback = null) {
		StartCoroutine (this.HandleExplosionBomb(callback));
	}

	protected IEnumerator HandleExplosionBomb(UnityAction callback = null) {
		while (this.m_CurrentIndex < this.m_NewIndex) {
			yield return this.m_WaitFor;
		}
		this.m_ExplosionAnimator.SetTrigger ("isExplosion");
		if (callback != null) {
			callback.Invoke ();
		}
	}

	public void SetPosition(Vector2 newPos) {
		this.m_RectTransform = this.transform as RectTransform;
		this.m_RectTransform.anchoredPosition = newPos;
	}

}
