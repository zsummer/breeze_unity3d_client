using System;
using UnityEngine;


//外观类
//所有单例从该类中引出
public class MainUI: MonoBehaviour
{
	
    public static Transform AvatarMode { get { return _avatarMode; } set { _avatarMode = value; } }
    public static Proto4z.AvatarBaseInfo AvatarInfo { get { return _avatarInfo; } set { _avatarInfo = value; } }
	public static Proto4z.SceneGroupInfo GroupInfo { get { return _groupInfo; } set { _groupInfo = value; } }

    static GameObject _facade = null;
    static Transform _avatarMode = null;
    static Proto4z.AvatarBaseInfo _avatarInfo = null;
	static Proto4z.SceneGroupInfo _groupInfo = null;

    private static System.Collections.Generic.Dictionary<string, object> _singletons;
    
    
    public static void Init()
    {
        Debug.Log("Init Facade.");

        _singletons = new System.Collections.Generic.Dictionary<string, object>();
        if (_facade != null)
        {
            return;
        }
        _facade = Resources.Load<GameObject>("Facade");
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

    public static void CreateAvatar(int modelID)
    {
        Vector3 spawnpoint = new Vector3(-63.37f, -13.198f, 73.3f);
        Quaternion quat = new Quaternion();
        if (_avatarMode != null)
        {
            spawnpoint = _avatarMode.position;
            quat = _avatarMode.rotation;
            GameObject.Destroy(_avatarMode.gameObject);
            _avatarMode = null;

        }
        
        string name = Facade.GetSingleton<ModelMgr>().GetModelName(modelID);
        if (name == null)
        {
            name = "jing_ling_nv_001_ty";
        }

        var res = Resources.Load<GameObject>("Character/Model/" + name);
        if (res == null)
        {
            Debug.LogError("can't load resouce model [" + name + "].");
            return;
        }
        var obj = Instantiate(res);
        if (obj == null)
        {
            Debug.LogError("can't Instantiate model[" + name + "].");
            return;
        }

        obj.AddComponent<Rigidbody>();
        obj.AddComponent<CapsuleCollider>();
        if (obj.GetComponent<Animation>() == null)
        {
            obj.AddComponent<Animation>();
        }
        obj.AddComponent<AvatarController>();
        obj.transform.position = spawnpoint;
        obj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        obj.transform.rotation = quat;
        Rigidbody rd = obj.GetComponent<Rigidbody>();
        rd.freezeRotation = true;
        _avatarMode = obj.transform;
        Debug.Log("create avatar");
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
