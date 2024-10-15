using UnityEngine;
using UnityEngine.Rendering;

public class ControlsButton : MonoBehaviour
{
    [SerializeField] private GameObject controlsPanel; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controlsPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void TogglePanel()
    {
        controlsPanel.SetActive(!controlsPanel.activeSelf);
    }
}
