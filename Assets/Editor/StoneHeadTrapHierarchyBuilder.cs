using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class StoneHeadTrapHierarchyBuilder
{
    const string Level3SceneName = "Level 3";
    const string HitAnimationPath = "Assets/Pixel Adventure 1/Assets/Traps/Rock Head/Hit.anim";

    static StoneHeadTrapHierarchyBuilder()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.hierarchyChanged += TryBuildActiveScene;
        TryBuildActiveScene();
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        TryBuild(scene);
    }

    static void TryBuildActiveScene()
    {
        TryBuild(SceneManager.GetActiveScene());
    }

    static void TryBuild(Scene scene)
    {
        if (!scene.IsValid() || !string.Equals(scene.name, Level3SceneName, StringComparison.OrdinalIgnoreCase))
            return;

        bool changed = false;
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
            changed |= TryBuildInChildren(roots[i].transform);

        if (changed)
            EditorSceneManager.MarkSceneDirty(scene);
    }

    static bool TryBuildInChildren(Transform root)
    {
        if (root == null)
            return false;

        bool changed = false;
        Animator animator = root.GetComponent<Animator>();
        if (animator != null && IsStoneHead(root.gameObject, animator))
            changed |= EnsureTrap(root.gameObject);

        for (int i = 0; i < root.childCount; i++)
            changed |= TryBuildInChildren(root.GetChild(i));

        return changed;
    }

    static bool EnsureTrap(GameObject target)
    {
        StoneHeadTrap trap = target.GetComponent<StoneHeadTrap>();
        bool changed = false;
        if (trap == null)
        {
            trap = target.AddComponent<StoneHeadTrap>();
            changed = true;
        }

        AnimationClip hitClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(HitAnimationPath);
        if (hitClip != null)
        {
            changed |= SetPrivateField(trap, "hitAnimation", hitClip);
            changed |= SetPrivateField(trap, "hitSprites", ExtractSpriteFrames(hitClip));
        }

        changed |= SetPrivateField(trap, "damageHearts", 2);
        changed |= SetPrivateField(trap, "hitStateName", "Hit");
        changed |= SetPrivateField(trap, "hitSpriteFrameSeconds", 0.12f);
        changed |= SetPrivateField(trap, "hitFinalFrameHoldSeconds", 0.15f);
        changed |= SetPrivateField(trap, "debugLogs", true);
        changed |= SetPrivateField(trap, "detectionOffset", new Vector2(0f, -2.25f));
        changed |= SetPrivateField(trap, "detectionSize", new Vector2(2f, 7f));
        changed |= EnsureDamageHitbox(target, trap);

        if (changed)
            EditorUtility.SetDirty(target);

        return changed;
    }

    static bool EnsureDamageHitbox(GameObject target, StoneHeadTrap trap)
    {
        Transform existing = target.transform.Find("StoneHeadDamageHitbox");
        GameObject hitboxObject = existing != null ? existing.gameObject : new GameObject("StoneHeadDamageHitbox");
        bool changed = existing == null;

        if (existing == null)
        {
            hitboxObject.transform.SetParent(target.transform, false);
            hitboxObject.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            hitboxObject.transform.localRotation = Quaternion.identity;
            hitboxObject.transform.localScale = Vector3.one;
        }

        BoxCollider2D damageHitbox = hitboxObject.GetComponent<BoxCollider2D>();
        if (damageHitbox == null)
        {
            damageHitbox = hitboxObject.AddComponent<BoxCollider2D>();
            damageHitbox.offset = Vector2.zero;
            damageHitbox.size = new Vector2(1f, 0.35f);
            changed = true;
        }

        if (!damageHitbox.isTrigger)
        {
            damageHitbox.isTrigger = true;
            changed = true;
        }

        StoneHeadTrapHitbox relay = hitboxObject.GetComponent<StoneHeadTrapHitbox>();
        if (relay == null)
        {
            relay = hitboxObject.AddComponent<StoneHeadTrapHitbox>();
            changed = true;
        }
        relay.SetOwner(trap);

        changed |= SetPrivateField(trap, "damageHitbox", damageHitbox);
        changed |= SetPrivateField(trap, "damageHitboxOffset", new Vector2(0f, -0.45f));
        changed |= SetPrivateField(trap, "damageHitboxSize", new Vector2(1f, 0.35f));

        if (changed)
        {
            EditorUtility.SetDirty(hitboxObject);
            EditorUtility.SetDirty(damageHitbox);
        }

        return changed;
    }

    static bool SetPrivateField<T>(StoneHeadTrap trap, string fieldName, T value)
    {
        FieldInfo field = typeof(StoneHeadTrap).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            return false;

        object current = field.GetValue(trap);
        if (current is Array currentArray && value is Array newArray && ArraysMatch(currentArray, newArray))
            return false;

        if (Equals(current, value))
            return false;

        field.SetValue(trap, value);
        EditorUtility.SetDirty(trap);
        return true;
    }

    static Sprite[] ExtractSpriteFrames(AnimationClip clip)
    {
        if (clip == null)
            return Array.Empty<Sprite>();

        EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        for (int i = 0; i < bindings.Length; i++)
        {
            EditorCurveBinding binding = bindings[i];
            if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite")
                continue;

            ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            Sprite[] sprites = new Sprite[keyframes.Length];
            for (int keyframeIndex = 0; keyframeIndex < keyframes.Length; keyframeIndex++)
                sprites[keyframeIndex] = keyframes[keyframeIndex].value as Sprite;

            return sprites;
        }

        return Array.Empty<Sprite>();
    }

    static bool ArraysMatch(Array current, Array next)
    {
        if (current.Length != next.Length)
            return false;

        for (int i = 0; i < current.Length; i++)
        {
            if (!Equals(current.GetValue(i), next.GetValue(i)))
                return false;
        }

        return true;
    }

    static bool IsStoneHead(GameObject candidate, Animator animator)
    {
        string objectName = candidate.name.ToLowerInvariant().Replace(" ", "");
        if (objectName.Contains("stonehead") || objectName.Contains("rockhead"))
            return true;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
            return false;

        string controllerName = controller.name.ToLowerInvariant().Replace(" ", "");
        return controllerName.Contains("stonehead") || controllerName.Contains("rockhead");
    }
}
