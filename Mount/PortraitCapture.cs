using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace etchebarren
{
    public class PortraitCapture : MonoBehaviour
    {
        [Header("Type")]
        public bool horse = true;
        public bool saveDefaultSprite = false;

        public Camera characterCamera;
        public RenderTexture renderTexture;
        private Image characterImage;

        void Start()
        {
            if (horse) characterImage = StablesShop.instance.horsePortrait;
        }

        //void OnEnable()
        //{
        //    Capture();
        //}

        public void Capture()
        {
            // Capture the character view and set it as the sprite for the UI image
            Texture2D capturedTexture = CaptureCharacterView();
            Sprite characterSprite = Sprite.Create(capturedTexture, new Rect(0, 0, capturedTexture.width, capturedTexture.height), Vector2.one * 0.5f);
            characterImage.sprite = characterSprite;

            if (saveDefaultSprite) SaveDefaultSprite(capturedTexture);
        }

        // Function to capture the RenderTexture
        public Texture2D CaptureCharacterView()
        {
            // Set the target texture of the camera
            characterCamera.targetTexture = renderTexture;

            // Render the camera
            characterCamera.Render();

            // Read pixels from the RenderTexture
            RenderTexture.active = renderTexture;
            Texture2D characterTexture = new Texture2D(renderTexture.width, renderTexture.height);
            characterTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            characterTexture.Apply();

            // Reset the active RenderTexture (important for Unity's rendering pipeline)
            RenderTexture.active = null;

            return characterTexture;
        }

        public void SaveDefaultSprite(Texture2D capturedTexture)
        {
            string path = "Assets/DefaultHorseSprite.png";
            // Convert the Texture2D to a byte array
            byte[] textureData = capturedTexture.EncodeToPNG();

            // Save the texture data to a file
            File.WriteAllBytes(path, textureData);

            // Refresh the Unity Editor to make sure it recognizes the new asset
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}
