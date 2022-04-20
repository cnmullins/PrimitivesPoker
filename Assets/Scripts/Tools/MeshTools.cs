
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshTools
{
    /// <summary>
    /// Cleans up mesh heavy objects made during compilation.
    /// </summary>
    /// <param name="parentTrans">Parent object</param>
    /// <returns>Boolean if error has occured</returns>
    public static bool CombineMeshToParent(Transform parentTrans) {
        MeshFilter[] filters;
        CombineInstance[] combiners;
        try {
            filters = parentTrans.GetComponentsInChildren<MeshFilter>();
            Debug.Log(parentTrans.name + " is combining " + filters.Length + " meshes!");
            combiners = new CombineInstance[filters.Length];
        } catch (Exception e) {
            Debug.LogError(e + ": parentTrans GameObject must contain a MeshFilter and MeshRenderer!");
            return false;
        }
        var finalMesh = new Mesh();
        for (int i = 0; i < filters.Length; ++i) {
        	combiners[i].subMeshIndex = 0;
        	combiners[i].mesh = filters[i].sharedMesh;
        	combiners[i].transform = filters[i].transform.localToWorldMatrix;
        }
        finalMesh.CombineMeshes(combiners);
        parentTrans.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        //clean up
        for (int i = 0; i < parentTrans.childCount; ++i) {
            MonoBehaviour.Destroy(parentTrans.GetChild(0).gameObject);
        }
        return true;
    }
}
