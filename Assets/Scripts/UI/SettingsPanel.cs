using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Настройки: громкость музыки и звуков.
/// </summary>
public class SettingsPanel : UIPanel
{
    [Header("Audio")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Navigation")]
    [SerializeField] private Button _backButton;

    private const string MusicKey = "MusicVolume";
    private const string SfxKey   = "SfxVolume";

    private void Start()
    {
        _backButton.onClick.AddListener(OnBackClicked);

        _musicSlider.value = PlayerPrefs.GetFloat(MusicKey, 1f);
        _sfxSlider.value   = PlayerPrefs.GetFloat(SfxKey,   1f);

        _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    private void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat(MusicKey, value);
    }

    private void OnSfxChanged(float value)
    {
        PlayerPrefs.SetFloat(SfxKey, value);
    }

    private void OnBackClicked()
    {
        UIManager.Instance.ShowMainMenu();
    }
}
