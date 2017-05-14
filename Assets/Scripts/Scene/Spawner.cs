using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    static bool IS_EDITOR = false;
    public static bool isComplete = false;
	public GameObject obstaclePrefab;

    static int[] obstacles = new int[] {
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,
        0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
    };
    void Start()
	{
        obstaclePrefab = Resources.Load<GameObject>("Scene/Cube");

        SpawnObstacles();
	}
	void SpawnObstacles()
	{
        var parent = GameObject.Find("Obstacles");
        List<string> t_list = new List<string>();
        int y=0;
		int x=0;
        int id = 0;
		for (int i=0; i < obstacles.Length; i++)
		{
			if (obstacles[i] == 1)
            {
                id++;
                var newGO = Instantiate(obstaclePrefab, new Vector3(-210 + x*20, -3, 40 + y*20), Quaternion.identity);
                newGO.transform.SetParent(parent.transform);
                if(IS_EDITOR)
                {
                    float t_x = newGO.transform.position.x;
                    float t_y = newGO.transform.position.z;
                    float t_halfW = obstaclePrefab.transform.localScale.x / 2f;
                    float t_halfH = obstaclePrefab.transform.localScale.z / 2f;

                    Vector2 p1 = new Vector2(t_x - t_halfW, t_y - t_halfH);
                    Vector2 p2 = new Vector2(t_x + t_halfW, t_y - t_halfH);
                    Vector2 p3 = new Vector2(t_x + t_halfW, t_y + t_halfH);
                    Vector2 p4 = new Vector2(t_x - t_halfW, t_y + t_halfH);

                    t_list.Add( p1.x + "," + p1.y + " " +
                                p2.x + "," + p2.y + " " +
                                p3.x + "," + p3.y + " " +
                                p4.x + "," + p4.y );
                }
            }
			if (i % 20 == 0) {
				y = y + 1;
				x = 0;
			}
			else
			{
				x = x + 1;
			}
		}
        if(t_list.Count > 0 && IS_EDITOR)
        {
            Write(t_list);
        }
        isComplete = true;
    }
    public void Write(List<string> list)
    {
        string path = Path.Combine(Application.dataPath, "obstacle.txt");
        if(File.Exists(path))
        {
            File.Delete(path);
        }
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        foreach (var item in list)
        {
            sw.WriteLine(item);
        }

        sw.Flush();
        sw.Close();
        fs.Close();
    }
}
