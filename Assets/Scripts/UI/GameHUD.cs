using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private float    _matchDuration = 180f;

    private float _timeLeft;
    private bool  _running;

    public static GameHUD Instance { get; private set; }

    private void Awake()
    {
        Instance  = this;
        _timeLeft = _matchDuration;
    }

    public void StartTimer() => _running = true;
    public void StopTimer()  => _running = false;

    private void Update()
    {
        if (!_running) return;
        _timeLeft -= Time.deltaTime;
        if (_timeLeft < 0f) _timeLeft = 0f;
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (_timerText == null) return;
        int minutes = Mathf.FloorToInt(_timeLeft / 60f);
        int seconds  = Mathf.FloorToInt(_timeLeft % 60f);
        _timerText.text  = $"{minutes:00}:{seconds:00}";
        _timerText.color = _timeLeft < 30f
            ? new Color(1f, 0.227f, 0.227f)
            : new Color(0.910f, 0.910f, 0.941f);
    }
}
