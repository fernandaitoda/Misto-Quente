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
        [SerializeField] Dropdown resolutionDropdown;

        [SerializeField] RectTransform redView;
        [SerializeField] RectTransform blueView;
        [SerializeField] Slider viewDistance;
        [SerializeField] float offSetSlider;
        [SerializeField] float maxValueSlider;
        
        RectTransform rightView;
        RectTransform leftView;

        float rotationAngleLeftView = 0f;
        float rotationAngleRightView = 0f;
        private void OnEnable()
        {
            rightView = blueView;
            leftView = redView;

            buttonRotationLeft.onClick.AddListener(RotateLeft);
            buttonRotationRight.onClick.AddListener(RotateRight);
            mirrorViewsButton.onClick.AddListener(MirrorView);
            viewDistance.onValueChanged.AddListener(DistanceView);
            resolutionDropdown.onValueChanged.AddListener(ResolutionChange);
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

            // max distance value = 50 and min = -70
            if (leftView.anchoredPosition.x <= 50 && leftView.anchoredPosition.x >= -70) 
            {
                leftView.anchoredPosition = new Vector2(+ distance, 0);
                rightView.anchoredPosition = new Vector2(- distance, 0);
            }
            else 
            {
                leftView.anchoredPosition = new Vector2( - 960 + distance, 0);
                rightView.anchoredPosition = new Vector2( 960 - distance, 0);
            }
        }

        void MirrorView()
        {
            // Flip the positions of the left and right views
            Vector3 oldLeftPosition = leftView.transform.position;
            Vector3 oldRightPosition = rightView.transform.position;

            leftView.transform.position = oldRightPosition;
            rightView.transform.position = oldLeftPosition;

            Debug.Log("after change");

            // Swap the RectTransform references
            if (rightView == blueView)
            {
            rightView = redView;
            leftView = blueView;
            }
            else {
            rightView = blueView;
            leftView = redView;
            }
        }

        void ResolutionChange(int value)
        {
            Debug.Log("Selected Option: " + resolutionDropdown.options[value].text);
        
            switch (value) 
            {
                case 0:
                    // Handle the first option
                    Debug.Log("Option 1 selected");
                    break;
                case 1:
                    // Handle the second option
                    Debug.Log("Option 2 selected");
                    break;
                // Add more cases as needed
                default:
                    break;

            }
        
        }
    }
}