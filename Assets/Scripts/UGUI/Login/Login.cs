using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Login : MonoBehaviour {

    // Use this for initialization
    public Button _loginButton;
    public InputField _accountInput;
    public InputField _passwdInput;
    void Start ()
    {
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


        _loginButton.onClick.AddListener(delegate () {
            Facade.GetSingleton<NetController>();
            if (_accountInput.text.Trim().Length > 15 || _accountInput.text.Trim().Length < 2)
            {
                return;
            }
            if (_passwdInput.text.Trim().Length > 15 || _passwdInput.text.Trim().Length < 2)
            {
                return;
            }
            Facade.GetSingleton<Dispatcher>().TriggerEvent("Login", new object[] { "127.0.0.1", (ushort)26001, _accountInput.text.Trim(), _passwdInput.text.Trim() });
        });
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
