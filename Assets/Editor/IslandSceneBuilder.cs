#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Construtor Automático da Cena Tiny Island.
/// Monta o cenário inteiro (ilha, oceano, materiais coloridos, jogador, câmera, itens e manager)
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
            // Se já tiver um jogador na cena, não sobrescreve automaticamente
            if (GameObject.Find("Player") == null && GameObject.Find("Ilha") == null)
            {
                Debug.Log("<color=cyan>[TinyIsland]</color> Primeira inicialização detectada! Construindo a ilha automaticamente...");
                BuildCompleteIsland();
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

        // 2. Cria os materiais estilizados (Cores URP)
        Material matGrass = GetOrCreateMaterial("Assets/Materials/M_Grama.mat", new Color(0.28f, 0.75f, 0.35f));
        Material matSand = GetOrCreateMaterial("Assets/Materials/M_Areia.mat", new Color(0.92f, 0.82f, 0.58f));
        Material matWater = GetOrCreateMaterial("Assets/Materials/M_Oceano.mat", new Color(0.15f, 0.65f, 0.85f));
        Material matPlayer = GetOrCreateMaterial("Assets/Materials/M_Player.mat", new Color(0.95f, 0.42f, 0.22f));
        Material matGold = GetOrCreateMaterial("Assets/Materials/M_Ouro.mat", new Color(1.0f, 0.84f, 0.0f));

        // 3. Limpa objetos antigos se existirem (para não duplicar)
        CleanUpOldObjects("Ilha", "GramaTopo", "Oceano", "Player", "GameManager", "Colecionaveis", "Palmeiras");

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

        // 12. Salva a cena para persistir tudo
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=green><b>[TinyIsland] SUCESSO!</b></color> A ilha completa foi montada, colorida e configurada!");
    }

    private static Material GetOrCreateMaterial(string path, Color color)
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
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
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
