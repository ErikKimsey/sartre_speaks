using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GeometryDash3D
{
    public class DemoLevelBuilder : MonoBehaviour
    {
        public const string MaterialsFolder = "Assets/Materials/GeometryDash3D";

#if UNITY_EDITOR
        [MenuItem("Tools/Geometry Dash 3D/Build Demo Scene")]
        public static void BuildDemoSceneMenu()
        {
            BuildCompleteLevel();
        }

        public static void BuildCompleteLevel()
        {
            EnsureAssetFolders();

            // 1. Create or retrieve Materials
            Material matPlayer = GetOrCreateMaterial("Mat_PlayerNeon", new Color(0f, 0.95f, 1f), new Color(0f, 1.2f, 1.5f), true);
            Material matFloor = GetOrCreateMaterial("Mat_FloorDark", new Color(0.08f, 0.1f, 0.16f), Color.black, false);
            Material matHazard = GetOrCreateMaterial("Mat_HazardRed", new Color(1f, 0.1f, 0.2f), new Color(1.5f, 0.15f, 0.25f), true);
            Material matBounce = GetOrCreateMaterial("Mat_BouncePad", new Color(0.45f, 1f, 0.05f), new Color(0.6f, 1.5f, 0.1f), true);
            Material matGoal = GetOrCreateMaterial("Mat_GoalGold", new Color(1f, 0.85f, 0.1f), new Color(1.8f, 1.4f, 0.2f), true);
            Material matBlock = GetOrCreateMaterial("Mat_PlatformBlock", new Color(0.14f, 0.16f, 0.26f), new Color(0.1f, 0.12f, 0.22f), false);

            // 2. Clear previous level hierarchy if any
            GameObject existingLevel = GameObject.Find("--- GEOMETRY DASH 3D ---");
            if (existingLevel != null)
            {
                Undo.DestroyObjectImmediate(existingLevel);
            }

            GameObject root = new GameObject("--- GEOMETRY DASH 3D ---");
            Undo.RegisterCreatedObjectUndo(root, "Build GD3D Level");

            GameObject levelEnvironment = new GameObject("Environment");
            levelEnvironment.transform.SetParent(root.transform);

            GameObject hazardsRoot = new GameObject("Hazards");
            hazardsRoot.transform.SetParent(root.transform);

            GameObject interactablesRoot = new GameObject("Interactables");
            interactablesRoot.transform.SetParent(root.transform);

            // 3. Create Player Death Particles Prefab
            GameObject deathParticlesGo = CreateDeathParticleObject(root.transform, matPlayer);

            // 4. Create Player
            GameObject playerGo = new GameObject("Player");
            playerGo.transform.SetParent(root.transform);
            playerGo.transform.position = new Vector3(0f, 0.5f, 0f);

            var boxCol = playerGo.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(0.85f, 0.85f, 0.85f);
            boxCol.center = Vector3.zero;

            var rb = playerGo.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var inputHandler = playerGo.AddComponent<PlayerInputHandler>();
            var playerCtrl = playerGo.AddComponent<PlayerController3D>();

            // Player Visual Cube
            GameObject playerVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playerVisual.name = "VisualCube";
            playerVisual.transform.SetParent(playerGo.transform);
            playerVisual.transform.localPosition = Vector3.zero;
            playerVisual.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            Object.DestroyImmediate(playerVisual.GetComponent<Collider>());
            playerVisual.GetComponent<MeshRenderer>().sharedMaterial = matPlayer;

            // Player Trail
            GameObject trailGo = new GameObject("Trail");
            trailGo.transform.SetParent(playerGo.transform);
            trailGo.transform.localPosition = Vector3.zero;
            var trail = trailGo.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.6f;
            trail.endWidth = 0.05f;
            trail.sharedMaterial = matPlayer;

            // Wire Player fields via SerializedObject
            SerializedObject playerSo = new SerializedObject(playerCtrl);
            playerSo.FindProperty("_visualTransform").objectReferenceValue = playerVisual.transform;
            playerSo.FindProperty("_trailRenderer").objectReferenceValue = trail;
            playerSo.FindProperty("_deathParticlePrefab").objectReferenceValue = deathParticlesGo.GetComponent<ParticleSystem>();
            playerSo.ApplyModifiedPropertiesWithoutUndo();

            // 5. Construct Platforms & Terrain
            // Section 1: Starting Runway
            CreatePlatform(levelEnvironment.transform, matFloor, new Vector3(6f, -0.5f, 0f), new Vector3(26f, 1f, 3f));

            // Spike 1: Single spike at X = 9
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(9f, 0f, 0f));

            // Spike 2: Double spike at X = 15, 16.5
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(15f, 0f, 0f));
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(16.5f, 0f, 0f));

            // Section 2: Stepped Elevated Platform over small pit
            // (Pit from X = 19 to 22)
            CreatePlatform(levelEnvironment.transform, matBlock, new Vector3(29f, 1.5f, 0f), new Vector3(14f, 1f, 3f));
            // Pillars supporting platform
            CreatePlatform(levelEnvironment.transform, matFloor, new Vector3(23f, -1.5f, 0f), new Vector3(2f, 3f, 2.5f));
            CreatePlatform(levelEnvironment.transform, matFloor, new Vector3(35f, -1.5f, 0f), new Vector3(2f, 3f, 2.5f));

            // Spike on elevated platform at X = 27
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(27f, 2f, 0f));
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(31.5f, 2f, 0f));

            // Section 3: Low Pit with Launch Pad
            // Platform in low pit (X = 38 to 44, top at Y = -1)
            CreatePlatform(levelEnvironment.transform, matFloor, new Vector3(41f, -2.5f, 0f), new Vector3(10f, 1f, 3f));

            // Bounce Pad at X = 41 (launches player over wide pit up to high platform)
            CreateBouncePad(interactablesRoot.transform, matBounce, new Vector3(41f, -1.95f, 0f));

            // Section 4: High Platform (X = 54 to 72, top at Y = 3.5)
            CreatePlatform(levelEnvironment.transform, matBlock, new Vector3(63f, 3f, 0f), new Vector3(18f, 1f, 3f));

            // Stepping blocks on high platform
            CreateBlockObstacle(levelEnvironment.transform, hazardsRoot.transform, matBlock, matHazard, new Vector3(59f, 4f, 0f));
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(64f, 3.5f, 0f));
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(65.5f, 3.5f, 0f));

            // Section 5: Mid Platform Stair Down (X = 75 to 88, top at Y = 1.0)
            CreatePlatform(levelEnvironment.transform, matFloor, new Vector3(81.5f, 0.5f, 0f), new Vector3(15f, 1f, 3f));
            // Bounce pad at X = 83 launching into final runway
            CreateBouncePad(interactablesRoot.transform, matBounce, new Vector3(83f, 1.05f, 0f));

            // Section 6: Final Runway (X = 90 to 118, top at Y = 0)
            CreatePlatform(levelEnvironment.transform, matFloor, new Vector3(104f, -0.5f, 0f), new Vector3(28f, 1f, 3f));

            // Spikes in final stretch
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(96f, 0f, 0f));
            CreateSpike(hazardsRoot.transform, matHazard, new Vector3(98f, 0f, 0f));

            // Goal Trigger Portal at X = 112
            GameObject goalGo = CreateGoalPortal(interactablesRoot.transform, matGoal, new Vector3(112f, 1.5f, 0f));
            var goalTrigger = goalGo.GetComponent<GoalTrigger>();

            // 6. Camera Setup
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                var camGo = new GameObject("Main Camera");
                mainCam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }

            var camCtrl = mainCam.GetComponent<CameraController3D>();
            if (camCtrl == null)
            {
                camCtrl = mainCam.gameObject.AddComponent<CameraController3D>();
            }
            camCtrl.Target = playerGo.transform;
            camCtrl.SnapToTarget();

            // 7. UI Canvas & Progress
            GameObject canvasGo = CreateOrSetupUI(root.transform);
            var progressUI = canvasGo.GetComponent<LevelProgressUI>();

            // 8. Level Manager Setup
            GameObject levelMgrGo = new GameObject("LevelManager");
            levelMgrGo.transform.SetParent(root.transform);
            var levelMgr = levelMgrGo.AddComponent<LevelManager>();

            levelMgr.ConfigureCourse(playerCtrl, camCtrl, goalTrigger, new Vector3(0f, 0.5f, 0f));

            // 9. Directional Light & Environment Polish
            Light dirLight = Object.FindFirstObjectByType<Light>();
            if (dirLight == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                dirLight = lightGo.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                lightGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            }
            dirLight.color = new Color(0.9f, 0.95f, 1f);
            dirLight.intensity = 1.1f;

            // Set background color to deep synthwave dark blue
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.04f, 0.05f, 0.09f, 1f);

            // Mark Scene Dirty and Save
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log("<color=cyan>[GeometryDash3D] Level successfully built and saved!</color>");
        }

        private static void EnsureAssetFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "GeometryDash3D");
            }
        }

        private static Material GetOrCreateMaterial(string name, Color baseColor, Color emissionColor, bool hasEmission)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = baseColor;
            if (hasEmission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject CreatePlatform(Transform parent, Material mat, Vector3 pos, Vector3 size)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = $"Platform_{pos.x}";
            platform.transform.SetParent(parent);
            platform.transform.position = pos;
            platform.transform.localScale = size;
            platform.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return platform;
        }

        private static GameObject CreateSpike(Transform parent, Material mat, Vector3 pos)
        {
            GameObject spikeGo = new GameObject($"Spike_{pos.x}");
            spikeGo.transform.SetParent(parent);
            spikeGo.transform.position = pos;

            MeshFilter mf = spikeGo.AddComponent<MeshFilter>();
            MeshRenderer mr = spikeGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mf.sharedMesh = GeneratePyramidMesh();

            var col = spikeGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(0.7f, 0.9f, 0.7f);
            col.center = new Vector3(0f, 0.45f, 0f);

            spikeGo.AddComponent<Hazard>();

            return spikeGo;
        }

        private static Mesh GeneratePyramidMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "SpikePyramidMesh";

            Vector3[] vertices = new Vector3[]
            {
                // Base
                new Vector3(-0.45f, 0f, -0.45f),
                new Vector3(0.45f, 0f, -0.45f),
                new Vector3(0.45f, 0f, 0.45f),
                new Vector3(-0.45f, 0f, 0.45f),
                // Apex
                new Vector3(0f, 1f, 0f)
            };

            int[] triangles = new int[]
            {
                // Front
                0, 4, 1,
                // Right
                1, 4, 2,
                // Back
                2, 4, 3,
                // Left
                3, 4, 0,
                // Bottom
                0, 2, 1,
                0, 3, 2
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static GameObject CreateBlockObstacle(Transform parent, Transform hazardsParent, Material blockMat, Material hazardMat, Vector3 pos)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = $"StepBlock_{pos.x}";
            block.transform.SetParent(parent);
            block.transform.position = pos;
            block.transform.localScale = new Vector3(1f, 1f, 1.5f);
            block.GetComponent<MeshRenderer>().sharedMaterial = blockMat;

            return block;
        }

        private static GameObject CreateBouncePad(Transform parent, Material mat, Vector3 pos)
        {
            GameObject padGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            padGo.name = $"BouncePad_{pos.x}";
            padGo.transform.SetParent(parent);
            padGo.transform.position = pos;
            padGo.transform.localScale = new Vector3(1.6f, 0.25f, 1.8f);
            padGo.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var col = padGo.GetComponent<BoxCollider>();
            col.isTrigger = true;

            var bounce = padGo.AddComponent<BounceSurface>();
            return padGo;
        }

        private static GameObject CreateGoalPortal(Transform parent, Material mat, Vector3 pos)
        {
            GameObject portalRoot = new GameObject("GoalPortal");
            portalRoot.transform.SetParent(parent);
            portalRoot.transform.position = pos;

            // Arch pillars
            GameObject pillarL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillarL.transform.SetParent(portalRoot.transform);
            pillarL.transform.localPosition = new Vector3(-0.8f, 0f, 0f);
            pillarL.transform.localScale = new Vector3(0.3f, 3.5f, 0.5f);
            pillarL.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(pillarL.GetComponent<Collider>());

            GameObject pillarR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillarR.transform.SetParent(portalRoot.transform);
            pillarR.transform.localPosition = new Vector3(0.8f, 0f, 0f);
            pillarR.transform.localScale = new Vector3(0.3f, 3.5f, 0.5f);
            pillarR.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(pillarR.GetComponent<Collider>());

            GameObject topBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topBar.transform.SetParent(portalRoot.transform);
            topBar.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            topBar.transform.localScale = new Vector3(1.9f, 0.3f, 0.5f);
            topBar.GetComponent<MeshRenderer>().sharedMaterial = mat;
            Object.DestroyImmediate(topBar.GetComponent<Collider>());

            // Trigger field
            var col = portalRoot.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.6f, 3.5f, 2f);
            col.center = new Vector3(0f, 0f, 0f);

            portalRoot.AddComponent<GoalTrigger>();

            return portalRoot;
        }

        private static GameObject CreateDeathParticleObject(Transform parent, Material mat)
        {
            GameObject deathGo = new GameObject("DeathParticlesPrefab");
            deathGo.transform.SetParent(parent);
            deathGo.transform.localPosition = Vector3.zero;

            ParticleSystem ps = deathGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = 0.5f;
            main.startSpeed = 8f;
            main.startSize = 0.18f;
            main.startColor = new Color(0f, 1f, 1f);

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var rend = deathGo.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = mat;

            deathGo.SetActive(false);
            return deathGo;
        }

        private static GameObject CreateOrSetupUI(Transform parent)
        {
            GameObject canvasGo = new GameObject("Canvas_HUD");
            canvasGo.transform.SetParent(parent);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Ensure EventSystem
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.transform.SetParent(parent);
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Top HUD Bar
            GameObject hudPanel = new GameObject("TopHUD");
            hudPanel.transform.SetParent(canvasGo.transform, false);
            RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0f, 1f);
            hudRect.anchorMax = new Vector2(1f, 1f);
            hudRect.pivot = new Vector2(0.5f, 1f);
            hudRect.anchoredPosition = new Vector2(0f, -20f);
            hudRect.sizeDelta = new Vector2(0f, 60f);

            // Progress Slider
            GameObject sliderGo = new GameObject("ProgressBar");
            sliderGo.transform.SetParent(hudPanel.transform, false);
            RectTransform sliderRect = sliderGo.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.sizeDelta = new Vector2(400f, 16f);

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;

            // Slider Background
            GameObject bgGo = new GameObject("Background");
            bgGo.transform.SetParent(sliderGo.transform, false);
            RectTransform bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.12f, 0.2f, 0.7f);

            // Slider Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            GameObject fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0f, 0.9f, 1f);

            slider.fillRect = fillRect;

            // Progress Text (e.g. 52%)
            GameObject textGo = new GameObject("ProgressText");
            textGo.transform.SetParent(hudPanel.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, -28f);
            textRect.sizeDelta = new Vector2(200f, 35f);
            TextMeshProUGUI progressText = textGo.AddComponent<TextMeshProUGUI>();
            progressText.text = "0%";
            progressText.alignment = TextAlignmentOptions.Center;
            progressText.fontSize = 24;
            progressText.fontStyle = FontStyles.Bold;
            progressText.color = new Color(0f, 0.95f, 1f);

            // Attempt Text (e.g. ATTEMPT 1)
            GameObject attemptGo = new GameObject("AttemptText");
            attemptGo.transform.SetParent(hudPanel.transform, false);
            RectTransform attemptRect = attemptGo.AddComponent<RectTransform>();
            attemptRect.anchorMin = new Vector2(0f, 0.5f);
            attemptRect.anchorMax = new Vector2(0f, 0.5f);
            attemptRect.pivot = new Vector2(0f, 0.5f);
            attemptRect.anchoredPosition = new Vector2(40f, 0f);
            attemptRect.sizeDelta = new Vector2(250f, 40f);
            TextMeshProUGUI attemptText = attemptGo.AddComponent<TextMeshProUGUI>();
            attemptText.text = "ATTEMPT 1";
            attemptText.alignment = TextAlignmentOptions.Left;
            attemptText.fontSize = 24;
            attemptText.fontStyle = FontStyles.Bold;
            attemptText.color = Color.white;

            // Victory Panel
            GameObject victoryPanelGo = new GameObject("VictoryPanel");
            victoryPanelGo.transform.SetParent(canvasGo.transform, false);
            RectTransform victoryRect = victoryPanelGo.AddComponent<RectTransform>();
            victoryRect.anchorMin = new Vector2(0.5f, 0.5f);
            victoryRect.anchorMax = new Vector2(0.5f, 0.5f);
            victoryRect.pivot = new Vector2(0.5f, 0.5f);
            victoryRect.sizeDelta = new Vector2(500f, 300f);
            Image victoryBg = victoryPanelGo.AddComponent<Image>();
            victoryBg.color = new Color(0.05f, 0.08f, 0.15f, 0.95f);

            // Victory Title
            GameObject vicTitleGo = new GameObject("Title");
            vicTitleGo.transform.SetParent(victoryPanelGo.transform, false);
            RectTransform vicTitleRect = vicTitleGo.AddComponent<RectTransform>();
            vicTitleRect.anchorMin = new Vector2(0.5f, 0.8f);
            vicTitleRect.anchorMax = new Vector2(0.5f, 0.8f);
            vicTitleRect.sizeDelta = new Vector2(450f, 60f);
            TextMeshProUGUI vicTitleText = vicTitleGo.AddComponent<TextMeshProUGUI>();
            vicTitleText.text = "LEVEL COMPLETE!";
            vicTitleText.alignment = TextAlignmentOptions.Center;
            vicTitleText.fontSize = 36;
            vicTitleText.fontStyle = FontStyles.Bold;
            vicTitleText.color = new Color(1f, 0.85f, 0.1f);

            // Victory Subtext
            GameObject vicSubGo = new GameObject("AttemptsSummary");
            vicSubGo.transform.SetParent(victoryPanelGo.transform, false);
            RectTransform vicSubRect = vicSubGo.AddComponent<RectTransform>();
            vicSubRect.anchorMin = new Vector2(0.5f, 0.5f);
            vicSubRect.anchorMax = new Vector2(0.5f, 0.5f);
            vicSubRect.sizeDelta = new Vector2(400f, 50f);
            TextMeshProUGUI vicSubText = vicSubGo.AddComponent<TextMeshProUGUI>();
            vicSubText.text = "COMPLETED IN 1 ATTEMPTS!";
            vicSubText.alignment = TextAlignmentOptions.Center;
            vicSubText.fontSize = 22;
            vicSubText.color = Color.white;

            // Restart Button
            GameObject btnGo = new GameObject("RestartButton");
            btnGo.transform.SetParent(victoryPanelGo.transform, false);
            RectTransform btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.2f);
            btnRect.anchorMax = new Vector2(0.5f, 0.2f);
            btnRect.sizeDelta = new Vector2(200f, 50f);
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0f, 0.7f, 0.9f);
            Button btn = btnGo.AddComponent<Button>();

            GameObject btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(btnGo.transform, false);
            RectTransform btnTextRect = btnTextGo.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnText.text = "PLAY AGAIN";
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 20;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;

            // Wire LevelProgressUI
            var uiComp = canvasGo.AddComponent<LevelProgressUI>();
            SerializedObject uiSo = new SerializedObject(uiComp);
            uiSo.FindProperty("_progressBar").objectReferenceValue = slider;
            uiSo.FindProperty("_progressText").objectReferenceValue = progressText;
            uiSo.FindProperty("_attemptText").objectReferenceValue = attemptText;
            uiSo.FindProperty("_victoryPanel").objectReferenceValue = victoryPanelGo;
            uiSo.FindProperty("_victoryAttemptsText").objectReferenceValue = vicSubText;
            uiSo.FindProperty("_restartButton").objectReferenceValue = btn;
            uiSo.ApplyModifiedPropertiesWithoutUndo();

            victoryPanelGo.SetActive(false);

            return canvasGo;
        }
#endif
    }
}
