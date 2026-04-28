using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DelayedAnimationLoop : MonoBehaviour
{
    [SerializeField] private float delayBetweenLoops = 1f;
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

        RestartLoopFromStart();
    }

    void OnDisable()
    {
        if (loopRoutine != null)
            StopCoroutine(loopRoutine);
        loopRoutine = null;

        if (animator != null)
            animator.speed = 1f;
    }

    public void RestartLoopFromStart()
    {
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
