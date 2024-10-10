using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    struct Timestamp
    {
        public int Hour;
        public int Minute;
        public int Second;

        public Timestamp(int hours, int minutes, int seconds)
        {
            Hour = hours;
            Minute = minutes;
            Second = seconds;
        }

        public double GetTimeInSeconds()
        {
            return (double) (Hour * 3600d + Minute * 60d + Second);
        }
            
    }
    
    [SerializeField] private VideoPlayer videoPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int _speedSetting = 0;
    private int _currentCheckpoint = 0;

    private readonly Timestamp[] _checkpoints = new Timestamp[]
    {
        new Timestamp(0, 0, 11),
        new Timestamp(0, 0, 24),
        new Timestamp(0, 0, 27),
        new Timestamp(0, 0, 20)
    };
    
    void Start()
    {
        videoPlayer.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if we have advanced by any checkpoints;
        if (videoPlayer.time > _checkpoints[(_currentCheckpoint+1)%_checkpoints.Length].GetTimeInSeconds())
        {
            videoPlayer.Pause();
            _currentCheckpoint += 1;
            videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
        }
            
        if (Input.GetKeyDown(KeyCode.Space))
        {
            videoPlayer.Play();
        }
        // buttonUp 0 is left click
        if (Input.GetMouseButtonUp(0))
        { 
            videoPlayer.Play();
        }
        
    }
    
    public void SpeedUpShot()
    {
        _speedSetting += 1;
        _speedSetting %= 3;
        if (_speedSetting == 0)
            videoPlayer.playbackSpeed = 1f;
        else if (_speedSetting == 1)
            videoPlayer.playbackSpeed = 5f;
        else if (_speedSetting == 2)
            videoPlayer.playbackSpeed = 20f;
    }

    public void NextCheckpoint()
    {
        _currentCheckpoint += 1;
        _currentCheckpoint %= _checkpoints.Length;
        videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
    }
}
