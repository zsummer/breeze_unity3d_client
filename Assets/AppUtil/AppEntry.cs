#define CLEAR_PREF

using UnityEngine;
using System.Collections;
using Unistar;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AppEntry : MonoBehaviour{

	[RuntimeInitializeOnLoadMethod]
	static void Initialize()
	{
		Debug.Log( "RuntimeInitializeOnLoadMethod" );
		GameObject appEntry = Instantiate (Resources.Load<Object> ("AppEntry")) as GameObject;
		appEntry.name = "AppEntry";
		appEntry.SetActive (true);
	}

	void Awake() {
		Singleton.m_Container = this.gameObject;
		AppManager.commonConfig = this.GetComponent<AppCommonConfig> ();
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	
	}


}
