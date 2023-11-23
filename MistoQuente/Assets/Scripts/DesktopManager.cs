using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class DualWebcamManager : MonoBehaviour
{
    [SerializeField] RawImage rawImage1;
    [SerializeField] RawImage rawImage2;
    [SerializeField] TMP_Dropdown webcamDropdown1;
    [SerializeField] TMP_Dropdown webcamDropdown2;
    [SerializeField] TMP_Dropdown dropdownResolution1;
    [SerializeField] TMP_Dropdown dropdownResolution2;
    [SerializeField] GameObject cameraInUsePopupPrefab;
    [SerializeField] Button refresh; 

    private WebCamTexture webcamTexture1 = null;
    private WebCamTexture webcamTexture2 = null;
    private List<WebCamDevice> devices;

    void Start()
    {
        refresh.onClick.AddListener(RefreshCams);
        RefreshCams();
        
    }

    void RefreshCams() {
        WebCamDevice[] devicesArray = WebCamTexture.devices;
        devices = new List <WebCamDevice>(devicesArray);

        if (webcamTexture1 != null) 
        {
            webcamTexture1.Stop();
        }
        if (webcamTexture2 != null)
        {
            webcamTexture2.Stop();
        }

        if (devices.Count >= 2)
        {
            List<TMP_Dropdown.OptionData> options = getOptionsDropdown();
            webcamDropdown1.ClearOptions();
            webcamDropdown1.AddOptions(options);
            webcamDropdown2.ClearOptions();
            webcamDropdown2.AddOptions(options);


            webcamDropdown1.value = options.Count - 1;
            webcamDropdown2.value = options.Count - 1;
            dropdownResolution1.value = 0;
            dropdownResolution2.value = 0;
            rawImage1.color = Color.blue;
            rawImage2.color = Color.red;

        }
        else
        {
            Debug.LogError("Not enough webcams found. You need at least 2 webcams.");
        }
    }

    void ResizeResolution (ref WebCamTexture webcamTexture, ref RawImage rawImage, int resolution)
    // SD 480p (640 x 480) - (0)
    // HD 720p (1280 x 720) - (1)
    // Full HD 1080p (1920 x 1080) - (2)
    {
        int newWidth = 640;
        int newHeight = 480;

        Debug.Log("old resolution> wxh" + webcamTexture.requestedWidth + " " + webcamTexture.requestedHeight + " " + webcamTexture.requestedFPS);
        StopIfPlaying(ref webcamTexture);

        switch (resolution)
        {
            case 1:
                Debug.Log("720p");
                newWidth = 1280;
                newHeight = 720;
                break;

            case 2:
                Debug.Log("1080p");
                newWidth = 1920;
                newHeight = 1080;
                break;
            default:
                Debug.Log("480p");
                break;

        }

        webcamTexture = new WebCamTexture(webcamTexture.deviceName, newWidth, newHeight, 30);
        rawImage.texture = webcamTexture;
        webcamTexture.Play();

        int width = webcamTexture.requestedWidth;
        int height = webcamTexture.requestedHeight;
        float fps = webcamTexture.requestedFPS;

        Debug.Log("New Width: " + width + ", New Height: " + height + ", New fps: " + fps );
    }

    bool IsWebcamDeviceWorking(WebCamDevice device)
    {
        try
        {
            WebCamTexture tempTexture = new WebCamTexture(device.name);

            tempTexture.Play();
            
            int width = tempTexture.requestedWidth;
            int height = tempTexture.requestedHeight;

            Debug.Log("Width: " + width + ", Height: " + height);
            bool isPlaying = tempTexture.isPlaying;

            tempTexture.Stop();

            if (!isPlaying)
            {
                Debug.Log("Camera not working: " + device.name);
            }

            return isPlaying;
        }
        catch (Exception e)
        {
            Debug.LogError("Error checking webcam device: " + device.name + "\n" + e.Message);
            return false;
        }
    }


    List<TMP_Dropdown.OptionData> getOptionsDropdown()
    {

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        List<WebCamDevice> devicesToRemove = new List<WebCamDevice>();
    
        foreach (var device in devices)
        {
            if (IsWebcamDeviceWorking(device))
            {
                string limitedDeviceName = device.name.Substring(0, Mathf.Min(25, device.name.Length));
                options.Add(new TMP_Dropdown.OptionData(limitedDeviceName));
            }
            else 
            {
                devicesToRemove.Add(device);
            }
        }
        foreach (var deviceToRemove in devicesToRemove)
        {
            RemoveDeviceByName(deviceToRemove.name);
        }

        options.Add(new TMP_Dropdown.OptionData("No Camera"));
        return options;
    }

    void RemoveDeviceByName(string deviceName)
    {
        // Find the index of the device with the specified name
        int indexToRemove = devices.FindIndex(device => device.name == deviceName);

        // Check if the device with the specified name was found
        if (indexToRemove != -1)
        {
            // Remove the device from the list
            devices.RemoveAt(indexToRemove);
        }
        else
        {
            Debug.LogWarning("Device with name " + deviceName + " not found.");
        }
    }

    void SetSelectedWebcam(ref WebCamTexture webcamTexture, ref RawImage rawImage, WebCamDevice device)
    {
        if (IsWebcamInUse(device))
        {
            // Show a popup indicating that the camera is already in use
            ShowCameraInUsePopup(device.name);
            return;
        }

        StopIfPlaying(ref webcamTexture);

        // set 480p default
        webcamTexture = new WebCamTexture(device.name, 640, 480, 30);
        rawImage.color = Color.white;
        rawImage.texture = webcamTexture;

        if (webcamTexture == webcamTexture1)
        {
            dropdownResolution1.value = 0;
        }
        else 
        {
           dropdownResolution2.value = 0;
        }

        int width = webcamTexture.requestedWidth;
        int height = webcamTexture.requestedHeight;

        Debug.Log("Selecting " + device.name + " Width: " + width + ", Height: " + height);


    }


    bool IsWebcamInUse(WebCamDevice device)
    {
        return (webcamTexture1 != null && webcamTexture1.isPlaying && webcamTexture1.deviceName == device.name) ||
               (webcamTexture2 != null && webcamTexture2.isPlaying && webcamTexture2.deviceName == device.name);
    }

    void ShowCameraInUsePopup(string cameraName)
    {
        GameObject popupObject = Instantiate(cameraInUsePopupPrefab, transform);

        TextMeshProUGUI popupText = popupObject.GetComponent<TextMeshProUGUI>();
        if (popupText != null)
        {
            // Set the popup text
            popupText.text = "This camera is already in use!";

        }

        // Destroy the popup after a certain time (adjust as needed)
        Destroy(popupObject, 3f);
    }

    void StopIfPlaying (ref WebCamTexture webcamTexture)
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
        }
    }
 
    /////////////////// CALLBACK FUNCTIONS ///////////////////
    
    // Callback for the camera selection 1 value change
    public void OnDropdown1ValueChanged()
    {
        int selectedIndex = webcamDropdown1.value;
        
        if (webcamTexture1 != null)
        {
            webcamTexture1.Stop();
        }

        if (selectedIndex == devices.Count)
        {
            rawImage1.texture = null;   
        }
        else if (selectedIndex < devices.Count)
        {
            WebCamDevice selectedDevice = devices[selectedIndex];
            SetSelectedWebcam(ref webcamTexture1, ref rawImage1, selectedDevice);

            webcamTexture1.Play();

        }
        else
        {
            Debug.Log ("Error: Index out of range: " + selectedIndex);
        }
    }

    // Callback for the camera selection 2 value change
     public void OnDropdown2ValueChanged()
    {
        int selectedIndex = webcamDropdown2.value;

        StopIfPlaying (ref webcamTexture2);

        if (selectedIndex == devices.Count)
        {
            rawImage2.texture = null;   
        }
        else if (selectedIndex < devices.Count)
        {
            WebCamDevice selectedDevice = devices[selectedIndex];
            SetSelectedWebcam(ref webcamTexture2, ref rawImage2, selectedDevice);
            webcamTexture2.Play();
        }
        else
        {
            Debug.Log ("Error: Index out of range: " + selectedIndex);
        }
    }

    // Callback for the camera resolution 1 value change
    public void OnDropdownResolution1ValueChanged()
    {
        StopIfPlaying(ref webcamTexture1);

        int selectedIndex = dropdownResolution1.value;
        
        ResizeResolution(ref webcamTexture1, ref rawImage1, selectedIndex);
    }

    // Callback for the camera resolution 2 value change
    public void OnDropdownResolution2ValueChanged()
    {
        StopIfPlaying(ref webcamTexture2);

        int selectedIndex = dropdownResolution2.value;
        
        ResizeResolution(ref webcamTexture2, ref rawImage2, selectedIndex);
    }

}
