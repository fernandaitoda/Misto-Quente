using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationScript : MonoBehaviour
{
    void Update()
    {
        // Verifique se o botão está sendo pressionado (por exemplo, o botão "RotateButton").
        if (Input.GetButtonDown("ButtonRotateRight"))
        {
            // Rotacione o objeto no sentido horário.
            transform.Rotate(0, 0, 90); // Ajuste o ângulo conforme necessário.
        }
    }

}
