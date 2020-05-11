using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AssetSpawner : MonoBehaviour
{
    public List<GameObject> AssetsToSpawn;

    void Start()
    {
        var newObj = Instantiate(AssetsToSpawn[Random.Range(0, AssetsToSpawn.Count)], gameObject.transform.position, Quaternion.identity);
        newObj.transform.parent = gameObject.transform;
    }
}
