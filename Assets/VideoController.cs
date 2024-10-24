using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.DedicatedServer;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
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
    [SerializeField] private RawImage player2Overlay;
    private bool gameStarted = false;
    [FormerlySerializedAs("DataToSend")] [SerializeField] TMP_Text dataToSend;
    [SerializeField] TMP_Text DataReturned;
    [SerializeField] private TMP_InputField UrlInputField;
    private float _timeSinceDataReturned = float.PositiveInfinity;
    private bool _returnedError;
    Color invisibleMagenta = new Color(1.0f, 0.0f, 1.0f, 0.0f);
    Color invisibleCyan = new Color(0.0f, 1.0f, 1.0f, 0.0f);
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int _speedSetting = 0;
    private readonly float[] _speeds = {1f, 5f, 20f};
    private int _currentCheckpoint = 0;
    
    private int _currentPlayer = 1;
    private Dictionary<int, string> _playerNames = new Dictionary<int, string>()
    {
        {1, "Juli"}, 
        {2, "Tiger"}
    };

    
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
        //check for max checkpoint, if so check if video is playing, if so check if video
        if (_currentCheckpoint < _checkpoints.Length-1 && videoPlayer.isPlaying && videoPlayer.time > _checkpoints[_currentCheckpoint+1].GetTimeInSeconds())
        {
            IncrementCheckpointAndPause();
            if (sendLiveDataToggle.isOn)
            {
                // Send shot data to server
                dataToSend.text = ConstructShotExecutedJson();
                SendData();
                if (_currentCheckpoint is 5 or 13 or 17)
                {
                    //  Send hole completed data to server
                    dataToSend.text = ConstructHoleCompletedJson();
                    SendData();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchPlayer(1);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchPlayer(2);
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
        dataToSend.text = sendLiveDataToggle.isOn ? ConstructShotExecutedJson() : "Live Data sending is off";
        
        //Send startgame data
        if (sendLiveDataToggle.isOn && videoPlayer.isPlaying && gameStarted == false && _currentCheckpoint == 0)
        {
            gameStarted = true;
            dataToSend.text = ConstructShotExecutedJson();
            SendData();
        }
        _timeSinceDataReturned += Time.deltaTime;
        if (_timeSinceDataReturned < 3)
        {
            if (_returnedError)
            {
                DataReturned.faceColor = Color.white - (invisibleCyan * (1 - _timeSinceDataReturned / 3));
            }
            else
            {
                DataReturned.faceColor = Color.white - (invisibleMagenta * (1 - _timeSinceDataReturned / 3));
            }
        }

        string args = "";
        string[] arguments = Environment.GetCommandLineArgs();
        int argcount = 0; 
        foreach (string arg in arguments)
        {
            args += argcount + "." + arg + " ";
            argcount++;
        }

        checkpointSpeedText.text = "Current Checkpoint: " + _currentCheckpoint + "\n" +
                                   "At Checkpoint: " + CurrentlyAtCheckpoint() + "\n" +
                                   "Current PlaybackSpeed: " + _speeds[_speedSetting] + "\n" +
                                   "Current Time: " + Timestamp.GetStringFromSeconds(videoPlayer.time)
                                   + "\n" + "Current Player: " + _currentPlayer
                                   + "\n" + "Command Line Args: " + args;

    }
    public void SwitchPlayer()
    {
        if (_currentPlayer == 1)
        {
            SwitchPlayer(2);
        }
        else
        {
            SwitchPlayer(1);
        }
    }
    private void SwitchPlayer(int player)
    {
        _currentPlayer = player;
        if (player == 2)
        {
            player2Overlay.gameObject.SetActive(true);
        }
        else
        {
            player2Overlay.gameObject.SetActive(false);
        }
            
            
    }

    private string ConstructHoleCompletedJson()
    {
        JObject json = new JObject();
        json["event_type"] = "in_game_event";
        json["session_id"] = "2";
        json["metadata"] = new JObject
        {
            ["sub_type"] = "hole_completed",
            ["player_id"] = "p" + _currentPlayer,
            ["hole_number"] = GetHoleNumber(),
            ["timestamp"] = "2024-08-06T10:15:00Z",
            ["details"] = new JObject
            {
                ["leaderboard"] = new JArray
                {
                    new JObject
                    {
                        ["player_id"] = "p1",
                        ["player_name"] = _playerNames[1],
                        ["last_played_hole"] = 4,
                        ["overall_score"] = 1,
                        ["cumulative_par"] = 4,
                        ["leaderboard_position"] = 1
                    },
                    new JObject
                    {
                        ["player_id"] = "p2",
                        ["player_name"] = _playerNames[2],
                        ["last_played_hole"] = 3,
                        ["overall_score"] = 0,
                        ["cumulative_par"] = 4,
                        ["leaderboard_position"] = 2
                    }
                },
                ["current_hole_stats"] = new JObject
                {
                    ["par"] = 4,
                    ["total_yards"] = 350,
                    ["strokes"] = 1,
                    ["hole_score"] = -3,
                    ["info"] = "eagle"
                },
                ["overall_stats"] = new JObject
                {
                    ["cumulative_par"] = 4,
                    ["overall_score"] = 0,
                    ["cumulative_strokes_gained"] = 0.5,
                    ["cumulative_strokes_lost"] = 0.5,
                    ["cumulative_fairways_hit"] = 2,
                    ["fairways_hit_percent"] = 100,
                    ["driving_accuracy"] = 90,
                    ["gir_percent"] = 50,
                    ["average_putts_per_green"] = 2,
                    ["average_putts_per_round"] = 2,
                    ["one_put_percent"] = 15
                }
            }
        };
        return json.ToString();
    }

    private int GetHoleNumber()
    {
        if (_currentCheckpoint is >= 0 and <= 5)
        {
            return 1;
        }
        if (_currentCheckpoint is >= 6 and <= 13)
        {
            return 2;
        }
        return 3;
    }

    private string ConstructShotExecutedJson()
    {
        JObject json = new JObject();
        string currentTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
        int holeNumber = GetHoleNumber();
        
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
                        ["num_holes"] = 3,
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
        else if (_currentCheckpoint % 2 == 1)
        {
            json["event_type"] = "in_game_event";
            json["session_id"] = "2";
            json["metadata"] = new JObject
            {
                ["sub_type"] = "shot_executed",
                ["player_id"] = "p" + _currentPlayer,
                ["hole_number"] = holeNumber,
                ["timestamp"] = currentTime,
                ["details"] = new JObject
                {
                    ["shot_number"] = _currentCheckpoint/2 + 1,
                    ["info"] = "bad_shot",
                    ["club_used"] = "Driver",
                    ["yards"] = 250,
                    ["strokes"] = 1,
                    ["hole_score"] = -3,
                    ["strokes_gained"] = 0.2,
                    ["hole_completed"] = false
                }
            };
        }
        return json.ToString();
    }
    
    public void SendData()
    {
        print("Sending data to the server");
        StartCoroutine(PostData());
    }
    
    IEnumerator PostData()
    {
        var request = new UnityWebRequest(UrlInputField.text, "POST");
        string jsonDataToSend = dataToSend.text;
        JObject json = JObject.Parse(jsonDataToSend);
        string jtext = json.ToString();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jtext);
        
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        // print(request.GetRequestHeader("Content-Type"));
        DataReturned.text = "Sending data to the server";
        _timeSinceDataReturned = 0;
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
            _returnedError = true;
        }
        else
        {
            output += "\nErrors: None";
            _returnedError = false;
        }
        output += "\nDownload Handler: " + request.downloadHandler.text;
        output += "\nUpload Handler: " + request.uploadHandler.data;
        
        Debug.Log(output);
        
        DataReturned.text = output;
        _timeSinceDataReturned = 0;
    }
    
    private void IncrementCheckpointAndPause()
    {
        videoPlayer.Pause();
        _currentCheckpoint += 1;
        _currentCheckpoint %= _checkpoints.Length;
        videoPlayer.time = _checkpoints[_currentCheckpoint].GetTimeInSeconds();
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
