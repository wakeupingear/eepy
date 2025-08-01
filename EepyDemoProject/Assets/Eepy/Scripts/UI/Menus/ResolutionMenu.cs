using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Eepy
{
    public class ResolutionMenu : MenuScreen
    {   
        // Prefab to create resolution buttons from
        [SerializeField]
        private GameObject resolutionButtonPrefab;
        // Maximum number of resolutions to display
        [SerializeField]
        private int maxResolutions = 12;
        // If we have to filter out some resolutions, we will prioritize these resolutions to be kept
        [SerializeField]
        private List<int> targetHeights = new() { 720, 1280, 1440, 1600, 2160, 3840 };

        private List<Resolution> resolutions = new List<Resolution>();
        private List<MenuButton> resolutionButtons = new List<MenuButton>();

        protected override void OnFocused()
        {
            base.OnFocused();
            
            // Filter out duplicates
            foreach (var res in Screen.resolutions)
            {
                if (!resolutions.Contains(res))
                {
                    resolutions.Add(res);
                }
            }

            if (resolutions.Count > maxResolutions)
            {
                // Remove adjacent resolutions that have different framerates
                for (int i = resolutions.Count - 2; i >= 0; i--)
                {
                    if (resolutions[i].width == resolutions[i + 1].width &&
                        resolutions[i].height == resolutions[i + 1].height &&
                        resolutions[i].refreshRateRatio.numerator != resolutions[i + 1].refreshRateRatio.numerator)
                    {
                        resolutions.RemoveAt(i + 1);
                    }
                }

                if (resolutions.Count > maxResolutions)
                {
                    // Remove all resolutions that aren't a multiple of 10
                    resolutions.RemoveAll(res => res.width % 10 != 0 || res.height % 10 != 0);

                    if (resolutions.Count > maxResolutions)
                    {
                        // Keep removing resolutions that aren't in the target resolutions list
                        List<Resolution> toRemove = new();
                        foreach (var res in resolutions)
                        {
                            if (!targetHeights.Contains(res.height) && !targetHeights.Contains(res.width))
                            {
                                toRemove.Add(res);
                            }
                        }
                        toRemove = toRemove.GetRange(0, Math.Clamp(resolutions.Count - maxResolutions, 0, toRemove.Count));
                        foreach (var res in toRemove)
                        {
                            resolutions.Remove(res);
                        }

                        if (resolutions.Count > maxResolutions)
                        {
                            // Just remove the low end ._.
                            resolutions.RemoveRange(0, resolutions.Count - maxResolutions);
                        }
                    }
                }
            }

            while (resolutionButtons.Count > resolutions.Count)
            {
                Destroy(resolutionButtons[resolutionButtons.Count - 1].gameObject);
                resolutionButtons.RemoveAt(resolutionButtons.Count - 1);
            }
            while (resolutionButtons.Count < resolutions.Count)
            {
                var button = Instantiate(resolutionButtonPrefab, layoutGroup.transform);
                button.transform.SetParent(layoutGroup.transform, false);
                resolutionButtons.Add(button.GetComponent<MenuButton>());
            }

            startingButton = resolutionButtons[0];
            for (int i = 0; i < resolutionButtons.Count; i++)
            {
                var button = resolutionButtons[i];
                var res = resolutions[i];

                button.menuScreen = this;
                button.GetPrimaryText().SetText($"{res.width} x {res.height}");
                button.name = "ResolutionButton_" + res.width + "_" + res.height;
                button.OnClick.RemoveAllListeners();
                button.OnClick.AddListener(() =>
                {
                    SettingsManager.SetResolution(res.width, res.height);
                    CloseMenu();
                });
                
                bool underline = SettingsManager.resolution == SettingsManager.ResolutionToString(res.width, res.height);
                button.GetPrimaryText().SetUnderline(underline);
                if (underline)
                {
                    startingButton = button;
                }

                if (i > 0)
                {
                    button.up = resolutionButtons[i - 1];
                    resolutionButtons[i - 1].down = button;
                }
            }
            resolutionButtons[^1].down = resolutionButtons[0];
            resolutionButtons[0].up = resolutionButtons[^1];
        }
    }
};