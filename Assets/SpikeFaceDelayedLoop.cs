using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SpikeFaceDelayedLoop : MonoBehaviour
{
    [SerializeField] private float delayBetweenLoops = 2.5f;
    [SerializeField] private float fallbackPlaySeconds = 1f;
    [SerializeField] private string stateName = "";

    private Animator animator;
    private Coroutine loopRoutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        RestartLoopFromIdle();
    }

    void OnDisable()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);
        loopRoutine = null;

        if (animator != null)
            animator.speed = 1f;
    }

    public void RestartLoopFromIdle()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null || !isActiveAndEnabled)
            return;

        if (loopRoutine != null)
            StopCoroutine(loopRoutine);

        loopRoutine = StartCoroutine(LoopWithDelay());
    }

    IEnumerator LoopWithDelay()
    {
        WaitForSeconds delay = new WaitForSeconds(Mathf.Max(0f, delayBetweenLoops));

        while (true)
        {
            PlayOnceFromStart();
            yield return WaitForCurrentAnimationOnce();

            animator.speed = 0f;
            yield return delay;
            animator.speed = 1f;
        }
    }

    void PlayOnceFromStart()
    {
        animator.enabled = true;
        animator.speed = 1f;

        if (!string.IsNullOrWhiteSpace(stateName))
            animator.Play(stateName, 0, 0f);
        else
            animator.Play(0, 0, 0f);

        animator.Update(0f);
    }

    IEnumerator WaitForCurrentAnimationOnce()
    {
        float fallbackEndTime = Time.time + Mathf.Max(0.05f, fallbackPlaySeconds);

        while (Time.time < fallbackEndTime)
        {
            if (animator == null)
                yield break;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.length > 0.01f && state.normalizedTime >= 1f && !animator.IsInTransition(0))
                yield break;

            yield return null;
        }
    }
}

public static class SpikeFaceDelayedLoopInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallOnStartup()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    static void TryInstall(Scene scene)
    {
        if (!scene.IsValid() || !scene.name.ToLowerInvariant().Replace(" ", "").Contains("level4"))
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
            InstallInChildren(roots[i].transform);
    }

    static void InstallInChildren(Transform root)
    {
        if (root == null)
            return;

        Animator animator = root.GetComponent<Animator>();
        if (animator != null && IsSpikeFace(root.gameObject, animator) && root.GetComponent<SpikeFaceDelayedLoop>() == null)
            root.gameObject.AddComponent<SpikeFaceDelayedLoop>();

        for (int i = 0; i < root.childCount; i++)
            InstallInChildren(root.GetChild(i));
    }

    static bool IsSpikeFace(GameObject candidate, Animator animator)
    {
        string objectName = candidate.name.ToLowerInvariant().Replace(" ", "");
        if (objectName.Contains("spikeface") || objectName.Contains("spikeface"))
            return true;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
            return false;

        string controllerName = controller.name.ToLowerInvariant().Replace(" ", "");
        return controllerName.Contains("spikeface") || controllerName.Contains("spikeface");
    }
}

