#define CLEAR_PREF

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AppEntry : MonoBehaviour{

	[RuntimeInitializeOnLoadMethod]
	static void Initialize()
	{
		Debug.logger.Log( "RuntimeInitializeOnLoadMethod" );
        Facade.Init();
        Facade.GetSingleton<NetController>();
	}

	void Awake() {
		AppManager.commonConfig = this.GetComponent<AppCommonConfig> ();
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	
	}


}
