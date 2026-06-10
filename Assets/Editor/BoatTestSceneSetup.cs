#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using CrocHunter;

public static class BoatTestSceneSetup
{
    private const string ScenePath = "Assets/Scenes/BoatTestScene.unity";
    private const string PrefabsPath = "Assets/Prefabs";
    private const string SettingsPath = "Assets/Scripts/Boat/WorldScrollSettings.asset";
    private const string ShootableLayerName = "Shootable";

    [MenuItem("CrocHunter/Create Boat Test Scene")]
    public static void CreateScene()
    {
        int shootableLayer = LayerMask.NameToLayer(ShootableLayerName);
        if (shootableLayer == -1)
        {
            Debug.LogError(
                $"[CrocHunter] Layer '{ShootableLayerName}' not found. " +
                "Go to Edit → Project Settings → Tags and Layers and add a User Layer named 'Shootable', then run this menu item again."
            );
            return;
        }

        // Load the input actions asset
        string[] guids = AssetDatabase.FindAssets("t:InputActionAsset LightgunInputActions");
        if (guids.Length == 0)
        {
            Debug.LogError("[CrocHunter] LightgunInputActions asset not found. Make sure Assets/Input/LightgunInputActions.inputactions exists.");
            return;
        }
        var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));

        // Get or create the scroll settings asset
        WorldScrollSettings scrollSettings = GetOrCreateScrollSettings();

        // Ensure Prefabs folder exists
        if (!AssetDatabase.IsValidFolder(PrefabsPath))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // New empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ---- Materials ----
        Material waterMat = CreateURPMaterial("WaterMat", new Color(0.13f, 0.27f, 0.55f));
        Material bankMat  = CreateURPMaterial("BankMat",  new Color(0.47f, 0.36f, 0.17f));
        Material crocMat  = CreateURPMaterial("CrocMat",  new Color(0.24f, 0.45f, 0.20f));
        Material headMat  = CreateURPMaterial("HeadMat",  new Color(0.30f, 0.55f, 0.15f));
        Material fishMat  = CreateURPMaterial("FishMat",  new Color(0.75f, 0.85f, 0.35f));

        // ---- Camera ----
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 200f;
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<UniversalAdditionalCameraData>();
        camGO.transform.SetPositionAndRotation(new Vector3(0f, 1.5f, -8f), Quaternion.Euler(5f, 0f, 0f));

        // ---- Directional Light ----
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.shadows = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ---- Environment (scrolls) ----
        var envGO = new GameObject("Environment");
        var scroller = envGO.AddComponent<WorldScroller>();
        SetPrivateField(scroller, "settings", scrollSettings);

        var waterGO = CreateCube("WaterPlane", envGO.transform, new Vector3(0f, -0.05f, 45f), new Vector3(22f, 0.1f, 100f), waterMat);
        waterGO.GetComponent<BoxCollider>().enabled = false;

        CreateCube("LeftBank",  envGO.transform, new Vector3(-13f, 1.4f, 45f), new Vector3(5f, 3f, 100f), bankMat);
        CreateCube("RightBank", envGO.transform, new Vector3( 13f, 1.4f, 45f), new Vector3(5f, 3f, 100f), bankMat);

        // ---- GameManager ----
        var managerGO = new GameObject("GameManager");
        managerGO.AddComponent<PlayerHealth>();
        var spawner = managerGO.AddComponent<SpawnManager>();
        SetPrivateField(spawner, "settings", scrollSettings);
        SetPrivateField(spawner, "crocInterval", 4f);
        SetPrivateField(spawner, "fishInterval", 7f);
        SetPrivateField(spawner, "spawnZ", 20f);
        SetPrivateField(spawner, "riverHalfWidth", 3f);
        SetPrivateField(spawner, "crocSpawnY", -2f);
        SetPrivateField(spawner, "fishSpawnY", 0f);

        // ---- InputController ----
        var inputGO = new GameObject("InputController");
        var playerInput = inputGO.AddComponent<PlayerInput>();
        playerInput.actions = inputActions;
        playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        var aimCtrl = inputGO.AddComponent<AimController>();
        var shooter = inputGO.AddComponent<GameShooter>();
        SetPrivateField(shooter, "aimController", aimCtrl);
        SetPrivateField(shooter, "maxRayDistance", 100f);
        SetPrivateField(shooter, "shootableLayer", (LayerMask)(1 << shootableLayer));

        SetPrivateField(shooter, "maxAmmo", 2);
        SetPrivateField(shooter, "reloadTime", 1f);

        // Wire PlayerInput action events
        WirePlayerInputEvents(playerInput, shooter, aimCtrl);

        // ---- Canvas ----
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Health panel (top-left)
        var panelGO = new GameObject("HealthPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot     = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(20f, -20f);
        panelRect.sizeDelta = new Vector2(150f, 40f);
        var hLayout = panelGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 5f;
        hLayout.childForceExpandWidth  = false;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlWidth  = false;
        hLayout.childControlHeight = false;

        var chunkImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var chunkGO = new GameObject($"Chunk{i + 1}");
            chunkGO.transform.SetParent(panelGO.transform, false);
            var chunkRect = chunkGO.AddComponent<RectTransform>();
            chunkRect.sizeDelta = new Vector2(40f, 40f);
            var img = chunkGO.AddComponent<Image>();
            img.color = Color.green;
            chunkImages[i] = img;
        }

        // Damage flash (full screen, red, invisible)
        var flashGO = new GameObject("DamageFlash");
        flashGO.transform.SetParent(canvasGO.transform, false);
        var flashRect = flashGO.AddComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;
        var flashImg = flashGO.AddComponent<Image>();
        flashImg.color = new Color(1f, 0f, 0f, 0f);
        flashImg.raycastTarget = false;

        // Crosshair (small centred dot)
        var crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvasGO.transform, false);
        var crosshairRect = crosshairGO.AddComponent<RectTransform>();
        crosshairRect.sizeDelta = new Vector2(24f, 24f);
        var crosshairImg = crosshairGO.AddComponent<Image>();
        crosshairImg.color = Color.white;
        crosshairImg.raycastTarget = false;

        // Ammo panel (bottom-right) — 2 shell icons + reload label
        var ammoPanelGO = new GameObject("AmmoPanel");
        ammoPanelGO.transform.SetParent(canvasGO.transform, false);
        var ammoPanelRect = ammoPanelGO.AddComponent<RectTransform>();
        ammoPanelRect.anchorMin = new Vector2(1f, 0f);
        ammoPanelRect.anchorMax = new Vector2(1f, 0f);
        ammoPanelRect.pivot     = new Vector2(1f, 0f);
        ammoPanelRect.anchoredPosition = new Vector2(-20f, 20f);
        ammoPanelRect.sizeDelta = new Vector2(110f, 50f);
        var ammoLayout = ammoPanelGO.AddComponent<HorizontalLayoutGroup>();
        ammoLayout.spacing = 8f;
        ammoLayout.childForceExpandWidth  = false;
        ammoLayout.childForceExpandHeight = false;
        ammoLayout.childControlWidth  = false;
        ammoLayout.childControlHeight = false;
        ammoLayout.childAlignment = TextAnchor.MiddleRight;

        var shellImages = new Image[2];
        for (int i = 0; i < 2; i++)
        {
            var shellGO = new GameObject($"Shell{i + 1}");
            shellGO.transform.SetParent(ammoPanelGO.transform, false);
            var shellRect = shellGO.AddComponent<RectTransform>();
            shellRect.sizeDelta = new Vector2(30f, 46f);
            var shellImg = shellGO.AddComponent<Image>();
            shellImg.color = new Color(0.90f, 0.60f, 0.10f);
            shellImages[i] = shellImg;
        }

        // "RELOADING" text sits inside the ammo panel
        var reloadTextGO = new GameObject("ReloadText");
        reloadTextGO.transform.SetParent(ammoPanelGO.transform, false);
        var reloadTextRect = reloadTextGO.AddComponent<RectTransform>();
        reloadTextRect.sizeDelta = new Vector2(110f, 30f);
        var reloadTxt = reloadTextGO.AddComponent<Text>();
        reloadTxt.text = "RELOAD";
        reloadTxt.fontSize = 18;
        reloadTxt.fontStyle = FontStyle.Bold;
        reloadTxt.color = Color.red;
        reloadTxt.alignment = TextAnchor.MiddleCenter;
        reloadTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        reloadTextGO.SetActive(false);

        var ammoUI = canvasGO.AddComponent<AmmoUI>();
        SetPrivateField(ammoUI, "shooter", shooter);
        SetPrivateField(ammoUI, "shellImages", shellImages);
        SetPrivateField(ammoUI, "reloadText", reloadTxt);

        // HealthUI component on canvas root
        var healthUI = canvasGO.AddComponent<HealthUI>();
        SetPrivateField(healthUI, "chunkImages", chunkImages);
        SetPrivateField(healthUI, "damageFlashImage", flashImg);
        SetPrivateField(healthUI, "flashDuration", 0.3f);
        SetPrivateField(healthUI, "crosshairRect", crosshairRect);
        SetPrivateField(healthUI, "aimController", aimCtrl);
        SetPrivateField(healthUI, "canvas", canvas);

        // EventSystem (required for UI)
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ---- Prefabs ----
        var crocPrefab = BuildCrocPrefab(scrollSettings, crocMat, headMat, shootableLayer);
        var fishPrefab = BuildFishPrefab(scrollSettings, fishMat, shootableLayer);

        SetPrivateField(spawner, "crocPrefab", crocPrefab);
        SetPrivateField(spawner, "fishPrefab", fishPrefab);

        // ---- Save ----
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        Debug.Log(
            "[CrocHunter] BoatTestScene created at " + ScenePath + "\n" +
            "Open the scene, enter Play Mode, and follow the verification steps in the plan."
        );
    }

    // -------------------------------------------------------------------------

    private static WorldScrollSettings GetOrCreateScrollSettings()
    {
        WorldScrollSettings asset = AssetDatabase.LoadAssetAtPath<WorldScrollSettings>(SettingsPath);
        if (asset != null) return asset;

        asset = ScriptableObject.CreateInstance<WorldScrollSettings>();
        string dir = Path.GetDirectoryName(SettingsPath);
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder(Path.GetDirectoryName(dir), Path.GetFileName(dir));
        AssetDatabase.CreateAsset(asset, SettingsPath);
        AssetDatabase.SaveAssets();
        return asset;
    }

    private static Material CreateURPMaterial(string name, Color baseColor)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("URP/Lit");
        if (shader == null) shader = Shader.Find("Standard"); // fallback
        var mat = new Material(shader) { name = name };
        mat.SetColor("_BaseColor", baseColor);
        mat.color = baseColor;
        return mat;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    private static void WirePlayerInputEvents(PlayerInput playerInput, GameShooter shooter, AimController aimCtrl)
    {
        EditorUtility.SetDirty(playerInput);

        bool wiredFire   = false;
        bool wiredPoint  = false;
        bool wiredReload = false;

        foreach (var actionEvent in playerInput.actionEvents)
        {
            if (actionEvent.actionName.EndsWith("/Fire") && !wiredFire)
            {
                UnityEventTools.AddPersistentListener<InputAction.CallbackContext>(actionEvent, shooter.OnFire);
                wiredFire = true;
            }
            else if (actionEvent.actionName.EndsWith("/Point") && !wiredPoint)
            {
                UnityEventTools.AddPersistentListener<InputAction.CallbackContext>(actionEvent, aimCtrl.OnPoint);
                wiredPoint = true;
            }
            else if (actionEvent.actionName.EndsWith("/Reload") && !wiredReload)
            {
                UnityEventTools.AddPersistentListener<InputAction.CallbackContext>(actionEvent, shooter.OnReload);
                wiredReload = true;
            }
        }

        if (!wiredFire)
            Debug.LogWarning("[CrocHunter] Could not auto-wire 'Fire' event. Manually wire: PlayerInput → Events → Player → Fire → GameShooter.OnFire");
        if (!wiredPoint)
            Debug.LogWarning("[CrocHunter] Could not auto-wire 'Point' event. Manually wire: PlayerInput → Events → Player → Point → AimController.OnPoint");
        if (!wiredReload)
            Debug.LogWarning("[CrocHunter] Could not auto-wire 'Reload' event. Manually wire: PlayerInput → Events → Player → Reload → GameShooter.OnReload");

        EditorUtility.SetDirty(playerInput);
    }

    private static GameObject BuildCrocPrefab(WorldScrollSettings settings, Material bodyMat, Material headMat, int layer)
    {
        var root = new GameObject("Croc");
        var ctrl = root.AddComponent<CrocController>();
        SetPrivateField(ctrl, "settings", settings);
        SetPrivateField(ctrl, "emergeTime", 1.5f);
        SetPrivateField(ctrl, "warningTime", 2.5f);
        SetPrivateField(ctrl, "lungeSpeed", 10f);
        SetPrivateField(ctrl, "hiddenY", -2f);
        SetPrivateField(ctrl, "emergedY", 0f);

        // Body cube
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1.5f, 0.6f, 2f);
        body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;
        body.layer = layer;
        var bodyHitbox = body.AddComponent<CrocHitbox>();
        SetPrivateField(bodyHitbox, "controller", ctrl);
        SetPrivateField(bodyHitbox, "isHead", false);

        // Head cube
        var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.55f, 0.3f);
        head.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        head.GetComponent<MeshRenderer>().sharedMaterial = headMat;
        head.layer = layer;
        var headHitbox = head.AddComponent<CrocHitbox>();
        SetPrivateField(headHitbox, "controller", ctrl);
        SetPrivateField(headHitbox, "isHead", true);

        string prefabPath = PrefabsPath + "/CrocPrefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject BuildFishPrefab(WorldScrollSettings settings, Material mat, int layer)
    {
        var fishGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fishGO.name = "Fish";
        fishGO.transform.localScale = new Vector3(0.4f, 0.2f, 0.6f);
        fishGO.GetComponent<MeshRenderer>().sharedMaterial = mat;
        fishGO.layer = layer;
        var fish = fishGO.AddComponent<BarramundiFish>();
        SetPrivateField(fish, "settings", settings);
        SetPrivateField(fish, "arcHeight", 3f);
        SetPrivateField(fish, "arcDuration", 2.5f);

        string prefabPath = PrefabsPath + "/FishPrefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(fishGO, prefabPath);
        Object.DestroyImmediate(fishGO);
        return prefab;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }
            type = type.BaseType;
        }
        Debug.LogWarning($"[CrocHunter] Field '{fieldName}' not found on {target.GetType().Name}");
    }
}
#endif
