using UnityEngine;
using System.Collections;
using Proto4z;
using System;


public class SceneManager : MonoBehaviour
{
    private static System.Collections.Generic.Dictionary<ulong, EntityShell> _shells = new System.Collections.Generic.Dictionary<ulong, EntityShell>();

	Transform _scene = null;
	float _sceneEndTime = 0f;

    GameObject _rcsHomeScene = null;
    GameObject _rcsMeleeScene = null;

    void Awake()
	{
		Facade.dispatcher.AddListener("ClickChangeModel", (System.Action)ClickChangeModel);
		Facade.dispatcher.AddListener("ChangeModeIDResp", (System.Action<ChangeModeIDResp>)OnChangeModeIDResp);
        Facade.dispatcher.AddListener("ClickAttack", (System.Action)ClickAttack);

        Facade.dispatcher.AddListener("SceneSectionNotice", (System.Action<SceneSectionNotice>)OnSceneSectionNotice);
		Facade.dispatcher.AddListener("SceneRefreshNotice", (System.Action<SceneRefreshNotice>)OnSceneRefreshNotice);
		Facade.dispatcher.AddListener("AddEntityNotice", (System.Action<AddEntityNotice>)OnAddEntityNotice);
		Facade.dispatcher.AddListener("RemoveEntityNotice", (System.Action<RemoveEntityNotice>)OnRemoveEntityNotice);
		Facade.dispatcher.AddListener("MoveNotice", (System.Action<MoveNotice>)OnMoveNotice);
		Facade.dispatcher.AddListener("AddBuffNotice", (System.Action<AddBuffNotice>)OnAddBuffNotice);
		Facade.dispatcher.AddListener("RemoveBuffNotice", (System.Action<RemoveBuffNotice>)OnRemoveBuffNotice);
		Facade.dispatcher.AddListener("UseSkillNotice", (System.Action<UseSkillNotice>)OnUseSkillNotice);
		Facade.dispatcher.AddListener("MoveResp", (System.Action<MoveResp>)OnMoveResp);
        Facade.dispatcher.AddListener("UseSkillResp", (System.Action<UseSkillResp>)OnUseSkillResp);
        Facade.dispatcher.AddListener("SceneEventNotice", (System.Action<SceneEventNotice>)OnSceneEventNotice);

        

	}

    void Start ()
    {

	}

    void FixedUpdate()
    {

    }

	public Transform GetScene()
	{
		return _scene;
	}
	public float GetSceneCountdown()
	{
		if (_scene == null)
			return 0;
		return _sceneEndTime - Time.realtimeSinceStartup;
	}

	public void DestroyCurrentScene()
	{
		if (_scene != null) 
		{
            CleanShells();
			GameObject.Destroy(_scene.gameObject);
			_scene = null;
			Facade.mainUI.skillPanel.gameObject.SetActive(false);
			Facade.mainUI.touchPanel.gameObject.SetActive(false);
            Facade.mainUI.miniMap.gameObject.SetActive(false);
			Facade.mainUI.SetActiveBG(true);
            Facade.audioManager._byebye.Play(0);
        }

	}

    public void RefreshEntityMove(Proto4z.EntityMoveArray moves)
    {
        foreach (var mv in moves)
        {
            var shell = GetShell(mv.eid);
            if (shell != null)
            {
                shell.RefreshMoveInfo(mv);
            }
        }
    }
    public void RefreshEntityState(Proto4z.EntityStateArray states)
    {
        foreach (var state in states)
        {
            var entity = GetShell(state.eid);
            if (entity != null)
            {
                if(entity.ghost.state.modelID != state.modelID)
                {
                    entity.ghost.state = state;
                    BuildShell(entity.ghost);
                }
                else
                {
                    entity.ghost.state = state;
                }
                

            }
        }
    }
    public EntityShell GetShell(ulong entityID)
    {
        EntityShell ret = null;
        _shells.TryGetValue(entityID, out ret);
        return ret;
    }
    public System.Collections.Generic.Dictionary<ulong, EntityShell> GetShells()
    {
        return _shells;
    }
	public void CleanShells()
	{
        foreach (var e in _shells)
        {
			if (e.Value.ghost.state.eid == Facade.myShell) 
			{
				Facade.myShell = 0;
			}
            GameObject.Destroy(e.Value.gameObject);
        }
        _shells.Clear();
	}
    public void DestroyShell(ulong entityID)
    {
        var entity = GetShell(entityID);
        if (entity == null)
        {
            return;
        }
        if (entity.ghost.state.eid == Facade.myShell)
        {
            Facade.myShell = 0;
        }
        _shells.Remove(entityID);
        GameObject.Destroy(entity.gameObject);
    }

    public void BuildShell(Proto4z.EntityFullData ghost)
    {
        EntityShell oldShell = GetShell(ghost.state.eid);
        
        Vector3 spawnpoint = new Vector3((float)ghost.mv.position.x, -13.198f, (float)ghost.mv.position.y);
        Quaternion rotation = new Quaternion();
        if (oldShell != null && oldShell != null)
        {
            spawnpoint = oldShell.gameObject.transform.position;
            rotation = oldShell.gameObject.transform.rotation;
        }

        string modelName = Facade.modelDict.GetModelName(ghost.state.modelID);
        if (modelName == null)
        {
            modelName = "jing_ling_nv_001_ty";
        }

        var modelRes = Resources.Load<GameObject>("Character/Model/" + modelName);
        if (modelRes == null)
        {
            Debug.LogError("can't load resouce model [" + modelName + "].");
            return;
        }
        var model = Instantiate(modelRes);
        if (model == null)
        {
            Debug.LogError("can't Instantiate model[" + modelName + "].");
            return;
        }

        model.AddComponent<Rigidbody>();
        if (model.GetComponent<Animation>() == null)
        {
            model.AddComponent<Animation>();
        }
        Rigidbody rd = model.GetComponent<Rigidbody>();
        rd.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
//        Light lt = model.AddComponent<Light>();
//        lt.range = 1.0f;
//        lt.intensity = 8.0f;

        var newShellObj = new GameObject();
        newShellObj.name = ghost.state.avatarName;
        model.transform.SetParent(newShellObj.transform);


        var newShell = newShellObj.AddComponent<EntityShell>();
        newShell._model = model.transform;
        newShell.ghost = ghost;
        newShellObj.transform.position = spawnpoint;
        newShellObj.transform.rotation = rotation;

        if (ghost.state.etype == (ushort)Proto4z.ENTITY_TYPE.ENTITY_PLAYER)
        {
            newShellObj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            newShell._modelHeight *= 2.5f;
        }
        else
        {
            newShellObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            newShell._modelHeight *= 1.5f;
        }

        DestroyShell(ghost.state.eid);


        _shells[ghost.state.eid] = newShell;
        if (newShell.ghost.state.avatarID == Facade.avatarInfo.avatarID 
            && newShell.ghost.state.etype == (ushort)Proto4z.ENTITY_TYPE.ENTITY_PLAYER)
        {
            Facade.myShell = newShell.ghost.state.eid;
            var selected = Instantiate(Resources.Load<GameObject>("Effect/other/selected"));
            selected.transform.SetParent(newShellObj.transform,false);
        }
        if (newShell.ghost.state.state == (ushort)Proto4z.ENTITY_STATE.ENTITY_STATE_DIED
            || newShell.ghost.state.state == (ushort)Proto4z.ENTITY_STATE.ENTITY_STATE_LIE)
        {
            newShell.PlayDeath();
        }
        Debug.Log("create avatar");
    }


	void OnChangeModeIDResp(ChangeModeIDResp resp)
	{
	}
	void ClickChangeModel()
	{
		Facade.serverProxy.SendToGame(new ChangeModeIDReq(Facade.avatarInfo.modeID%45+1));
	}

	void OnSceneSectionNotice(SceneSectionNotice notice)
	{
		if (_scene != null)
		{
			GameObject.Destroy(_scene.gameObject);
		}
		_sceneEndTime = Time.realtimeSinceStartup + (float)notice.section.sceneEndTime - (float)notice.section.serverTime;
        Debug.Log("SceneManager::OnSceneSectionNotice begin Load scene.");

        GameObject rcsScene = null;
        DateTime now =  DateTime.Now;
        if (notice.section.sceneType == (ushort)SCENE_TYPE.SCENE_HOME)
        {
            if (_rcsHomeScene == null)
            {
                _rcsHomeScene = Resources.Load<GameObject>("Scene/Home");
            }
            rcsScene = _rcsHomeScene;
        }
        else if (notice.section.sceneType == (ushort)SCENE_TYPE.SCENE_MELEE)
        {
            if (_rcsMeleeScene == null)
            {
                _rcsMeleeScene = Resources.Load<GameObject>("Scene/Melee");
                //StartCoroutine(CreateRandomMap());
            }
            rcsScene = _rcsMeleeScene;
        }
        else if (notice.section.sceneType == (ushort)SCENE_TYPE.SCENE_ARENA)
        {
            if (_rcsHomeScene == null)
            {
                _rcsHomeScene = Resources.Load<GameObject>("Scene/Home");
            }
            rcsScene = _rcsHomeScene;
        }
        if (rcsScene == null)
        {
            Debug.LogError("SceneManager::OnSceneSectionNotice can not load scene.");
            return;
        }
        Debug.Log("SceneManager::OnSceneSectionNotice Resources.Load time=" + (DateTime.Now - now).TotalMilliseconds.ToString(".#") + " ms");
        Facade.mainUI.chatUI.GetComponent<ChatUI>().PushMessage("Resources.Load time=" + (DateTime.Now - now).TotalMilliseconds.ToString(".#") + " ms");
        now = DateTime.Now;
        _scene = Instantiate(rcsScene).transform;
        _scene.gameObject.SetActive(true);
        Facade.mainUI.SetActiveBG(false);
        Facade.mainUI.touchPanel.gameObject.SetActive(true);
        Facade.mainUI.skillPanel.gameObject.SetActive(true);
        Facade.mainUI.miniMap.gameObject.SetActive(true);
        Facade.audioManager._welcome.Play(0);
        Debug.Log("SceneManager::OnSceneSectionNotice Instantiate scene time=" + (DateTime.Now - now).TotalMilliseconds.ToString(".#") + " ms");
        Facade.mainUI.chatUI.GetComponent<ChatUI>().PushMessage("Resources.Instantiate time=" + (DateTime.Now - now).TotalMilliseconds.ToString(".#") + " ms");
    }

    private IEnumerator CreateRandomMap()
    {
        GameObject  go = new GameObject("_Spawner___");
        go.AddComponent<Spawner>();
        while (!Spawner.isComplete)
        {
            yield return null;
        }
        yield break;
    }

    void OnSceneRefreshNotice(SceneRefreshNotice notice)
	{
		Facade.sceneManager.RefreshEntityState(notice.entityStates);
		Facade.sceneManager.RefreshEntityMove(notice.entityMoves);
	}
	void OnAddEntityNotice(AddEntityNotice notice)
	{
		foreach (var ghost in notice.entitys)
		{
			Facade.sceneManager.BuildShell(ghost);
		}
	}
	void OnRemoveEntityNotice(RemoveEntityNotice notice)
	{
		Debug.Log(notice);
	}
	void OnMoveNotice(MoveNotice notice)
	{
//        UnityEngine.Debug.Log("MoveNotice[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]eid=" + notice.moveInfo.eid
//            + ", action=" + notice.moveInfo.action + ", posx=" + notice.moveInfo.position.x
//            + ", posy=" + notice.moveInfo.position.y);
    }
	void OnAddBuffNotice(AddBuffNotice notice)
	{
		Debug.Log(notice);
	}
	void OnRemoveBuffNotice(RemoveBuffNotice notice)
	{
		Debug.Log(notice);
	}
	void OnUseSkillNotice(UseSkillNotice notice)
	{
		var entity = GetShell (notice.eid);
		if (entity == null)
		{
			return;
		}
        entity.transform.LookAt(new Vector3((float)notice.skill.activeDst.x, entity.transform.position.y, (float)notice.skill.activeDst.y));
        entity.PlayAttack();
	}
	void OnMoveResp(MoveResp resp)
	{
		
	}
	void OnSceneEventNotice(SceneEventNotice notice)
	{
        foreach (var ev in notice.info)
        {
            var e = GetShell(ev.dst);
            if (e == null)
            {
                continue;
            }
            Debug.Log("OnSceneEventNotice[" + e.ghost.state.avatarName + "] event=" + ev.ev);
            if (ev.ev == (ushort)SCENE_EVENT.SCENE_EVENT_REBIRTH)
            {
                var strPos = ev.mix.Split(',');
                e.ghost.mv.position.x = double.Parse(strPos[0]);
                e.ghost.mv.position.y = double.Parse(strPos[1]);
                e.ghost.state.curHP = ev.val;
                e.transform.position = new Vector3((float)e.ghost.mv.position.x, e.transform.position.y, (float)e.ghost.mv.position.y);
                e.PlayFree();
                StartCoroutine(e.CreateEffect("Effect/other/fuhuo", Vector3.zero, 0f, 5f));
            }
            else if (ev.ev == (ushort)SCENE_EVENT.SCENE_EVENT_LIE)
            {
                e.PlayDeath();
                
            }
            else if (ev.ev == (ushort)SCENE_EVENT.SCENE_EVENT_HARM_ATTACK
                || ev.ev == (ushort)SCENE_EVENT.SCENE_EVENT_HARM_HILL
                || ev.ev == (ushort)SCENE_EVENT.SCENE_EVENT_HARM_CRITICAL
                || ev.ev == (ushort)SCENE_EVENT.SCENE_EVENT_HARM_MISS)
            {
                GameObject obj = new GameObject();
                obj.name = "FightFloatingText";
                obj.transform.position = e.transform.position;
                obj.transform.localScale = e.transform.localScale;
                var text = obj.AddComponent<FightFloatingText>();
                if (ev.ev == (ushort)SCENE_EVENT.SCENE_EVENT_HARM_HILL)
                {
                    text._text = "+" + ev.val;
                    text._textColor = Color.blue;
                }
                else
                {
                    text._text = "-" + ev.val;
                    text._textColor = Color.red;
                }
                obj.SetActive(true);
                StartCoroutine(e.CreateEffect("Effect/skill/hit/hit_steal", Vector3.up, 0.1f, 2f));
            }
        }
	}	

	void OnUseSkillResp(UseSkillResp resp)
	{
		
	}	

	void ClickAttack()
	{
        var shell = Facade.sceneManager.GetShell(Facade.myShell);
        if (shell == null)
        {
            return;
        }
        shell.ClickAttack();

	}

}
