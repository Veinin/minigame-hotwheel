using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    private Rigidbody m_Rigidbody;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 force)
    {
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.AddForce(force);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.AddForce(new Vector3(1, 0, 1) * 500);
        }
    }
}
