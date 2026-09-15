using Unity.Mathematics.Geometry;
using UnityEngine;

public class PillarGap : MonoBehaviour
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
        var localPosition = m_Transform.localPosition;
        if (localPosition.x < -10)
        {
            localPosition = new Vector3(localPosition.x + 20, Mathf.Lerp(-2, 2, Random.value),
                localPosition.z);
            m_Transform.localPosition = localPosition;
        }
    }
}