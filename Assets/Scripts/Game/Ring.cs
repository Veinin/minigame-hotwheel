using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ring : MonoBehaviour
{
    public UnityAction<bool> OnCompleteAction;

    public float speed = 1;
    public float height = 2;
    public Vector3 force;
    public Transform target;

    private Rigidbody m_Rigidbody;

    private bool m_IsHit;
    private bool m_IsFire;

    private float m_FireTimer;
    private Vector3 m_Origin;
    private Vector3 m_Destination;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 force)
    {
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.AddForce(force);
    }

    public void Fire(Vector3 destination, float h)
    {
        height = h;
        m_IsFire = true;
        m_FireTimer = 0;
        m_Origin = transform.position;
        m_Destination = destination;
    }

    public void Fire(Transform t, float h)
    {
        height = h;
        target = t;
        m_IsFire = true;
        m_FireTimer = 0;
        m_Origin = transform.position;
        m_Destination = target.position + new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("Slot"))
        {
            m_IsHit = true;
        }
    }

    void Update()
    {
        if (m_IsFire)
        {
            m_FireTimer += speed * Time.deltaTime;
            m_FireTimer = Mathf.Clamp(m_FireTimer, 0, 1);
            transform.position = MathParabola.Parabola(m_Origin, m_Destination, height, m_FireTimer);

            if (m_FireTimer == 1)
            {
                m_IsFire = false;
                m_Rigidbody.isKinematic = false;
                m_Rigidbody.AddForce(force);

                this.RegisterTimer(0.5f, () =>
                {
                    OnComplete();
                });
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_IsFire = true;
            m_FireTimer = 0;
            m_Origin = transform.position;
            m_Destination = target.position + new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            m_Rigidbody.isKinematic = true;
            transform.position = new Vector3(0, 2.13f, 1.34f);
            transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
    }

    void OnComplete()
    {
        if (OnCompleteAction != null)
        {
            OnCompleteAction.Invoke(m_IsHit);
        }
    }
}
