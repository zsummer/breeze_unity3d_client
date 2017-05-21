using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{

	void Start ()
    {
		
	}
	void Update ()
    {
		
	}

    Vector2 toVector2(Proto4z.EPosition ep)
    {
        return new Vector2((float)ep.x, (float)ep.y);
    }

    void OnGUI()
    {
        RectTransform rt = transform as RectTransform;
        if (Facade._entityID == 0 )
        {
            return;
        }
        EntityModel player = Facade._sceneManager.GetEntity(Facade._entityID);
        if (player == null)
        {
            return;
        }

        Vector2 org = new Vector2(Screen.width / 2.0f, rt.sizeDelta.y/2.0f);

        GUIStyle st = new GUIStyle();
        st.normal.textColor = Color.green;
        
        GUI.Box(new Rect(org, new Vector2(20, 20)), "*", st);

        foreach (var e in Facade._sceneManager.GetEntity())
        {
            if (e.Value._info.state.eid == player._info.state.eid)
            {
                continue;
            }
            Vector2 vt = toVector2(e.Value._info.mv.position) - toVector2(player._info.mv.position);
            vt /= 1.0f;
            vt.y = -vt.y;
            vt = org + vt;
            if (vt.y > rt.sizeDelta.y || vt.x < org.x - rt.sizeDelta.x/2.0f || vt.x > org.x + rt.sizeDelta.x/2.0f)
            {
                continue;
            }
            if (e.Value._info.state.etype == (ushort)Proto4z.ENTITY_TYPE.ENTITY_AI)
            {
                st.normal.textColor = Color.yellow;
                GUI.Box(new Rect(vt, new Vector2(20, 20)), "+", st);
            }
            else if (e.Value._info.state.etype == (ushort)Proto4z.ENTITY_TYPE.ENTITY_PLAYER)
            {
                st.normal.textColor = Color.red;
                GUI.Box(new Rect(vt, new Vector2(20, 20)), "*", st);
            }
        }


    }
}
