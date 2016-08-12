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
        Facade.GetSingleton<Dispatcher>();
        Facade.GetSingleton<NetController>();
        

        UnityEngine.EventSystems.EventSystem eventSys = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSys == null)
        {
            GameObject o = new GameObject("EventSystem");
            o.AddComponent<UnityEngine.EventSystems.EventSystem>();
            o.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }




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
