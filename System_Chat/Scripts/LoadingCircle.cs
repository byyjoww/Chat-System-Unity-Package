using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    public List<GameObject> disableElements = new List<GameObject>();
    public GameObject loadingCircle;

    public void StartListUpdate()
    {
        foreach (var item in disableElements)
        {
            if (item != null) { item.SetActive(false); }
        }

        loadingCircle.SetActive(true);
    }

    public void EndListUpdate()
    {
        foreach (var item in disableElements)
        {
            if (item != null) { item.SetActive(true); }
        }

        loadingCircle.SetActive(false);
    }
}
