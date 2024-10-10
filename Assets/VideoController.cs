using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int speedSetting = 0;
    void Start()
    {
        videoPlayer.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            videoPlayer.Play();
        }
        // buttonup 0 is left click
        if (Input.GetMouseButtonUp(0))
        { 
            videoPlayer.Play();
        }
        
    }
    
    private void SpeedUpShot()
    {
        speedSetting += 1;
        speedSetting %= 3;
        if (speedSetting == 0)
            videoPlayer.playbackSpeed = 1f;
        else if (speedSetting == 1)
            videoPlayer.playbackSpeed = 5f;
        else if (speedSetting == 2)
            videoPlayer.playbackSpeed = 20f;
    }
}
