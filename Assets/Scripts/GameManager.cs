using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private Vector3 SpawnPos = new Vector3(0, 2.1f, -2);
    private Vector3 OriginPos = new Vector3(0, 2.1f, 1.3f);

    public Ring ringPrefab;
    public float maxSlideDistance   = 200;
    public float maxFireDistance    = 45f;
    public float maxFireHeight      = 3f;

    private HomeView m_HomeView;
    private PlayerView m_PlayerView;

    private Ring m_CurrntRing;
    private List<Ring> m_RingList;

    private List<Transform> m_Slots;

    private bool m_IsStarted;

    void Awake()
    {
        m_RingList = new List<Ring>();
        m_Slots = new List<Transform>();

        var level = GameObject.Find("Level_1").transform;
        for (var i = 0; i < level.childCount; i++)
        {
            m_Slots.Add(level.GetChild(i));
        }

        var canvas = GameObject.Find("Canvas");
        m_HomeView = canvas.GetComponentInChildren<HomeView>(true);
        m_HomeView.OnStartAction += OnGameStart;

        m_PlayerView = canvas.GetComponentInChildren<PlayerView>(true);
        m_PlayerView.OnBackAction += OnGameBack;
    }

    void OnGameStart()
    {
        StartGame();
    }

    void OnGameBack()
    {
        m_IsStarted = false;
        m_HomeView.Back();
        m_PlayerView.gameObject.SetActive(false);
    }

    #region Game

    void StartGame()
    {
        m_PlayerView.gameObject.SetActive(true);
        SpawnRing();
    }

    void SpawnRing()
    {
        var ring = GameObject.Instantiate<Ring>(ringPrefab, SpawnPos, Quaternion.Euler(-90, 0, 0));
        ring.transform.DOMove(OriginPos, 0.5f).OnComplete(() =>
        {
            m_IsStarted = true;
        });
        m_CurrntRing = ring;
        m_RingList.Add(ring);
    }

    void Update()
    {
        CheckSlide();
    }

    private Vector3 beginPos;

    void CheckSlide()
    {
        if (!m_IsStarted)
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0))
        {
            TouchBegin(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            TouchMoved(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            TouchEnd(Input.mousePosition);
        }
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 1)
        {
            var touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
            {
                TouchBegin(touch.position);
            }

            if (touch.phase == TouchPhase.Moved)
            {
                TouchMoved(touch.position);
            }

            if (touch.phase == TouchPhase.Ended)
            {
                TouchEnd(touch.position);
            }
        }
#endif
    }

    void TouchBegin(Vector3 position)
    {
        beginPos = position;
        m_PlayerView.ShowForceEnergy();
    }

    void TouchMoved(Vector3 position)
    {
        var distance = (position - beginPos).magnitude;
        distance = Mathf.Clamp(distance, 0, maxSlideDistance);

        var percent = distance / maxSlideDistance;
        m_PlayerView.SetForcePercent(percent);
    }

    void TouchEnd(Vector3 position)
    {
        OnFire(position - beginPos);
        m_PlayerView.HideForceEnergy();
    }

    void OnFire(Vector2 dir)
    {
        var touchDir = dir.normalized;
        var touchDistance = dir.magnitude;
        var touchPercent = Mathf.Clamp(touchDistance, 0, maxSlideDistance) / maxSlideDistance;

        var distance = maxFireDistance * touchPercent;
        var height = 2 + maxFireHeight * touchPercent;
        var destination = new Vector3(touchDir.x * distance, 2, touchDir.y * distance);

        Transform target;
        if (FindClosestSlot(destination, out target))
        {
            m_CurrntRing.Fire(target);
        }
        else
        {
            m_CurrntRing.Fire(destination, height);
        }

        SpawnRing();
    }

    bool FindClosestSlot(Vector3 position, out Transform target)
    {
        Transform t = null;
        float minDistance = float.MaxValue;

        foreach(var slot in m_Slots)
        {
            var dist = Vector3.Distance(slot.position, position);
            if (dist < minDistance)
            {
                t = slot;
                minDistance = dist;
            }
        }

        target = t;
        return minDistance < 1.5f;
    }

    #endregion
}
