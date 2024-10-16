using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class JSONTestingMenu : MonoBehaviour
{
    [FormerlySerializedAs("MainMenuUI")] public GameObject mainMenuUi;

    [FormerlySerializedAs("SettingsMenuUI")] public GameObject settingsMenuUi;
    
    [SerializeField] TMP_InputField textInput;
    [SerializeField] TMP_InputField textOutput;

    // Start is called before the first frame update
    void Start()
    {
    }
    
    public void PlayGame()
    {
    }
    public void QuitGame()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendDataTest();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMain();
        }
        // if (timestampInput)
        // {
        //     timestampInput.text = 
        //         timestampInput.text = DateTime.Now.ToUniversalTime()
        //             .ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture); 
        // }

    }

    public void OpenSettings()
    {
        settingsMenuUi.SetActive(true);
        mainMenuUi.SetActive(false);
    }

    public void SendDataTest()
    {
        print("Sending data to the server");
        StartCoroutine(TestPost());
    }
    
    IEnumerator TestPost()
    {
        var request = new UnityWebRequest("localhost:8080/api/events/create/", "POST");
        string jsonDataToSend = textInput.text;
        JObject json = JObject.Parse(jsonDataToSend);
        string jtext = json.ToString();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jtext);
        
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        // print(request.GetRequestHeader("Content-Type"));
        textOutput.text = "Sending data to the server";
        yield return request.Send();
        string output = "Status Code: " + request.responseCode;
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
        
        textOutput.text = output;
    }
    
    public void ReturnToMain()
    {
        settingsMenuUi.SetActive(false);
        mainMenuUi.SetActive(true);
    }
}