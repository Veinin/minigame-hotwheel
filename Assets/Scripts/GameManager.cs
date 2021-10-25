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

    private Ring m_CurrntRing;
    private List<Ring>  m_RingList;

    void Awake()
    {
        m_RingList = new List<Ring>();

        var canvas = GameObject.Find("Canvas");
        m_Joystick = canvas.GetComponentInChildren<FixedJoystick>(true);
        m_Joystick.gameObject.SetActive(false);
        m_Joystick.OnPointUpAction += OnJoystickUp;
        m_Joystick.OnPointDownAction += OnJoystickDown;
        m_Joystick.OnPointDragAction += OnPointDragAction;

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
        SpawnRing();
    }

    void SpawnRing()
    {
        var ring = GameObject.Instantiate<Ring>(ringPrefab, SpawnPos, Quaternion.identity);
        ring.transform.DOMove(OriginPos, 0.5f).OnComplete(() => {
            m_Joystick.gameObject.SetActive(true);
        });
        m_CurrntRing = ring;
        m_RingList.Add(ring);
    }

    void OnJoystickUp()
    {
        m_PlayerView.back.gameObject.SetActive(false);
    }

    void OnPointDragAction(float value)
    {
        if (value >= 0.99)
        {
            m_PlayerView.StartFocoEnergia();
        }
        else
        {
            m_PlayerView.StopFocoEnergia();
        }
    }

    void OnJoystickDown()
    {
        var baseForce = 2000f;
        var baseHeight = 0.25f;

        var energy = m_PlayerView.Energy;
        if (energy > 0)
        {
            var incr = 1 + energy*1f/100;
            baseForce = baseForce * incr * 0.5f;
            baseHeight = baseHeight * incr * 0.5f;
        }

        var direction = m_Joystick.Direction;
        m_CurrntRing.Fire(new Vector3(-direction.x, baseHeight, -direction.y) * baseForce);

        m_PlayerView.StopFocoEnergia();
        SpawnRing();
    }

    #endregion
}
