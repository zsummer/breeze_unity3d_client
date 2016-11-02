using UnityEngine;
using System.Collections;
using Proto4z;
public class ModelDict : MonoBehaviour
{
    System.Collections.Generic.Dictionary<string, int> _modelNames;
    System.Collections.Generic.Dictionary<int, string> _modelIDs;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _modelNames = new System.Collections.Generic.Dictionary<string, int>();
        _modelNames["ai_ge_001_ty"] = 1;
        _modelNames["hai_wu_shi_001_ty"] = 2;
        _modelNames["jlxb_yc_001_ty"] = 3;
        _modelNames["lisa_001_ty"] = 4;
        //_modelNames["shop_001_ty"] = 5;
        _modelNames["xue_jing_ling_001_ty"] = 6;
        _modelNames["bian_fu_001_ty"] = 7;
        _modelNames["hei_mo_lv_001_ty"] = 8;
        _modelNames["ju_du_zhu_001_ty"] = 9;
        _modelNames["mai_nu_001_ty"] = 10;
        _modelNames["tai_yu_shen_001_ty"] = 11;
        _modelNames["ye_lang_001_ty"] = 12;
        _modelNames["cheng_jie_zhe_001_ty"] = 13;
        _modelNames["hun_mo_001_ty"] = 14;
        _modelNames["ju_ren_001_ty"] = 15;
        _modelNames["nv_yao_001_ty"] = 16;
        _modelNames["tan_shi_gui_001_ty"] = 17;
        _modelNames["yi_liao_yuan_ren_001_ty"] = 18;
        _modelNames["fei_long_001_ty"] = 19;
        _modelNames["jia_chong_001_ty"] = 20;
        _modelNames["ku_li_yuan_ren_001_ty"] = 21;
        _modelNames["pang_ge_001_ty"] = 22;
//        _modelNames["tys_hand_001_ty"] = 23;
        _modelNames["yu_ren_001_ty"] = 24;
        _modelNames["fu_shi_long_001_ty"] = 25;
        _modelNames["jia_long_001_ty"] = 26;
        _modelNames["ku_lou_qi_shi_001_ty"] = 27;
        _modelNames["sa_qi_er_001_ty"] = 28;
        _modelNames["wen_yi_wu_shi_001_ty"] = 29;
        _modelNames["zha_dan_ren_001_ty"] = 30;
        _modelNames["gong_cheng_che_001_ty"] = 31;
        _modelNames["jing_ling_nan_001_ty"] = 32;
        _modelNames["kui_she_001_ty"] = 33;
        _modelNames["sha_er_meng_001_ty"] = 34;
        _modelNames["wlxb_jz_001_ty"] = 35;
        _modelNames["gui_wu_shi_001_ty"] = 36;
        _modelNames["jing_ling_nv_001_ty"] = 37;
        _modelNames["lei_ling_001_ty"] = 38;
        _modelNames["shi_hun_zhe_001_ty"] = 39;
        _modelNames["wlxb_yc_001_ty"] = 40;
        _modelNames["hai_qi_shi_001_ty"] = 41;
        _modelNames["jlxb_jz_001_ty"] = 42;
        _modelNames["ling_hun_zhan_che_001_ty"] = 43;
        _modelNames["shi_mo_001_ty"] = 44;
        _modelNames["xiong_001_ty"] = 45;
        _modelIDs = new System.Collections.Generic.Dictionary<int, string>();
        foreach (var item in _modelNames)
        {
            _modelIDs[item.Value] = item.Key;
        }
    }
    public string GetModelName(int id)
    {
        string ret;
        if (_modelIDs.TryGetValue(id, out ret))
        {
            return ret;
        }
        return null;
    }
    public int GetNextModelID(int id)
    {
        int ret = -1;
        foreach (var item in _modelIDs)
        {
            if (ret == -1 || ret > item.Key)
            {
                ret = item.Key;
            }
            if (item.Key > id)
            {
                return item.Key;
            }
        }
        return ret;
    }
    public int GetModelID(string name)
    {
        int id = 0;
        if (_modelNames.TryGetValue(name, out id))
        {
            return id;
        }
        return 0;
    }
    // Use this for initialization
    void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
