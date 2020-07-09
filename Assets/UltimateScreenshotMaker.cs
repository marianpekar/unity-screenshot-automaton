using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private ScreenshotCamera[] cameras;

    [SerializeField] private GameObject[] prefabs;

    [SerializeField] private Transform target;

    private GameObject[] pool;

    [SerializeField] private int scale = 1;

    void Start()
    {
        pool = new GameObject[prefabs.Length];

        for (var i = 0; i < prefabs.Length; i++)
        {
            var instance = Instantiate(prefabs[i], target.position, Quaternion.identity, target.parent);
            pool[i] = instance;
            instance.SetActive(false);
        }

        foreach (var light in lights)
            light.enabled = false;

        foreach (var camera in cameras)
        {
            if(camera.LookAtTarget)
                camera.Camera.transform.LookAt(target);
        }

        StartCoroutine(TakeScreenshot());
    }

    private IEnumerator TakeScreenshot()
    {
        foreach (var gameObject in pool)
        {
            gameObject.SetActive(true);

            foreach (var light in lights)
            {
                light.enabled = true;

                foreach (var camera in cameras)
                {
                    camera.Camera.gameObject.SetActive(true);

                    var fileName = $"{gameObject.name}_{light.name}_{camera.Camera.name}.png";
                    ScreenCapture.CaptureScreenshot(fileName);
                    Debug.Log($"{fileName} saved.");

                    yield return new WaitForSeconds(0.5f);

                    camera.Camera.gameObject.SetActive(false);
                }

                light.enabled = false;
            }
            gameObject.SetActive(false);
        }
    }

}
