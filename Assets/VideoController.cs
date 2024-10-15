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
        // Hole Hit 1 start
        new Timestamp(0, 0, 15.85), 
        // Hit 1 end
        new Timestamp(0, 0, 24),
        // Hit 2 start
        new Timestamp(0, 0, 28.7),
        // Hit 2 end
        new Timestamp(0, 0, 33),
        // Hit 3 start
        new Timestamp(0, 0, 44.6),
        // Hit 3 end
        new Timestamp(0, 0, 50.55),
        // Hole 2 Hit 4 start
        new Timestamp(0, 0, 53.25),
        // Hit 4 end
        new Timestamp(0, 1, 1.85),
        // Hit 5 start
        new Timestamp(0, 1, 4.8),
        // Hit 5 end
        new Timestamp(0, 1, 16.5),
        // Hit 6 start
        new Timestamp(0, 1, 20.95),
        // Hit 6 end
        new Timestamp(0, 1, 25),
        // Hit 7 start
        new Timestamp(0, 2, 16.5),
        // Hit 7 end
        new Timestamp(0, 2, 20.5),
        // Hit 8 start
        new Timestamp(0, 2, 16.5),
        // Hit 8 end
        new Timestamp(0, 2, 20.5),
        new Timestamp(0, 2, 16.5),
        new Timestamp(0, 3, 20.5),
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
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Space))
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
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
                    videoPlayer.time -= 1d;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
                    videoPlayer.time += 1d;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            videoPlayer.SetDirectAudioMute(0, !videoPlayer.GetDirectAudioMute(0));
        }
        // buttonUp 0 is left click
        if (Input.GetMouseButtonUp(0)) 
        { 
            videoPlayer.Play();
        }

        checkpointSpeedText.text = "" +
                                   "Current Checkpoint: " + _currentCheckpoint + "\n" +
                                   "Current PlaybackSpeed: " + _speeds[_speedSetting] + "\n" +
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
