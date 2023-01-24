using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectQueue
{
    private Transform reuseTransform;
    private GameObject[] prefabArr;
    private Dictionary<int, List<Transform>> objectDict;

    public void Create(GameObject[] targetPrefabArr, GameObject reuse) {
        prefabArr = targetPrefabArr;
        objectDict = new Dictionary<int,List<Transform>>();
        for (int i = 0; i < prefabArr.Length; i++) {
			objectDict[i] = new List<Transform>();
		}
        reuseTransform = reuse.GetComponent<Transform>();

        // always set reuse active false
        reuse.SetActive(false);
    }

    public GameObject GetObjectWithType(int objectType, Vector3 position) {
        if (objectDict[objectType].Count == 0) {
            return UnityEngine.Object.Instantiate(prefabArr[objectType], position, Quaternion.identity);
        } else {
            Transform copyObjTransform = objectDict[objectType][0];
            objectDict[objectType].RemoveAt(0);
            copyObjTransform.SetPositionAndRotation(position, Quaternion.identity);
            return copyObjTransform.GetComponent<GameObject>();
        }
    }

    void HideObject(Transform targetTransform, int objectType) {
        targetTransform.SetParent(reuseTransform);
        objectDict[objectType].Add(targetTransform);
    }
}
