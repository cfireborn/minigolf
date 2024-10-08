using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [FormerlySerializedAs("MainMenuUI")] public GameObject mainMenuUi;

    [FormerlySerializedAs("SettingsMenuUI")] public GameObject settingsMenuUi;
    
    [SerializeField] TMP_InputField[] textInputs;

    [SerializeField] private TMP_InputField timestampInput;
    
    
    public AudioMixer masterMixer;
    
    // Start is called before the first frame update
    void Start()
    {
    }
    
    public void PlayGame()
    {
        SaveLoader.LoadGame();
        SceneManager.LoadScene("TutorialScene");
    }
    public void QuitGame()
    {
        Debug.Log("Quitting ...");
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendData();
        }
        timestampInput.text = 
            timestampInput.text = DateTime.Now.ToUniversalTime()
                .ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
    }

    public void OpenSettings()
    {
        settingsMenuUi.SetActive(true);
        mainMenuUi.SetActive(false);
    }

    public void SendData()
    {
        print("Sending data to the server");
        StartCoroutine(PostData());
    }
    public void SendDataTest()
    {
        print("Sending data to the server");
        StartCoroutine(TestPost());
    }
    
    IEnumerator TestPost()
    {
        var request = new UnityWebRequest("localhost:8080/api/events/create/", "POST");
        string jsonDataToSend = "{\n    \"event_type\": \"game_end\",\n    \"event_data\": {\n        \"event_type\": \"game_end\",\n        \"metadata\": {\n            \"timestamp\": \"2024-08-06T14:00:00Z\",\n            \"game_details\": {\n                \"final_scores\": [\n                    {\n                        \"player_id\": 1,\n                        \"score\": 15,\n                        \"leaderboard_position\": 1\n                    },\n                    {\n                        \"player_id\": 2,\n                        \"score\": 4,\n                        \"leaderboard_position\": 2\n                    }\n                ],\n                \"winner\": {\n                    \"player_id\": 1,\n                    \"score\": 5,\n                    \"leaderboard_position\": 1\n                }\n            }\n        },\n        \"session_id\": \"2\"\n    },\n    \"created\": \"2024-10-07T23:30:39.314357Z\",\n    \"processed\": true\n}"; 
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonDataToSend);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        print(request.GetRequestHeader("Content-Type"));
        yield return request.Send();

        Debug.Log("Status Code: " + request.responseCode);
        Debug.Log(request.error);
    }
    
    private IEnumerator PostData()
    {
        //"event": "shot_executed",
        //   "matchId": "12345",
        //   "playerId": "p1",
        //   "playerName": "John Doe",
        //   "holeNumber": 1,
        //   "timestamp": "2024-08-06T10:15:00Z",  // Time when the shot was executed
        //   "shot": {
        //     "shotNumber": 1,
        //     "clubUsed": "Driver",
        //     "yards": 250,  // Actual number of yards made with the shot
        //     "strokes": 1,  // Number of strokes made, so far
        //     "holeScore": -3,  // Score of the hole, so far (strokes - par)
        //     "strokesGained": 0.2,  // Strokes gained in this shot
        //     "holeCompleted": false  // Indicates whether the hole has been completed, or not (true if the hole has been completed)
        //   },
        //    "currentHoleStats": {
        //     "par": 4,
        //     "totalYards": 350,  // Hole total yards
        //     "baseline": 3.8,
        //   }
        // }
        // 
        WWWForm form = new WWWForm();
        Dictionary<string, JObject> json_eventObject = new Dictionary<string,JObject>();
        
        foreach (TMP_InputField inputField in textInputs)
        {
            // TODO: Future refactor to get rid of this string manipulation to achieve field names
            // This is currently dependent on the object names in Unity which is brittle/fragile/prone to breaking
            string fieldName = inputField.transform.parent.name;
            fieldName = Char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);
            fieldName = fieldName.Replace("Text", "");
            form.AddField(fieldName, inputField.text);
        }
        
        
        
        Debug.Log("datetime = " + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture));
        
        UnityWebRequest www = UnityWebRequest.Post("localhost:8080/api/events/create/", form);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string response = www.downloadHandler.text;
            Debug.Log(response);
        }
        www.Dispose();
        
        // var request = new UnityWebRequest(url, "POST");
        // byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        // request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        // request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        // request.SetRequestHeader("Content-Type", "application/json");
        // yield return request.SendWebRequest();
        // Debug.Log("Status Code: " + request.responseCode);
        // request.Dispose();
    }
    public void ReturnToMain()
    {
        settingsMenuUi.SetActive(false);
        mainMenuUi.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        Debug.Log(volume);
        masterMixer.SetFloat("masterVolume", volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        Debug.Log(volume);
        masterMixer.SetFloat("musicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        Debug.Log(volume);
        masterMixer.SetFloat("sfxVolume", volume);
    }
}