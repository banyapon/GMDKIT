using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class OpenIQGMDKITEditor : EditorWindow
{
    private const string TitleScenePath = "Assets/Scenes/Title.unity";
    private const string GameScenePath = "Assets/Scenes/Game.unity";
    private const string GameOverScenePath = "Assets/Scenes/GameOver.unity";
    private const string BulletPrefabPath = "Assets/Resources/bullet/bullet.prefab";
    private const string EnemyPrefabPath = "Assets/Resources/enemy/enemy.prefab";
    private const string BossPrefabPath = "Assets/Resources/boss/boss.prefab";
    private const string ObstaclePrefabPath = "Assets/Resources/obstacle/obstacle.prefab";
    private const string LogoPath = "Assets/Editor/logo.png";

    private Texture2D logo;

    [MenuItem("OpenIQ GMDKIT/Open Panel")]
    public static void ShowWindow()
    {
        var window = GetWindow<OpenIQGMDKITEditor>("OpenIQ GMDKIT");
        window.minSize = new Vector2(360f, 420f);
    }

    private void OnEnable()
    {
        logo = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
    }

    private void OnGUI()
    {
        GUILayout.Space(12f);

        if (logo != null)
        {
            float width = Mathf.Min(position.width - 24f, 320f);
            Rect rect = GUILayoutUtility.GetRect(width, 96f, GUILayout.ExpandWidth(false));
            rect.x = (position.width - width) * 0.5f;
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
        }

        GUILayout.Space(8f);
        EditorGUILayout.LabelField("SpaceShip", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Create starter scenes, prefabs, tags, and gameplay objects for a simple spaceship shooter.", MessageType.Info);

        if (GUILayout.Button("Add Title Scene", GUILayout.Height(34f)))
        {
            AddTitleScene();
        }

        if (GUILayout.Button("Config", GUILayout.Height(34f)))
        {
            ApplyConfig();
        }

        if (GUILayout.Button("Add Player", GUILayout.Height(34f)))
        {
            AddPlayer();
        }

        if (GUILayout.Button("Add UI", GUILayout.Height(34f)))
        {
            AddUI();
        }

        if (GUILayout.Button("Add GameLogic", GUILayout.Height(34f)))
        {
            AddGameLogic();
        }

        if (GUILayout.Button("Add Moving Grounds", GUILayout.Height(34f)))
        {
            AddMovingGrounds();
        }

        if (GUILayout.Button("Add SpawnPoint", GUILayout.Height(34f)))
        {
            AddSpawnPoint();
        }

        if (GUILayout.Button("Add Enemy", GUILayout.Height(34f)))
        {
            AddEnemy();
        }

        if (GUILayout.Button("Add Boss", GUILayout.Height(34f)))
        {
            AddBoss();
        }

        if (GUILayout.Button("Add Obstacle", GUILayout.Height(34f)))
        {
            AddObstacle();
        }
    }

    private static void AddTitleScene()
    {
        EnsureFolder("Assets/Scenes");
        EnsureRequiredTags();
        EnsureInputHandlingIsCompatible();

        bool anyExists = File.Exists(TitleScenePath) || File.Exists(GameScenePath) || File.Exists(GameOverScenePath);
        if (anyExists)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "OpenIQ GMDKIT",
                "One or more scenes (Title, Game, GameOver) already exist. Overwrite them?\n\nChoose Cancel to add existing scenes to Build Settings without changes.",
                "Overwrite",
                "Cancel");

            if (!overwrite)
            {
                AddScenesToBuildSettings(TitleScenePath, GameScenePath, GameOverScenePath);
                if (File.Exists(TitleScenePath))
                    EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
                EditorUtility.DisplayDialog("OpenIQ GMDKIT", "Existing scenes added to Build Settings without overwriting.", "OK");
                return;
            }
        }

        // Save unsaved changes in currently open scenes before switching (returns false if user cancels)
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene titleScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        titleScene.name = "Title";
        EnsureEventSystem();
        CreateTitleCanvas(titleScene);
        EditorSceneManager.SaveScene(titleScene, TitleScenePath);

        Scene gameScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        gameScene.name = "Game";
        EnsureGameplayCamera();
        EditorSceneManager.SaveScene(gameScene, GameScenePath);

        CreateGameOverScene();
        AddScenesToBuildSettings(TitleScenePath, GameScenePath, GameOverScenePath);
        EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);
        EditorUtility.DisplayDialog("OpenIQ GMDKIT", "Created Title, Game, and GameOver scenes, then added them to Build Settings.", "OK");
    }

    private static void ApplyConfig()
    {
        EnsureInputHandlingIsCompatible();
        EnsureEventSystem();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("OpenIQ GMDKIT", "Config applied: Active Input Handling set to Both and EventSystem updated for the Input System package.", "OK");
    }

    private static void AddPlayer()
    {
        EnsureRequiredTags();
        EnsureGameplayCamera();
        EnsureBulletPrefab();

        GameObject existing = GameObject.Find("Player");
        if (existing != null)
        {
            Selection.activeGameObject = existing;
            EditorUtility.DisplayDialog("OpenIQ GMDKIT", "A Player already exists in this scene.", "OK");
            return;
        }

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 0f, 0f);
        player.transform.localScale = new Vector3(1.2f, 0.6f, 1.8f);

        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = Color.white;
        }

        Rigidbody body = player.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezePositionZ |
                           RigidbodyConstraints.FreezeRotationX |
                           RigidbodyConstraints.FreezeRotationY |
                           RigidbodyConstraints.FreezeRotationZ;

        BoxCollider trigger = player.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = Vector3.one * 1.05f;

        player.AddComponent<Player>();

        Selection.activeGameObject = player;
        EditorSceneManager.MarkSceneDirty(player.scene);
    }

    private static void AddMovingGrounds()
    {
        EnsureGameplayCamera();

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "MovingGrounds";
        ground.transform.position = new Vector3(0f, 0f, 25f);
        ground.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        ground.transform.localScale = new Vector3(0.6f, 1f, 0.6f);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = new Color(0.08f, 0.08f, 0.12f, 1f);
        }

        ground.AddComponent<MovingGrounds>();
        Selection.activeGameObject = ground;
        EditorSceneManager.MarkSceneDirty(ground.scene);
    }

    private static void AddSpawnPoint()
    {
        EnsureRequiredTags();
        EnsureEnemyPrefab();
        EnsureGameplayCamera();

        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.position = new Vector3(0f, 0f, 18f);
        SpawnPoint spawner = spawnPoint.AddComponent<SpawnPoint>();
        spawner.minSpawnInterval = 1f;
        spawner.maxSpawnInterval = 3f;

        Selection.activeGameObject = spawnPoint;
        EditorSceneManager.MarkSceneDirty(spawnPoint.scene);
    }

    private static void AddEnemy()
    {
        EnsureRequiredTags();
        EnsureGameplayCamera();
        GameObject enemyPrefab = EnsureEnemyPrefab();

        GameObject enemy = enemyPrefab != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab)
            : CreateEnemyObject();

        enemy.name = "Enemy";
        enemy.transform.position = new Vector3(0f, 0f, 16f);
        Selection.activeGameObject = enemy;
        EditorSceneManager.MarkSceneDirty(enemy.scene);
    }

    private static void AddUI()
    {
        EnsureEventSystem();

        GameObject canvasObject = GameObject.Find("GameplayUI");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("GameplayUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Slider healthSlider = CreateHealthSlider(canvasObject.transform);
        Text scoreText = CreateScoreText(canvasObject.transform, font);

        GameHUD gameHud = canvasObject.GetComponent<GameHUD>();
        if (gameHud == null)
        {
            gameHud = canvasObject.AddComponent<GameHUD>();
        }

        gameHud.healthSlider = healthSlider;
        gameHud.scoreText = scoreText;

        Selection.activeGameObject = canvasObject;
        EditorSceneManager.MarkSceneDirty(canvasObject.scene);
    }

    private static void AddGameLogic()
    {
        GameObject gameLogicObject = GameObject.Find("GameLogic");
        if (gameLogicObject == null)
        {
            gameLogicObject = new GameObject("GameLogic");
        }

        GameLogic gameLogic = gameLogicObject.GetComponent<GameLogic>();
        if (gameLogic == null)
        {
            gameLogic = gameLogicObject.AddComponent<GameLogic>();
        }

        gameLogic.maxHits = 5;
        gameLogic.gameOverSceneName = "GameOver";
        Selection.activeGameObject = gameLogicObject;
        EditorSceneManager.MarkSceneDirty(gameLogicObject.scene);
    }

    private static void AddBoss()
    {
        EnsureRequiredTags();
        EnsureGameplayCamera();
        EnsureBossPrefab();

        GameObject bossSpawnerObject = GameObject.Find("BossSpawner");
        if (bossSpawnerObject == null)
        {
            bossSpawnerObject = new GameObject("BossSpawner");
        }

        bossSpawnerObject.transform.position = new Vector3(0f, 2f, 15f);
        BossSpawner bossSpawner = bossSpawnerObject.GetComponent<BossSpawner>();
        if (bossSpawner == null)
        {
            bossSpawner = bossSpawnerObject.AddComponent<BossSpawner>();
        }

        bossSpawner.bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
        bossSpawner.useScoreTrigger = true;
        bossSpawner.useTimeTrigger = false;
        bossSpawner.scoreTrigger = 20;
        bossSpawner.timeTrigger = 30f;

        Selection.activeGameObject = bossSpawnerObject;
        EditorSceneManager.MarkSceneDirty(bossSpawnerObject.scene);
    }

    private static void AddObstacle()
    {
        EnsureRequiredTags();
        EnsureGameplayCamera();
        EnsureObstaclePrefab();

        GameObject obstacleSpawnerObject = GameObject.Find("ObstacleSpawner");
        if (obstacleSpawnerObject == null)
        {
            obstacleSpawnerObject = new GameObject("ObstacleSpawner");
        }

        obstacleSpawnerObject.transform.position = new Vector3(0f, 0f, 18f);
        ObstacleSpawner obstacleSpawner = obstacleSpawnerObject.GetComponent<ObstacleSpawner>();
        if (obstacleSpawner == null)
        {
            obstacleSpawner = obstacleSpawnerObject.AddComponent<ObstacleSpawner>();
        }

        obstacleSpawner.obstaclePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ObstaclePrefabPath);
        obstacleSpawner.minSpawnInterval = 2f;
        obstacleSpawner.maxSpawnInterval = 10f;

        Selection.activeGameObject = obstacleSpawnerObject;
        EditorSceneManager.MarkSceneDirty(obstacleSpawnerObject.scene);
    }

    private static void CreateTitleCanvas(Scene activeScene)
    {
        GameObject canvasObject = GameObject.Find("TitleCanvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("TitleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        TitleController controller = canvasObject.GetComponent<TitleController>();
        if (controller == null)
        {
            controller = canvasObject.AddComponent<TitleController>();
        }
        controller.sceneName = "Game";
        CreateTitleLogo(canvasObject.transform);

        GameObject buttonObject = GameObject.Find("StartButton");
        if (buttonObject == null)
        {
            buttonObject = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.35f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.35f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(280f, 80f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = Color.white;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            Text text = textObject.GetComponent<Text>();
            text.text = "Start Game";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 30;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(controller.LoadScene);

        EditorSceneManager.MarkSceneDirty(activeScene);
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        }

        GameObject eventSystemObject = eventSystem.gameObject;
        StandaloneInputModule standaloneModule = eventSystemObject.GetComponent<StandaloneInputModule>();
        if (standaloneModule != null)
        {
            Object.DestroyImmediate(standaloneModule);
        }

#if ENABLE_INPUT_SYSTEM
        if (eventSystemObject.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }
#else
        if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }

    private static Camera EnsureGameplayCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.transform.rotation = Quaternion.identity;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.05f, 0.09f, 1f);
        return camera;
    }

    private static void AddScenesToBuildSettings(string titleScenePath, string gameScenePath)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        AddSceneIfMissing(scenes, titleScenePath);
        AddSceneIfMissing(scenes, gameScenePath);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void AddScenesToBuildSettings(params string[] scenePaths)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenePaths.Length; i++)
        {
            AddSceneIfMissing(scenes, scenePaths[i]);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void EnsureRequiredTags()
    {
        EnsureTagExists("enemy");
        EnsureTagExists("bullet");
    }

    private static void EnsureTagExists(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            SerializedProperty tagProperty = tagsProperty.GetArrayElementAtIndex(i);
            if (tagProperty.stringValue == tag)
            {
                return;
            }
        }

        tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
        tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureInputHandlingIsCompatible()
    {
        string projectSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "ProjectSettings/ProjectSettings.asset");
        if (!File.Exists(projectSettingsPath))
        {
            return;
        }

        string projectSettings = File.ReadAllText(projectSettingsPath);
        const string oldValue = "  activeInputHandler: 1";
        const string inputOnlyValue = "  activeInputHandler: 0";
        const string bothValue = "  activeInputHandler: 2";

        if (projectSettings.Contains(bothValue))
        {
            return;
        }

        if (projectSettings.Contains(oldValue))
        {
            projectSettings = projectSettings.Replace(oldValue, bothValue);
        }
        else if (projectSettings.Contains(inputOnlyValue))
        {
            projectSettings = projectSettings.Replace(inputOnlyValue, bothValue);
        }
        else
        {
            return;
        }

        File.WriteAllText(projectSettingsPath, projectSettings);
        AssetDatabase.Refresh();
    }

    private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            return;
        }

        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == scenePath)
            {
                scenes[i].enabled = true;
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
    }

    private static GameObject EnsureBulletPrefab()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/bullet");

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "bullet";
        bullet.tag = "bullet";
        bullet.transform.localScale = Vector3.one * 0.35f;

        Renderer renderer = bullet.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = Color.cyan;
        }

        SphereCollider collider = bullet.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        Rigidbody body = bullet.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        bullet.AddComponent<Bullet>();

        prefab = PrefabUtility.SaveAsPrefabAsset(bullet, BulletPrefabPath);
        Object.DestroyImmediate(bullet);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return prefab;
    }

    private static GameObject EnsureEnemyPrefab()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/enemy");

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        GameObject enemy = CreateEnemyObject();
        prefab = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
        Object.DestroyImmediate(enemy);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return prefab;
    }

    private static GameObject EnsureBossPrefab()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/boss");

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
        if (prefab != null)
        {
            Boss prefabBoss = prefab.GetComponent<Boss>();
            if (prefabBoss != null)
            {
                prefabBoss.maxHealth = 50;
                prefabBoss.fixedZPosition = 15f;
                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
            }

            return prefab;
        }

        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boss.name = "Boss";
        boss.tag = "enemy";
        boss.transform.localScale = new Vector3(3f, 1.8f, 1.8f);
        boss.transform.position = new Vector3(0f, 2f, 15f);

        Renderer renderer = boss.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = new Color(0.7f, 0.2f, 1f, 1f);
        }

        BoxCollider collider = boss.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        Rigidbody body = boss.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        Boss bossComponent = boss.AddComponent<Boss>();
        bossComponent.maxHealth = 50;
        bossComponent.fixedZPosition = 15f;
        prefab = PrefabUtility.SaveAsPrefabAsset(boss, BossPrefabPath);
        Object.DestroyImmediate(boss);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return prefab;
    }

    private static GameObject EnsureObstaclePrefab()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/obstacle");

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ObstaclePrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "Obstacle";
        obstacle.tag = "enemy";
        obstacle.transform.localScale = new Vector3(2.5f, 2.5f, 1.5f);

        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = new Color(0.65f, 0.65f, 0.7f, 1f);
        }

        BoxCollider collider = obstacle.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        Rigidbody body = obstacle.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        obstacle.AddComponent<Obstacle>();
        prefab = PrefabUtility.SaveAsPrefabAsset(obstacle, ObstaclePrefabPath);
        Object.DestroyImmediate(obstacle);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return prefab;
    }

    private static GameObject CreateEnemyObject()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "Enemy";
        enemy.tag = "enemy";
        enemy.transform.localScale = new Vector3(1.1f, 0.8f, 1.4f);

        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial.color = new Color(1f, 0.35f, 0.35f, 1f);
        }

        BoxCollider collider = enemy.GetComponent<BoxCollider>();
        collider.isTrigger = true;

        Rigidbody body = enemy.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;

        enemy.AddComponent<Enemy>();
        return enemy;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folderName = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        if (!string.IsNullOrEmpty(parent))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static Slider CreateHealthSlider(Transform parent)
    {
        GameObject sliderObject = GameObject.Find("HealthSlider");
        if (sliderObject == null)
        {
            sliderObject = new GameObject("HealthSlider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
        }

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.03f, 0.93f);
        sliderRect.anchorMax = new Vector2(0.28f, 0.97f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.direction = Slider.Direction.LeftToRight;

        Transform backgroundTransform = sliderObject.transform.Find("Background");
        GameObject background = backgroundTransform != null
            ? backgroundTransform.gameObject
            : new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderObject.transform, false);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = new Color(0.08f, 0.12f, 0.08f, 0.9f);

        Transform fillAreaTransform = sliderObject.transform.Find("Fill Area");
        GameObject fillArea = fillAreaTransform != null
            ? fillAreaTransform.gameObject
            : new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(6f, 6f);
        fillAreaRect.offsetMax = new Vector2(-6f, -6f);

        Transform fillTransform = fillArea.transform.Find("Fill");
        GameObject fill = fillTransform != null
            ? fillTransform.gameObject
            : new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color(0.12f, 0.82f, 0.22f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        Transform handleArea = sliderObject.transform.Find("Handle Slide Area");
        if (handleArea != null)
        {
            Object.DestroyImmediate(handleArea.gameObject);
        }

        slider.targetGraphic = fillImage;
        slider.fillRect = fillRect;
        slider.handleRect = null;
        slider.transition = Selectable.Transition.None;

        return slider;
    }

    private static Text CreateScoreText(Transform parent, Font font)
    {
        GameObject scoreObject = GameObject.Find("ScoreText");
        if (scoreObject == null)
        {
            scoreObject = new GameObject("ScoreText", typeof(RectTransform), typeof(Text));
            scoreObject.transform.SetParent(parent, false);
        }

        RectTransform scoreRect = scoreObject.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.72f, 0.92f);
        scoreRect.anchorMax = new Vector2(0.97f, 0.98f);
        scoreRect.offsetMin = Vector2.zero;
        scoreRect.offsetMax = Vector2.zero;

        Text scoreText = scoreObject.GetComponent<Text>();
        scoreText.font = font;
        scoreText.fontSize = 28;
        scoreText.alignment = TextAnchor.MiddleRight;
        scoreText.color = Color.white;
        scoreText.text = "Score : 0";
        return scoreText;
    }

    private static void CreateTitleLogo(Transform parent)
    {
        GameObject logoObject = GameObject.Find("TitleLogo");
        if (logoObject == null)
        {
            logoObject = new GameObject("TitleLogo", typeof(RectTransform), typeof(RawImage));
            logoObject.transform.SetParent(parent, false);
        }

        RectTransform logoRect = logoObject.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 1f);
        logoRect.anchorMax = new Vector2(0.5f, 1f);
        logoRect.pivot = new Vector2(0.5f, 1f);
        logoRect.anchoredPosition = new Vector2(0f, -48f);
        logoRect.sizeDelta = new Vector2(360f, 140f);

        RawImage logoImage = logoObject.GetComponent<RawImage>();
        logoImage.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
        logoImage.color = Color.white;
    }

    private static void CreateGameOverScene()
    {
        Scene gameOverScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        gameOverScene.name = "GameOver";
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("GameOverCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject titleObject = new GameObject("GameOverText", typeof(RectTransform), typeof(Text));
        titleObject.transform.SetParent(canvasObject.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.3f, 0.62f);
        titleRect.anchorMax = new Vector2(0.7f, 0.78f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        Text titleText = titleObject.GetComponent<Text>();
        titleText.font = font;
        titleText.fontSize = 46;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.text = "Game Over";

        GameObject buttonObject = new GameObject("BackToTitleButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(SceneButtonLoader));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.4f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.6f, 0.5f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = Color.white;

        SceneButtonLoader loader = buttonObject.GetComponent<SceneButtonLoader>();
        loader.sceneName = "Title";

        GameObject buttonTextObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        Text buttonText = buttonTextObject.GetComponent<Text>();
        buttonText.font = font;
        buttonText.fontSize = 28;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.black;
        buttonText.text = "Back To Title";

        EditorSceneManager.SaveScene(gameOverScene, GameOverScenePath);
    }
}
