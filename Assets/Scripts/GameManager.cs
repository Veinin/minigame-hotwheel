using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private readonly Vector3 SpawnPos = new Vector3(0, 2.1f, -2);
    private readonly Vector3 OriginPos = new Vector3(0, 2.1f, 1.3f);

    public Ring ringPrefab;
    public float maxFireDistance    = 45f;
    public float maxFireHeight      = 3f;
    public float maxSlidePixelPercent  = 0.2f;

    public int enlargeTriggerCount = 5;
    public int enlargeTriggerProb  = 30;
    public int enlargeTriggerProbPromote = 10;
    public int[] enlargeScaleRate = new int[3]{20, 35, 50};

    private HomeView m_HomeView;
    private PlayerView m_PlayerView;

    private Ring m_CurrntRing;
    private List<Ring> m_RingList;
    private List<Transform> m_Slots;

    private bool m_IsReady;
    private float m_MaxSlideDistance;
    private Vector3 m_TouchBeginPos;

    private int m_HitCount;
    private int m_MissCount;

    private bool m_IsEnlarge;
    private int m_EnlargeScale = 0;
    private int m_EnlargeProb = 0;
    private int m_EnlargeScaleIndex = 0;

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

        m_MaxSlideDistance = Screen.height * maxSlidePixelPercent;
    }

    void OnGameStart()
    {
        StartGame();
    }

    void OnGameBack()
    {
        m_IsReady = false;
        m_HomeView.Back();
        m_PlayerView.gameObject.SetActive(false);
    }

    #region Game

    void StartGame()
    {
        m_HitCount = 0;
        m_MissCount = 0;
        m_PlayerView.gameObject.SetActive(true);
        m_PlayerView.UpdateHitCount(0);

        ResetEnlarge();
        SpawnRing();
    }

    void SpawnRing()
    {
        var ring = GameObject.Instantiate<Ring>(ringPrefab, SpawnPos, Quaternion.Euler(-90, 0, 0));

        if (m_IsEnlarge)
        {
            var scale = 1 + enlargeScaleRate[m_EnlargeScaleIndex] * 1f / 100;
            ring.transform.localScale = new Vector3(scale, scale, scale);
        }
        
        ring.transform.DOMove(OriginPos, 0.25f).OnComplete(() =>
        {
            m_IsReady = true;
        });
        m_CurrntRing = ring;
        m_CurrntRing.OnCompleteAction += OnRingResult;
        m_RingList.Add(ring);
    }
    
    void OnRingResult(bool isHit)
    {
        if (isHit)
        {
            m_HitCount++;
            m_PlayerView.UpdateHitCount(m_HitCount);
            ResetEnlarge();
        }
        else
        {
            m_MissCount++;
            RandomEnlarge();
        }

        SpawnRing();
    }

    void ResetEnlarge()
    {
        m_IsEnlarge = false;
        m_EnlargeScale = 0;
        m_EnlargeProb = enlargeTriggerProb;
        m_EnlargeScaleIndex = -1;
    }

    void RandomEnlarge()
    {
        if (m_MissCount < enlargeTriggerCount)
        {
            return;
        }

        var isSucc = Random.Range(0, 100) < m_EnlargeProb;
        if (isSucc)
        {
            m_IsEnlarge = true;
            m_MissCount = 0;
            m_EnlargeProb = enlargeTriggerProb;
            m_EnlargeScaleIndex++;
            m_EnlargeScaleIndex = Mathf.Min(++m_EnlargeScaleIndex, enlargeScaleRate.Length - 1);
        }
        else
        {
            m_EnlargeProb += enlargeTriggerProbPromote;
        }
    }

    void Update()
    {
        CheckSlide();
    }

    void CheckSlide()
    {
        if (!m_IsReady)
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
        m_TouchBeginPos = position;
        m_PlayerView.ShowForceEnergy();
    }

    void TouchMoved(Vector3 position)
    {
        var distance = (position - m_TouchBeginPos).magnitude;
        distance = Mathf.Clamp(distance, 0, m_MaxSlideDistance);

        var percent = distance / m_MaxSlideDistance;
        m_PlayerView.SetForcePercent(percent);
    }

    void TouchEnd(Vector3 position)
    {
        OnFire(position - m_TouchBeginPos);
        m_PlayerView.HideForceEnergy();
    }

    void OnFire(Vector2 dir)
    {
        var touchDir = dir.normalized;
        touchDir.x = Mathf.Clamp(touchDir.x, -0.5f, 0.5f);

        var touchDistance = dir.magnitude;
        var touchPercent = Mathf.Clamp(touchDistance, 0, m_MaxSlideDistance) / m_MaxSlideDistance;

        var distance = maxFireDistance * touchPercent;
        var height = 2 + maxFireHeight * touchPercent;
        var destination = new Vector3(touchDir.x * distance, 2, touchDir.y * distance);

        Transform target;
        if (FindClosestSlot(destination, out target))
        {
            m_CurrntRing.Fire(target, height);
        }
        else
        {
            m_CurrntRing.Fire(destination, height);
        }

        m_IsReady = false;
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
        return minDistance < 3f;
    }

    #endregion
}
