using System;
using UnityEngine;


//外观类
//所有单例从该类中引出


public class Facade: MonoBehaviour
{
    public static Proto4z.SceneGroupInfo sceneState = null;
    public static Proto4z.AvatarBaseInfo avatarInfo = null;
    public static ulong myShell = 0;

    public static ModelDict modelDict = null;
    public static Dispatcher dispatcher = null;
    public static SceneManager sceneManager = null;
    public static ServerProxy serverProxy = null;
    public static AudioManager audioManager = null;

    public static MainUI mainUI = null;
    
 
    private static System.Collections.Generic.Dictionary<string, object> _singletons;
    private static GameObject _facade = null;

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        Debug.Log("Facade Init");


        UnityEngine.EventSystems.EventSystem eventSys = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSys == null)
        {
            GameObject o = new GameObject("EventSystem");
            o.AddComponent<UnityEngine.EventSystems.EventSystem>();
            o.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }


        _singletons = new System.Collections.Generic.Dictionary<string, object>();
        if (_facade != null)
        {
            Debug.LogError("Facade Init Error. Duplicate Call");
            return;
        }

        _facade = new GameObject();
        _facade.name = "Facade";
        _facade.SetActive(true);
        _facade.AddComponent(typeof(Facade));

        dispatcher = Facade.AddSingleton<Dispatcher>();
        Facade.AddSingleton<GameOption>();
        modelDict = Facade.AddSingleton<ModelDict>();
        serverProxy =  Facade.AddSingleton<ServerProxy>();
        sceneManager = Facade.AddSingleton<SceneManager>();
        audioManager = Facade.AddSingleton<AudioManager>();
    }
    void Awake()
    {
        Debug.Log("Facade Awake");
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Debug.Log("Facade Start");
    }

    void Update()
    {
    }

    void OnApplicationQuit()
    {
        Debug.Log("Facade Quit");
        GameObject.Destroy(_facade);
        _facade = null;
    }



	public static object GetSingleton (string name)
	{
        if (_facade == null || _singletons == null)
        {
            return null;
        }
        if (!_singletons.ContainsKey(name))
        {
            return null;
        }
        return _singletons[name];
	}

	public static T GetSingleton<T> () where T: MonoBehaviour
    {
		string name = typeof(T).Name;
		return (GetSingleton(name) as T);
	}

    public static object AddSingleton(string name)
    {
        if (_facade == null || _singletons == null)
        {
            return null;
        }
        if (_singletons.ContainsKey(name))
        {
            return _singletons[name];
        }

        var typeInfo = Type.GetType(name);
        if (typeInfo == null)
        {
            Debug.LogWarning("Facade Can't Add Single Script " + name);
            return null;
        }
        var obj = _facade.AddComponent(Type.GetType(name));
        _singletons.Add(name, obj);
        return obj;
    }

    public static T AddSingleton<T>() where T : MonoBehaviour
    {
        string name = typeof(T).Name;
        return (AddSingleton(name) as T);
    }

    public static void RemoveSingleton(string name)
	{
        if (_facade == null || _singletons == null)
        {
            return ;
        }
        if (_singletons.ContainsKey(name))
        {
            UnityEngine.Object.Destroy((UnityEngine.Object)(_singletons[name]));
            _singletons.Remove(name);
            Debug.LogWarning("Facade Remove Script " + name);
        }
	}

}
