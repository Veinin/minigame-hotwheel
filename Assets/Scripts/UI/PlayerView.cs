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

    public void ShowForceEnergy()
    {
        back.gameObject.SetActive(false);
        energyBar.gameObject.SetActive(true);
    }

    public void HideForceEnergy()
    {
        back.gameObject.SetActive(true);
        energyBar.gameObject.SetActive(false);
    }

    public void SetForcePercent(float value)
    {
        energyBar.value = value;
        energyValue.text = Mathf.Floor(value * 100) + "%";
    }
}
