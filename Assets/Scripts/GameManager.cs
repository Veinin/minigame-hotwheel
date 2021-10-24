using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private Vector3 SpawnPos = new Vector3(0, 2.1f, -2);
    private Vector3 OriginPos = new Vector3(0, 2.1f, 1.3f);

    public Ring ringPrefab;

    private HomeView    m_HomeView;
    private Joystick    m_Joystick;
    private PlayerView  m_PlayerView;

    private List<Ring>  m_RingList;

    void Awake()
    {
        m_RingList = new List<Ring>();

        var canvas = GameObject.Find("Canvas");
        m_Joystick = canvas.GetComponentInChildren<FixedJoystick>(true);
        m_Joystick.gameObject.SetActive(false);
        m_Joystick.OnPointUpAction += OnJoystickUp;
        m_Joystick.OnPointDownAction += OnJoystickDown;

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
        m_HomeView.Back();
        m_Joystick.gameObject.SetActive(false);
        m_PlayerView.gameObject.SetActive(false);
    }

    #region Game

    void StartGame()
    {
        m_PlayerView.gameObject.SetActive(true);
        SpawnRing(true);
    }

    void SpawnRing(bool isFirst)
    {
        var ring = GameObject.Instantiate<Ring>(ringPrefab, SpawnPos, Quaternion.identity);
        ring.transform.DOMove(OriginPos, 0.5f).OnComplete(() => {
            m_Joystick.gameObject.SetActive(true);
        });
        m_RingList.Add(ring);
    }

    void OnJoystickUp()
    {
        m_PlayerView.back.gameObject.SetActive(false);
    }

    void OnJoystickDown()
    {
        var direction = m_Joystick.Direction;
    }

    #endregion
}
