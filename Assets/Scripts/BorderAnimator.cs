using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BorderAnimator : MonoBehaviour
{
    public float speed = 30f;
    private RectTransform rect;
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    void Update()
    {
        rect.localPosition += Vector3.right * Mathf.Sin(Time.time * speed) * 0.2f;
    }
}
