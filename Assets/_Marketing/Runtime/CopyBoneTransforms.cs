using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CopyBoneTransforms : MonoBehaviour
{
    public Transform sourceRoot;
    public Transform targetRoot;
    public bool copyOnStart = true;

    void Start()
    {
        if (copyOnStart)
        {
            Copy();
            SaveChanges();
        }
    }

    [Button]
    void Copy()
    {
        CopyTransforms(sourceRoot, targetRoot);
    }

    void CopyTransforms(Transform source, Transform target)
    {
        if (source == null || target == null)
            return;

        target.position = source.position;
        target.rotation = source.rotation;

        for (int i = 0; i < source.childCount; i++)
        {
            CopyTransforms(source.GetChild(i), target.GetChild(i));
        }
    }

    [Button]
    void SaveChanges()
    {
#if UNITY_EDITOR
        PrefabUtility.ApplyPrefabInstance(targetRoot.gameObject, InteractionMode.UserAction);
#endif
    }
}

