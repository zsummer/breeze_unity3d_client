using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Proto4z;

public class Login : MonoBehaviour {

    // Use this for initialization
    public Button _loginButton;
    public InputField _accountInput;
    public InputField _passwdInput;
    public Image _busyTips;
    void Start ()
    {
        _busyTips.gameObject.SetActive(false);
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

            PlayerPrefs.SetString("account", _accountInput.text);
            PlayerPrefs.SetString("passwd", _passwdInput.text);

            Facade.GetSingleton<NetController>().SetMainSessionDelegate(
                null,
                delegate (){ _busyTips.gameObject.SetActive(true); },
                delegate (bool sus){
                    _busyTips.gameObject.SetActive(false);
                });
            Facade.GetSingleton<NetController>().Login("127.0.0.1", (ushort)26001, _accountInput.text.Trim(), _passwdInput.text.Trim());
            _busyTips.gameObject.SetActive(true);
        });
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
