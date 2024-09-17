using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SettingsMenu : MonoBehaviour
{
    [FormerlySerializedAs("MainMenuUI")] public GameObject mainMenuUi;

    [FormerlySerializedAs("SettingsMenuUI")] public GameObject settingsMenuUi;

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
            ReturnToMain();
        }
    }

    public void OpenSettings()
    {
        settingsMenuUi.SetActive(true);
        mainMenuUi.SetActive(false);
    }

    public bool sendData()
    {
        // "event": "shot_executed",
        // "matchId": "12345",
        // "playerId": "p1",
        // "playerName": "John Doe",
        // "holeNumber": 1,
        // "timestamp": "2024-08-06T10:15:00Z",  // Time when the shot was executed
        // "shot": {
        //     "shotNumber": 1,
        //     "clubUsed": "Driver",
        //     "yards": 250,  // Actual number of yards made with the shot
        //     "strokes": 1,  // Number of strokes made, so far
        //     "holeScore": -3,  // Score of the hole, so far (strokes - par)
        //     "strokesGained": 0.2,  // Strokes gained in this shot
        //     "holeCompleted": false  // Indicates whether the hole has been completed, or not (true if the hole has been completed)
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