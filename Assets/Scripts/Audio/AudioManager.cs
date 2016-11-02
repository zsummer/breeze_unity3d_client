using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

    public AudioSource _welcome = null;
    public AudioSource _byebye = null;
    void Start ()
    {
	    
	}

    void Awake()
    {
        _welcome = LoadAudio("media/welcome");
        _welcome.volume = GameOption._AudioVolume;
        _byebye = LoadAudio("media/byebye");
        _byebye.volume = GameOption._AudioVolume;
    }

    void Update ()
    {
	
	}

    AudioSource LoadAudio(string path)
    {
        var source = Resources.Load<AudioSource>(path);
        if (source != null)
        {
            return Instantiate< AudioSource>(source);

        }
        return null;
    }
}
