using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    struct Timestamp
    {
        public int Hour;
        public int Minute;
        public double Second;

        public Timestamp(int hours, int minutes, double seconds)
        {
            Hour = hours;
            Minute = minutes;
            Second = seconds;
        }

        public double GetTimeInSeconds()
        {
            return (Hour * 3600d + Minute * 60d + Second);
        }
        
        public Timestamp GetTimeStampFromSeconds(double seconds)
        {
            int hours = (int) (seconds / 3600);
            seconds -= hours * 3600d;
            int minutes = (int) (seconds / 60d);
            seconds -= minutes * 60d;
            return new Timestamp(hours, minutes, seconds);
        }

        public static string GetStringFromSeconds(double seconds)
        {
            int hours = (int) (seconds / 3600);
            seconds -= hours * 3600d;
            int minutes = (int) (seconds / 60d);
            seconds -= minutes * 60d;
            return hours +
                   ":" +
                   minutes +
                   ":" +
                   seconds.ToString("F2");
        }
        public override string ToString()
        {
            return Hour +
                   ":" +
                   Minute +
                   ":" +
                   Second;
        }
    }
    
    [SerializeField] private TextMeshProUGUI checkpointSpeedText;
    [SerializeField] private VideoPlayer videoPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int _speedSetting = 0;
    private readonly float[] _speeds = {1f, 5f, 20f};
    private int _currentCheckpoint = 0;

    private readonly Timestamp[] _checkpoints = new Timestamp[]
    {
        // Hole 1 start
        new Timestamp(0, 0, 16), 
        // Hole 1 end
        new Timestamp(0, 0, 24),
        // Hole 2 start
        new Timestamp(0, 0, 29),
        // Hole 2 End
        new Timestamp(0, 0, 33),
        new Timestamp(0, 0, 44),
        new Timestamp(0, 0, 52),
        new Timestamp(0, 1, 4),
        new Timestamp(0, 1, 19),
        new Timestamp(0, 1, 38),
        new Timestamp(0, 1, 47),
        new Timestamp(0, 2, 0),
        new Timestamp(0, 2, 13),
    };
    
    void Start()
    {
        videoPlayer.time = _checkpoints[0].GetTimeInSeconds();
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
            _currentCheckpoint %= _checkpoints.Length;
            videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            videoPlayer.Play();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
                    ResetCheckpoint();
        }
                
        if (Input.GetKeyDown(KeyCode.B))
        {
                    PreviousCheckpoint();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
                    SkipAnimation();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
                    NextCheckpoint();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
                    SpeedUpShot();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
            else
            {
                videoPlayer.Play();
            }
        }
        // buttonUp 0 is left click
        if (Input.GetMouseButtonUp(0))
        { 
            videoPlayer.Play();
        }

        checkpointSpeedText.text = "" +
                                   "Current Checkpoint: " + _currentCheckpoint + "\n" +
                                   "Current PlaybackSpeed: \n" + _speeds[_speedSetting] +
                                   "Current Time: " + Timestamp.GetStringFromSeconds(videoPlayer.time);
       
    }
    
    public void SpeedUpShot()
    {
        _speedSetting += 1;
        _speedSetting %= 3;
        videoPlayer.playbackSpeed = _speeds[_speedSetting];
    }
    
    public void ResetCheckpoint()
    {
        _currentCheckpoint = 0;
        videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
    }
    public void NextCheckpoint()
    {
        _currentCheckpoint += 1;
        _currentCheckpoint %= _checkpoints.Length;
        videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
    }

    public void SkipAnimation()
    {
        videoPlayer.time = _checkpoints[(_currentCheckpoint + 1) % _checkpoints.Length].GetTimeInSeconds();
    }

    public void PreviousCheckpoint()
    {
        _currentCheckpoint -= 2;
        _currentCheckpoint %= _checkpoints.Length;
        if (_currentCheckpoint < 0)
        {
            _currentCheckpoint += _checkpoints.Length;
        }
        videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
    }
}
