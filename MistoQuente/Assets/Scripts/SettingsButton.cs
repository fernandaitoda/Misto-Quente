using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Serenegiant.UVC
{
    public class SettingsButton : MonoBehaviour
    {
        [SerializeField] GameObject settingsPanel;
        [SerializeField] Button settingsButton;

        public bool activeSettings = false;
        void OnEnable()
        {
            settingsButton.onClick.AddListener(OnClick);
        }
        void OnClick()
        {
            activeSettings = !activeSettings;
            settingsPanel.SetActive(activeSettings);
        }
    }
}