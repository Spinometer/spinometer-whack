using DG.Tweening;
using GetBack.Spinometer.Screens.WhackGame;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace GetBack.Spinometer
{

  public class App : MonoBehaviour
  {
    public enum State {
      Disclaimer,
      Running
    };

    [SerializeField] private UiDataSource _uiDataSource;
    [SerializeField] private Settings _settings;

    private string _sceneName_debug = "Debug";
    private string _sceneName_extra = "Extra";
    private string _sceneName_disclaimer = "Disclaimer";
    private string _sceneName_settings = "Settings";
    private string _sceneName_easySetupCamera = "EasySetupCamera";
    private string _sceneName_easySetupAngle = "EasySetupAngle";
    private string _sceneName_easySetupDistance = "EasySetupDistance";
    private string _sceneName_spinometer = "Spinometer";
    private string _sceneName_whackGame = "WhackGame";
    private string _sceneName_whackResult = "WhackResult";

    private State _state;
    private bool _settingsInitialized = false;

    // TODO: move to dedicated class
    public bool InitialSetupAlreadyCompleted {
      get => PlayerPrefs.GetInt("opt_initial_setup_already_completed", 0) != 0;
      set { PlayerPrefs.SetInt("opt_initial_setup_already_completed", value ? 1 : 0); }
    }

    void Awake()
    {
      DOTween.Init();
    }

    void Start()
    {
      ChangeLocale("en");
      _state = State.Disclaimer;
#if UNITY_EDITOR
      if (SceneLoaded(_sceneName_whackGame)) {
        _state = State.Running;
        LoadWhackGameScene(); // initialize already loaded scene
      }
      if (SceneLoaded(_sceneName_whackResult)) {
        _state = State.Running;
        LoadWhackResultScene(); // initialize already loaded scene
      }
#endif
      if (_state != State.Disclaimer)
        CloseDisclaimerScene();
      else
        LoadDisclaimerScene();
    }

    private bool SceneLoaded(string sceneName)
    {
      var scene = SceneManager.GetSceneByName(sceneName);
      return scene != null && scene.isLoaded;
    }

    void Update()
    {
      if (!_uiDataSource || !_settings)
        return;
      if (!_settingsInitialized) {
        _settings.Awake();
        _settingsInitialized = true;
      }
      Application.targetFrameRate = _settings.opt_targetFrameRate;
#if !UNITY_EDITOR
    int vSyncCount = 60 / _settings.opt_targetFrameRate;
    QualitySettings.vSyncCount = (int)Mathf.Clamp(vSyncCount, 1, 4);
#endif

#if false
      if (Keyboard.current.dKey.wasPressedThisFrame)
        ToggleDebugUI();
      if (Keyboard.current.eKey.wasPressedThisFrame)
        ToggleExtraUI();
#endif
    }

    public void MoveFromGameToResult()
    {
      CloseScene(_sceneName_whackGame);
      LoadWhackResultScene();
    }

    public void MoveFromResultToGame()
    {
      CloseScene(_sceneName_whackResult);
      LoadWhackGameScene();
    }

    private async void ToggleDebugUI()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_debug);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_debug, LoadSceneMode.Additive);
      } else {
        await SceneManager.UnloadSceneAsync(scene);
      }
    }

    private async void ToggleExtraUI()
    {
      var scene = SceneManager.GetSceneByName(_sceneName_extra);
      if (scene == null || !scene.isLoaded) {
        await SceneManager.LoadSceneAsync(_sceneName_extra, LoadSceneMode.Additive);
        GameObject.Find("/SK_Skeleton/FaceProxyOrigin/CameraOrigin/FaceProxy/Cube").SetActive(true);
      } else {
        await SceneManager.UnloadSceneAsync(scene);
        GameObject.Find("/SK_Skeleton/FaceProxyOrigin/CameraOrigin/FaceProxy/Cube").SetActive(false);
      }
    }

    private async void LoadDisclaimerScene()
    {
      if (!SceneLoaded(_sceneName_disclaimer)) {
        await SceneManager.LoadSceneAsync(_sceneName_disclaimer, LoadSceneMode.Additive);
      }

      var uidoc = GameObject.Find("/DisclaimerUIDocument").GetComponent<UIDocument>();
      uidoc.rootVisualElement.style.opacity = 0f;
      RegisterLocaleChangeButtonEvents(uidoc);
      var btnOk = uidoc.rootVisualElement.Q<Button>("btn-disclaimer-ok");
      btnOk.style.opacity = 0f;
      var tw = DOTween.To(() => uidoc.rootVisualElement.style.opacity.value,
                          x => uidoc.rootVisualElement.style.opacity = x, 1.0f, 2.0f);
      tw.onComplete += (() =>
          {
            btnOk.clicked += CloseDisclaimerScene;
            btnOk.style.opacity = 1f;
          }
        );
      tw.Play();
    }

    private async void CloseDisclaimerScene()
    {
      Debug.Log("Close");
      var uidoc = GameObject.Find("/DisclaimerUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        var btnOk = uidoc.rootVisualElement.Q<Button>();
        btnOk.clicked += CloseDisclaimerScene;
      }

      if (!SceneLoaded(_sceneName_whackGame) && !SceneLoaded(_sceneName_whackResult))
        LoadWhackGameScene();

      var scene = SceneManager.GetSceneByName(_sceneName_disclaimer);
      if (scene != null && scene.isLoaded) {
        Debug.Log("Unload");
        SceneManager.UnloadSceneAsync(scene);
      }

      if (!InitialSetupAlreadyCompleted)
        LoadSettingsOrEasySetupScene();
    }

    private async void LoadSpinometerScene()
    {
      if (!SceneLoaded(_sceneName_spinometer)) {
        await SceneManager.LoadSceneAsync(_sceneName_spinometer, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/SpinometerUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("settings");
          btn.clicked += LoadSettingsOrEasySetupScene;
        }
        RegisterLocaleChangeButtonEvents(uidoc);
      }
      ToggleExtraUI();
    }

    private async void LoadWhackGameScene()
    {
      if (!SceneLoaded(_sceneName_whackGame)) {
        await SceneManager.LoadSceneAsync(_sceneName_whackGame, LoadSceneMode.Additive);
      }
      GameObject.Find("/WhackGame").GetComponent<WhackGame>().app = this;
      var uidoc = GameObject.Find("/WhackGameUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("settings");
          btn.clicked += LoadSettingsOrEasySetupScene;
        }
        RegisterLocaleChangeButtonEvents(uidoc);
      }
      //ToggleExtraUI();
    }

    private async void LoadWhackResultScene()
    {
      if (!SceneLoaded(_sceneName_whackResult)) {
        await SceneManager.LoadSceneAsync(_sceneName_whackResult, LoadSceneMode.Additive);
      }
      //GameObject.Find("/WhackResult")?.GetComponent<WhackResult>().app = this;
      var uidoc = GameObject.Find("/WhackResultUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("settings");
          btn.clicked += LoadSettingsOrEasySetupScene;
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-retry");
          btn.clicked += MoveFromResultToGame;
        }
        RegisterLocaleChangeButtonEvents(uidoc);
      }
      //ToggleExtraUI();
    }

    private static void RegisterLocaleChangeButtonEvents(UIDocument uidoc)
    {
      {
        var btn = uidoc.rootVisualElement.Q<Button>("change-locale-en");
        if (btn != null) {
          btn.clicked += () => ChangeLocale("en");
        }
      }
      {
        var btn = uidoc.rootVisualElement.Q<Button>("change-locale-ja");
        if (btn != null) {
          btn.clicked += () => ChangeLocale("ja");
        }
      }
    }

    private static void ChangeLocale(string localeName)
    {
      if (localeName != "ja")
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
      else
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
    }

    private void LoadSettingsOrEasySetupScene()
    {
      if (Keyboard.current.aKey.isPressed)
        LoadSettingsScene();
      else
        LoadEasySetupCameraScene();
    }

    private async void LoadSettingsScene()
    {
      if (!SceneLoaded(_sceneName_settings)) {
        await SceneManager.LoadSceneAsync(_sceneName_settings, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/SettingsUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        var btnOk = uidoc.rootVisualElement.Q<Button>("btn-settings-close");
        btnOk.clicked += () => CloseScene(_sceneName_settings);
        RegisterLocaleChangeButtonEvents(uidoc);
      }
    }

    private void CloseScene(string sceneName)
    {
      var scene = SceneManager.GetSceneByName(sceneName);
      if (scene != null && scene.isLoaded) {
        SceneManager.UnloadSceneAsync(scene);
      }
    }

    private async void LoadEasySetupCameraScene()
    {
      if (!SceneLoaded(_sceneName_easySetupCamera)) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupCamera, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupCameraUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-close");
          btn.clicked += () => CloseScene(_sceneName_easySetupCamera);
          RegisterLocaleChangeButtonEvents(uidoc);
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-next");
          btn.clicked += () => {
            CloseScene(_sceneName_easySetupCamera);
            LoadEasySetupAngleScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }

    private async void LoadEasySetupAngleScene()
    {
      if (!SceneLoaded(_sceneName_easySetupAngle)) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupAngle, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupAngleUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-back");
          btn.clicked += () => {
            CloseScene(_sceneName_easySetupAngle);
            LoadEasySetupCameraScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-next");
          btn.clicked += () => {
            var tracker = GameObject.Find("/Tracker").GetComponent<TrackerNeuralNet>();
            tracker?.CalibrateAngle();
            CloseScene(_sceneName_easySetupAngle);
            LoadEasySetupDistanceScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }
    
    private async void LoadEasySetupDistanceScene()
    {
      if (!SceneLoaded(_sceneName_easySetupDistance)) {
        await SceneManager.LoadSceneAsync(_sceneName_easySetupDistance, LoadSceneMode.Additive);
      }
      var uidoc = GameObject.Find("/EasySetupDistanceUIDocument")?.GetComponent<UIDocument>();
      if (uidoc != null) {
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-back");
          btn.clicked += () => {
            CloseScene(_sceneName_easySetupDistance);
            LoadEasySetupAngleScene();
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
        {
          var btn = uidoc.rootVisualElement.Q<Button>("btn-easy-setup-finish");
          btn.clicked += () =>
          {
            var tracker = GameObject.Find("/Tracker").GetComponent<TrackerNeuralNet>();
            tracker?.CalibrateDistance();
            CloseScene(_sceneName_easySetupDistance);
            _settings.SaveSettings();
            InitialSetupAlreadyCompleted = true;
          };
          RegisterLocaleChangeButtonEvents(uidoc);
        }
      }
    }
  }

}
