using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TutorialParallax : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;
    [SerializeField] private float parallaxMultiplier = 0.15f;

    private Vector3 basePosition;
    private Vector3 cameraStartPosition;
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;

        basePosition = transform.position;
        if (targetCamera != null)
        {
            cameraStartPosition = targetCamera.position;
            lastCameraPosition = targetCamera.position;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
                cameraStartPosition = targetCamera.position;
                basePosition = transform.position;
                lastCameraPosition = targetCamera.position;
            }
            return;
        }

        Vector3 deltaFromStart = targetCamera.position - cameraStartPosition;
        transform.position = new Vector3(
            basePosition.x + (deltaFromStart.x * parallaxMultiplier),
            basePosition.y + (deltaFromStart.y * parallaxMultiplier),
            basePosition.z);
        lastCameraPosition = targetCamera.position;
    }

    public void Configure(float multiplier)
    {
        parallaxMultiplier = multiplier;
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;
        if (targetCamera != null)
        {
            basePosition = transform.position;
            cameraStartPosition = targetCamera.position;
            lastCameraPosition = targetCamera.position;
        }
    }
}

public static class TutorialParallaxBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallOnLoad()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    private static void TryInstall(Scene scene)
    {
        if (!string.Equals(scene.name, "Tutorial", StringComparison.OrdinalIgnoreCase))
            return;

        AttachToLayers(scene, "BackgroundLayer1", 0.32f);
        AttachToLayers(scene, "BackgroundLayer2", 0.62f);
        AttachToLayers(scene, "BackgroundLayer3", 0.82f);
        AttachToLayers(scene, "BackgroundLayer4", 0.88f);
    }

    private static void AttachToLayers(Scene scene, string objectName, float multiplier)
    {
        if (string.IsNullOrWhiteSpace(objectName) || !scene.IsValid())
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                Transform t = transforms[j];
                if (t == null || !string.Equals(t.name, objectName, StringComparison.Ordinal))
                    continue;

                TutorialParallax parallax = t.GetComponent<TutorialParallax>();
                if (parallax == null)
                    parallax = t.gameObject.AddComponent<TutorialParallax>();
                parallax.Configure(multiplier);
            }
        }
    }
}
