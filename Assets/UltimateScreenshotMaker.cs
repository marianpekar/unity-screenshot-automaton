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

    [Header("Main Camera")]
    [Tooltip("When ticked, custom cameras (if any) above will be ignored.")]
    [SerializeField] private bool UseOnlyMainCamera;

    [Tooltip("When ticked, main camera will be rotated towards the target.")]
    [SerializeField] public bool LookAtTarget;

    [Header("Prefabs")]
    [Tooltip("Tip: Select all prefabs you want to make a screenshot of and drag-and-drop them on Prefabs label below")]
    [SerializeField] private GameObject[] prefabs;

    [Header("Target")]
    [Tooltip("An empty GameObject that servers as a position where your prefabs will be instantiated. If none, position [0,0,0] will be used instead.")]
    [SerializeField] private Transform target;

    [Header("Output")]
    [Tooltip("When more than 1, a larger resolution screenshot will be produced. For example, passing 4 will make the screenshot be 4x4 larger than normal.")]
    [SerializeField] private int scale = 1;

    private GameObject[] pool;
    private string savePath;

    private void Start()
    {
        savePath = Application.dataPath.Replace("Assets", "");

        if (!target)
        {
            target = new GameObject().transform;
            target.name = "Target";
        }

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
            var instance = Instantiate(prefabs[i], target.position, Quaternion.identity, target.parent);
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
        if (cameras.Length == 0 || UseOnlyMainCamera)
        {
            var mainCamera = Camera.main;

            foreach (var camera in cameras)
                camera.Camera.gameObject.SetActive(false);

            mainCamera.gameObject.SetActive(true);

            cameras = new[]
            {
                new ScreenshotCamera
                {
                    Camera = mainCamera,
                    LookAtTarget = LookAtTarget
                }
            };
        }

        foreach (var camera in cameras)
        {
            if (camera.LookAtTarget)
                camera.Camera.transform.LookAt(target);
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
        ScreenCapture.CaptureScreenshot(fileName);
        Debug.Log($"{savePath}{fileName} saved.");
    }
}