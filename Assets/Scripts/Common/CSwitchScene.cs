using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class CSwitchScene : MonoBehaviour {

	[Serializable]
	public class UnityEventBool : UnityEvent<bool> {}

	[Header("Evetnes")]
	public UnityEventBool OnLoading;

	public static bool onLoadScene = false;

	protected virtual void Awake() {
		SceneManager.activeSceneChanged += (arg0, arg1) => {
			onLoadScene = arg0.buildIndex == arg1.buildIndex;
			if (this.OnLoading != null && onLoadScene == false) {
				this.OnLoading.Invoke (false);
			}
		};
	}

	public void LoadSceneImmediately(string name) {
		var currentScene = SceneManager.GetActiveScene ();
		if (onLoadScene || currentScene.name == name)
			return;
		if (this.OnLoading != null) {
			this.OnLoading.Invoke (true);
		}
		SceneManager.LoadScene (name);
		onLoadScene = true;
	}

	public void LoadScene(string name) {
		var currentScene = SceneManager.GetActiveScene ();
		if (onLoadScene || currentScene.name == name)
			return;
		if (this.OnLoading != null) {
			this.OnLoading.Invoke (true);
		}
		SceneManager.LoadScene (name);
		onLoadScene = true;
	}

	public void LoadSceneAfterSeconds(string name, float time) {
		var currentScene = SceneManager.GetActiveScene ();
		if (onLoadScene|| currentScene.name == name)
			return;
		if (this.OnLoading != null) {
			this.OnLoading.Invoke (true);
		}
		StartCoroutine (this.HandleAfterTime(time, name));
		onLoadScene = true;
	}

	protected IEnumerator HandleAfterTime(float time, string name) {
		if (this.OnLoading != null) {
			this.OnLoading.Invoke (true);
		}
		yield return new WaitForSeconds (time);
		SceneManager.LoadScene (name);
	}

}
