using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class StressLevel : MonoBehaviour, ISavable
{
    [SerializeField] float level = 0;
    //[SerializeField] bool lostMoreTimeRow = false;
    [SerializeField] float loseMultiply = 0.1f;
    [SerializeField] UnityEngine.Video.VideoPlayer videoPlayer;
    [Header("Meditacion 1")]
    [SerializeField] UnityEngine.Video.VideoClip medite1;
    [SerializeField] string urlVideo1;

    [Header("Meditacion 2")]
    [SerializeField] UnityEngine.Video.VideoClip medite2;
    [SerializeField] string urlVideo2;

    [Header("Meditacion 3")]
    [SerializeField] UnityEngine.Video.VideoClip medite3;
    [SerializeField] string urlVideo3;

    [Header("Meditacion 4")]
    [SerializeField] UnityEngine.Video.VideoClip medite4;
    [SerializeField] string urlVideo4;

    [Header("Meditacion 4.1")]
    [SerializeField] UnityEngine.Video.VideoClip medite41;
    [SerializeField] string urlVideo41;

    [Header("Meditacion 5")]
    [SerializeField] UnityEngine.Video.VideoClip medite5;
    [SerializeField] string urlVideo5;

    int countLose = 0;
    bool finish = false;

    public event Action OnStressLevelChange;

    public static StressLevel i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public void Init()
    {
        OnStressLevelChange?.Invoke();
    }

    public void AddLevel(float newLevel)
    {
        //if (lostMoreTimeRow)
        //    level += newLevel * 2;
        level += newLevel;

        //lostMoreTimeRow = lost? true: false;
        OnStressLevelChange?.Invoke();
    }


    public void ResetLevel()
    {
        level = 0;
        OnStressLevelChange?.Invoke();
    }

    public void PlayVideo()
    {
        GameController.Instance.PauseGame(true);
        videoPlayer.transform.gameObject.SetActive(true);
        float duration = 0;

        //duration = videosInternet();
        duration = videosLocal();
        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.loopPointReached += InactiveGameObject;
        //videoPlayer.Play();
        //duration = (float)videoPlayer.clip.length;

        //Invoke("InactiveGameObject", duration);
    }


    void InactiveGameObject(VideoPlayer vp)
    {
        videoPlayer.transform.gameObject.SetActive(false);
        //GameController.Instance.PauseGame(false);
        GameController.Instance.StartFreeRoamState();
        AudioManager.i.Volume(true);
    }

    private void OnPrepareCompleted(VideoPlayer player)
    {
        videoPlayer.Play();
    }

    public float videosInternet()
    {
        float duration = 0;
        if (finish)
        {
            //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            GameController.Instance.State = GameState.Paused;
            videoPlayer.url = urlVideo5;
            videoPlayer.Prepare();
            duration = (float)medite5.length;
        }
        else if (countLose == 1)
        {
            //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            videoPlayer.url = urlVideo1;
            videoPlayer.Prepare();
            duration = (float)medite1.length;
        }
        else if (countLose == 2)
        {
            //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            videoPlayer.url = urlVideo2;
            videoPlayer.Prepare();
            duration = (float)medite2.length;
        }
        else if (countLose == 3)
        {
            //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            videoPlayer.url = urlVideo3;
            videoPlayer.Prepare();
            duration = (float)medite3.length;
        }
        else if (countLose == 4)
        {
            //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            videoPlayer.url = urlVideo4;
            videoPlayer.Prepare();
            duration = (float)medite4.length;
        }
        else if (countLose == 5)
        {
            //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
            videoPlayer.url = urlVideo41;
            videoPlayer.Prepare();
            duration = (float)medite41.length;
        }
        else
        {
            int randomNumber = UnityEngine.Random.Range(1, 4);
            switch (randomNumber)
            {
                case 1:
                    //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
                    videoPlayer.url = urlVideo2;
                    videoPlayer.Prepare();
                    duration = (float)medite2.length;
                    break;
                case 2:
                    //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
                    videoPlayer.url = urlVideo3;
                    videoPlayer.Prepare();
                    duration = (float)medite3.length;
                    break;
                case 3:
                    //videoPlayer.source = UnityEngine.Video.VideoSource.Url;
                    videoPlayer.url = urlVideo41;
                    videoPlayer.Prepare();
                    duration = (float)medite41.length;
                    break;
            }
        }
        return duration;
    }

    public float videosLocal()
    {
        float duration = 0;
        if (finish)
        {
            GameController.Instance.State = GameState.Paused;
            videoPlayer.clip = medite5;
            duration = (float)medite5.length;
        }
        else if (countLose == 1)
        {
            videoPlayer.clip = medite1;
            duration = (float)medite1.length;
        }
        else if (countLose == 2)
        {
            videoPlayer.clip = medite2;
            duration = (float)medite2.length;
        }
        else if (countLose == 3)
        {
            videoPlayer.clip = medite3;
            duration = (float)medite3.length;
        }
        else if (countLose == 4)
        {
            videoPlayer.clip = medite4;
            duration = (float)medite4.length;
        }
        else if (countLose == 5)
        {
            videoPlayer.clip = medite41;;
            duration = (float)medite41.length;
        }
        else
        {
            int randomNumber = UnityEngine.Random.Range(1, 4);
            switch (randomNumber)
            {
                case 1:
                    videoPlayer.clip = medite2;
                    duration = (float)medite2.length;
                    break;
                case 2:
                    videoPlayer.clip = medite3;
                    duration = (float)medite3.length;
                    break;
                case 3:
                    videoPlayer.clip = medite41;
                    duration = (float)medite41.length;
                    break;
            }
        }
        return duration;
    }

    public object CaptureState()
    {
        var saveData = new StressLevelData()
        {
            level = this.level,
            countLose = this.countLose,
            finish = this.finish,
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var savedData = state as StressLevelData;
        this.level = savedData.level;
        this.countLose = savedData.countLose;
        this.finish = savedData.finish;
        OnStressLevelChange?.Invoke();
    }

    public float Level => level;

    //public bool LostMoreTimeRow
    //{
    //    get { return lostMoreTimeRow; }
    //    set { lostMoreTimeRow = value; }
    //}

    public float LoseMultiply => loseMultiply;

    public int CountLose
    {
        get { return countLose; }
        set { countLose = value; }
    }
    public bool Finish
    {
        get { return finish; }
        set { finish = value; }
    }
}

[Serializable]
public class StressLevelData
{
    public float level;
    public int countLose;
    public bool finish = false;
}
