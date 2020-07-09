using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UltimateScreenshotMaker : MonoBehaviour
{
    [Serializable]
    private class ScreenshotCamera
    {
        public Camera Camera;
        public bool LookAtTarget;
    }

    [SerializeField] private Light[] lights;

    [SerializeField] private bool UseOnlyMainCamera;

    [SerializeField] public bool LookAtTarget;

    [SerializeField] private ScreenshotCamera[] cameras;

    [SerializeField] private GameObject[] prefabs;

    [SerializeField] private Transform target;

    private GameObject[] pool;

    [SerializeField] private int scale = 1;

    void Start()
    {
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
        if (lights.Length == 0)
        {
            lights = new Light[1];
            var lightGameObject = new GameObject().AddComponent<Light>();
            lightGameObject.name = "DummyLight";
            lightGameObject.intensity = 0;
            lightGameObject.range = 0;
            lights[0] = lightGameObject;
            return;
        }

        foreach (var light in lights)
            light.enabled = false;
    }

    private void SetupCameras()
    {
        if (cameras.Length == 0 || UseOnlyMainCamera)
        {
            var mainCamera = Camera.main;

            foreach (var camera in cameras)
                camera.Camera.gameObject.SetActive(false);

            mainCamera.gameObject.SetActive(true);

            cameras = new ScreenshotCamera[]
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

            foreach (var light in lights)
            {
                light.enabled = true;

                foreach (var camera in cameras)
                {
                    if(cameras.Length > 1)
                        camera.Camera.gameObject.SetActive(true);

                    var fileName = $"{gameObject.name}_{light.name}_{camera.Camera.name}.png";
                    ScreenCapture.CaptureScreenshot(fileName);
                    Debug.Log($"{fileName} saved.");

                    yield return new WaitForSeconds(0.5f);

                    if (cameras.Length > 1)
                        camera.Camera.gameObject.SetActive(false);
                }

                light.enabled = false;
            }
            gameObject.SetActive(false);
        }
    }
}
