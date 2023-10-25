using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Serenegiant.UVC 
{

    public class RotateCamera : MonoBehaviour
    {
        public float rotationAngle = 90.0f; // ângulo de rotação em graus

        // public RawImage rawImage;

        public void Rotate(bool clockwise)
        {
            // Verifique se temos uma imagem crua (RawImage) atribuída
            // if (rawImage != null)
            // {
            // Encontre o objeto de imagem que você deseja rotacionar (se não funcuonar usando rawimage, tirar a linha 9e 18  e descomentar o código abaixo)
            Transform imageTransform = GetComponent<Transform>();
                // Transform imageTransform = rawImage.transform;
            // Determine a direção da rotação com base no botão clicado
            float angle = clockwise ? rotationAngle : -rotationAngle;

            // Rotacione a imagem na direção apropriada
            imageTransform.Rotate(Vector3.forward, angle);    
            // }         
            // else
            // {
            //     Debug.LogWarning("Nenhuma RawImage atribuída para rotação.");
            // }
        }
    }

}