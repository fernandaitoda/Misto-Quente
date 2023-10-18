using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public float rotationAngle = 90.0f; // ângulo de rotação em graus

    public void Rotate(bool clockwise)
    {
        // Encontre o objeto de imagem que você deseja rotacionar
        Transform imageTransform = GetComponent<Transform>();

        // Determine a direção da rotação com base no botão clicado
        float angle = clockwise ? rotationAngle : -rotationAngle;

        // Rotacione a imagem na direção apropriada
        imageTransform.Rotate(Vector3.forward, angle);    
    }
}
