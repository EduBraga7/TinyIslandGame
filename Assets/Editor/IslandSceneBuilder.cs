#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Construtor Automático do Arquipélago Completo de Tiny Island.
/// Monta o cenário inteiro (Ilha Principal em 3 níveis, Ilhota do Santuário a Leste,
/// Ilhota do Recife a Oeste, Píer Sul, Veleiro, Ponte de Madeira, Farol Tropical,
/// Acampamento com Fogueira, Ruínas Antigas, Vegetação Rica e 12 Moedas em locais estratégicos).
/// </summary>
public static class IslandSceneBuilder
{
    [InitializeOnLoadMethod]
    private static void AutoSetupCheck()
    {
        EditorApplication.delayCall += () =>
        {
            if (GameObject.Find("Player") == null && GameObject.Find("Ilha") == null)
            {
                Debug.Log("<color=cyan>[TinyIsland]</color> Primeira inicialização detectada! Construindo o arquipélago completo...");
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

        // 2. Cria/Atualiza todos os materiais estilizados (URP Lit)
        Material matGrass       = GetOrCreateMaterial("Assets/Materials/M_Grama.mat",       new Color(0.28f, 0.76f, 0.34f), 0f, 0.22f);
        Material matGrassHill   = GetOrCreateMaterial("Assets/Materials/M_GramaColina.mat", new Color(0.22f, 0.68f, 0.30f), 0f, 0.22f);
        Material matSand        = GetOrCreateMaterial("Assets/Materials/M_Areia.mat",       new Color(0.94f, 0.84f, 0.58f), 0f, 0.12f);
        Material matWaterDeep   = GetOrCreateMaterial("Assets/Materials/M_Oceano.mat",      new Color(0.09f, 0.48f, 0.78f), 0.15f, 0.92f);
        Material matWaterShallow= GetOrCreateMaterial("Assets/Materials/M_AguaRasa.mat",    new Color(0.18f, 0.78f, 0.86f), 0.10f, 0.90f);
        Material matGold        = GetOrCreateMaterial("Assets/Materials/M_Ouro.mat",        new Color(1.00f, 0.84f, 0.05f), 0.95f, 0.88f, true, new Color(0.42f, 0.30f, 0.02f));
        Material matWood        = GetOrCreateMaterial("Assets/Materials/M_ChestWood.mat",   new Color(0.46f, 0.26f, 0.11f), 0f, 0.30f);
        Material matWoodDark    = GetOrCreateMaterial("Assets/Materials/M_WoodDark.mat",    new Color(0.32f, 0.18f, 0.08f), 0f, 0.25f);
        Material matRope        = GetOrCreateMaterial("Assets/Materials/M_Rope.mat",        new Color(0.80f, 0.68f, 0.44f), 0f, 0.20f);
        Material matIron        = GetOrCreateMaterial("Assets/Materials/M_ChestIron.mat",   new Color(0.18f, 0.19f, 0.22f), 0.85f, 0.65f);
        Material matRock        = GetOrCreateMaterial("Assets/Materials/M_Rock.mat",        new Color(0.52f, 0.51f, 0.49f), 0f, 0.20f);
        Material matPathStone   = GetOrCreateMaterial("Assets/Materials/M_PathStone.mat",   new Color(0.64f, 0.62f, 0.58f), 0f, 0.25f);
        Material matWhiteWall   = GetOrCreateMaterial("Assets/Materials/M_WhiteWall.mat",   new Color(0.96f, 0.94f, 0.88f), 0f, 0.35f);
        Material matCoralRed    = GetOrCreateMaterial("Assets/Materials/M_CoralRed.mat",    new Color(0.88f, 0.22f, 0.18f), 0f, 0.40f);
        Material matGlowLight   = GetOrCreateMaterial("Assets/Materials/M_GlowYellow.mat",  new Color(1.00f, 0.92f, 0.40f), 0.2f, 0.95f, true, new Color(1.0f, 0.85f, 0.25f) * 2.2f);
        Material matTentOrange  = GetOrCreateMaterial("Assets/Materials/M_TentOrange.mat",  new Color(0.94f, 0.48f, 0.15f), 0f, 0.30f);
        Material matFireEmber   = GetOrCreateMaterial("Assets/Materials/M_FireEmber.mat",   new Color(1.00f, 0.42f, 0.05f), 0f, 0.80f, true, new Color(1.0f, 0.45f, 0.05f) * 3.0f);
        Material matCrystal     = GetOrCreateMaterial("Assets/Materials/M_Crystal.mat",     new Color(0.12f, 0.90f, 0.96f), 0.25f, 0.95f, true, new Color(0.08f, 0.75f, 0.95f) * 1.8f);
        Material matBushLeaf    = GetOrCreateMaterial("Assets/Materials/M_BushLeaf.mat",    new Color(0.20f, 0.68f, 0.28f), 0f, 0.25f);
        Material matFlowerPink  = GetOrCreateMaterial("Assets/Materials/M_FlowerPink.mat",  new Color(0.96f, 0.28f, 0.58f), 0f, 0.45f);
        Material matFoam        = GetOrCreateTransparentMaterial("Assets/Materials/M_EspumaMar.mat", new Color(1.0f, 1.0f, 1.0f, 0.72f), 0.92f);

        // 3. Limpa objetos antigos da cena para reconstruir do zero sem duplicatas
        CleanUpOldObjects(
            "Ilha", "GramaTopo", "Oceano", "Player", "GameManager",
            "Colecionaveis", "Palmeiras", "BauDoTesouro", "Pedras",
            "Estruturas", "Vegetacao", "Trilhas", "EspumaMar", "GlobalVolume"
        );

        // =========================================================================
        // 4. OCEANO PROFUNDO, LAGOAS COSTEIRAS E ESPUMA COSTEIRA DINÂMICA
        // =========================================================================
        GameObject oceanGroup = new GameObject("Oceano");

        GameObject deepOcean = GameObject.CreatePrimitive(PrimitiveType.Plane);
        deepOcean.name = "OceanoProfundo";
        deepOcean.transform.SetParent(oceanGroup.transform);
        deepOcean.transform.position = new Vector3(0f, -0.45f, 0f);
        deepOcean.transform.localScale = new Vector3(20f, 1f, 20f); // 200x200m
        deepOcean.GetComponent<Renderer>().sharedMaterial = matWaterDeep;
        Object.DestroyImmediate(deepOcean.GetComponent<Collider>());

        // Anéis de água rasa turquesa ao redor das 3 ilhas
        CreateShallowRing(oceanGroup.transform, "LagoaPrincipal", new Vector3(0f, -0.36f, 0f), new Vector3(44f, 0.06f, 44f), matWaterShallow);
        CreateShallowRing(oceanGroup.transform, "LagoaSantuario", new Vector3(26.5f, -0.36f, 2f), new Vector3(19f, 0.06f, 19f), matWaterShallow);
        CreateShallowRing(oceanGroup.transform, "LagoaRecife",    new Vector3(-26.5f, -0.36f, -3f), new Vector3(14f, 0.06f, 14f), matWaterShallow);

        // Anéis de Espuma Costeira Dinâmica (com respiração de maré e variação de opacidade)
        GameObject foamGroup = new GameObject("EspumaMar");
        foamGroup.transform.SetParent(oceanGroup.transform);
        CreateShorelineFoamRing(foamGroup.transform, "EspumaCosteira_Principal", new Vector3(0f, -0.315f, 0f), new Vector3(40.6f, 0.035f, 40.6f), matFoam, 0f, 0.90f, 0.035f);
        CreateShorelineFoamRing(foamGroup.transform, "EspumaCosteira_Santuario", new Vector3(26.5f, -0.315f, 2.0f), new Vector3(17.5f, 0.035f, 17.5f), matFoam, 1.2f, 0.85f, 0.032f);
        CreateShorelineFoamRing(foamGroup.transform, "EspumaCosteira_Recife",    new Vector3(-26.5f, -0.315f, -3.0f), new Vector3(12.2f, 0.035f, 12.2f), matFoam, 2.4f, 0.95f, 0.038f);

        // Marolas de espuma ao redor das pedras de travessia marítima
        CreateShorelineFoamRing(foamGroup.transform, "EspumaPedra_1", new Vector3(-18.2f, -0.315f, -2.8f), new Vector3(3.3f, 0.025f, 3.3f), matFoam, 0.7f, 1.1f, 0.045f);
        CreateShorelineFoamRing(foamGroup.transform, "EspumaPedra_2", new Vector3(-20.4f, -0.315f, -3.3f), new Vector3(3.2f, 0.025f, 3.2f), matFoam, 1.8f, 1.1f, 0.045f);
        CreateShorelineFoamRing(foamGroup.transform, "EspumaPedra_3", new Vector3(-22.5f, -0.315f, -2.7f), new Vector3(3.3f, 0.025f, 3.3f), matFoam, 2.9f, 1.1f, 0.045f);

        // =========================================================================
        // 5. GEOMETRIA DO ARQUIPÉLAGO (ILHA PRINCIPAL EM NÍVEIS + 2 ILHOTAS)
        // =========================================================================
        GameObject islandGroup = new GameObject("Ilha");

        // 5.1 Ilha Principal - Praia de Areia (Raio 19m, Topo em Y = 0.45)
        CreateCylinderTerrain(islandGroup.transform, "PraiaPrincipal", new Vector3(0f, -0.15f, 0f), new Vector3(38f, 0.60f, 38f), matSand);

        // 5.2 Ilha Principal - Platô Central de Grama (Raio 15.5m, Topo em Y = 0.65)
        CreateCylinderTerrain(islandGroup.transform, "GramaTopo", new Vector3(0f, 0.25f, 0f), new Vector3(31f, 0.40f, 31f), matGrass);

        // 5.3 Colina Norte - Nível 1 (Topo em Y = 1.45, borda frontal em Z = 0.7)
        CreateCylinderTerrain(islandGroup.transform, "ColinaNorte_Nivel1", new Vector3(0f, 0.85f, 7.2f), new Vector3(15.5f, 0.60f, 13.0f), matGrassHill);

        // Escadaria larga subindo do Platô (Y=0.65) para a Colina Nível 1 (Y=1.45)
        CreateStepPlatform(islandGroup.transform, "DegrauColina_1", new Vector3(0f, 0.65f, -0.55f), new Vector3(3.6f, 0.40f, 1.4f), matPathStone); // Topo 0.85
        CreateStepPlatform(islandGroup.transform, "DegrauColina_2", new Vector3(0f, 0.85f,  0.10f), new Vector3(3.4f, 0.40f, 1.4f), matPathStone); // Topo 1.05
        CreateStepPlatform(islandGroup.transform, "DegrauColina_3", new Vector3(0f, 1.05f,  0.75f), new Vector3(3.2f, 0.40f, 1.4f), matPathStone); // Topo 1.25

        // 5.4 Colina Norte - Nível 2 / Mirante do Farol (Topo em Y = 2.25, borda frontal em Z = 4.4)
        CreateCylinderTerrain(islandGroup.transform, "ColinaNorte_Mirante", new Vector3(0f, 1.55f, 9.2f), new Vector3(9.6f, 0.70f, 9.6f), matGrass);

        // Escadaria subindo do Nível 1 (Y=1.45) para o Mirante do Farol (Y=2.25)
        CreateStepPlatform(islandGroup.transform, "DegrauMirante_1", new Vector3(-1.4f, 1.45f, 3.15f), new Vector3(2.8f, 0.40f, 1.3f), matPathStone); // Topo 1.65
        CreateStepPlatform(islandGroup.transform, "DegrauMirante_2", new Vector3(-1.2f, 1.65f, 3.80f), new Vector3(2.6f, 0.40f, 1.3f), matPathStone); // Topo 1.85
        CreateStepPlatform(islandGroup.transform, "DegrauMirante_3", new Vector3(-1.0f, 1.85f, 4.45f), new Vector3(2.4f, 0.40f, 1.3f), matPathStone); // Topo 2.05

        // 5.5 Ilhota Leste - Santuário das Ruínas e do Baú (Raio 7.5m areia, 6m grama, Topo em Y = 0.65)
        CreateCylinderTerrain(islandGroup.transform, "IlhotaSantuario_Areia", new Vector3(26.5f, -0.15f, 2.0f), new Vector3(15.5f, 0.60f, 15.5f), matSand);
        CreateCylinderTerrain(islandGroup.transform, "IlhotaSantuario_Grama", new Vector3(26.5f, 0.25f, 2.0f), new Vector3(12.2f, 0.40f, 12.2f), matGrass);

        // 5.6 Ilhota Oeste - Recife das Palmeiras (Topo areia Y = 0.45, Topo grama Y = 0.75)
        CreateCylinderTerrain(islandGroup.transform, "IlhotaRecife_Areia", new Vector3(-26.5f, -0.15f, -3.0f), new Vector3(10.5f, 0.60f, 10.5f), matSand);
        CreateCylinderTerrain(islandGroup.transform, "IlhotaRecife_Grama", new Vector3(-26.5f, 0.35f, -3.0f), new Vector3(7.4f, 0.40f, 7.4f), matGrassHill);

        // Pedras de travessia sobre o mar (Stepping Stones entre a Praia Oeste X=-17.5 e a Ilhota do Recife X=-22.5)
        CreateCylinderTerrain(islandGroup.transform, "PedraTravessia_1", new Vector3(-18.2f, 0.15f, -2.8f), new Vector3(2.5f, 0.35f, 2.5f), matPathStone);
        CreateCylinderTerrain(islandGroup.transform, "PedraTravessia_2", new Vector3(-20.4f, 0.20f, -3.3f), new Vector3(2.4f, 0.35f, 2.4f), matPathStone);
        CreateCylinderTerrain(islandGroup.transform, "PedraTravessia_3", new Vector3(-22.5f, 0.25f, -2.7f), new Vector3(2.5f, 0.35f, 2.5f), matPathStone);

        // =========================================================================
        // 6. TRILHAS DE LAJES DE PEDRA NA ILHA PRINCIPAL
        // =========================================================================
        GameObject pathsGroup = new GameObject("Trilhas");
        Vector3[] steppingStones = new Vector3[]
        {
            // Trilha Sul (até o Píer)
            new Vector3(0f, 0.66f, -4.5f),
            new Vector3(0.3f, 0.66f, -7.0f),
            new Vector3(-0.2f, 0.66f, -9.5f),
            new Vector3(0.1f, 0.66f, -12.0f),
            new Vector3(0f, 0.56f, -14.8f),
            // Trilha Leste (até a Ponte do Santuário)
            new Vector3(3.0f, 0.66f, -1.0f),
            new Vector3(5.6f, 0.66f, 0.2f),
            new Vector3(8.2f, 0.66f, 1.2f),
            new Vector3(10.8f, 0.66f, 1.8f),
            // Trilha Oeste (até o Acampamento e Recife)
            new Vector3(-3.0f, 0.66f, -1.8f),
            new Vector3(-5.8f, 0.66f, -1.6f),
            new Vector3(-11.8f, 0.66f, -2.2f),
            new Vector3(-14.5f, 0.56f, -2.6f)
        };
        for (int s = 0; s < steppingStones.Length; s++)
        {
            GameObject slab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            slab.name = $"LajeTrilha_{s + 1}";
            slab.transform.SetParent(pathsGroup.transform);
            slab.transform.position = steppingStones[s];
            slab.transform.localScale = new Vector3(1.5f, 0.05f, 1.5f);
            slab.GetComponent<Renderer>().sharedMaterial = matPathStone;
            Object.DestroyImmediate(slab.GetComponent<Collider>());
        }

        // =========================================================================
        // 7. ESTRUTURAS 3D (FAROL, PÍER, VELEIRO, PONTE, ACAMPAMENTO, RUÍNAS)
        // =========================================================================
        GameObject structuresGroup = new GameObject("Estruturas");

        // 7.1 Farol Tropical no topo do Mirante Norte (Y = 2.25)
        GameObject lighthousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Lighthouse.fbx");
        if (lighthousePrefab != null)
        {
            GameObject lh = (GameObject)PrefabUtility.InstantiatePrefab(lighthousePrefab);
            lh.name = "FarolTropical";
            lh.transform.SetParent(structuresGroup.transform);
            lh.transform.position = new Vector3(0f, 2.25f, 10.2f);
            lh.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            lh.transform.localScale = Vector3.one * 1.15f;

            AssignMaterials(lh, new Material[] { matRock, matWhiteWall, matCoralRed, matIron, matGlowLight });
            SetupAccurateMeshColliders(lh);

            // Luz quente no topo do Farol
            GameObject lhLightObj = new GameObject("LighthouseBeaconLight");
            lhLightObj.transform.SetParent(lh.transform);
            lhLightObj.transform.localPosition = new Vector3(0f, 5.25f, 0f);
            Light lhLight = lhLightObj.AddComponent<Light>();
            lhLight.type = LightType.Point;
            lhLight.color = new Color(1.0f, 0.88f, 0.45f);
            lhLight.range = 18f;
            lhLight.intensity = 4.5f;
        }

        // 7.2 Píer de Madeira na Costa Sul
        GameObject dockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/WoodenDock.fbx");
        if (dockPrefab != null)
        {
            GameObject dock = (GameObject)PrefabUtility.InstantiatePrefab(dockPrefab);
            dock.name = "PierDeMadeira";
            dock.transform.SetParent(structuresGroup.transform);
            dock.transform.position = new Vector3(0f, -0.05f, -19.8f);
            dock.transform.rotation = Quaternion.identity;
            dock.transform.localScale = Vector3.one * 1.1f;

            AssignMaterials(dock, new Material[] { matWood, matWoodDark, matRope });
            // Colisão física exata em todas as tábuas, vigas e em todos os postes laterais
            SetupAccurateMeshColliders(dock);
        }

        // 7.3 Veleiro Tropical ancorado ao lado do Píer Sul
        GameObject boatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/SailBoat.fbx");
        if (boatPrefab != null)
        {
            GameObject boat = (GameObject)PrefabUtility.InstantiatePrefab(boatPrefab);
            boat.name = "VeleiroTropical";
            boat.transform.SetParent(structuresGroup.transform);
            boat.transform.position = new Vector3(3.1f, -0.22f, -21.2f);
            boat.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            boat.transform.localScale = Vector3.one * 1.15f;

            AssignMaterials(boat, new Material[] { matWood, matGold, matWhiteWall, matCoralRed });
            // Colisão física exata no casco, convés, mastro, verga, vela e na bandeira
            SetupAccurateMeshColliders(boat);
        }

        // 7.4 Ponte Arqueada de Madeira (conectando Ilha Principal X=13.5 à Ilhota do Santuário X=21.5)
        GameObject bridgePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/WoodenBridge.fbx");
        if (bridgePrefab != null)
        {
            GameObject bridge = (GameObject)PrefabUtility.InstantiatePrefab(bridgePrefab);
            bridge.name = "PonteDeMadeira";
            bridge.transform.SetParent(structuresGroup.transform);
            bridge.transform.position = new Vector3(17.2f, 0.45f, 2.0f);
            bridge.transform.rotation = Quaternion.identity;
            bridge.transform.localScale = new Vector3(1.18f, 1.0f, 1.05f);

            AssignMaterials(bridge, new Material[] { matWood, matWoodDark, matRope });
            SetupAccurateMeshColliders(bridge);
        }

        // 7.5 Acampamento do Explorador (Barraca + Fogueira + Troncos) no Platô Oeste
        GameObject campPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/CampSite.fbx");
        if (campPrefab != null)
        {
            GameObject camp = (GameObject)PrefabUtility.InstantiatePrefab(campPrefab);
            camp.name = "AcampamentoExplorador";
            camp.transform.SetParent(structuresGroup.transform);
            camp.transform.position = new Vector3(-8.2f, 0.65f, -1.8f);
            camp.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            camp.transform.localScale = Vector3.one * 1.1f;

            AssignMaterials(camp, new Material[] { matTentOrange, matWood, matRock, matFireEmber });
            // Colisão física exata em toda a lona e paredes da barraca, nos troncos de sentar e nas pedras da fogueira
            SetupAccurateMeshColliders(camp);

            // Luz quente da fogueira
            GameObject fireLightObj = new GameObject("CampfireLight");
            fireLightObj.transform.SetParent(camp.transform);
            fireLightObj.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            Light fireLight = fireLightObj.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1.0f, 0.55f, 0.15f);
            fireLight.range = 9f;
            fireLight.intensity = 3.2f;

            // Sistema de partículas de fogo/brasas subindo da fogueira
            GameObject fireFx = new GameObject("CampfireParticles");
            fireFx.transform.SetParent(camp.transform);
            fireFx.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            ParticleSystem ps = fireFx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 0.9f;
            main.startSpeed = 1.4f;
            main.startSize = 0.18f;
            main.startColor = new Color(1.0f, 0.60f, 0.12f, 0.9f);
            var emission = ps.emission;
            emission.rateOverTime = 14f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.22f;
        }

        // 7.6 Santuário das Ruínas Antigas na Ilhota Leste
        GameObject ruinsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/AncientRuins.fbx");
        if (ruinsPrefab != null)
        {
            GameObject ruins = (GameObject)PrefabUtility.InstantiatePrefab(ruinsPrefab);
            ruins.name = "SantuarioRuinas";
            ruins.transform.SetParent(structuresGroup.transform);
            ruins.transform.position = new Vector3(27.8f, 0.65f, 2.0f);
            ruins.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            ruins.transform.localScale = Vector3.one * 1.15f;

            AssignMaterials(ruins, new Material[] { matRock, matGrassHill, matCrystal });
            SetupAccurateMeshColliders(ruins);
        }

        // =========================================================================
        // 8. BAÚ DO TESOURO LENDÁRIO (NO ALTAR DO SANTUÁRIO LESTE)
        // =========================================================================
        BuildChest();

        // =========================================================================
        // 9. VEGETAÇÃO RICA (PALMEIRAS, ARBUSTOS FLORIDOS E PEDRAS)
        // =========================================================================
        GameObject palmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/PalmTree.fbx");
        if (palmPrefab != null)
        {
            GameObject palmGroup = new GameObject("Palmeiras");
            (Vector3 pos, float rot, float scale)[] palms = new (Vector3, float, float)[]
            {
                // Ilha Principal - Praias e Platô
                (new Vector3(9.5f,  0.65f,  6.5f),  25f, 1.05f),
                (new Vector3(-9.5f, 0.65f,  5.8f), 110f, 0.95f),
                (new Vector3(-12.2f,0.65f,  2.2f), 200f, 1.10f),
                (new Vector3(-10.8f,0.65f, -7.5f), 315f, 0.90f),
                (new Vector3(10.2f, 0.65f, -6.8f),  75f, 1.00f),
                (new Vector3(6.5f,  0.65f,-11.2f), 150f, 0.85f),
                (new Vector3(-6.2f, 0.65f,-11.5f), 245f, 1.05f),
                (new Vector3(14.8f, 0.45f, -9.5f),  40f, 0.88f),
                (new Vector3(-15.2f,0.45f,  8.5f), 130f, 0.92f),
                // Colina Norte
                (new Vector3(5.2f,  1.45f,  8.5f),  85f, 0.85f),
                (new Vector3(-5.2f, 1.45f,  8.8f), 265f, 0.82f),
                // Ilhota do Santuário (Leste)
                (new Vector3(25.0f, 0.65f,  6.2f),  35f, 0.95f),
                (new Vector3(25.2f, 0.65f, -2.4f), 195f, 0.90f),
                (new Vector3(30.8f, 0.45f,  2.2f), 120f, 1.10f),
                // Ilhota do Recife (Oeste)
                (new Vector3(-27.8f,0.75f, -1.2f),  60f, 0.95f),
                (new Vector3(-25.8f,0.75f, -5.0f), 210f, 0.80f)
            };

            for (int p = 0; p < palms.Length; p++)
            {
                GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(palmPrefab);
                tree.name = $"Palmeira_{p + 1}";
                tree.transform.SetParent(palmGroup.transform);
                tree.transform.position = palms[p].pos;
                tree.transform.rotation = Quaternion.Euler(0f, palms[p].rot, 0f);
                tree.transform.localScale = Vector3.one * palms[p].scale;

                AssignMaterials(tree, new Material[] { matWood, matWoodDark, matBushLeaf, matGrass });

                CapsuleCollider treeCol = tree.AddComponent<CapsuleCollider>();
                treeCol.center = new Vector3(0f, 2f, 0f);
                treeCol.radius = 0.42f;
                treeCol.height = 4f;
            }
        }

        // Arbustos Floridos Tropicais
        GameObject bushPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TropicalBush.fbx");
        if (bushPrefab != null)
        {
            GameObject vegGroup = new GameObject("Vegetacao");
            Vector3[] bushPositions = new Vector3[]
            {
                new Vector3(4.2f,  0.65f,  3.5f),
                new Vector3(-4.5f, 0.65f,  3.2f),
                new Vector3(-10.5f,0.65f,  4.0f),
                new Vector3(-6.8f, 0.65f, -4.8f),
                new Vector3(7.5f,  0.65f, -4.2f),
                new Vector3(3.2f,  0.65f, -9.5f),
                new Vector3(-3.5f, 0.65f, -9.2f),
                new Vector3(4.0f,  1.45f,  6.2f),
                new Vector3(-4.2f, 1.45f,  6.5f),
                new Vector3(2.8f,  2.25f, 10.5f),
                new Vector3(-2.8f, 2.25f, 10.2f),
                new Vector3(23.8f, 0.65f,  4.5f),
                new Vector3(23.8f, 0.65f, -0.5f),
                new Vector3(-25.2f,0.75f, -1.5f)
            };

            for (int b = 0; b < bushPositions.Length; b++)
            {
                GameObject bush = (GameObject)PrefabUtility.InstantiatePrefab(bushPrefab);
                bush.name = $"ArbustoFloral_{b + 1}";
                bush.transform.SetParent(vegGroup.transform);
                bush.transform.position = bushPositions[b];
                bush.transform.rotation = Quaternion.Euler(0f, (b * 67f) % 360f, 0f);
                float sc = 0.85f + (b % 3) * 0.18f;
                bush.transform.localScale = Vector3.one * sc;

                AssignMaterials(bush, new Material[] { matBushLeaf, matFlowerPink, matGold });
            }
        }

        // Pedras Decorativas e Costeiras
        GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Rock.fbx");
        if (rockPrefab != null)
        {
            GameObject rockGroup = new GameObject("Pedras");
            (Vector3 pos, float rot, float scale)[] rocks = new (Vector3, float, float)[]
            {
                (new Vector3(14.5f, 0.45f,  7.5f),  35f, 1.25f),
                (new Vector3(-14.2f,0.45f, -8.5f), 120f, 1.10f),
                (new Vector3(6.5f,  0.45f,-15.5f), 210f, 1.35f),
                (new Vector3(-6.8f, 0.45f,-15.2f),  85f, 0.95f),
                (new Vector3(12.2f, 0.45f,-11.5f), 275f, 1.20f),
                (new Vector3(-13.5f,0.45f, 11.2f), 160f, 1.40f),
                (new Vector3(5.8f,  1.45f, 10.5f),  45f, 1.15f),
                (new Vector3(-5.8f, 1.45f, 10.8f), 190f, 1.05f),
                (new Vector3(31.2f, 0.45f,  5.5f),  70f, 1.30f),
                (new Vector3(31.0f, 0.45f, -1.8f), 230f, 1.10f),
                (new Vector3(-29.5f,0.45f, -5.2f), 140f, 1.25f),
                (new Vector3(-29.0f,0.45f,  0.5f), 310f, 0.95f)
            };

            for (int r = 0; r < rocks.Length; r++)
            {
                GameObject rock = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                rock.name = $"Pedra_{r + 1}";
                rock.transform.SetParent(rockGroup.transform);
                rock.transform.position = rocks[r].pos;
                rock.transform.rotation = Quaternion.Euler(0f, rocks[r].rot, 0f);
                rock.transform.localScale = Vector3.one * rocks[r].scale;

                Renderer ren = rock.GetComponentInChildren<Renderer>();
                if (ren != null) ren.sharedMaterial = matRock;

                MeshCollider mc = rock.AddComponent<MeshCollider>();
                mc.convex = true;
            }
        }

        // =========================================================================
        // 10. 12 MOEDAS DE OURO EM DIFERENTES LOCAIS DO ARQUIPÉLAGO
        // =========================================================================
        GameObject collectiblesGroup = new GameObject("Colecionaveis");
        GameObject coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Coin.fbx");

        (Vector3 pos, string label)[] coinLocations = new (Vector3, string)[]
        {
            // 1. Costa Sul: Na ponta do Píer de Madeira
            (new Vector3(0.0f,   0.78f, -22.5f), "Moeda do Píer Sul"),
            // 2. Costa Sul: Em cima do convés do Veleiro ancorado
            (new Vector3(3.1f,   0.85f, -21.2f), "Moeda do Veleiro"),
            // 3. Praia Sudoeste: Na areia dourada perto das rochas costeiras
            (new Vector3(-11.5f, 0.65f, -12.2f), "Moeda da Praia Sudoeste"),
            // 4. Platô Oeste: Ao lado da fogueira no Acampamento do Explorador
            (new Vector3(-6.8f,  0.85f,  -2.2f), "Moeda do Acampamento"),
            // 5. Bosque Noroeste: Entre os coqueiros e arbustos floridos
            (new Vector3(-10.8f, 0.85f,   4.8f), "Moeda do Bosque Tropical"),
            // 6. Travessia Oeste: Sobre a pedra central no mar rumo ao Recife
            (new Vector3(-20.4f, 0.65f,  -3.3f), "Moeda das Pedras do Mar"),
            // 7. Ilhota Oeste: No topo da Ilhota do Recife
            (new Vector3(-26.5f, 0.95f,  -3.0f), "Moeda da Ilhota do Recife"),
            // 8. Colina Norte (Nível 1): No patamar intermediário da subida
            (new Vector3(2.8f,   1.65f,   5.8f), "Moeda da Encosta Norte"),
            // 9. Colina Norte (Mirante): Ao pé do Farol Tropical
            (new Vector3(2.2f,   2.45f,   9.2f), "Moeda do Mirante do Farol"),
            // 10. Segredo Norte: Escondida atrás da torre do Farol no penhasco
            (new Vector3(0.0f,   2.45f,  12.6f), "Moeda Secreta do Farol"),
            // 11. Travessia Leste: No alto do arco da Ponte de Madeira
            (new Vector3(17.2f,  1.35f,   2.0f), "Moeda da Ponte de Madeira"),
            // 12. Ilhota Leste: Na entrada do Santuário das Ruínas Antigas
            (new Vector3(24.2f,  0.85f,   2.0f), "Moeda do Santuário Antigo")
        };

        for (int i = 0; i < coinLocations.Length; i++)
        {
            GameObject coinObj;
            if (coinPrefab != null)
            {
                coinObj = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);
                coinObj.name = $"MoedaOuro_{i + 1}";
                coinObj.transform.localScale = Vector3.one * 1.15f;
                coinObj.transform.rotation = Quaternion.Euler(0f, (i * 35f) % 360f, 0f);

                Renderer r = coinObj.GetComponentInChildren<Renderer>();
                if (r != null) r.sharedMaterial = matGold;

                SphereCollider sc = coinObj.AddComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = 0.85f;
                sc.center = new Vector3(0f, 0.55f, 0f);
            }
            else
            {
                coinObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coinObj.name = $"MoedaOuro_{i + 1}";
                coinObj.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                coinObj.GetComponent<Renderer>().sharedMaterial = matGold;
            }

            coinObj.transform.SetParent(collectiblesGroup.transform);
            coinObj.transform.position = coinLocations[i].pos;

            CollectibleItem itemScript = coinObj.AddComponent<CollectibleItem>();
            itemScript.itemName = coinLocations[i].label;
            itemScript.points = 1;
        }

        // =========================================================================
        // 11. JOGADOR E GERENCIADOR DE JOGO
        // =========================================================================
        BuildPlayer();

        GameObject gmObj = new GameObject("GameManager");
        IslandGameManager gm = gmObj.AddComponent<IslandGameManager>();
        gm.targetCoins = coinLocations.Length;
        gmObj.AddComponent<IslandAtmosphereManager>();

        // 12. Pós-processamento URP (Volume Global) e Calibração de Sombras
        SetupPostProcessingVolume();

        GameObject sunObj = GameObject.Find("Directional Light");
        if (sunObj != null)
        {
            Light sunL = sunObj.GetComponent<Light>();
            if (sunL != null)
            {
                sunL.shadows = LightShadows.Soft;
                sunL.shadowStrength = 0.85f;
            }
        }

        // 13. Salva a cena
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log($"<color=green><b>[TinyIsland] SUCESSO!</b></color> Arquipélago completo construído com {coinLocations.Length} moedas em locais estratégicos!");
    }

    [MenuItem("TinyIsland/🎨 Atualizar Direção de Arte e Iluminação")]
    public static void UpdateArtDirectionAndLighting()
    {
        SetupPostProcessingVolume();

        GameObject sunObj = GameObject.Find("Directional Light");
        if (sunObj != null)
        {
            Light sunL = sunObj.GetComponent<Light>();
            if (sunL != null)
            {
                sunL.shadows = LightShadows.Soft;
                sunL.shadowStrength = 0.85f;
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("<color=green><b>[TinyIsland]</b></color> Direção de Arte, Pós-Processamento e Iluminação URP atualizados com sucesso!");
    }

    [MenuItem("TinyIsland/📦 Atualizar Baú do Tesouro")]
    public static GameObject BuildChest()
    {
        CleanUpOldObjects("BauDoTesouro");

        Material matGold     = GetOrCreateMaterial("Assets/Materials/M_Ouro.mat",      new Color(1.0f, 0.84f, 0.0f), 0.95f, 0.85f);
        Material matWood     = GetOrCreateMaterial("Assets/Materials/M_ChestWood.mat", new Color(0.42f, 0.22f, 0.08f), 0f, 0.3f);
        Material matIron     = GetOrCreateMaterial("Assets/Materials/M_ChestIron.mat", new Color(0.18f, 0.18f, 0.20f), 0.85f, 0.65f);
        Material matRuby     = GetOrCreateMaterial("Assets/Materials/M_Ruby.mat",      new Color(0.92f, 0.08f, 0.16f), 0.1f, 0.95f);
        Material matEmerald  = GetOrCreateMaterial("Assets/Materials/M_Emerald.mat",   new Color(0.05f, 0.85f, 0.35f), 0.1f, 0.95f);
        Material matSapphire = GetOrCreateMaterial("Assets/Materials/M_Sapphire.mat",  new Color(0.10f, 0.38f, 0.95f), 0.1f, 0.95f);

        GameObject chestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/TreasureChest.fbx");
        if (chestPrefab == null)
        {
            Debug.LogError("[TinyIsland] TreasureChest.fbx não encontrado em Assets/Models/TreasureChest.fbx!");
            return null;
        }

        GameObject chestObj = (GameObject)PrefabUtility.InstantiatePrefab(chestPrefab);
        chestObj.name = "BauDoTesouro";
        // Posicionado no centro do Altar das Ruínas Antigas na Ilhota Leste!
        chestObj.transform.position = new Vector3(27.5f, 1.15f, 2.0f);
        chestObj.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        chestObj.transform.localScale = Vector3.one * 1.4f;

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

        BoxCollider chestCol = chestObj.AddComponent<BoxCollider>();
        chestCol.center = new Vector3(0f, 0.38f, 0f);
        chestCol.size = new Vector3(1.1f, 0.76f, 0.75f);

        TreasureChest chestScript = chestObj.AddComponent<TreasureChest>();
        chestScript.targetOpenAngle = -100f;
        chestScript.openDuration = 1.2f;

        Transform lidT = chestObj.transform.Find("Chest_Lid");
        if (lidT == null) lidT = chestObj.transform.Find("Chest_Base/Chest_Lid");
        chestScript.lidTransform = lidT;

        GameObject lightObj = new GameObject("TreasureGlowLight");
        lightObj.transform.SetParent(chestObj.transform);
        lightObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        Light tLight = lightObj.AddComponent<Light>();
        tLight.type = LightType.Point;
        tLight.color = new Color(1.0f, 0.85f, 0.3f);
        tLight.range = 8f;
        tLight.intensity = 4.0f;
        tLight.enabled = false;
        chestScript.treasureLight = tLight;

        GameObject fxObj = new GameObject("ChestOpenParticles");
        fxObj.transform.SetParent(chestObj.transform);
        fxObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        ParticleSystem ps = fxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = 1.5f;
        main.loop = false;
        main.startLifetime = 1.2f;
        main.startSpeed = 3.5f;
        main.startSize = 0.22f;
        main.startColor = new Color(1.0f, 0.85f, 0.2f);
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 35) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.3f;
        chestScript.openParticles = ps;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        return chestObj;
    }

    [MenuItem("TinyIsland/🚶 Atualizar Jogador (Explorador)")]
    public static GameObject BuildPlayer()
    {
        CleanUpOldObjects("Player");

        Material matSkin     = GetOrCreateMaterial("Assets/Materials/M_PlayerSkin.mat",     new Color(0.96f, 0.78f, 0.64f), 0f, 0.4f);
        Material matShirt    = GetOrCreateMaterial("Assets/Materials/M_PlayerShirt.mat",    new Color(0.96f, 0.44f, 0.18f), 0f, 0.3f);
        Material matVest     = GetOrCreateMaterial("Assets/Materials/M_PlayerVest.mat",     new Color(0.78f, 0.68f, 0.48f), 0f, 0.3f);
        Material matShorts   = GetOrCreateMaterial("Assets/Materials/M_PlayerShorts.mat",   new Color(0.22f, 0.32f, 0.50f), 0f, 0.3f);
        Material matBoots    = GetOrCreateMaterial("Assets/Materials/M_PlayerBoots.mat",    new Color(0.30f, 0.16f, 0.08f), 0f, 0.4f);
        Material matHat     = GetOrCreateMaterial("Assets/Materials/M_PlayerHat.mat",     new Color(0.86f, 0.78f, 0.58f), 0f, 0.35f);
        Material matLeather  = GetOrCreateMaterial("Assets/Materials/M_PlayerLeather.mat",  new Color(0.24f, 0.12f, 0.05f), 0f, 0.5f);
        Material matHair     = GetOrCreateMaterial("Assets/Materials/M_PlayerHair.mat",     new Color(0.22f, 0.12f, 0.06f), 0f, 0.3f);
        Material matEyes     = GetOrCreateMaterial("Assets/Materials/M_PlayerEyes.mat",     new Color(0.10f, 0.10f, 0.12f), 0f, 0.8f);
        Material matBlush    = GetOrCreateMaterial("Assets/Materials/M_PlayerBlush.mat",    new Color(0.98f, 0.55f, 0.55f), 0f, 0.2f);
        Material matWhite    = GetOrCreateMaterial("Assets/Materials/M_PlayerWhite.mat",    new Color(1.0f, 1.0f, 1.0f),    0f, 0.5f);
        Material matBackpack = GetOrCreateMaterial("Assets/Materials/M_PlayerBackpack.mat", new Color(0.42f, 0.22f, 0.10f), 0f, 0.4f);
        Material matBedroll  = GetOrCreateMaterial("Assets/Materials/M_PlayerBedroll.mat",  new Color(0.25f, 0.55f, 0.40f), 0f, 0.3f);
        Material matGold     = GetOrCreateMaterial("Assets/Materials/M_PlayerGold.mat",     new Color(1.0f, 0.82f, 0.15f), 0.95f, 0.85f);

        Material[] playerMultiMats = new Material[]
        {
            matSkin, matShirt, matVest, matShorts, matBoots, matHat,
            matLeather, matHair, matEyes, matBlush, matWhite,
            matBackpack, matBedroll, matGold
        };

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/PlayerExplorer.fbx");
        GameObject playerObj;
        if (playerPrefab != null)
        {
            playerObj = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerObj.name = "Player";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0f, 0.66f, -2.0f);
            playerObj.transform.rotation = Quaternion.identity;
            playerObj.transform.localScale = Vector3.one;

            Renderer[] renderers = playerObj.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.sharedMaterials = playerMultiMats;
            }
        }
        else
        {
            playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Player";
            playerObj.tag = "Player";
            playerObj.transform.position = new Vector3(0f, 1.5f, -2.0f);
        }

        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc == null) cc = playerObj.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 0.74f, 0f);
        cc.height = 1.48f;
        cc.radius = 0.38f;
        cc.stepOffset = 0.45f;
        cc.slopeLimit = 55f;

        PlayerController pc = playerObj.GetComponent<PlayerController>();
        if (pc == null) pc = playerObj.AddComponent<PlayerController>();
        pc.moveSpeed = 7.5f;
        pc.jumpHeight = 1.65f;
        pc.rotationSpeed = 14f;
        pc.FindBodyParts();

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            camFollow.target = playerObj.transform;
            camFollow.offset = new Vector3(0f, 7.2f, -6.5f);
            camFollow.smoothTime = 0.14f;
            camFollow.mode = CameraFollow.CameraMode.MenuOrbit;
            camFollow.orbitCenter = new Vector3(2f, 1.2f, 0f);
            camFollow.orbitDistance = 30f;
            camFollow.orbitHeight = 14.5f;
            mainCam.transform.position = camFollow.CalculateOrbitPosition(camFollow.currentOrbitAngle);
            mainCam.transform.LookAt(camFollow.orbitCenter + Vector3.up * 0.8f);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        return playerObj;
    }

    private static GameObject CreateCylinderTerrain(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.AddComponent<MeshCollider>();
        return obj;
    }

    private static GameObject CreateStepPlatform(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }

    private static GameObject CreateShallowRing(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }

    private static void AssignMaterials(GameObject root, Material[] mats)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            int count = r.sharedMaterials.Length;
            if (count <= 1 && mats.Length > 0)
            {
                r.sharedMaterial = mats[0];
            }
            else
            {
                Material[] assigned = new Material[count];
                for (int i = 0; i < count; i++)
                {
                    assigned[i] = mats[Mathf.Min(i, mats.Length - 1)];
                }
                r.sharedMaterials = assigned;
            }
        }
    }

    private static void SetupAccurateMeshColliders(GameObject obj)
    {
        Collider[] oldCols = obj.GetComponentsInChildren<Collider>();
        foreach (Collider c in oldCols)
        {
            Object.DestroyImmediate(c);
        }

        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in filters)
        {
            if (mf.sharedMesh != null)
            {
                MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false;
            }
        }
    }

    private static Material GetOrCreateMaterial(string path, Color color, float metallic = 0f, float smoothness = 0.5f, bool emissive = false, Color emissionColor = default)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material GetOrCreateTransparentMaterial(string path, Color color, float smoothness = 0.9f)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        // Configura modo transparente no URP Lit
        mat.SetFloat("_Surface", 1f); // 1 = Transparent
        mat.SetFloat("_Blend", 0f);   // 0 = Alpha blend
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3050; // Renderiza acima do oceano sem z-fighting
        mat.SetOverrideTag("RenderType", "Transparent");

        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        mat.EnableKeyword("_EMISSION");
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", new Color(0.95f, 0.98f, 1.0f) * 0.55f);

        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject CreateShorelineFoamRing(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat, float phaseOffset = 0f, float waveSpeed = 0.90f, float amplitude = 0.035f)
    {
        GameObject foamObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        foamObj.name = name;
        foamObj.transform.SetParent(parent);
        foamObj.transform.position = pos;
        foamObj.transform.localScale = scale;
        foamObj.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(foamObj.GetComponent<Collider>());

        WaterShorelineFoam foamScript = foamObj.AddComponent<WaterShorelineFoam>();
        foamScript.phaseOffset = phaseOffset;
        foamScript.waveSpeed = waveSpeed;
        foamScript.waveScaleAmplitude = amplitude;
        foamScript.minAlpha = 0.25f;
        foamScript.maxAlpha = 0.85f;
        foamScript.verticalBobAmount = 0.006f;

        return foamObj;
    }

    private static void SetupPostProcessingVolume()
    {
        CleanUpOldObjects("GlobalVolume");

        GameObject volObj = new GameObject("GlobalVolume");
        Volume volume = volObj.AddComponent<Volume>();
        volume.isGlobal = true;

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, "Assets/Settings/SampleSceneProfile.asset");
        }

        profile.components.RemoveAll(c => c == null);

        // Bloom suave (ouro cintilante e farol)
        if (!profile.TryGet<Bloom>(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>(true);
            AssetDatabase.AddObjectToAsset(bloom, profile);
        }
        bloom.active = true;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = 0.85f;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = 1.05f;
        bloom.scatter.overrideState = true;
        bloom.scatter.value = 0.70f;
        bloom.tint.overrideState = true;
        bloom.tint.value = new Color(1.0f, 0.94f, 0.82f);

        // Color Adjustments (cores quentes e vibrantes da ilha tropical)
        if (!profile.TryGet<ColorAdjustments>(out ColorAdjustments colorAdj))
        {
            colorAdj = profile.Add<ColorAdjustments>(true);
            AssetDatabase.AddObjectToAsset(colorAdj, profile);
        }
        colorAdj.active = true;
        colorAdj.postExposure.overrideState = true;
        colorAdj.postExposure.value = 0.22f;
        colorAdj.contrast.overrideState = true;
        colorAdj.contrast.value = 14f;
        colorAdj.saturation.overrideState = true;
        colorAdj.saturation.value = 18f;
        colorAdj.colorFilter.overrideState = true;
        colorAdj.colorFilter.value = new Color(1.0f, 0.98f, 0.94f);

        // White Balance (sol tropical acolhedor)
        if (!profile.TryGet<WhiteBalance>(out WhiteBalance wb))
        {
            wb = profile.Add<WhiteBalance>(true);
            AssetDatabase.AddObjectToAsset(wb, profile);
        }
        wb.active = true;
        wb.temperature.overrideState = true;
        wb.temperature.value = 8f;
        wb.tint.overrideState = true;
        wb.tint.value = 0f;

        // Tonemapping (Neutral filmic)
        if (!profile.TryGet<Tonemapping>(out Tonemapping tm))
        {
            tm = profile.Add<Tonemapping>(true);
            AssetDatabase.AddObjectToAsset(tm, profile);
        }
        tm.active = true;
        tm.mode.overrideState = true;
        tm.mode.value = TonemappingMode.Neutral;

        // Vignette (efeito diorama / miniatura focado no centro)
        if (!profile.TryGet<Vignette>(out Vignette vig))
        {
            vig = profile.Add<Vignette>(true);
            AssetDatabase.AddObjectToAsset(vig, profile);
        }
        vig.active = true;
        vig.intensity.overrideState = true;
        vig.intensity.value = 0.22f;
        vig.smoothness.overrideState = true;
        vig.smoothness.value = 0.35f;

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        volume.sharedProfile = profile;
    }

    private static void CleanUpOldObjects(params string[] names)
    {
        foreach (string name in names)
        {
            GameObject obj = GameObject.Find(name);
            while (obj != null)
            {
                Object.DestroyImmediate(obj);
                obj = GameObject.Find(name);
            }
        }
    }
}
#endif
