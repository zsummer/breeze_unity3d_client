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
        Facade.GetSingleton<Dispatcher>().AddListener("ChatResp", (System.Action<ChatResp>)OnChatResp);
        _inputField.onEndEdit.AddListener(delegate (string msg)
        {
            if (Facade._avatarID != 0 && msg.Length > 0)
            {
                _inputField.text = "";
                Facade.GetSingleton<NetController>().Send<ChatReq>(new ChatReq((ushort)ChatChannelEnum.CC_WORLD, 0, msg));
                _inputField.ActivateInputField();
            }
            else
            {
                _inputField.text = "";
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
                buf += "<color=red>[世界]</color>:来自玩家<color=blue>[" + item.sourceName + "]的发言: </color> "; 
            }
            else if (item.channelID == (ushort)ChatChannelEnum.CC_PRIVATE)
            {
                buf += "<color=yellow>[私聊]</color>:来自玩家<color=blue>[" + item.sourceName + "]的发言: </color> ";
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
