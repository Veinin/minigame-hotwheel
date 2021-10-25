using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerView : MonoBehaviour
{
    private static string TipsFormat = "全部套完通关：{0}/{1}";

    [HideInInspector]
    public UnityAction OnBackAction;

    public Text tips;
    public Button back;
    public Slider energyBar;
    public Text energyValue;

    private bool    m_IsFocos;
    private float   m_Energy;

    void Awake()
    {
        tips.text = string.Format(TipsFormat, 0, 5);
        energyBar.gameObject.SetActive(false);
        back.onClick.AddListener(OnBack);
    }

    void OnBack()
    {
        if (OnBackAction != null)
        {
            OnBackAction();
        }
    }

    public int Energy
    {
        get 
        {
            return (int) m_Energy;
        }
    }

    public void StartFocoEnergia()
    {
        m_Energy = 0;
        m_IsFocos = true;
        energyBar.gameObject.SetActive(true);
    }

    public void StopFocoEnergia()
    {
        m_IsFocos = false;
        energyBar.gameObject.SetActive(false);
    }

    void FixedUpdate() 
    {
        if (m_IsFocos || m_Energy > 100)
        {
            return;
        }

        m_Energy += Time.fixedDeltaTime * 5;
        energyValue.text = Mathf.Min(100, Mathf.Floor(m_Energy)).ToString();
    }
}
