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
                entity._info.info = info;
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
			if (e.Value._info.info.eid == Facade._entityID) 
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
        if (entity._info.info.eid == Facade._entityID)
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
            DestroyEntity(entity._info.info.eid);
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
        EntityModel oldEnity = GetEntity(data.info.eid);
        
        Vector3 spawnpoint = new Vector3((float)data.mv.position.x, -13.198f, (float)data.mv.position.y);
        Quaternion rotation = new Quaternion();
        if (oldEnity != null && oldEnity != null)
        {
            spawnpoint = oldEnity.gameObject.transform.position;
            rotation = oldEnity.gameObject.transform.rotation;
        }

        string modelName = Facade._modelDict.GetModelName(data.baseInfo.modelID);
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
        Light lt = model.AddComponent<Light>();
        lt.range = 1.0f;
        lt.intensity = 8.0f;

        var entity = new GameObject();
        entity.name = data.baseInfo.modelName;
        model.transform.SetParent(entity.transform);


        var entityScrpt = entity.AddComponent<EntityModel>();
        entityScrpt._model = model.transform;
        entityScrpt._info = data;
        entity.transform.position = spawnpoint;
        entity.transform.rotation = rotation;

        if (data.info.etype == (ushort)Proto4z.EntityType.ENTITY_PLAYER)
        {
            entity.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            entityScrpt._modelHeight *= 2.5f;
        }
        else
        {
            entity.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            entityScrpt._modelHeight *= 1.5f;
        }



        DestroyEntity(data.info.eid);


        _entitys[data.info.eid] = entityScrpt;
        if (data.baseInfo.avatarID != 0)
        {
            _players[data.baseInfo.avatarID] = entityScrpt;
        }
        if (entityScrpt._info.baseInfo.avatarID == Facade._avatarInfo.avatarID 
            && entityScrpt._info.info.etype == (ushort)Proto4z.EntityType.ENTITY_PLAYER)
        {
            Facade._entityID = entityScrpt._info.info.eid;
            var selected = Instantiate(Resources.Load<GameObject>("Effect/other/selected"));
            selected.transform.SetParent(entity.transform,false);
        }
        if (entityScrpt._info.info.state == (ushort)Proto4z.EntityState.ENTITY_STATE_DIED
            || entityScrpt._info.info.state == (ushort)Proto4z.EntityState.ENTITY_STATE_LIE)
        {
            entityScrpt.PlayDeath();
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
		entity.PlayAttack();
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
                var strPos = ev.mix.Split(',');
                e._info.mv.position.x = double.Parse(strPos[0]);
                e._info.mv.position.y = double.Parse(strPos[1]);
                e._info.info.curHP = ev.val;
                e.transform.position = new Vector3((float)e._info.mv.position.x, e.transform.position.y, (float)e._info.mv.position.y);
                e.PlayFree();
                StartCoroutine(e.CreateEffect("Effect/other/fuhuo", Vector3.zero, 0f, 5f));
            }
            else if (ev.ev == (ushort) SceneEvent.SCENE_EVENT_LIE)
            {
                e.PlayDeath();
                
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
                if (ev.ev == (ushort)SceneEvent.SCENE_EVENT_HARM_HILL)
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

	void OnButtonAttack()
	{
        var et = Facade._sceneManager.GetEntity(Facade._entityID);
        if (et == null)
        {
            return;
        }
        et.DoAttack();

	}

}
