using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Serenegiant.UVC
{

    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Button buttonRotationLeft;
        [SerializeField] Button buttonRotationRight;
        [SerializeField] Button mirrorViewsButton;

        [SerializeField] RectTransform leftView;
        [SerializeField] RectTransform rightView;
        [SerializeField] Slider viewDistance;
        [SerializeField] float offSetSlider;
        [SerializeField] float maxValueSlider;
        

        float rotationAngleLeftView = 0f;
        float rotationAngleRightView = 0f;
        private void OnEnable()
        {
            buttonRotationLeft.onClick.AddListener(RotateLeft);
            buttonRotationRight.onClick.AddListener(RotateRight);
            mirrorViewsButton.onClick.AddListener(MirrorView);
            viewDistance.onValueChanged.AddListener(DistanceView);
        }
        void RotateLeft()
        {
            if (rotationAngleLeftView < 270f)
            {
                rotationAngleLeftView += 90f;
            }
            else
                rotationAngleLeftView = 0f;

            leftView.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngleLeftView);
        }

        void RotateRight()
        {
            if (rotationAngleRightView < 270f)
            {
                rotationAngleRightView += 90f;
            }
            else
                rotationAngleRightView = 0f;

            rightView.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngleRightView);
        }

        void DistanceView(float distance)
        {
            distance = viewDistance.value;

            leftView.anchoredPosition = new Vector2(distance, 0);
            rightView.anchoredPosition = new Vector2(-distance, 0);
        }

        void MirrorView()
        {
            Vector3 leftViewAux = leftView.transform.position;
            Vector3 rightViewAux = rightView.transform.position;

            leftView.transform.position = rightViewAux;
            rightView.transform.position = leftViewAux;
        }
    }
}