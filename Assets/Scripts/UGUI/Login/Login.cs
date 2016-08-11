using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Login : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        GameObject obj = GameObject.Find("LoginUI/LoginButton");
        UnityEngine.UI.Button btn = obj.GetComponent<Button>();
        btn.onClick.AddListener(delegate () { Facade.GetSingleton<NetController>(); });
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
