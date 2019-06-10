// Copyright (C) 2016 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.

namespace GoogleVR.VideoDemo
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class SwitchVideos_S : MonoBehaviour
    {

        public GameObject[] videoSamples;
        public static SwitchVideos_S instance = null;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);

            string NATIVE_LIBS_MISSING_MESSAGE = "Video Support libraries not found or could not be loaded!\n" +
                  "Please add the <b>GVRVideoPlayer.unitypackage</b>\n to this project";
            try
            {
                IntPtr ptr = GvrVideoPlayerTexture.CreateVideoPlayer();
                if (ptr != IntPtr.Zero)
                {
                }
                else
                {
                    Debug.LogError(NATIVE_LIBS_MISSING_MESSAGE);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(NATIVE_LIBS_MISSING_MESSAGE);
            }
        }
        public void ShowSample(int index)
        {
            // If the libs are missing, always show the main menu.
            Debug.Log("show sample index : " + index);
            for (int i = 0; i < videoSamples.Length; i++)
            {
                if (videoSamples[i] != null)
                {

                    if (i != index)
                    {
                        if (videoSamples[i].activeSelf)
                        {
                            videoSamples[i].GetComponentInChildren<GvrVideoPlayerTexture>().CleanupVideo();
                        }
                    }
                    else
                    {
                        videoSamples[i].GetComponentInChildren<GvrVideoPlayerTexture>().ReInitializeVideo();
                    }
                    videoSamples[i].SetActive(i == index);
                }
            }
            GetComponent<Canvas>().enabled = index == -1;
        }
    }
}
