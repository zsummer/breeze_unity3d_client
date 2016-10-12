#define CLEAR_PREF

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameEntry : MonoBehaviour
{

	[RuntimeInitializeOnLoadMethod]
	static void Initialize()
	{
		Debug.logger.Log( "RuntimeInitializeOnLoadMethod" );

        UnityEngine.EventSystems.EventSystem eventSys = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSys == null)
        {
            GameObject o = new GameObject("EventSystem");
            o.AddComponent<UnityEngine.EventSystems.EventSystem>();
            o.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Facade.Init();
        Facade.AddSingleton<Dispatcher>();
        Facade.AddSingleton<ServerProxy>();
        Facade.AddSingleton<ModelDict>();
        Facade._gameScene = Facade.AddSingleton<GameScene>();
    }

    void Awake()
    {
	}

	void Start ()
    {

	}

	void Update ()
    {
	
	}
}
