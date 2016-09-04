using System;
using UnityEngine;


//外观类
//所有单例从该类中引出
public class Facade: MonoBehaviour
{
	
    public static Transform AvatarMode { get { return _avatarMode; } set { _avatarMode = value; } }
    public static Proto4z.AvatarBaseInfo AvatarInfo { get { return _avatarInfo; } set { _avatarInfo = value; } }

    static GameObject _facade = null;
    static Transform _avatarMode = null;
    static Proto4z.AvatarBaseInfo _avatarInfo = null;
    private static System.Collections.Generic.Dictionary<string, object> _singletons;
    
    
    public static void Init()
    {
        Debug.Log("Init Facade.");

        _singletons = new System.Collections.Generic.Dictionary<string, object>();
        if (_facade != null)
        {
            return;
        }
        _facade = Resources.Load<GameObject>("Prefabs/Facade");
        if (_facade == null)
        {
            _facade = new GameObject();
        }
        else
        {
           _facade = Instantiate(_facade);
        }
        _facade.name = "Facade";
        _facade.SetActive(true);
        _facade.AddComponent(typeof(Facade));
    }
    void Awake()
    {
        Debug.Log("Awake Facade.");
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Debug.Log("Start Facade.");
    }

    void Update()
    {
        //Debug.Log("Facade Update");
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit Destroy Facade");
        GameObject.Destroy(_facade);
        _facade = null;
    }

    public static void CreateAvatar()
    {
        if (_avatarMode != null)
        {
            GameObject.Destroy(_avatarMode.gameObject);
            _avatarMode = null;
        }
        string name;
        if (_avatarInfo.modeID == 0)
        {
            name = "jing_ling_nv_001_ty";
        }
        else
        {
            name = "jing_ling_nv_001_ty";
        }

        _avatarMode = GameObject.Find(name).transform;
        if (_avatarMode == null)
        {
            Debug.Log("not found avatar");
        }

    }


	public static object GetSingleton (string name)
	{
        if (_facade == null || _singletons == null)
        {
            return null;
        }
        if (!_singletons.ContainsKey(name))
        {
            var obj = _facade.GetComponent(name);
            if (obj == null)
            {
                var typeInfo = Type.GetType(name);
                if (typeInfo == null)
                {
                    Debug.LogWarning("not found type " + name);
                    return null;
                }
                obj = _facade.AddComponent(Type.GetType(name));
            }
            _singletons.Add(name, obj);
        }
        return _singletons[name];
	}

	public static T GetSingleton<T> () where T: MonoBehaviour
    {
		string name = typeof(T).Name;
		return (GetSingleton(name) as T);
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
            Debug.LogWarning("RemoveSingleton " + name);
        }
	}

}
