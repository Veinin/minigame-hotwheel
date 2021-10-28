using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private Vector3 SpawnPos = new Vector3(0, 2.1f, -2);
    private Vector3 OriginPos = new Vector3(0, 2.1f, 1.3f);

    public Ring ringPrefab;
    public float maxFireDistance    = 200;
    public float maxHeightForce     = 500;
    public float maxForwardForce       = 2000;

    private HomeView    m_HomeView;
    private PlayerView  m_PlayerView;

    private Ring m_CurrntRing;
    private List<Ring>  m_RingList;

    private bool m_IsStarted;

    void Awake()
    {
        m_RingList = new List<Ring>();

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
        var ring = GameObject.Instantiate<Ring>(ringPrefab, SpawnPos, Quaternion.identity);
        ring.transform.DOMove(OriginPos, 0.5f).OnComplete(() => {
            m_IsStarted = true;
        });
        m_CurrntRing = ring;
        m_RingList.Add(ring);
    }

    void Update()
    {
        CheckSlide();
    }

    private Vector3 downPos;

    void CheckSlide()
    {
        if (!m_IsStarted)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            downPos = Input.mousePosition;
            m_PlayerView.ShowForceEnergy();
        }

        if (Input.GetMouseButton(0))
        {
            var distance = (Input.mousePosition - downPos).magnitude;
            distance = Mathf.Clamp(distance, 0, maxFireDistance);
            var percent = distance / maxFireDistance;
            m_PlayerView.SetForcePercent(percent);
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnFire(Input.mousePosition - downPos);
            m_PlayerView.HideForceEnergy();
        }
    }

    void OnFire(Vector2 dir)
    {
        var d = dir.normalized;
        var distance = dir.magnitude;
        var distancePercent = Mathf.Clamp(distance, 0, maxFireDistance) / maxFireDistance;
        var uForce = distancePercent * maxHeightForce;
        var dForce = distancePercent * maxForwardForce;
        var force = new Vector3(d.x * dForce, uForce, d.y * dForce);
        m_CurrntRing.Fire(force);
        
        SpawnRing();
    }

    #endregion
}
