using UnityEngine;
using System.Collections.Generic;



//优化静态特效,不可见停止渲染以及更新
public class OptimizeStaticEffect : MonoBehaviour
{
    Collider _collider = null;
    bool _visible = true;
    List<GameObject> childObjs = new List<GameObject>();
    float _delta = 0;
    void Start()
    {
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            return;
        }
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform ts = gameObject.transform.GetChild(i);
            GameObject psObj = ts.gameObject;
            childObjs.Add(psObj);
        }
    }

    

    void FixedUpdate()
    {
        _delta += Time.deltaTime;
        if (_delta <= 1.1f)
        {
            return;
        }
        _delta = 0;

        if (_visible != GameOption._specialEffect)
        {
            _visible = GameOption._specialEffect;
            if (_visible)
            {
                setActive(true);
            }
            else
            {
                setActive(false);
            }
        }
    }

    void setActive(bool flag)
    {     
        foreach (GameObject obj in childObjs)
        {
            obj.SetActive(flag);
        }
    }
}
