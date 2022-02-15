using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QA_Grow : MonoBehaviour
{
    private float currentSize;
    [SerializeField] private GameObject SheepBody;
    public void growSheepBigger()
    {
        currentSize++;
        SheepBody.gameObject.transform.localScale = new Vector3(currentSize, currentSize, 1);
    }
}
