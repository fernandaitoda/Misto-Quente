using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraAccess : MonoBehaviour
{
    private bool camAvailable;
    private WebCamTexture backCam;
    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fit;

    void Start()
    {
        defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0) {
            Debug.Log("Nenhuma camera detectada");
            camAvailable = false;
            return;
        }

        for(int i = 0; i < devices.Length; i++) 
            Debug.Log(devices[i].name.ToString());

        for(int i = 0; i < devices.Length; i++) {
            //if(devices[i].isFrontFacing) {
                
                
            //}
        }

        backCam = new WebCamTexture(devices[2].name, Screen.width, Screen.height);

        if(backCam == null) {
            Debug.Log("Nao foi possivel acessar a camera");
        }

        backCam.Play();
        background.texture = backCam;

        camAvailable = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!camAvailable)
            return;

        float ratio = (float)backCam.width / (float)backCam.height;
        fit.aspectRatio = ratio;

        float scaleY = backCam.videoVerticallyMirrored ? -1f: 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -backCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }
}
