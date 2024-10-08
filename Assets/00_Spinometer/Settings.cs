using System.Collections.Generic;
using GetBack.Spinometer.SpinalAlignmentCore;
using UnityEngine;
using UnityEngine.UIElements;

namespace GetBack.Spinometer
{
  [CreateAssetMenu(fileName = "settingsInstance", menuName = "ScriptableObjects/Settings", order = 1)]
  public class Settings : ScriptableObject
  {
    public SpinalAlignmentEstimator.Options opt_spinalAlignmentEstimatorOptions; 

    public string opt_webCamDeviceName;
    public List<string> opt_webCamDeviceNameList;
    public int opt_webCamDeviceNameIndex;

    public float opt_distance_correction_angle_distance_factor = 0f;
    public float opt_distance_correction_cos_factor = 64f;

    /// <summary>
    ///   Angle of the display surface from the front vector (based on the user) in degrees.
    ///   0 means it is completely flatten away and 90 indicates it is right up.
    /// </summary>
    public float opt_displaySurfaceAngle;

    /// <summary>
    /// </summary>
    public float opt_additionalPitchOffset;

    /// <summary>
    ///   The display diagonal field of view in degrees.
    /// </summary>
    public float opt_displayDiagonalFov;

    [Range(1, 30)] public int opt_poseEstimationFrequency = 4; // per sec

    [Range(1, 240)] public int opt_targetFrameRate = 120;

    [Range(1, 30)] public int opt_extra_updateFrequency = 2;

    public int opt_difficulty_textLength = 0;
    public int opt_difficulty_exposingDuration = 0;
    public int opt_difficulty_motion = 0;
    public int opt_difficulty_textSize = 0;

    public float timeMultiplier
    {
      get => 4.0f / (1 + opt_difficulty_exposingDuration);
    }

    public float textLengthMultiplier
    {
      get => (1 + opt_difficulty_exposingDuration) / 4.0f;
    }

    public float velocityMultiplier
    {
      get => (1 + opt_difficulty_motion) / 4.0f;
    }

    public float textSizeMultiplier
    {
      get => 4.0f / (1 + opt_difficulty_textSize);
    }


    public void Awake()
    {
      opt_webCamDeviceNameList.Clear();
      opt_webCamDeviceNameIndex = -1;
      RevertSettings();
    }

    public void RegisterSettingsUIEventHandler()
    {
      var uidoc = GameObject.Find("/SettingsUIDocument").GetComponent<UIDocument>();
      var btnSettingsSave = uidoc.rootVisualElement.Q<Button>("btn-settings-save");
      var btnSettingsRevert = uidoc.rootVisualElement.Q<Button>("btn-settings-revert");
      btnSettingsSave.clicked += SaveSettings;
      btnSettingsRevert.clicked += RevertSettings;
    }

    private void RevertSettings()
    {
      Debug.Log("Settings#RevertSettings(): reverting settings...");
      LoadSettings();
      if (!string.IsNullOrEmpty(opt_webCamDeviceName)) {
        if (!opt_webCamDeviceNameList.Exists(name => name == opt_webCamDeviceName))
          opt_webCamDeviceNameList.Add(opt_webCamDeviceName);
        opt_webCamDeviceNameIndex = opt_webCamDeviceNameList.FindIndex(name => name == opt_webCamDeviceName);
      }

      Debug.Log("Settings#RevertSettings(): done.");
    }

    public void LoadSettings()
    {
      Debug.Log("Settings#LoadSettings(): loading settings...");
      opt_spinalAlignmentEstimatorOptions = JsonUtility.FromJson<SpinalAlignmentEstimator.Options>(PlayerPrefs.GetString("opt_spinalAlignmentEstimatorOptions", "{}"));
      opt_webCamDeviceName = PlayerPrefs.GetString("opt_webCamDeviceName", null);
      opt_displaySurfaceAngle = PlayerPrefs.GetFloat("opt_displaySurfaceAngle", 65f);
      opt_additionalPitchOffset = PlayerPrefs.GetFloat("opt_additionalPitchOffset", 15f);
      opt_displayDiagonalFov = PlayerPrefs.GetFloat("opt_displayDiagonalFov", 56f);
      opt_targetFrameRate = PlayerPrefs.GetInt("opt_targetFrameRate", 15);
      opt_poseEstimationFrequency = PlayerPrefs.GetInt("opt_poseEstimationFrequency", 15);
      opt_extra_updateFrequency = PlayerPrefs.GetInt("opt_extra_updateFrequency", 2);
      opt_difficulty_textLength = PlayerPrefs.GetInt("opt_difficulty_textLength", 0);
      opt_difficulty_exposingDuration = PlayerPrefs.GetInt("opt_difficulty_exposingDuration", 0);
      opt_difficulty_motion = PlayerPrefs.GetInt("opt_difficulty_motion", 0);
      opt_difficulty_textSize = PlayerPrefs.GetInt("opt_difficulty_textSize", 0);

      Debug.Log("difficulty:");
      Debug.Log($"  timeMultiplier = {timeMultiplier}");
      Debug.Log($"  textLengthMultiplier = {textLengthMultiplier}");
      Debug.Log($"  velocityMultiplier = {velocityMultiplier}");
      Debug.Log($"  textSizeMultiplier = {textSizeMultiplier}");

      Debug.Log("Settings#LoadSettings(): done.");
    }

    public void SaveSettings()
    {
      Debug.Log("Settings#SaveSettings(): saving settings...");
      PlayerPrefs.SetString("opt_webCamDeviceName", opt_webCamDeviceName);
      PlayerPrefs.SetFloat("opt_displaySurfaceAngle", opt_displaySurfaceAngle);
      PlayerPrefs.SetFloat("opt_additionalPitchOffset", opt_additionalPitchOffset);
      PlayerPrefs.SetFloat("opt_displayDiagonalFov", opt_displayDiagonalFov);
      PlayerPrefs.SetString("opt_spinalAlignmentEstimatorOptions", JsonUtility.ToJson(opt_spinalAlignmentEstimatorOptions));
      PlayerPrefs.SetInt("opt_poseEstimationFrequency", opt_poseEstimationFrequency);
      PlayerPrefs.SetInt("opt_extra_updateFrequency", opt_extra_updateFrequency);
      PlayerPrefs.SetInt("opt_difficulty_textLength", opt_difficulty_textLength);
      PlayerPrefs.SetInt("opt_difficulty_exposingDuration", opt_difficulty_exposingDuration);
      PlayerPrefs.SetInt("opt_difficulty_motion", opt_difficulty_motion);
      PlayerPrefs.SetInt("opt_difficulty_textSize", opt_difficulty_textSize);
      Debug.Log("Settings#SaveSettings(): done.");
    }
  }
}
