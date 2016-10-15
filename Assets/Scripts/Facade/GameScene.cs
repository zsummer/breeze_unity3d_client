using UnityEngine;
using System.Collections;



public class GameScene : MonoBehaviour
{
    private static System.Collections.Generic.Dictionary<ulong, EntityModel> _entitys = new System.Collections.Generic.Dictionary<ulong, EntityModel>();
    private static System.Collections.Generic.Dictionary<ulong, EntityModel> _players = new System.Collections.Generic.Dictionary<ulong, EntityModel>();



    void Start ()
    {

	}

    void FixedUpdate()
    {
    }
    public void RefreshEntityMove(Proto4z.EntityMoveArray moves)
    {
        foreach (var mv in moves)
        {
            var entity = GetEntity(mv.eid);
            if (entity != null)
            {
                entity._info.entityMove = mv;
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
        if (entity._info.baseInfo.avatarID == Facade._avatarInfo.avatarID 
            && entity._info.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_AVATAR)
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
        
        Vector3 spawnpoint = new Vector3((float)data.entityMove.pos.x, -13.198f, (float)data.entityMove.pos.y);
        Quaternion quat = new Quaternion();
        if (oldEnity != null && oldEnity != null)
        {
            spawnpoint = oldEnity.gameObject.transform.position;
            quat = oldEnity.gameObject.transform.rotation;
        }

        string name = Facade.GetSingleton<ModelDict>().GetModelName(data.baseInfo.modeID);
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
        obj.AddComponent<CapsuleCollider>();
        if (obj.GetComponent<Animation>() == null)
        {
            obj.AddComponent<Animation>();
        }
        obj.AddComponent<EntityModel>();
        obj.transform.position = spawnpoint;
        if (data.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_AVATAR)
        {
            obj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        }
        else
        {
            obj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        obj.transform.rotation = quat;
        Rigidbody rd = obj.GetComponent<Rigidbody>();
        rd.freezeRotation = true;
        DestroyEntity(data.entityInfo.eid);


		var newEntity = obj.GetComponent<EntityModel>();
        newEntity._info = data;

        _entitys[data.entityInfo.eid] = newEntity;
        if (data.baseInfo.avatarID != 0)
        {
            _players[data.baseInfo.avatarID] = newEntity;
        }
        if (newEntity._info.baseInfo.avatarID == Facade._avatarInfo.avatarID 
            && newEntity._info.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_AVATAR)
        {
            Facade._entityID = newEntity._info.entityInfo.eid;
        }
        Debug.Log("create avatar");
    }
}
