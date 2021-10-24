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

    void Awake()
    {
        tips.text = string.Format(TipsFormat, 0, 5);
        back.onClick.AddListener(OnBack);
    }

    void OnBack()
    {
        if (OnBackAction != null)
        {
            OnBackAction();
        }
    }
}
