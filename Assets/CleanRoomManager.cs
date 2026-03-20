using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CleanRoomManager : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem ambientDust;
    [SerializeField] private ParticleSystem doorDust;

    [Header("Wind Settings")]
    [SerializeField] private float minWind = 0f;
    [SerializeField] private float maxWind = 10f;
    [SerializeField] private float defaultWind = 3f;

    [Header("UI")]
    [SerializeField] private Slider windSlider;
    [SerializeField] private Text windLabel;
    [SerializeField] private Text doorLabel;

    private float _wind;
    private bool _doorOpen = false;
    private ParticleSystem.ForceOverLifetimeModule _ambientForce;
    private ParticleSystem.ForceOverLifetimeModule _doorForce;

    void Start()
    {
        _ambientForce = ambientDust.forceOverLifetime;
        _ambientForce.enabled = true;

        _doorForce = doorDust.forceOverLifetime;
        _doorForce.enabled = true;

        // Door starts closed
        var e = doorDust.emission;
        e.enabled = false;

        SetWind(defaultWind);

        if (windSlider != null)
            windSlider.SetValueWithoutNotify(Mathf.InverseLerp(minWind, maxWind, defaultWind));

        UpdateUI();
    }

       public void OnSliderChanged(float t)
    {
        SetWind(Mathf.Lerp(minWind, maxWind, t));
    }

    public void IncreaseWind() => SetWind(_wind + 1f);
    public void DecreaseWind() => SetWind(_wind - 1f);

    public void ToggleDoor()
    {
        _doorOpen = !_doorOpen;
        var e = doorDust.emission;
        e.enabled = _doorOpen;
        UpdateUI();
    }

    private void SetWind(float force)
    {
        _wind = Mathf.Clamp(force, minWind, maxWind);
        ApplyWind(ref _ambientForce);
        ApplyWind(ref _doorForce);
        UpdateUI();
    }

    private void ApplyWind(ref ParticleSystem.ForceOverLifetimeModule m)
    {
        Vector3 v = Vector3.down * _wind;
        m.x = v.x;
        m.y = v.y;
        m.z = v.z;
    }

    private void UpdateUI()
    {
        if (windLabel != null)
            windLabel.text = $"Wind: {_wind:F1}";
        if (doorLabel != null)
            doorLabel.text = _doorOpen ? "Door: OPEN" : "Door: CLOSED";
        if (windSlider != null)
            windSlider.SetValueWithoutNotify(Mathf.InverseLerp(minWind, maxWind, _wind));
    }
}