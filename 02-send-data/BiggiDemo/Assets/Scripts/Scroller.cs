using System;
using UnityEngine;

public class Scroller : MonoBehaviour
{
    private Transform m_Transform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        m_Transform = transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_Transform.Translate(-1 * Time.fixedDeltaTime, 0f, 0, Space.World);
        if (m_Transform.localPosition.x < -8)
        {
            m_Transform.Translate(8, 0f, 0, Space.World);
        }
    }
}