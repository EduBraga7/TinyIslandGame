#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Construtor Automático da Cena Tiny Island.
/// Monta o cenário inteiro (ilha, oceano, materiais coloridos, jogador, câmera,
/// moedas coletáveis, palmeiras, pedras decorativas, baú do tesouro e manager)
/// com apenas um clique ou automaticamente ao carregar!
/// </summary>
public static class IslandSceneBuilder
{
    // Executa automaticamente quando o Unity terminar de compilar este script
    [InitializeOnLoadMethod]
    private static void AutoSetupCheck()
    {
        EditorApplication.delayCall += () =>
        {
            // Se já tiver um jogador na cena, verifica se o baú precisa ser atualizado
            if (GameObject.Find("Player") == null && GameObject.Find("Ilha") == null)
            {
                Debug.Log("<color=cyan>[TinyIsland]</color> Primeira inicialização detectada! Construindo a ilha automaticamente...");
                BuildCompleteIsland();
            }
            else
            {
                GameObject chestObj = GameObject.Find("BauDoTesouro");
                if (chestObj != null)
                {
                    TreasureChest tc = chestObj.GetComponent<TreasureChest>();
                    // Atualiza automaticamente se tiver o ângulo antigo (-70) ou sem partículas
                    if (tc != null && (tc.targetOpenAngle > -80f || tc.openParticles == null))
                    {
                        Debug.Log("<color=cyan>[TinyIsland]</color> Atualizando Baú do Tesouro para a nova versão aprimorada...");
                        BuildChest();
                    }
                }
            }
        };
    }

    [MenuItem("TinyIsland/🏝️ Construir Ilha Completa")]
    public static void BuildCompleteIsland()
    {
        // 1. Garante que a pasta de Materiais exista
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }

        // 2. Cria os materiais estilizados (Cores URP com brilho e metálico adequados)
        Material matGrass = GetOrCreateMaterial("Assets/Materials/M_Grama.mat", new Color(0.28f, 0.75f, 0.35f), 0f, 0.2f);
        Material matSand = GetOrCreateMaterial("Assets/Materials/M_Areia.mat", new Color(0.92f, 0.82f, 0.58f), 0f, 0.1f);
        Material matWater = GetOrCreateMaterial("Assets/Materials/M_Oceano.mat", new Color(0.15f, 0.65f, 0.85f), 0.1f, 0.9f);
        Material matPlayer = GetOrCreateMaterial("Assets/Materials/M_Player.mat", new Color(0.95f, 0.42f, 0.22f), 0f, 0.4f);
        Material matGold = GetOrCreateMaterial("Assets/Materials/M_Ouro.mat", new Color(1.0f, 0.84f, 0.0f), 0.95f, 0.85f);
        Material matWood = GetOrCreateMaterial("Assets/Materials/M_ChestWood.mat", new Color(0.42f, 0.22f, 0.08f), 0f, 0.3f);
        Material matIron = GetOrCreateMaterial("Assets/Materials/M_ChestIron.mat", new Color(0.18f, 0.18f, 0.20f), 0.85f, 0.65f);
        Material matRock = GetOrCreateMaterial("Assets/Materials/M_Rock.mat", new Color(0.50f, 0.49f, 0.46f), 0f, 0.2f);
        Material matRuby = GetOrCreateMaterial("Assets/Materials/M_Ruby.mat", new Color(0.92f, 0.08f, 0.16f), 0.1f, 0.95f);
        Material matEmerald = GetOrCreateMaterial("Assets/Materials/M_Emerald.mat", new Color(0.05f, 0.85f, 0.35f), 0.1f, 0.95f);
        Material matSapphire = GetOrCreateMaterial("Assets/Materials/M_Sapphire.mat", new Color(0.10f, 0.38f, 0.95f), 0.1f, 0.95f);

        // 3. Limpa objetos antigos se existirem (para não duplicar)
        CleanUpOldObjects("Ilha", "GramaTopo", "Oceano", "Player", "GameManager", "Colecionaveis", "Palmeiras", "BauDoTesouro", "Pedras");

        // 4. Cria o Oceano (Água ao redor)
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ocean.name = "Oceano";
        ocean.transform.position = new Vector3(0, -0.3f, 0);
        ocean.transform.localScale = new Vector3(10, 1, 10); // 100x100 metros
        ocean.GetComponent<Renderer>().sharedMaterial = matWater;

        // 5. Cria a Ilha - Base de Areia
        GameObject islandBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        islandBase.name = "Ilha";
        islandBase.transform.position = new Vector3(0, -0.1f, 0);
        islandBase.transform.localScale = new Vector3(26, 0.6f, 26);
        islandBase.GetComponent<Renderer>().sharedMaterial = matSand;
        // Substitui o CapsuleCollider padrão (que vira uma bolha) por MeshCollider plano
        Object.DestroyImmediate(islandBase.GetComponent<Collider>());
        islandBase.AddComponent<MeshCollider>();

        // 6. Cria o Platô de Grama em cima da ilha
        GameObject grassTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        grassTop.name = "GramaTopo";
        grassTop.transform.position = new Vector3(0, 0.25f, 0);
        grassTop.transform.localScale = new Vector3(21, 0.4f, 21);
        grassTop.GetComponent<Renderer>().sharedMaterial = matGrass;
        // Substitui o CapsuleCollider padrão por MeshCollider plano
        Object.DestroyImmediate(grassTop.GetComponent<Collider>());
        grassTop.AddComponent<MeshCollider>();

        // 7. Cria o Jogador (Player)
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1.5f, 0);
        player.transform.localScale = Vector3.one;
        player.GetComponent<Renderer>().sharedMaterial = matPlayer;
        player.tag = "Player";

        // Remove o CapsuleCollider comum para usar o CharacterController que é muito melhor para movimentação
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 0, 0);
        cc.height = 2f;
        cc.radius = 0.5f;

        // Adiciona o script de movimentação
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 7f;
        pc.rotationSpeed = 14f;

        // 8. Configura a Câmera Principal
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null)
            {
                camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            }
            camFollow.target = player.transform;
            camFollow.offset = new Vector3(0, 10f, -9f);
            camFollow.smoothTime = 0.15f;

            mainCam.transform.position = player.transform.position + camFollow.offset;
            mainCam.transform.LookAt(player.transform.position + Vector3.up);
        }

        // 9. Cria o Gerenciador da Ilha
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<IslandGameManager>();

        // 10. Cria Moedas Coletáveis modeladas no Blender
        GameObject collectiblesGroup = new GameObject("Colecionaveis");
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Coin.fbx");

        Vector3[] itemPositions = new Vector3[]
        {
            new Vector3(4.5f, 1.1f, 4.5f),
            new Vector3(-5f, 1.1f, 3.5f),
            new Vector3(3f, 1.1f, -6f),
            new Vector3(-4.5f, 1.1f, -4.5f),
            new Vector3(0f, 1.1f, 7f),
            new Vector3(1.5f, 1.1f, 2f)
        };

        for (int i = 0; i < itemPositions.Length; i++)
        {
            GameObject coinObj;
            if (coinPrefab != null)
            {
                coinObj = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);
                coinObj.name = $"MoedaOuro_{i + 1}";
                coinObj.transform.localScale = Vector3.one * 0.9f;
                
                Renderer r = coinObj.GetComponentInChildren<Renderer>();
                if (r != null) r.sharedMaterial = matGold;

                SphereCollider sc = coinObj.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = 0.7f;
                sc.center = new Vector3(0, 0.6f, 0);
            }
            else
            {
                coinObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coinObj.name = $"GemaDourada_{i + 1}";
                coinObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                coinObj.GetComponent<Renderer>().sharedMaterial = matGold;
            }

            coinObj.transform.SetParent(collectiblesGroup.transform);
            coinObj.transform.position = itemPositions[i];

            CollectibleItem itemScript = coinObj.AddComponent<CollectibleItem>();
            itemScript.itemName = $"Moeda Dourada #{i + 1}";
            itemScript.points = 1;
        }

        // 11. Carrega e posiciona as Palmeiras modeladas no Blender
        GameObject palmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/PalmTree.fbx");
        if (palmPrefab != null)
        {
            GameObject palmGroup = new GameObject("Palmeiras");
            Vector3[] palmPositions = new Vector3[]
            {
                new Vector3(6f, 0.4f, 2f),
                new Vector3(-6f, 0.4f, -3f),
                new Vector3(-2f, 0.4f, 6.5f),
                new Vector3(5f, 0.4f, -5f)
            };
            float[] rotations = new float[] { 25f, 110f, 215f, 305f };

            for (int p = 0; p < palmPositions.Length; p++)
            {
                GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(palmPrefab);
                tree.name = $"Palmeira_{p + 1}";
                tree.transform.SetParent(palmGroup.transform);
                tree.transform.position = palmPositions[p];
                tree.transform.rotation = Quaternion.Euler(0, rotations[p], 0);
                tree.transform.localScale = Vector3.one * 0.9f;

                // Colisor no tronco para o jogador não atravessar a árvore
                CapsuleCollider treeCol = tree.AddComponent<CapsuleCollider>();
                treeCol.center = new Vector3(0, 2f, 0);
                treeCol.radius = 0.5f;
                treeCol.height = 4f;
            }
        }

        // 12. Carrega e posiciona o Baú do Tesouro modelado no Blender
        BuildChest();

        // 13. Carrega e posiciona Pedras Decorativas na Praia modeladas no Blender
        GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Rock.fbx");
        if (rockPrefab != null)
        {
            GameObject rockGroup = new GameObject("Pedras");
            Vector3[] rockPositions = new Vector3[]
            {
                new Vector3(8.5f, 0.05f, 2f),
                new Vector3(-8.5f, 0.05f, -3f),
                new Vector3(4f, 0.05f, 8.5f),
                new Vector3(-5f, 0.05f, 7.5f),
                new Vector3(7f, 0.05f, -6f),
                new Vector3(-4f, 0.05f, -8f)
            };
            float[] rockRotations = new float[] { 35f, 120f, 210f, 85f, 275f, 160f };
            float[] rockScales = new float[] { 1.1f, 0.8f, 1.3f, 0.9f, 1.2f, 0.75f };

            for (int r = 0; r < rockPositions.Length; r++)
            {
                GameObject rock = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                rock.name = $"Pedra_{r + 1}";
                rock.transform.SetParent(rockGroup.transform);
                rock.transform.position = rockPositions[r];
                rock.transform.rotation = Quaternion.Euler(0, rockRotations[r], 0);
                rock.transform.localScale = Vector3.one * rockScales[r];

                Renderer ren = rock.GetComponentInChildren<Renderer>();
                if (ren != null) ren.sharedMaterial = matRock;

                MeshCollider mc = rock.AddComponent<MeshCollider>();
                mc.convex = true;
            }
        }

        // 14. Salva a cena para persistir tudo
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=green><b>[TinyIsland] SUCESSO!</b></color> A ilha completa foi montada com Baú do Tesouro e Pedras decorativas!");
    }

    [MenuItem("TinyIsland/📦 Atualizar Baú do Tesouro")]
    public static GameObject BuildChest()
    {
        CleanUpOldObjects("BauDoTesouro");

        Material matGold = GetOrCreateMaterial("Assets/Materials/M_Ouro.mat", new Color(1.0f, 0.84f, 0.0f), 0.95f, 0.85f);
        Material matWood = GetOrCreateMaterial("Assets/Materials/M_ChestWood.mat", new Color(0.42f, 0.22f, 0.08f), 0f, 0.3f);
        Material matIron = GetOrCreateMaterial("Assets/Materials/M_ChestIron.mat", new Color(0.18f, 0.18f, 0.20f), 0.85f, 0.65f);
        Material matRuby = GetOrCreateMaterial("Assets/Materials/M_Ruby.mat", new Color(0.92f, 0.08f, 0.16f), 0.1f, 0.95f);
        Material matEmerald = GetOrCreateMaterial("Assets/Materials/M_Emerald.mat", new Color(0.05f, 0.85f, 0.35f), 0.1f, 0.95f);
        Material matSapphire = GetOrCreateMaterial("Assets/Materials/M_Sapphire.mat", new Color(0.10f, 0.38f, 0.95f), 0.1f, 0.95f);

        GameObject chestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TreasureChest.fbx");
        if (chestPrefab == null)
        {
            Debug.LogError("[TinyIsland] TreasureChest.fbx não encontrado em Assets/Models/TreasureChest.fbx!");
            return null;
        }

        GameObject chestObj = (GameObject)PrefabUtility.InstantiatePrefab(chestPrefab);
        chestObj.name = "BauDoTesouro";
        chestObj.transform.position = new Vector3(0f, 0.46f, 5.5f);
        chestObj.transform.rotation = Quaternion.Euler(0, 180f, 0);
        chestObj.transform.localScale = Vector3.one * 1.35f;

        // Materiais do baú (madeira, ferro, ouro) e do tesouro (ouro, rubi, esmeralda, safira)
        Material[] chestMultiMats = new Material[] { matWood, matIron, matGold };
        Material[] goldPileMultiMats = new Material[] { matWood, matIron, matGold, matRuby, matEmerald, matSapphire };

        Renderer[] renderers = chestObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.name.Contains("GoldPile"))
            {
                r.sharedMaterials = goldPileMultiMats;
            }
            else
            {
                r.sharedMaterials = chestMultiMats;
            }
        }

        // Colisor físico sólido para a base do baú
        BoxCollider chestCol = chestObj.AddComponent<BoxCollider>();
        chestCol.center = new Vector3(0, 0.38f, 0);
        chestCol.size = new Vector3(1.1f, 0.76f, 0.75f);

        // Script de animação e controle do baú
        TreasureChest chestScript = chestObj.AddComponent<TreasureChest>();
        chestScript.targetOpenAngle = -100f;
        chestScript.openDuration = 1.2f;

        Transform lidT = chestObj.transform.Find("Chest_Lid");
        if (lidT == null) lidT = chestObj.transform.Find("Chest_Base/Chest_Lid");
        chestScript.lidTransform = lidT;

        // Luz suave dourada que emana do tesouro ao abrir
        GameObject lightObj = new GameObject("TreasureGlowLight");
        lightObj.transform.SetParent(chestObj.transform);
        lightObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        Light tLight = lightObj.AddComponent<Light>();
        tLight.type = LightType.Point;
        tLight.color = new Color(1.0f, 0.85f, 0.3f);
        tLight.range = 6f;
        tLight.intensity = 3.5f;
        tLight.enabled = false;
        chestScript.treasureLight = tLight;

        // Partículas festivas de ouro/estrelas ao abrir
        GameObject fxObj = new GameObject("ChestOpenParticles");
        fxObj.transform.SetParent(chestObj.transform);
        fxObj.transform.localPosition = new Vector3(0, 0.45f, 0);
        ParticleSystem ps = fxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 1.5f;
        main.loop = false;
        main.startLifetime = 1.2f;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.startColor = new Color(1.0f, 0.85f, 0.2f);
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 30) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.3f;
        chestScript.openParticles = ps;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("<color=green><b>[TinyIsland]</b></color> Baú do Tesouro atualizado com sucesso com laterais sólidas, gemas e efeitos!");
        return chestObj;
    }

    private static Material GetOrCreateMaterial(string path, Color color, float metallic = 0f, float smoothness = 0.5f)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            // Tenta usar o shader do URP Lit, com fallback para Standard
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    private static void CleanUpOldObjects(params string[] names)
    {
        foreach (string name in names)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
#endif
