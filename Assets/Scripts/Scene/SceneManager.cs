using UnityEngine;
using System.Collections;
using Proto4z;
using System;


public class SceneManager : MonoBehaviour
{
    private static System.Collections.Generic.Dictionary<ulong, EntityModel> _entitys = new System.Collections.Generic.Dictionary<ulong, EntityModel>();
    private static System.Collections.Generic.Dictionary<ulong, EntityModel> _players = new System.Collections.Generic.Dictionary<ulong, EntityModel>();

	Transform _scene = null;
	float _sceneEndTime = 0f;

    GameObject _rcsHomeScene = null;

    void Awake()
	{
		Facade._dispatcher.AddListener("OnChangeAvatarModel", (System.Action)OnChangeAvatarModel);
		Facade._dispatcher.AddListener("ChangeModeIDResp", (System.Action<ChangeModeIDResp>)OnChangeModeIDResp);

		Facade._dispatcher.AddListener("SceneSectionNotice", (System.Action<SceneSectionNotice>)OnSceneSectionNotice);
		Facade._dispatcher.AddListener("SceneRefreshNotice", (System.Action<SceneRefreshNotice>)OnSceneRefreshNotice);
		Facade._dispatcher.AddListener("AddEntityNotice", (System.Action<AddEntityNotice>)OnAddEntityNotice);
		Facade._dispatcher.AddListener("RemoveEntityNotice", (System.Action<RemoveEntityNotice>)OnRemoveEntityNotice);
		Facade._dispatcher.AddListener("MoveNotice", (System.Action<MoveNotice>)OnMoveNotice);
		Facade._dispatcher.AddListener("AddBuffNotice", (System.Action<AddBuffNotice>)OnAddBuffNotice);
		Facade._dispatcher.AddListener("RemoveBuffNotice", (System.Action<RemoveBuffNotice>)OnRemoveBuffNotice);
		Facade._dispatcher.AddListener("UseSkillNotice", (System.Action<UseSkillNotice>)OnUseSkillNotice);
		Facade._dispatcher.AddListener("MoveResp", (System.Action<MoveResp>)OnMoveResp);
        Facade._dispatcher.AddListener("UseSkillResp", (System.Action<UseSkillResp>)OnUseSkillResp);
        Facade._dispatcher.AddListener("SceneEventNotice", (System.Action<SceneEventNotice>)OnSceneEventNotice);

        Facade._dispatcher.AddListener("ButtonAttack", (System.Action)OnButtonAttack);

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
			CleanEntity();
			GameObject.Destroy(_scene.gameObject);
			_scene = null;
			Facade._mainUI._skillPanel.gameObject.SetActive(false);
			Facade._mainUI._touchPanel.gameObject.SetActive(false);
			Facade._mainUI.SetActiveBG(true);
            Facade._audioManager._byebye.Play(0);
        }

	}

    public void RefreshEntityMove(Proto4z.EntityMoveArray moves)
    {
        foreach (var mv in moves)
        {
            var entity = GetEntity(mv.eid);
            if (entity != null)
            {
                entity.RefreshMoveInfo(mv);
            }
        }
    }
    public void RefreshEntityInfo(Proto4z.EntityInfoArray infos)
    {
        foreach (var info in infos)
        {
            var entity = GetEntity(info.eid);
            if (entity != null)
            {
                entity._info.entityInfo = info;
            }
        }
    }
    public EntityModel GetEntity(ulong entityID)
    {
        EntityModel ret = null;
        _entitys.TryGetValue(entityID, out ret);
        return ret;
    }
    public EntityModel GetPlayer(ulong avatarID)
    {
        EntityModel ret = null;
        _players.TryGetValue(avatarID, out ret);
        return ret;
    }
	public void CleanEntity()
	{
        foreach (var e in _entitys)
        {
			if (e.Value._info.entityInfo.eid == Facade._entityID) 
			{
				Facade._entityID = 0;
			}
            GameObject.Destroy(e.Value.gameObject);
        }
        _entitys.Clear();
        _players.Clear();
	}
    public void DestroyEntity(ulong entityID)
    {
        var entity = GetEntity(entityID);
        if (entity == null)
        {
            return;
        }
        if (entity._info.entityInfo.eid == Facade._entityID)
        {
            Facade._entityID = 0;
        }
        if (entity._info.baseInfo.avatarID != 0)
        {
            _players.Remove(entity._info.baseInfo.avatarID);
        }
        _entitys.Remove(entityID);
        GameObject.Destroy(entity.gameObject);
    }
    public void DestroyPlayer(ulong avatarID)
    {
        var entity = GetPlayer(avatarID);
        if (entity != null)
        {
            DestroyEntity(entity._info.entityInfo.eid);
        }
    }

    public void CreateEntityByAvatarID(ulong avatarID)
    {
        var entity = GetPlayer(avatarID);
        if (entity != null)
        {
            Debug.LogError("CreateAvatarByAvatarID not found full data");
            return;
        }
        CreateEntity(entity._info);
    }
    public void CreateEntity(Proto4z.EntityFullData data)
    {
        EntityModel oldEnity = GetEntity(data.entityInfo.eid);
        
        Vector3 spawnpoint = new Vector3((float)data.entityMove.position.x, -13.198f, (float)data.entityMove.position.y);
        Quaternion quat = new Quaternion();
        if (oldEnity != null && oldEnity != null)
        {
            spawnpoint = oldEnity.gameObject.transform.position;
            quat = oldEnity.gameObject.transform.rotation;
        }

        string name = Facade._modelDict.GetModelName(data.baseInfo.modeID);
        if (name == null)
        {
            name = "jing_ling_nv_001_ty";
        }

        var res = Resources.Load<GameObject>("Character/Model/" + name);
        if (res == null)
        {
            Debug.LogError("can't load resouce model [" + name + "].");
            return;
        }
        var obj = Instantiate(res);
        if (obj == null)
        {
            Debug.LogError("can't Instantiate model[" + name + "].");
            return;
        }

        obj.AddComponent<Rigidbody>();
        if (obj.GetComponent<Animation>() == null)
        {
            obj.AddComponent<Animation>();
        }
        obj.AddComponent<EntityModel>();
        obj.AddComponent<Light>();
        obj.transform.position = spawnpoint;
        if (data.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_PLAYER)
        {
            obj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        }
        else
        {
            obj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        obj.transform.rotation = quat;
        Rigidbody rd = obj.GetComponent<Rigidbody>();
        rd.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        Light lt = obj.GetComponent<Light>();
        lt.range = 1.0f;
        lt.intensity = 8.0f;




        DestroyEntity(data.entityInfo.eid);


		var newEntity = obj.GetComponent<EntityModel>();
        newEntity._info = data;
        _entitys[data.entityInfo.eid] = newEntity;
        if (data.baseInfo.avatarID != 0)
        {
            _players[data.baseInfo.avatarID] = newEntity;
        }
        if (newEntity._info.baseInfo.avatarID == Facade._avatarInfo.avatarID 
            && newEntity._info.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_PLAYER)
        {
            Facade._entityID = newEntity._info.entityInfo.eid;
        }
        if (newEntity._info.entityInfo.state == (ushort)Proto4z.EntityState.ENTITY_STATE_DIED
            || newEntity._info.entityInfo.state == (ushort)Proto4z.EntityState.ENTITY_STATE_LIE)
        {
            newEntity.PlayerDeath();
        }
        Debug.Log("create avatar");
    }


	void OnChangeModeIDResp(ChangeModeIDResp resp)
	{
		Debug.logger.Log("ServerProxy::OnChangeModeIDResp ret=" + resp.retCode + ", newModelID= " + resp.modeID );
	}
	void OnChangeAvatarModel()
	{
		Facade._serverProxy.SendToGame(new ChangeModeIDReq(Facade._avatarInfo.modeID%45+1));
	}

	void OnSceneSectionNotice(SceneSectionNotice notice)
	{
		Debug.Log(notice);
		if (_scene != null)
		{
			GameObject.Destroy(_scene.gameObject);
		}
		_sceneEndTime = Time.realtimeSinceStartup + (float)notice.section.sceneEndTime - (float)notice.section.serverTime;

        GameObject rcsScene = null;
        if (notice.section.sceneType == (ushort)SceneType.SCENE_HOME)
        {
            if (_rcsHomeScene == null)
            {
                _rcsHomeScene = Resources.Load<GameObject>("Scene/Home");
            }
            rcsScene = _rcsHomeScene;
        }
        else if (notice.section.sceneType == (ushort)SceneType.SCENE_ARENA)
        {
            if (_rcsHomeScene == null)
            {
                _rcsHomeScene = Resources.Load<GameObject>("Scene/Home");
            }
            rcsScene = _rcsHomeScene;
        }
        if (rcsScene == null)
        {
            Debug.LogError("can not local scene. ");
            return;
        }
        Debug.Log("create scene");
        _scene = Instantiate(_rcsHomeScene).transform;
        _scene.gameObject.SetActive(true);
        Facade._mainUI.SetActiveBG(false);
        Facade._mainUI._touchPanel.gameObject.SetActive(true);
        Facade._mainUI._skillPanel.gameObject.SetActive(true);
        Facade._audioManager._welcome.Play(0);
    }
	void OnSceneRefreshNotice(SceneRefreshNotice notice)
	{
		Facade._sceneManager.RefreshEntityInfo(notice.entityInfos);
		Facade._sceneManager.RefreshEntityMove(notice.entityMoves);
	}
	void OnAddEntityNotice(AddEntityNotice notice)
	{
		Debug.Log(notice);
		foreach (var entity in notice.entitys)
		{
			Facade._sceneManager.CreateEntity(entity);
		}
	}
	void OnRemoveEntityNotice(RemoveEntityNotice notice)
	{
		Debug.Log(notice);
	}
	void OnMoveNotice(MoveNotice notice)
	{
        UnityEngine.Debug.Log("MoveNotice[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "]eid=" + notice.moveInfo.eid
            + ", action=" + notice.moveInfo.action + ", posx=" + notice.moveInfo.position.x
            + ", posy=" + notice.moveInfo.position.y);
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
		var entity = GetEntity (notice.eid);
		if (entity == null)
		{
			return;
		}
		entity.PlayerAttack();
	}
	void OnMoveResp(MoveResp resp)
	{
		
	}
	void OnSceneEventNotice(SceneEventNotice notice)
	{
        foreach (var ev in notice.info)
        {
            var e = GetEntity(ev.dst);
            if (e == null)
            {
                continue;
            }
            Debug.Log("OnSceneEventNotice[" + e._info.baseInfo.avatarName + "] event=" + ev.ev);
            if (ev.ev == (ushort)SceneEvent.SCENE_EVENT_REBIRTH)
            {
                e.transform.position = new Vector3((float)e._info.entityMove.position.x, e.transform.position.y, (float)e._info.entityMove.position.y);
                e.PlayerFree();
            }
            else if (ev.ev == (ushort) SceneEvent.SCENE_EVENT_LIE)
            {
                e.PlayerDeath();
            }
            else if (ev.ev == (ushort) SceneEvent.SCENE_EVENT_HARM_ATTACK
                || ev.ev == (ushort)SceneEvent.SCENE_EVENT_HARM_HILL
                || ev.ev == (ushort)SceneEvent.SCENE_EVENT_HARM_CRITICAL
                || ev.ev == (ushort)SceneEvent.SCENE_EVENT_HARM_MISS)
            {
                GameObject obj = new GameObject();
                obj.name = "FightFloatingText";
                obj.transform.position = e.transform.position;
                obj.transform.localScale = e.transform.localScale;
                var text = obj.AddComponent<FightFloatingText>();
                text._text = "" + ev.val;
                obj.SetActive(true);
            }
        }
	}	

	void OnUseSkillResp(UseSkillResp resp)
	{
		
	}	

	void OnButtonAttack()
	{
        var et = Facade._sceneManager.GetEntity(Facade._entityID);
        if (et == null)
        {
            return;
        }
        float a = et.transform.rotation.eulerAngles.y;
        Vector3 target = et.transform.rotation * Vector3.forward;
        Facade._serverProxy.SendToScene(new UseSkillReq(Facade._entityID, 1, 0, new EPosition(target.x, target.z)));
	}

}
