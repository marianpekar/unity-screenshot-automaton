using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UltimateScreenshotMaker : MonoBehaviour
{
    [Serializable]
    public class LightGroup : IEnumerable<Light>
    {
        public Light[] Lights;
        public IEnumerator<Light> GetEnumerator()
        {
            return Lights.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public Light this[int index]
        {
            get { return Lights[index]; }
            set { Lights[index] = value; }
        }
    }

    [Serializable]
    public class ScreenshotCamera
    {
        public Camera Camera;

        [Tooltip("When ticked, this camera will be rotated towards the target")]
        public bool LookAtTarget;
    }

    [Header("Lights")]
    [Tooltip("If empty, screenshots will be taken in the scene with lights as is.")]
    [SerializeField] private LightGroup[] lightGroups;

    [Header("Cameras")]
    [Tooltip("If empty, the main camera will be used by default")]
    [SerializeField] private ScreenshotCamera[] cameras;

    [Tooltip("When ticked, main and all (if any) cameras will be rotated towards the target.")]
    [SerializeField] private bool allLookAtTarget;

    [Tooltip("Offset is added to look at target position, which is the position of this object.")]
    [SerializeField] private Vector3 targetOffset;

    [Header("Prefabs")]
    [Tooltip("Tip: Select all prefabs you want to make a screenshot of and drag-and-drop them on Prefabs label below")]
    [SerializeField] private GameObject[] prefabs;

    [Header("Output")]
    [Tooltip("When more than 1, a larger resolution screenshot will be produced. For example, passing 4 will make the screenshot be 4x4 larger than normal.")]
    [SerializeField] private int scale = 1;

    private GameObject[] pool;
    private string savePath;

    private void Start()
    {
        savePath = Application.dataPath.Replace("Assets", "");

        InitializePool();
        DisableAllLights();
        SetupCameras();
        StartCoroutine(MainCoroutine());
    }

    private void InitializePool()
    {
        pool = new GameObject[prefabs.Length];

        for (var i = 0; i < prefabs.Length; i++)
        {
            var instance = Instantiate(prefabs[i], gameObject.transform.position, Quaternion.identity, gameObject.transform.parent);
            pool[i] = instance;
            instance.SetActive(false);
        }
    }

    private void DisableAllLights()
    {
        foreach (var lightGroup in lightGroups)
            foreach (var light in lightGroup)
                light.gameObject.SetActive(false);
    }

    private void SetupCameras()
    {
        if (cameras.Length == 0)
        {
            var mainCamera = Camera.main;

            mainCamera.gameObject.SetActive(true);

            cameras = new[]
            {
                new ScreenshotCamera
                {
                    Camera = mainCamera,
                    LookAtTarget = allLookAtTarget
                }
            };
        }

        foreach (var camera in cameras)
        {
            if (camera.LookAtTarget || allLookAtTarget)
            {
                camera.LookAtTarget = allLookAtTarget;
                camera.Camera.transform.LookAt(gameObject.transform.position + targetOffset);
            }
        }
    }

    private IEnumerator MainCoroutine()
    {
        foreach (var gameObject in pool)
        {
            gameObject.SetActive(true);

            if (lightGroups.Length != 0)
            {
                foreach (var lightGroup in lightGroups)
                {
                    foreach (var light in lightGroup)
                        light.gameObject.SetActive(true);

                    foreach (var camera in cameras)
                    {
                        if (cameras.Length > 1)
                            camera.Camera.gameObject.SetActive(true);

                        SaveScreenshot(gameObject, lightGroup[0].name, camera);

                        yield return new WaitForSeconds(0.5f);

                        if (cameras.Length > 1)
                            camera.Camera.gameObject.SetActive(false);
                    }

                    foreach (var light in lightGroup)
                        light.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (var camera in cameras)
                {
                    if (cameras.Length > 1)
                        camera.Camera.gameObject.SetActive(true);

                    SaveScreenshot(gameObject, "", camera);

                    yield return new WaitForSeconds(0.5f);

                    if (cameras.Length > 1)
                        camera.Camera.gameObject.SetActive(false);
                }
            }

            gameObject.SetActive(false);
        }
    }

    private void SaveScreenshot(GameObject gameObject, string lightGroupName, ScreenshotCamera camera)
    {
        var fileName = $"{gameObject.name}_{lightGroupName}_{camera.Camera.name}.png";
        ScreenCapture.CaptureScreenshot(fileName, scale);
        Debug.Log($"{savePath}{fileName} saved.");
    }
}