﻿using UnityEngine;

namespace Utils
{
#pragma warning disable 649
    public class ScreenshotUtils : MonoBehaviour
    {
        public Camera renderingCamera;

        public int resWidth = 3840;
        public int resHeight = 2160;

        public static string ScreenShotName(int width, int height)
        {
            return string.Format("screenshots/screen_{1}x{2}_{3}.png",
                                 Application.dataPath,
                                 width, height,
                                 System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }

        void LateUpdate()
        {
            if (CustomInput.Bindings.takeScreenshot)
            {
                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                renderingCamera.targetTexture = rt;
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                renderingCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                renderingCamera.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenShotName(resWidth, resHeight);
                System.IO.File.WriteAllBytes(filename, bytes);
                Debug.Log(string.Format("Took screenshot to: {0}", filename));
            }
        }
    }
}