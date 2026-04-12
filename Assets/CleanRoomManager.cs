using UnityEngine;
using UnityEngine.UI;

public class CleanRoomManager : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem ambientDust;
    [SerializeField] private ParticleSystem ambientDustUp;
    [SerializeField] private ParticleSystem doorDust;

    [Header("Wind Settings")]
    [SerializeField] private float minWind = 0f;
    [SerializeField] private float maxWind = 10f;
    [SerializeField] private float defaultWind = 3f;

    [Header("Turbulence (random float)")]
    [SerializeField] private float turbulenceStrength = 0.3f; // how much random drift

    [Header("UI")]
    [SerializeField] private Slider windSlider;
    [SerializeField] private Text windLabel;
    [SerializeField] private Text doorLabel;
    [SerializeField] private Text aqiLabel;   // Air Quality Index display

    private float _wind;
    private bool _doorOpen = false;
    private ParticleSystem.ForceOverLifetimeModule _ambientForce;
    private ParticleSystem.ForceOverLifetimeModule _ambientUpForce;
    private ParticleSystem.ForceOverLifetimeModule _doorForce;

    void Start()
    {
        _ambientForce = ambientDust.forceOverLifetime;
        _ambientForce.enabled = true;

        _ambientUpForce = ambientDustUp.forceOverLifetime;
        _ambientUpForce.enabled = true;

        _doorForce = doorDust.forceOverLifetime;
        _doorForce.enabled = true;

        var e = doorDust.emission;
        e.enabled = false;

        SetWind(defaultWind);

        if (windSlider != null)
            windSlider.SetValueWithoutNotify(Mathf.InverseLerp(minWind, maxWind, defaultWind));

        UpdateUI();
    }

    // ── Wind Controls ────────────────────────────────────────────────────────

    public void OnSliderChanged(float t)
    {
        SetWind(Mathf.Lerp(minWind, maxWind, t));
    }

    public void IncreaseWind() => SetWind(_wind + 1f);
    public void DecreaseWind() => SetWind(_wind - 1f);

    private void SetWind(float force)
    {
        _wind = Mathf.Clamp(force, minWind, maxWind);
        ApplyWind(ref _ambientForce);
        ApplyWind(ref _ambientUpForce);

        // Re-fetch doorForce every time to avoid stale struct cache
        _doorForce = doorDust.forceOverLifetime;
        ApplyWind(ref _doorForce);
        UpdateUI();
    }

    private void ApplyWind(ref ParticleSystem.ForceOverLifetimeModule m)
    {
        // Downward force from wind
        m.y = -_wind;

        // Random turbulence: MinMaxCurve with two constant values = random between them per particle
        m.x = new ParticleSystem.MinMaxCurve(-turbulenceStrength, turbulenceStrength);
        m.z = new ParticleSystem.MinMaxCurve(-turbulenceStrength, turbulenceStrength);

        // Must set mode to TwoConstants for random range to work
        ParticleSystem.MinMaxCurve xCurve = m.x;
        xCurve.mode = ParticleSystemCurveMode.TwoConstants;
        m.x = xCurve;

        ParticleSystem.MinMaxCurve zCurve = m.z;
        zCurve.mode = ParticleSystemCurveMode.TwoConstants;
        m.z = zCurve;
    }

    // ── Door Toggle ──────────────────────────────────────────────────────────

    public void ToggleDoor()
    {
        _doorOpen = !_doorOpen;
        var e = doorDust.emission;
        e.enabled = _doorOpen;
        UpdateUI();
    }

    // ── UI ───────────────────────────────────────────────────────────────────

    private void UpdateUI()
    {
        if (windLabel != null)
            windLabel.text = $"Wind: {_wind:F1}";

        if (doorLabel != null)
            doorLabel.text = _doorOpen ? "Door: OPEN" : "Door: CLOSED";

        if (aqiLabel != null)
            aqiLabel.text = GetAQIText();

        if (windSlider != null)
            windSlider.SetValueWithoutNotify(Mathf.InverseLerp(minWind, maxWind, _wind));
    }

    // ── Air Quality Index ────────────────────────────────────────────────────

    /// <summary>
    /// Simple AQI based on particle count and door state.
    /// Real clean rooms use ISO 14644 classifications (ISO 1-9).
    /// We simulate: higher wind = fewer particles = better AQI.
    /// </summary>
    private string GetAQIText()
    {
        // Base score: high wind = cleaner air
        float score = _wind / maxWind; // 0.0 to 1.0

        // Door open = contamination penalty
        if (_doorOpen) score -= 0.4f;

        score = Mathf.Clamp01(score);

        if (score > 0.7f) return "Air Quality: CLEAN ✓";
        if (score > 0.4f) return "Air Quality: MODERATE ⚠";
        return "Air Quality: CONTAMINATED ✗";
    }
}