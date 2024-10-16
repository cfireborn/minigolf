using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Toggle = UnityEngine.UI.Toggle;

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
    
    [SerializeField] private Toggle sendLiveDataToggle;
    [SerializeField] private TextMeshProUGUI checkpointSpeedText;
    [SerializeField] private VideoPlayer videoPlayer;
    private bool gameStarted = false;
    
    [SerializeField] TMP_Text DataToSend;
    [SerializeField] TMP_Text DataReturned;
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
        new Timestamp(0, 0, 48.3),
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
        new Timestamp(0, 1, 28.4),
        // Hit 7 start
        new Timestamp(0, 1, 38.1),
        // Hit 7 end
        new Timestamp(0, 1, 42),
        
        // checkpoint 14
        // Hole 3 Hit 8 start
        new Timestamp(0, 1, 47.5),
        // Hit 8 end
        new Timestamp(0, 1, 57.3),
        // Hit 9 start
        new Timestamp(0, 2, 7.3),
        // Hit 9 end
        new Timestamp(0, 2, 12.3),
        // Video End
        new Timestamp(0, 2, 16.5)
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
        if (_currentCheckpoint < _checkpoints.Length-1 && videoPlayer.isPlaying && videoPlayer.time > _checkpoints[_currentCheckpoint+1].GetTimeInSeconds())
        {
            IncrementCheckpointAndPause();
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
        // if (Input.GetMouseButtonUp(0)) 
        // { 
            // videoPlayer.Play();
        // }
        
        // Prep send data to server
        DataToSend.text = sendLiveDataToggle.isOn ? constructJson() : "Live Data sending is off";
       
        
        if (sendLiveDataToggle.isOn && videoPlayer.isPlaying && gameStarted == false && _currentCheckpoint == 0)
        {
            gameStarted = true;
            DataToSend.text = constructJson();
            SendData();
        }
        
        checkpointSpeedText.text = "Current Checkpoint: " + _currentCheckpoint + "\n" +
                                   "At Checkpoint: " + CurrentlyAtCheckpoint() + "\n" +
                                   "Current PlaybackSpeed: " + _speeds[_speedSetting] + "\n" +
                                   "Current Time: " + Timestamp.GetStringFromSeconds(videoPlayer.time);
       
    }

    private string constructJson()
    {
        JObject json = new JObject();
        string currentTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture); 
        
        if (_currentCheckpoint == 0)
        {
            json["event_type"] = "game_start";
            json["session_id"] = "2";
            json["metadata"] = new JObject
            {
                ["match_id"] = "12345",
                ["timestamp"] = currentTime,
                ["game_details"] = new JObject
                {
                    ["course_details"] = new JObject
                    {
                        ["course_id"] = "001",
                        ["course_name"] = "Sunnydale Golf Course",
                        ["num_holes"] = 18,
                        ["holes"] = new JArray
                        {
                            new JObject { ["hole_number"] = 1, ["par"] = 4, ["yards"] = 350, ["baseline"] = 3.8 },
                            new JObject { ["hole_number"] = 2, ["par"] = 3, ["yards"] = 180, ["baseline"] = 3.8 },
                            new JObject { ["hole_number"] = 3, ["par"] = 4, ["yards"] = 430, ["baseline"] = 3.8 }
                        }
                    }
                },
                ["players"] = new JArray
                {
                    new JObject { ["player_id"] = "p1" },
                    new JObject { ["player_id"] = "p2" }
                }
            };
        }
        // result += "{\n";
        // result += "  \"event_type\": \"game_end\",\n";
        // result += "  \"session_id\": \"2\",\n";    
        // "session_id": "2",
        //     "metadata": {
        //         "timestamp": "2024-08-06T14:00:00Z",
        //         "game_details": {
        //             "final_scores": [
        //             {
        //                 "player_id": "1",
        //                 "score": 5,
        //                 "leaderboard_position": 1
        //             },
        //             {
        //                 "player_id": "2",
        //                 "score": 4,
        //                 "leaderboard_position": 2
        //             }
        //             ],
        //             "winner": {
        //                 "player_id": "3",
        //                 "score": 5,
        //                 "leaderboard_position": 1
        //             }
        //         },
        //     }
        // }
        return json.ToString();
    }
    
    public void SendData()
    {
        print("Sending data to the server");
        StartCoroutine(PostData());
    }
    
    IEnumerator PostData()
    {
        var request = new UnityWebRequest("localhost:8080/api/events/create/", "POST");
        string jsonDataToSend = DataToSend.text;
        JObject json = JObject.Parse(jsonDataToSend);
        string jtext = json.ToString();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jtext);
        
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        // print(request.GetRequestHeader("Content-Type"));
        DataReturned.text = "Sending data to the server";
        yield return request.Send();
        string output = "Data sent: " + jtext + "\n"; 
        output += "Status Code: " + request.responseCode;
        if (request.responseCode == 201)
        {
            output += "\n" + "Created. Success! Request was fulfilled and a new resource was created";
        }
        if (request.error != null)
        {
            output += "\nErrors: " + request.error;
        }
        else
        {
            output += "\nErrors: None";
        }
        output += "\nDownload Handler: " + request.downloadHandler.text;
        output += "\nUpload Handler: " + request.uploadHandler.data;
        
        Debug.Log(output);
        
        DataReturned.text = output;
    }
    
    private void IncrementCheckpointAndPause()
    {
        videoPlayer.Pause();
        _currentCheckpoint += 1;
        _currentCheckpoint %= _checkpoints.Length;
        if (sendLiveDataToggle.isOn)
        {
            // Send data to server
        }
        if (_currentCheckpoint < _checkpoints.Length)
        {
            videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
        }
    }

    bool CurrentlyAtCheckpoint()
    {
        return Math.Abs(videoPlayer.time - _checkpoints[_currentCheckpoint].GetTimeInSeconds()) < 0.1; 
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
