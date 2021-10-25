using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class HomeView : MonoBehaviour
{
    public UnityAction OnStartAction;

    [Header("Money")]
    public Text rings;
    public Text points;
    public Text coins;

    [Header("Level")]
    public Text level;
    public Button start;

    [Header("Ohter")]
    public Button rankingList;
    public Button other;

    void Awake()
    {
        rings.text = "0";
        points.text = "0";
        coins.text = "0";

        level.text = "第 " + GameData.level + " 关（共5关）";
        start.onClick.AddListener(OnStart);
    }

    void OnStart()
    {
        var levelNode = level.transform.parent;
        var startNode = start.transform;
        var bottomNode = rankingList.transform.parent;

        levelNode.DOLocalMoveX(-1500, 0.5f);
        startNode.DOLocalMoveX(1500, 0.5f).OnComplete(() =>
        {
            if (OnStartAction != null)
            {
                OnStartAction();
            }
        });
        bottomNode.gameObject.SetActive(false);
    }

    public void Back()
    {
        var levelNode = level.transform.parent;
        var startNode = start.transform;
        var bottomNode = rankingList.transform.parent;

        levelNode.DOLocalMoveX(0, 0.5f);
        startNode.DOLocalMoveX(0, 0.5f);
        bottomNode.gameObject.SetActive(true);
    }
}
