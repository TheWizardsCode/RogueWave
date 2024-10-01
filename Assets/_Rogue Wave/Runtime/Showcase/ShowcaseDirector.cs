using NaughtyAttributes;
using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RogueWave.Editor
{
     public class ShowcaseDirector : MonoBehaviour
     {
#if UNITY_EDITOR
        [SerializeField, Tooltip("The parent transforms to spawn the enemies under.")]
        BasicEnemyShowcase[] enemyShowcaseSpots;

        [Header("Image Capture")]
        [SerializeField, Tooltip("Capture images of the object in the Primary (first) showcase slot?")]
        bool captureImages = false;
        [SerializeField, Tooltip("The camera to use to capture images."), ShowIf("captureImages")]
        Camera targetCamera;
        [SerializeField, Tooltip("The width of the image to capture. The center of the capture area is the center of the camera image."), ShowIf("captureImages")]
        public int width = 512;
        [SerializeField, Tooltip("The height of the image to capture. The center of the capture area is the center of the camera image."), ShowIf("captureImages")]
        public int height = 512;
        [SerializeField, Tooltip("The target framerate of the application."), ShowIf("captureImages")]
        public int targetFramerate = 60;
        [SerializeField, Tooltip("The file path to save the images to."), ShowIf("captureImages")]
        public string filePath = "Assets/_Dev/ShowcaseImages";
        [SerializeField, Tooltip("The number of images to capture."), ShowIf("captureImages")]
        public int numberOfImages = 10;
        [SerializeField, Tooltip("The number of frames between capturing images."), ShowIf("captureImages")]
        public int frameInterval = 10;

        private void Start()
        {
            List<BasicEnemyController> enemies = GetAllObjects<BasicEnemyController>();
            enemies = enemies.OrderBy(e => e.challengeRating).ToList();
            StartCoroutine(ShowcaseEnemies(enemies));
        }

        private List<T> GetAllObjects<T>() where T : MonoBehaviour
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            List<T> results = new List<T>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                T enemy = prefab.GetComponent<T>();
                if (enemy != null)
                {
                    results.Add(enemy);
                }
            }

            return results;
        }

        private IEnumerator ShowcaseEnemies(List<BasicEnemyController> enemies)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFramerate;

            foreach (BasicEnemyController enemy in enemies)
            {
                if (enemy.includeInShowcase == false)
                {
                    continue;
                }

                CleanupScene();

                List<Coroutine> coroutines = new List<Coroutine>();
                bool isFirst = true;
                foreach (BasicEnemyShowcase showcase in enemyShowcaseSpots)
                {
                    BasicEnemyController newEnemy = Instantiate(enemy, showcase.transform);
                    newEnemy.transform.localPosition = Vector3.zero;
                    newEnemy.transform.localRotation = Quaternion.identity;
                    newEnemy.transform.Rotate(Vector3.up, Random.Range(-30, 30));

                    coroutines.Add(StartCoroutine(showcase.StartShowcase()));

                    if (captureImages && isFirst)
                    {
                        StartCoroutine(CaptureScreenshotCoroutine(newEnemy.displayName));
                        isFirst = false;
                    }
                }

                foreach (Coroutine coroutine in coroutines)
                {
                    yield return coroutine;
                }

                yield return new WaitForSeconds(2.5f);
            }
        }

        private IEnumerator CaptureScreenshotCoroutine(string filename)
        {
            yield return new WaitForEndOfFrame();

            List<string> imagePaths = new List<string>();
            int imageCount = 0;
            int framesUntilNextCapture = frameInterval;
            while (imageCount < numberOfImages)
            {
                if (framesUntilNextCapture > 0)
                {
                    framesUntilNextCapture--;
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    string path = $"{filePath}/{filename}_{imageCount}.png";

                    RenderTexture renderTexture = new RenderTexture(width, height, 24);
                    targetCamera.targetTexture = renderTexture;
                    Texture2D screenImage = new Texture2D(width, height, TextureFormat.RGB24, false);

                    targetCamera.Render();
                    RenderTexture.active = renderTexture;

                    imageCount++;

                    screenImage.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    screenImage.Apply();

                    byte[] imageBytes = screenImage.EncodeToPNG();
                    File.WriteAllBytes(path, imageBytes);

                    framesUntilNextCapture = frameInterval;

                    targetCamera.targetTexture = null;
                    RenderTexture.active = null;
                    Destroy(renderTexture);

                    imagePaths.Add(path);
                }
            }
            AssetDatabase.Refresh();

            foreach (string imagePath in imagePaths)
            {
                AssetDatabase.ImportAsset(imagePath);

                TextureImporter textureImporter = (TextureImporter)UnityEditor.AssetImporter.GetAtPath(imagePath);
                textureImporter.textureType = TextureImporterType.Sprite;

                AssetDatabase.WriteImportSettingsIfDirty(imagePath);

                Debug.Log("Sprite of screenshot saved to: " + imagePath);
            }

            AssetDatabase.SaveAssets();
        }

        private void CleanupScene()
        {
            foreach (BasicEnemyShowcase showcase in enemyShowcaseSpots)
            {
                PickupTriggerZone[] pickups = FindObjectsOfType<PickupTriggerZone>();
                foreach (PickupTriggerZone pickup in pickups)
                {
                    Destroy(pickup.gameObject);
                }

                foreach (Transform child in showcase.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
#endif
    }
}