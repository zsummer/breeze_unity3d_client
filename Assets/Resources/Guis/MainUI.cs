﻿using System;
using UnityEngine;
using UnityEngine.UI;


public class MainUI: MonoBehaviour
{
    public Transform _busyTips = null;
    public Transform _chatUI = null;
    public Transform _loginUI = null;
    public Transform _skillPanel = null;
    public Transform _selectScenePanel = null;
    public Transform _touchPanel = null;
    public Transform _miniMap = null;

    RawImage _bgImg = null;
    public void SetActiveBG(bool enable)
    {
        if (_bgImg != null){ _bgImg.enabled = enable;}
    }

    void Awake()
    {
        Debug.Log("MainUI Awake.");
        DontDestroyOnLoad(gameObject);

        Facade.mainUI = this;
        _bgImg = gameObject.GetComponent<RawImage>();

        _busyTips = LoadUI("BusyTips", "Guis/BusyTips/BusyTips");
        _chatUI = LoadUI("ChatUI", "Guis/ChatUI/ChatUI");
        _loginUI = LoadUI("LoginUI", "Guis/LoginUI/LoginUI");

        _skillPanel = LoadUI("SkillPanel", "Guis/SkillPanel/SkillPanel");
        _skillPanel.Find("Attack").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("ButtonAttack", null); });

        _selectScenePanel = LoadUI("SelectScenePanel", "Guis/SelectScenePanel/SelectScenePanel");
        _selectScenePanel.Find("ExitScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("OnExitScene", null); });
        _selectScenePanel.Find("HomeScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("OnHomeScene", null); });
        _selectScenePanel.Find("MeleeScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("OnMeleeScene", null); });
        _selectScenePanel.Find("ArenaScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("OnArenaScene", null); });
		_selectScenePanel.Find("ChangeModel").GetComponent<Button>().onClick.AddListener(
			delegate () { Facade.dispatcher.TriggerEvent("OnChangeAvatarModel", null); });

        _touchPanel = LoadUI("TouchPanel", "Guis/TouchPanel/TouchPanel");

        _miniMap = LoadUI("Minimap", "Guis/Minimap/Minimap");
    }

    void Start()
    {
    }

    void Update()
    {
    }

    void OnApplicationQuit()
    {
        Debug.Log("MainUI Quit.");
    }

    Transform LoadUI(string name, string path)
    {
        var dstObj = GameObject.Find(name);
        if (dstObj != null)
        {
            return dstObj.transform;
        }
        Transform tf = null;
        var source = Resources.Load<GameObject>(path);
        if (source != null)
        {
            tf = Instantiate(source).transform;
            tf.SetParent(gameObject.transform, false);
            tf.gameObject.SetActive(false);
            return tf;
        }
        else
        {
            Debug.LogError("LoadUI  error. [" + path + "].");
        }
        return tf;
    }

}
