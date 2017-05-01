using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Proto4z;

public class LoginUI : MonoBehaviour {

    // Use this for initialization
    public Button _loginButton;
    public Toggle _devTonggle;
    public InputField _accountInput;
    public InputField _passwdInput;

    private float _lastClickLogin = 0;
    void Start ()
    {
        if (PlayerPrefs.GetString("account") != null)
        {
            _accountInput.text = PlayerPrefs.GetString("account");
        }
        if (PlayerPrefs.GetString("passwd") != null)
        {
            _passwdInput.text = PlayerPrefs.GetString("passwd");
        }

        _accountInput.onValueChanged.AddListener(delegate (string text)
        {
            if (text.Length > 0)
            {
                _accountInput.placeholder.enabled = false;
            }
            else
            {
                _accountInput.placeholder.enabled = true;
            }
        });
        _passwdInput.onValueChanged.AddListener(delegate (string text)
        {
            if (text.Length > 0)
            {
                _passwdInput.placeholder.enabled = false;
            }
            else
            {
                _passwdInput.placeholder.enabled = true;
            }
        });

 
        _loginButton.onClick.AddListener(delegate () 
        {
            if (_accountInput.text.Trim().Length > 15 || _accountInput.text.Trim().Length < 2)
            {
                return;
            }
            if (_passwdInput.text.Trim().Length > 15 || _passwdInput.text.Trim().Length < 2)
            {
                return;
            }
            float now = Time.realtimeSinceStartup;
            if (now - _lastClickLogin < 1f)
            {
                return;
            }
            _lastClickLogin = now;
            if (_devTonggle == null)
            {
                Debug.LogWarning("why tonggle is null?");
                return;
            }

            PlayerPrefs.SetString("account", _accountInput.text);
            PlayerPrefs.SetString("passwd", _passwdInput.text);
            if (_devTonggle.isOn)
            {
                Facade._serverProxy.Login("localhost", (ushort)26001, _accountInput.text.Trim(), _passwdInput.text.Trim());
            }
            else
            {
                Facade._serverProxy.Login("192.168.1.216", (ushort)26001, _accountInput.text.Trim(), _passwdInput.text.Trim());
            }

        });
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
