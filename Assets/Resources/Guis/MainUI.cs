using System;
using UnityEngine;
using UnityEngine.UI;


public class MainUI: MonoBehaviour
{
    public Transform busyTips = null;
    public Transform chatUI = null;
    public Transform loginUI = null;
    public Transform skillPanel = null;
    public Transform selectScenePanel = null;
    public Transform touchPanel = null;
    public Transform miniMap = null;

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


        busyTips = LoadUI("BusyTips", "Guis/BusyTips/BusyTips");
        chatUI = LoadUI("ChatUI", "Guis/ChatUI/ChatUI");
        loginUI = LoadUI("LoginUI", "Guis/LoginUI/LoginUI");

        skillPanel = LoadUI("SkillPanel", "Guis/SkillPanel/SkillPanel");
        skillPanel.Find("Attack").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("ClickAttack", null); });

        selectScenePanel = LoadUI("SelectScenePanel", "Guis/SelectScenePanel/SelectScenePanel");
        selectScenePanel.Find("ExitScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("ClickExitScene", null); });
        selectScenePanel.Find("HomeScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("ClickHomeScene", null); });
        selectScenePanel.Find("MeleeScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("ClickMeleeScene", null); });
        selectScenePanel.Find("ArenaScene").GetComponent<Button>().onClick.AddListener(
            delegate () { Facade.dispatcher.TriggerEvent("ClickArenaScene", null); });
		selectScenePanel.Find("ChangeModel").GetComponent<Button>().onClick.AddListener(
			delegate () { Facade.dispatcher.TriggerEvent("ClickChangeModel", null); });

        touchPanel = LoadUI("TouchPanel", "Guis/TouchPanel/TouchPanel");

        miniMap = LoadUI("Minimap", "Guis/Minimap/Minimap");
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
