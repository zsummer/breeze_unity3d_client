using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Proto4z;
public class ChatUI : MonoBehaviour
{
    public InputField _inputField;
    public Text _msgPanel;
    private System.Collections.Generic.Queue<ChatResp> _msgs = new System.Collections.Generic.Queue<ChatResp>();
	// Use this for initialization
	void Start ()
    {
        gameObject.SetActive(false);
        Facade.GetSingleton<Dispatcher>().AddListener("ChatResp", (System.Action<ChatResp>)OnChatResp);


        _inputField.onEndEdit.AddListener(delegate (string msg)
        {
            string text = msg;
            _inputField.text = "";
            Debug.Log("onEndEdit" + text);
            if (Facade.AvatarInfo != null && msg.Length > 0)
            {
                //_inputField.ActivateInputField();
                Facade.GetSingleton<NetController>().Send<ChatReq>(new ChatReq((ushort)ChatChannelEnum.CC_WORLD, 0, msg));
            }
        });
    }

	// Update is called once per frame
	void Update ()
    {
	
	}

    void OnChatResp(ChatResp resp)
    {
        _msgs.Enqueue(resp);
        if (_msgs.Count > 6)
        {
            _msgs.Dequeue();
        }

        string buf = "";
        foreach (var item in _msgs)
        {
            if (item.channelID == (ushort)ChatChannelEnum.CC_WORLD)
            {
                buf += "<color=red>[世界]</color>:来自玩家<color=blue>[" + item.sourceName + "]</color>的发言: "; 
            }
            else if (item.channelID == (ushort)ChatChannelEnum.CC_PRIVATE)
            {
                buf += "<color=yellow>[私聊]</color>:来自玩家<color=blue>[" + item.sourceName + "]</color>的发言: ";
            }
            else if (item.channelID == (ushort)ChatChannelEnum.CC_SYSTEM)
            {
                buf += "<color=yellow>[系统]</color>: ";
            }
            else
            {
                continue;
            }
            buf += item.msg;
            buf += "\r\n";
        }
        _msgPanel.text = buf;
    }

}
