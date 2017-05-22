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
    public Rect GetSpaceRect(RectTransform rect)
    {
        Rect spaceRect = rect.rect;
        spaceRect.x = spaceRect.x * rect.lossyScale.x + rect.position.x;
        spaceRect.y = spaceRect.y * rect.lossyScale.y + rect.position.y;
        spaceRect.width = spaceRect.width * rect.lossyScale.x;
        spaceRect.height = spaceRect.height * rect.lossyScale.y;
        return spaceRect;
    }


    void OnGUI()
    {
        RectTransform rt = transform as RectTransform;

        Vector2 org = new Vector2(rt.position.x, Screen.height - rt.position.y);



        if (Facade.entityID == 0 )
        {
            return;
        }
        EntityShell player = Facade.sceneManager.GetEntity(Facade.entityID);
        if (player == null)
        {
            return;
        }

    

        GUIStyle st = new GUIStyle();
        st.normal.textColor = Color.green;
        
        GUI.Box(new Rect(org, new Vector2(20, 20)), "*", st);

        foreach (var e in Facade.sceneManager.GetEntity())
        {
            if (e.Value._info.state.eid == player._info.state.eid)
            {
                continue;
            }
            Vector2 vt = toVector2(e.Value._info.mv.position) - toVector2(player._info.mv.position);
            vt /= 1.0f;
            vt.y = -vt.y;
            vt = org + vt;
            if (!GetSpaceRect(rt).Contains(new Vector2(vt.x, Screen.height - vt.y)))
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
