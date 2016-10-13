using UnityEngine;
using System.Collections;

public class ClientEntityData
{
    public EntityModel model = null;
    public Proto4z.EntityFullData data = null;
}


public class GameScene : MonoBehaviour
{
    private static System.Collections.Generic.Dictionary<ulong, ClientEntityData> _entitys = new System.Collections.Generic.Dictionary<ulong, ClientEntityData>();
    private static System.Collections.Generic.Dictionary<ulong, ClientEntityData> _players = new System.Collections.Generic.Dictionary<ulong, ClientEntityData>();



    void Start ()
    {

	}

    void FixedUpdate()
    {
    }

    public ClientEntityData GetEntity(ulong entityID)
    {
        ClientEntityData ret = null;
        _entitys.TryGetValue(entityID, out ret);
        return ret;
    }
    public ClientEntityData GetPlayer(ulong avatarID)
    {
        ClientEntityData ret = null;
        _players.TryGetValue(avatarID, out ret);
        return ret;
    }
	public void CleanEntity()
	{
        foreach (var e in _entitys)
        {
            GameObject.Destroy(e.Value.model.gameObject);
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
        _entitys.Remove(entityID);
        if (entity.model != null)
        {
            GameObject.Destroy(entity.model.gameObject);
            entity.model = null;
        }
        if (entity.data.baseInfo.avatarID == Facade._avatarInfo.avatarID && entity.data.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_AVATAR)
        {
            Facade._entityID = 0;
        }
        if (entity.data.baseInfo.avatarID != 0)
        {
            _players.Remove(entity.data.baseInfo.avatarID);
        }
    }
    public void DestroyPlayer(ulong avatarID)
    {
        var entity = GetPlayer(avatarID);
        if (entity != null)
        {
            DestroyEntity(entity.data.entityInfo.eid);
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
        CreateEntity(entity.data);
    }
    public void CreateEntity(Proto4z.EntityFullData data)
    {
        ClientEntityData oldEnity = GetEntity(data.entityInfo.eid);
        
        Vector3 spawnpoint = new Vector3((float)data.entityMove.pos.x, -13.198f, (float)data.entityMove.pos.y);
        Quaternion quat = new Quaternion();
        if (oldEnity != null && oldEnity.model != null)
        {
            spawnpoint = oldEnity.model.gameObject.transform.position;
            quat = oldEnity.model.gameObject.transform.rotation;
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
        obj.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        obj.transform.rotation = quat;
        Rigidbody rd = obj.GetComponent<Rigidbody>();
        rd.freezeRotation = true;
        DestroyEntity(data.entityInfo.eid);

        ClientEntityData newEntity = new ClientEntityData();
        newEntity.data = data;
		newEntity.model = obj.GetComponent<EntityModel>();
		newEntity.model.eid = data.entityInfo.eid;

        _entitys[data.entityInfo.eid] = newEntity;
        if (data.baseInfo.avatarID != 0)
        {
            _players[data.baseInfo.avatarID] = newEntity;
        }
        if (newEntity.data.baseInfo.avatarID == Facade._avatarInfo.avatarID && newEntity.data.entityInfo.etype == (ushort)Proto4z.EntityType.ENTITY_AVATAR)
        {
            Facade._entityID = newEntity.data.entityInfo.eid;
        }
        Debug.Log("create avatar");
    }
}
