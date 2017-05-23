using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Proto4z;

public class LoginUI : MonoBehaviour {

    // Use this for initialization
    public Button _loginButton;
    public InputField _hostInput;
    public InputField _portInput;
    public InputField _accountInput;
    public InputField _passwdInput;

    private float _lastClickLogin = 0;
    void setDefaultValue(ref InputField field, string key, string def)
    {
        string cache = PlayerPrefs.GetString(key);
        if (cache != null && cache.Length> 0)
        {
            field.text = cache;
        }
        if (field.text.Length == 0 && def.Length > 0)
        {
            field.text = def;
        }

    }
    void Start ()
    {
        setDefaultValue(ref _hostInput, "login.host", "beijing.zhonglushicai.com");
        setDefaultValue(ref _portInput, "login.port", "26001");
        setDefaultValue(ref _accountInput, "login.account", "");
        setDefaultValue(ref _passwdInput, "login.passwd", "");
        

 
        _loginButton.onClick.AddListener(delegate () 
        {
            if (_accountInput.text.Trim().Length > 15 || _accountInput.text.Trim().Length < 1)
            {
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (now - _lastClickLogin < 1f)
            {
                return;
            }
            ushort port = 0;
            if (!ushort.TryParse(_portInput.text, out port))
            {
                return;
            }
            _lastClickLogin = now;


            PlayerPrefs.SetString("login.host", _hostInput.text);
            PlayerPrefs.SetString("login.port", _portInput.text);
            PlayerPrefs.SetString("login.account", _accountInput.text);
            PlayerPrefs.SetString("login.passwd", _passwdInput.text);

            Facade.serverProxy.Login(_hostInput.text, port, _accountInput.text.Trim(), _passwdInput.text.Trim());
        });
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
