using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Victory / Defeat overlay.
/// P2 wins (персонаж): оранжевый акцент, текст "Персонаж добрался до выхода."
/// P1 wins (тетрис):   синий акцент,   текст "Персонаж погребён под блоками."
/// </summary>
public class VictoryPanel : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text   _titleText;      // ПОБЕДА!
    [SerializeField] private TMP_Text   _subtitleText;   // Player 1/2 Wins
    [SerializeField] private TMP_Text   _descriptionText;
    [SerializeField] private Image      _panelBorder;
    [SerializeField] private Image      _titleAccentLine;
    [SerializeField] private Button     _restartButton;
    [SerializeField] private Button     _menuButton;

    private static readonly Color ColorP1 = new Color(0.290f, 0.620f, 1f);    // #4A9EFF
    private static readonly Color ColorP2 = new Color(1f,     0.478f, 0.239f); // #FF7A3D

    private void Awake()
    {
        _panel?.SetActive(false);
        _restartButton?.onClick.AddListener(OnRestart);
        _menuButton?.onClick.AddListener(OnMenu);
    }

    public void ShowP1Wins()
    {
        Show(
            accentColor:  ColorP1,
            subtitle:     "Player 1 Wins",
            description:  "Персонаж погребён под блоками."
        );
    }

    public void ShowP2Wins()
    {
        Show(
            accentColor:  ColorP2,
            subtitle:     "Player 2 Wins",
            description:  "Персонаж добрался до выхода."
        );
    }

    private void Show(Color accentColor, string subtitle, string description)
    {
        _panel?.SetActive(true);

        if (_titleText      != null) _titleText.color      = accentColor;
        if (_subtitleText   != null) _subtitleText.text    = subtitle;
        if (_descriptionText != null) _descriptionText.text = description;
        if (_panelBorder    != null) _panelBorder.color    = accentColor;
        if (_titleAccentLine != null) _titleAccentLine.color = accentColor;

        // Slide-in снизу + fade
        if (_panel != null)
        {
            var rt = _panel.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -80f);
            rt.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);

            var cg = _panel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.25f).SetUpdate(true);
            }
        }
    }

    private void OnRestart() => GameManager.Instance?.Restart();

    private void OnMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.LocalDebug)
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        else
            Photon.Pun.PhotonNetwork.LeaveRoom();
    }
}
