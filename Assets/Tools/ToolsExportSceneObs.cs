using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR 
using UnityEditor;

public class  ToolsExportSceneObs: MonoBehaviour {

	void Start ()
    {
	}
	void Update ()
    {
	}

    [MenuItem("Export/ExportSceneObs")]

    static void ExportGameObjects()
    {
        string scenePath = EditorApplication.currentScene;
        string sceneName = scenePath.Substring(scenePath.LastIndexOf("/") + 1, scenePath.Length - scenePath.LastIndexOf("/") - 1);
        sceneName = sceneName.Substring(0, sceneName.LastIndexOf("."));
        string savePath = EditorUtility.SaveFilePanel("obs", "", sceneName, "txt");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        FileStream fs = new FileStream(savePath, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);




        sw.WriteLine("obs:");
        foreach (GameObject go in Object.FindObjectsOfType(typeof(GameObject)))
        {
            float redius = 0f;
            if (go.name == "jinglingfangyuta" || go.name == "wanglingfangyuta" )
            {
                redius = 2f;
            }
            else if (go.name == "finalbuildingA_effect" || go.name == "finalbuildingB_effect")
            {
                redius = 3f;
            }
            if (redius > 0f)
            {
                string str = (go.transform.position.x - redius).ToString() + ",";
                str += (go.transform.position.z - redius).ToString() + " ";

                str += (go.transform.position.x + redius).ToString() + ",";
                str += (go.transform.position.z - redius).ToString() + " ";

                str += (go.transform.position.x + redius).ToString() + ",";
                str += (go.transform.position.z + redius).ToString() + " ";

                str += (go.transform.position.x - redius).ToString() + ",";
                str += (go.transform.position.z + redius).ToString() + " ";
                sw.WriteLine(str);
            }

        }

        sw.WriteLine("ai:");
        foreach (GameObject go in Object.FindObjectsOfType(typeof(GameObject)))
        {
            if (go.name == "jitan004")
            {
                string str = go.transform.position.x.ToString("F2") + ",";
                str += go.transform.position.z.ToString("F2") + " ";
                sw.WriteLine(str);
            }

        }


        sw.Flush();
        sw.Close();
        fs.Close();
    }
}
#endif

