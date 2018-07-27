using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CSoundManager : MonoBehaviour {

	[SerializeField]	protected bool m_AutoPlay = true;
	[SerializeField]	protected bool m_IsPlayRandom = true;
	[SerializeField]	protected AudioClip[] m_AudioClips;

	protected AudioSource m_AudioSource;
	protected int m_ListIndex = 0;

	protected virtual void Awake() {
		this.m_AudioSource = this.GetComponent<AudioSource> ();
		this.m_ListIndex = this.m_IsPlayRandom ? Random.Range(0, this.m_AudioClips.Length) : 0;
		this.m_AudioSource.playOnAwake = this.m_AutoPlay;
		this.Play (this.m_ListIndex);
	}

	protected virtual void LateUpdate() {
		if (this.m_AudioSource.isPlaying == false) {
			this.m_ListIndex = this.m_IsPlayRandom ? Random.Range(0, this.m_AudioClips.Length) : this.m_ListIndex + 1;
			this.Play (this.m_ListIndex);
		}
	}

	public void Play(int index) {
		if (index < 0 || index >= this.m_AudioClips.Length)
			return;
		var clip = this.m_AudioClips [index];
		this.m_AudioSource.clip = clip;
		this.m_AudioSource.Play ();
	}

}
