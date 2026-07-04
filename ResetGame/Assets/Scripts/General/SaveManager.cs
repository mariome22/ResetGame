using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData
{
    // Escena y Progreso General
    public string escenaGuardada = "01_Hub";
    public int playerCores = 0;

    // Inventario Mundo 1
    public List<string> objetosNombres = new List<string>();
    public List<int> objetosCantidades = new List<int>();
    public List<string> coleccionablesNombres = new List<string>();
    public List<int> coleccionablesCantidades = new List<int>();

    // Estado Jugador Mundo 1
    public bool armaDesbloqueadaMundo1 = false;
    public int balasActualesCargadorMundo1 = 0;
    public int vidaActualMundo1 = 8;
    public float playerPosXMundo1 = 0f;
    public float playerPosYMundo1 = 0f;
    public bool tienePosicionGuardadaMundo1 = false;

    // Objetos destruidos/recogidos Mundo 1
    public List<string> objetosDestruidosMundo1 = new List<string>();

    // Estado Mundo 2 (Platformer)
    public int livesMundo2 = 3;
    public int totalCoinsMundo2 = 0;
    public int secretCoinsCollectedMundo2 = 0;
    public string lastCheckpointSceneMundo2 = "";
    public float lastCheckpointPosXMundo2 = 0f;
    public float lastCheckpointPosYMundo2 = 0f;
    public float remainingTimeMundo2 = 500f;
    public float checkpointTimeMundo2 = 500f;
    public int consecutiveCheckpointDeathsMundo2 = 0;
    public List<string> collectedCoinsMundo2 = new List<string>();
    // Diálogos ya vistos/reproducidos por el jugador
    public List<string> dialogosReproducidos = new List<string>();
}

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveManager>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("Global_Managers");
                    if (prefab != null)
                    {
                        GameObject instantiated = Instantiate(prefab);
                        _instance = instantiated.GetComponentInChildren<SaveManager>(true);

                        // Si el prefab estaba desactivado en Resources, forzar activación para que corra Awake/Start
                        if (!instantiated.activeSelf)
                        {
                            instantiated.SetActive(true);
                        }

                        DontDestroyOnLoad(instantiated);
                        Debug.Log("[SaveManager] Instanciado Global_Managers desde Resources dinámicamente al acceder a Instance.");
                    }
                    else
                    {
                        GameObject obj = new GameObject("SaveManager");
                        _instance = obj.AddComponent<SaveManager>();
                        DontDestroyOnLoad(obj);
                        Debug.LogWarning("[SaveManager] No se encontró el prefab Global_Managers en Resources. Se creó una instancia vacía dinámica.");
                    }
                }
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    [Header("Base de Datos de Objetos")]
    [Tooltip("Arrastra aquí todos los assets ItemData del proyecto para poder reconstruir el inventario al cargar")]
    public List<ItemData> baseDatosObjetos = new List<ItemData>();

    [HideInInspector]
    public List<string> destroyedObjects = new List<string>();

    [HideInInspector]
    public List<string> dialogosReproducidos = new List<string>();

    private string saveFilePath;
    private SaveData pendingLoadData = null;
    private static string previousSceneName = "";

    [HideInInspector]
    public bool isReloadingOnDeath = false;
    private SaveData runtimeState = null;



    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Asegurar que las listas nunca sean nulas debido a la inicialización de prefabs de Unity
            if (destroyedObjects == null) destroyedObjects = new List<string>();
            if (dialogosReproducidos == null) dialogosReproducidos = new List<string>();
        }
        else if (_instance != this)
        {
            // Si la instancia persistente tiene la base de datos vacía, pero esta instancia de escena la tiene llena, las copiamos
            if ((_instance.baseDatosObjetos == null || _instance.baseDatosObjetos.Count == 0) && (this.baseDatosObjetos != null && this.baseDatosObjetos.Count > 0))
            {
                _instance.baseDatosObjetos = new List<ItemData>(this.baseDatosObjetos);
                Debug.Log("[SaveManager] Copiada baseDatosObjetos desde la instancia de la escena cargada a la instancia persistente.");
            }

            // Destruimos el GameObject duplicado completo para evitar duplicar Canvases, EventSystems, etc.
            Destroy(gameObject);
        }
    }

    public void RegisterDestroyedObject(string id)
    {
        if (destroyedObjects == null) destroyedObjects = new List<string>();
        if (!destroyedObjects.Contains(id))
        {
            destroyedObjects.Add(id);
        }
    }

    public bool IsObjectDestroyed(string id)
    {
        if (destroyedObjects == null) destroyedObjects = new List<string>();
        return destroyedObjects.Contains(id);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool HasSaveGame()
    {
        return File.Exists(saveFilePath);
    }

    public void DeleteSaveGame()
    {
        if (HasSaveGame())
        {
            File.Delete(saveFilePath);
        }
        PlayerPrefs.DeleteKey("SavedLevel");
        PlayerPrefs.DeleteKey("PlayerCores");
        
        // Limpiar el registro de niveles completados para que en la nueva partida se vuelvan a otorgar los núcleos
        PlayerPrefs.DeleteKey("LevelCompleted_1_Level1");
        PlayerPrefs.DeleteKey("LevelCompleted_1_Level2");
        PlayerPrefs.DeleteKey("LevelCompleted_2_Level1");
        PlayerPrefs.DeleteKey("LevelCompleted_2_Level2");
        
        PlayerPrefs.Save();

        // Limpiamos también el estado en memoria para el Mundo 2 y Mundo 1
        PlayerPlatformerController.lastCheckpointScene = "";
        PlayerPlatformerController.lastCheckpointPos = Vector2.zero;
        PlayerPlatformerController.totalCoins = 0;
        PlayerPlatformerController.lives = 3;
        PlayerPlatformerController.secretCoinsCollected = 0;
        PlayerPlatformerController.consecutiveCheckpointDeaths = 0;
        PlayerPlatformerController.collectedCoinsActive.Clear();
        PlayerPlatformerController.collectedCoinsAtCheckpoint.Clear();

        destroyedObjects.Clear();
        dialogosReproducidos.Clear();
        previousSceneName = "";

        Debug.Log("Partida guardada borrada y estado en memoria reseteado.");
    }

    public void SaveGame()
    {
        try
        {
            SaveData data = new SaveData();

            // 1. Escena actual
            string currentScene = SceneManager.GetActiveScene().name;
            data.escenaGuardada = currentScene;

            // 2. Núcleos completados (usando PlayerPrefs para mantener compatibilidad)
            data.playerCores = PlayerPrefs.GetInt("PlayerCores", 0);

            // 3. Inventario Mundo 1 (si existe en la escena actual)
            if (InventarioManager.Instance != null)
            {
                foreach (var slot in InventarioManager.Instance.objetosGuardados)
                {
                    if (slot.objeto != null)
                    {
                        data.objetosNombres.Add(slot.objeto.nombreObjeto);
                        data.objetosCantidades.Add(slot.cantidad);
                    }
                }

                foreach (var slot in InventarioManager.Instance.coleccionablesGuardados)
                {
                    if (slot.objeto != null)
                    {
                        data.coleccionablesNombres.Add(slot.objeto.nombreObjeto);
                        data.coleccionablesCantidades.Add(slot.cantidad);
                    }
                }
            }

            // 4. Estado de jugador Mundo 1 (si existe en la escena)
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                var propArma = pc.GetType().GetField("armaDesbloqueada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (propArma != null) data.armaDesbloqueadaMundo1 = (bool)propArma.GetValue(pc);

                data.balasActualesCargadorMundo1 = pc.balasActualesCargador;
            }

            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
            {
                var propVida = ph.GetType().GetField("vidaActual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (propVida != null) data.vidaActualMundo1 = (int)propVida.GetValue(ph);
            }

            // Guardar posición del jugador si estamos en Mundo 1 o Hub
            if (currentScene == "01_Hub" || currentScene == "1_Level1" || currentScene == "1_Level2")
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    data.playerPosXMundo1 = playerObj.transform.position.x;
                    data.playerPosYMundo1 = playerObj.transform.position.y;
                    data.tienePosicionGuardadaMundo1 = true;
                }
            }

            // Garantizar inicialización de listas antes de clonarlas
            if (destroyedObjects == null) destroyedObjects = new List<string>();
            if (dialogosReproducidos == null) dialogosReproducidos = new List<string>();

            // Guardar lista de objetos destruidos de Mundo 1
            data.objetosDestruidosMundo1 = new List<string>(destroyedObjects);

            // Guardar lista de diálogos reproducidos de la partida
            data.dialogosReproducidos = new List<string>(dialogosReproducidos);
            Debug.Log($"[SaveManager] Guardando partida en disco (SaveGame). Diálogos guardados: {string.Join(", ", data.dialogosReproducidos)}");

            // 5. Estado Mundo 2
            // Si estamos guardando desde el Hub o Mundo 1, limpiamos el checkpoint de Mundo 2
            if (currentScene == "01_Hub" || currentScene == "1_Level1" || currentScene == "1_Level2")
            {
                data.livesMundo2 = 3;
                data.totalCoinsMundo2 = 0;
                data.secretCoinsCollectedMundo2 = 0;
                data.lastCheckpointSceneMundo2 = "";
                data.lastCheckpointPosXMundo2 = 0f;
                data.lastCheckpointPosYMundo2 = 0f;
                data.remainingTimeMundo2 = 500f;
                data.checkpointTimeMundo2 = 500f;
                data.consecutiveCheckpointDeathsMundo2 = 0;
                data.collectedCoinsMundo2 = new List<string>();
            }
            else // Mundo 2
            {
                if (currentScene == "2_Level1" && PlayerPlatformerController.lastCheckpointScene != "2_Level1")
                {
                    // No ha pasado el checkpoint en Nivel 1: todo empieza desde 0/valores iniciales
                    data.livesMundo2 = 3;
                    data.totalCoinsMundo2 = 0;
                    data.secretCoinsCollectedMundo2 = 0;
                    data.lastCheckpointSceneMundo2 = "";
                    data.lastCheckpointPosXMundo2 = 0f;
                    data.lastCheckpointPosYMundo2 = 0f;
                    data.remainingTimeMundo2 = 500f;
                    data.checkpointTimeMundo2 = 500f;
                    data.consecutiveCheckpointDeathsMundo2 = 0;
                    data.collectedCoinsMundo2 = new List<string>();
                }
                else
                {
                    data.livesMundo2 = PlayerPlatformerController.lives;
                    data.remainingTimeMundo2 = PlayerPlatformerController.remainingTime;
                    data.consecutiveCheckpointDeathsMundo2 = PlayerPlatformerController.consecutiveCheckpointDeaths;

                    // Si estamos en 2_Level2, borramos el checkpoint para que empiece desde el inicio de 2_Level2
                    if (currentScene == "2_Level2")
                    {
                        data.lastCheckpointSceneMundo2 = "";
                        data.lastCheckpointPosXMundo2 = 0f;
                        data.lastCheckpointPosYMundo2 = 0f;
                        data.checkpointTimeMundo2 = PlayerPlatformerController.remainingTime;

                        // Las monedas y galletas del Nivel 2 no se guardan en disco bajo ningún concepto
                        data.totalCoinsMundo2 = 0;
                        data.secretCoinsCollectedMundo2 = 0;
                        data.collectedCoinsMundo2 = new List<string>();
                    }
                    else
                    {
                        data.totalCoinsMundo2 = PlayerPlatformerController.totalCoins;
                        data.secretCoinsCollectedMundo2 = PlayerPlatformerController.secretCoinsCollected;
                        
                        if (PlayerPlatformerController.collectedCoinsActive != null)
                        {
                            data.collectedCoinsMundo2 = new List<string>(PlayerPlatformerController.collectedCoinsActive);
                        }
                        else
                        {
                            data.collectedCoinsMundo2 = new List<string>();
                        }

                        data.lastCheckpointSceneMundo2 = PlayerPlatformerController.lastCheckpointScene;
                        data.lastCheckpointPosXMundo2 = PlayerPlatformerController.lastCheckpointPos.x;
                        data.lastCheckpointPosYMundo2 = PlayerPlatformerController.lastCheckpointPos.y;
                        data.checkpointTimeMundo2 = PlayerPlatformerController.checkpointTime;
                    }
                }
            }

            // Guardamos también a PlayerPrefs los valores de compatibilidad
            PlayerPrefs.SetInt("SavedLevel", 1); // Indica que hay una partida cargable
            PlayerPrefs.SetInt("PlayerCores", data.playerCores);
            PlayerPrefs.Save();

            // Convertir a JSON y guardar
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Juego guardado en: " + saveFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveManager] ERROR CRÍTICO al guardar partida: " + e.ToString());
        }
    }

    public void LoadGame()
    {
        if (!HasSaveGame())
        {
            Debug.LogWarning("No hay archivo de guardado disponible.");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        pendingLoadData = JsonUtility.FromJson<SaveData>(json);

        // Cargamos la escena guardada
        Debug.Log("Cargando escena guardada: " + pendingLoadData.escenaGuardada);
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(pendingLoadData.escenaGuardada);
        }
        else
        {
            SceneManager.LoadScene(pendingLoadData.escenaGuardada);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LevelSelectorController.ResetFlags();

        // 1. Restaurar checkpoint y monedas de Mundo 2 si entramos desde el Hub
        if (pendingLoadData == null && previousSceneName == "01_Hub" && (scene.name == "2_Level1" || scene.name == "2_Level2") && HasSaveGame())
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData diskData = JsonUtility.FromJson<SaveData>(json);
                if (diskData != null)
                {
                    PlayerPlatformerController.lives = diskData.livesMundo2;
                    PlayerPlatformerController.totalCoins = diskData.totalCoinsMundo2;
                    PlayerPlatformerController.secretCoinsCollected = diskData.secretCoinsCollectedMundo2;
                    PlayerPlatformerController.consecutiveCheckpointDeaths = diskData.consecutiveCheckpointDeathsMundo2;
                    PlayerPlatformerController.remainingTime = diskData.remainingTimeMundo2;
                    PlayerPlatformerController.checkpointTime = diskData.checkpointTimeMundo2;

                    if (diskData.collectedCoinsMundo2 != null)
                    {
                        PlayerPlatformerController.collectedCoinsActive = new System.Collections.Generic.HashSet<string>(diskData.collectedCoinsMundo2);
                        PlayerPlatformerController.collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>(diskData.collectedCoinsMundo2);
                    }
                    else
                    {
                        PlayerPlatformerController.collectedCoinsActive = new System.Collections.Generic.HashSet<string>();
                        PlayerPlatformerController.collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>();
                    }

                    PlayerPlatformerController.lastCheckpointScene = diskData.lastCheckpointSceneMundo2;
                    PlayerPlatformerController.lastCheckpointPos = new Vector2(diskData.lastCheckpointPosXMundo2, diskData.lastCheckpointPosYMundo2);

                    if (diskData.lastCheckpointSceneMundo2 == scene.name)
                    {
                        GameObject playerObj = GameObject.FindWithTag("Player");
                        if (playerObj != null)
                        {
                            playerObj.transform.position = new Vector3(diskData.lastCheckpointPosXMundo2, diskData.lastCheckpointPosYMundo2, playerObj.transform.position.z);
                            Debug.Log($"[SaveManager] Estado y posición del checkpoint en '{scene.name}' restaurados al entrar desde el Hub.");
                        }
                    }
                    else
                    {
                        Debug.Log($"[SaveManager] Estado restaurado al entrar desde el Hub en '{scene.name}', pero el checkpoint activo es en otra escena o está vacío. Se inicia desde el principio.");
                    }

                    if (HUDPlatformerManager.Instance != null)
                    {
                        HUDPlatformerManager.Instance.UpdateHUD();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[SaveManager] Error al restaurar estado de Mundo 2 al transicionar desde el Hub: " + e.Message);
            }
        }

        // Limpiar checkpoints de Mundo 2 si entramos al Hub o Mundo 1
        if (scene.name == "01_Hub" || scene.name == "1_Level1" || scene.name == "1_Level2")
        {
            PlayerPlatformerController.lastCheckpointScene = "";
            PlayerPlatformerController.lastCheckpointPos = Vector2.zero;
            PlayerPlatformerController.totalCoins = 0;
            PlayerPlatformerController.lives = 3;
            PlayerPlatformerController.secretCoinsCollected = 0;
            PlayerPlatformerController.consecutiveCheckpointDeaths = 0;
            PlayerPlatformerController.collectedCoinsActive.Clear();
            PlayerPlatformerController.collectedCoinsAtCheckpoint.Clear();
        }

        // 2. Si estamos recargando la escena debido a la muerte del jugador
        if (isReloadingOnDeath)
        {
            isReloadingOnDeath = false;
            runtimeState = null; // Descartar estado temporal
 
            if (HasSaveGame())
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    SaveData diskData = JsonUtility.FromJson<SaveData>(json);
                    if (diskData != null)
                    {
                        Debug.Log("[SaveManager] Recarga por muerte detectada. Restaurando datos desde el disco...");
                        RestaurarDesdeDatos(diskData, scene.name, true);
                        Debug.Log("[SaveManager] Nivel reiniciado tras muerte. Cargado estado de partida desde el disco.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[SaveManager] Error al restaurar desde el disco tras muerte: " + e.Message);
                }
            }
            else
            {
                RestaurarEstadoPorDefecto();
                Debug.Log("[SaveManager] Nivel reiniciado tras muerte. No hay partida guardada en disco, se resetea al estado inicial.");
            }
            previousSceneName = scene.name;
            return;
        }
 
        // 3. Si es una transición de escena normal (caminando por portales)
        if (scene.name == "01_Hub" || scene.name == "1_Level1" || scene.name == "1_Level2")
        {
            // Caso A: Si hay un archivo de guardado en el disco Y coincide exactamente con esta escena,
            // significa que el jugador guardó dentro de este nivel y ahora está reentrando.
            // En este caso, cargamos su progreso, posición y vida exacta desde el disco.
            if (HasSaveGame())
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    SaveData diskData = JsonUtility.FromJson<SaveData>(json);
                    if (diskData != null && diskData.escenaGuardada == scene.name)
                    {
                        Debug.Log($"[SaveManager] Reentrando a '{scene.name}' que coincide con el guardado en disco. Restaurando posición, salud e inventario del disco...");
                        RestaurarDesdeDatos(diskData, scene.name, true);
                        runtimeState = null; // Descartar el estado temporal
                        previousSceneName = scene.name;
                        return;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[SaveManager] Error al intentar reanudar escena desde el disco: " + e.Message);
                }
            }

            // Caso B: Si no coincide con el archivo en disco, pero tenemos un estado en memoria (runtimeState)
            // de la escena de la que venimos (ej: cruzando un portal normal), restauramos su inventario y vida
            // pero sin posicionarlo en un punto de guardado (aparece en la entrada normal del nivel).
            if (runtimeState != null)
            {
                Debug.Log($"[SaveManager] Transición normal de escena a '{scene.name}'. Restaurando estado desde runtimeState en memoria (Diálogos: {string.Join(", ", runtimeState.dialogosReproducidos)})...");
                RestaurarDesdeDatos(runtimeState, scene.name, false);
                runtimeState = null; // Limpiar después de aplicar
                previousSceneName = scene.name;
                return;
            }
        }

        // 4. Si es la carga inicial desde el menú principal (pendingLoadData no es nulo)
        if (pendingLoadData != null)
        {
            RestaurarDesdeDatos(pendingLoadData, scene.name, true);

            // Aplicamos el progreso de núcleos a PlayerPrefs
            PlayerPrefs.SetInt("PlayerCores", pendingLoadData.playerCores);
            PlayerPrefs.Save();

            // Aplicar datos de Mundo 2 (Estáticos)
            PlayerPlatformerController.lives = pendingLoadData.livesMundo2;
            PlayerPlatformerController.totalCoins = pendingLoadData.totalCoinsMundo2;
            PlayerPlatformerController.secretCoinsCollected = pendingLoadData.secretCoinsCollectedMundo2;
            PlayerPlatformerController.lastCheckpointScene = pendingLoadData.lastCheckpointSceneMundo2;
            PlayerPlatformerController.lastCheckpointPos = new Vector2(pendingLoadData.lastCheckpointPosXMundo2, pendingLoadData.lastCheckpointPosYMundo2);
            PlayerPlatformerController.remainingTime = pendingLoadData.remainingTimeMundo2;
            PlayerPlatformerController.checkpointTime = pendingLoadData.checkpointTimeMundo2;
            PlayerPlatformerController.consecutiveCheckpointDeaths = pendingLoadData.consecutiveCheckpointDeathsMundo2;
            
            if (pendingLoadData.collectedCoinsMundo2 != null)
            {
                PlayerPlatformerController.collectedCoinsActive = new System.Collections.Generic.HashSet<string>(pendingLoadData.collectedCoinsMundo2);
                PlayerPlatformerController.collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>(pendingLoadData.collectedCoinsMundo2);
            }
            else
            {
                PlayerPlatformerController.collectedCoinsActive = new System.Collections.Generic.HashSet<string>();
                PlayerPlatformerController.collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>();
            }

            if (HUDPlatformerManager.Instance != null)
            {
                HUDPlatformerManager.Instance.UpdateHUD();
            }

            pendingLoadData = null;
            Debug.Log("[SaveManager] Carga inicial de partida aplicada correctamente.");
        }

        previousSceneName = scene.name;
    }

    public void SaveRuntimeState()
    {
        try
        {
            SaveData data = new SaveData();
            string currentScene = SceneManager.GetActiveScene().name;
            data.escenaGuardada = currentScene;
            data.playerCores = PlayerPrefs.GetInt("PlayerCores", 0);

            if (InventarioManager.Instance != null)
            {
                foreach (var slot in InventarioManager.Instance.objetosGuardados)
                {
                    if (slot.objeto != null)
                    {
                        data.objetosNombres.Add(slot.objeto.nombreObjeto);
                        data.objetosCantidades.Add(slot.cantidad);
                    }
                }
                foreach (var slot in InventarioManager.Instance.coleccionablesGuardados)
                {
                    if (slot.objeto != null)
                    {
                        data.coleccionablesNombres.Add(slot.objeto.nombreObjeto);
                        data.coleccionablesCantidades.Add(slot.cantidad);
                    }
                }
            }

            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                var propArma = pc.GetType().GetField("armaDesbloqueada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (propArma != null) data.armaDesbloqueadaMundo1 = (bool)propArma.GetValue(pc);
                data.balasActualesCargadorMundo1 = pc.balasActualesCargador;
            }

            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
            {
                var propVida = ph.GetType().GetField("vidaActual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (propVida != null) data.vidaActualMundo1 = (int)propVida.GetValue(ph);
            }

            if (destroyedObjects == null) destroyedObjects = new List<string>();
            data.objetosDestruidosMundo1 = new List<string>(destroyedObjects);

            if (dialogosReproducidos == null) dialogosReproducidos = new List<string>();
            data.dialogosReproducidos = new List<string>(dialogosReproducidos);

            runtimeState = data;
            Debug.Log($"[SaveManager] Guardado RuntimeState en memoria. Diálogos en runtimeState: {string.Join(", ", runtimeState.dialogosReproducidos)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SaveManager] Error al guardar el runtimeState en memoria: " + e.Message);
        }
    }

    private void RestaurarDesdeDatos(SaveData data, string escenaNombre, bool restaurarPosicion)
    {
        // 1. Restaurar posición del jugador
        if (restaurarPosicion && data.tienePosicionGuardadaMundo1 && data.escenaGuardada == escenaNombre)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerObj.transform.position = new Vector3(data.playerPosXMundo1, data.playerPosYMundo1, playerObj.transform.position.z);
                Debug.Log("[SaveManager] Posición del jugador en Mundo 1 restaurada: " + escenaNombre);
            }
        }

        // 2. Restaurar inventario
        if (InventarioManager.Instance != null)
        {
            InventarioManager.Instance.objetosGuardados.Clear();
            if (data.objetosNombres != null && data.objetosCantidades != null)
            {
                int count = Mathf.Min(data.objetosNombres.Count, data.objetosCantidades.Count);
                for (int i = 0; i < count; i++)
                {
                    ItemData item = BuscarItemPorNombre(data.objetosNombres[i]);
                    if (item != null)
                    {
                        InventarioManager.Instance.objetosGuardados.Add(new InventarioSlot(item, data.objetosCantidades[i]));
                    }
                }
            }

            InventarioManager.Instance.coleccionablesGuardados.Clear();
            if (data.coleccionablesNombres != null && data.coleccionablesCantidades != null)
            {
                int count = Mathf.Min(data.coleccionablesNombres.Count, data.coleccionablesCantidades.Count);
                for (int i = 0; i < count; i++)
                {
                    ItemData item = BuscarItemPorNombre(data.coleccionablesNombres[i]);
                    if (item != null)
                    {
                        InventarioManager.Instance.coleccionablesGuardados.Add(new InventarioSlot(item, data.coleccionablesCantidades[i]));
                    }
                }
            }

            InventarioManager.Instance.ActualizarUI();
            InventarioManager.Instance.ActualizarMenuPausa();
        }

        // 3. Restaurar estado de salud y armas del jugador
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            var propArma = pc.GetType().GetField("armaDesbloqueada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propArma != null) propArma.SetValue(pc, data.armaDesbloqueadaMundo1);

            var propUsando = pc.GetType().GetField("usandoArmaADistancia", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propUsando != null) propUsando.SetValue(pc, data.armaDesbloqueadaMundo1);

            pc.balasActualesCargador = data.balasActualesCargadorMundo1;
            pc.ActualizarHUDArma();
        }

        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            var propVida = ph.GetType().GetField("vidaActual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propVida != null) propVida.SetValue(ph, data.vidaActualMundo1);
            ph.SendMessage("ActualizarHUD", SendMessageOptions.DontRequireReceiver);
        }

        // 4. Sincronizar listas en memoria
        if (data.objetosDestruidosMundo1 != null)
        {
            destroyedObjects = new List<string>(data.objetosDestruidosMundo1);
        }
        else
        {
            destroyedObjects = new List<string>();
        }

        if (data.dialogosReproducidos != null)
        {
            dialogosReproducidos = new List<string>(data.dialogosReproducidos);
        }
        else
        {
            dialogosReproducidos = new List<string>();
        }
        Debug.Log($"[SaveManager] Restaurado estado desde datos (RestaurarDesdeDatos). Diálogos en memoria: {string.Join(", ", dialogosReproducidos)}");
    }

    private void RestaurarEstadoPorDefecto()
    {
        // Limpiar todas las listas de persistencia en memoria
        if (destroyedObjects == null) destroyedObjects = new List<string>();
        else destroyedObjects.Clear();

        if (dialogosReproducidos == null) dialogosReproducidos = new List<string>();
        else dialogosReproducidos.Clear();

        // Limpiar inventario
        if (InventarioManager.Instance != null)
        {
            InventarioManager.Instance.objetosGuardados.Clear();
            InventarioManager.Instance.coleccionablesGuardados.Clear();
            InventarioManager.Instance.ActualizarUI();
            InventarioManager.Instance.ActualizarMenuPausa();
        }

        // Resetear salud del jugador y bloquear armas
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            var propArma = pc.GetType().GetField("armaDesbloqueada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propArma != null) propArma.SetValue(pc, false);

            var propUsando = pc.GetType().GetField("usandoArmaADistancia", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propUsando != null) propUsando.SetValue(pc, false);

            pc.balasActualesCargador = 0;
            pc.ActualizarHUDArma();
        }

        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            var propVida = ph.GetType().GetField("vidaActual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propVida != null) propVida.SetValue(ph, ph.vidaMaxima);
            ph.SendMessage("ActualizarHUD", SendMessageOptions.DontRequireReceiver);
        }
    }

    private ItemData BuscarItemPorNombre(string nombre)
    {
        if (baseDatosObjetos == null || baseDatosObjetos.Count == 0)
        {
            Debug.LogWarning($"[SaveManager] baseDatosObjetos está vacía. No se puede buscar '{nombre}'.");
            return null;
        }
        foreach (var item in baseDatosObjetos)
        {
            if (item != null && item.nombreObjeto == nombre)
            {
                return item;
            }
        }
        Debug.LogWarning($"[SaveManager] No se encontró '{nombre}' en baseDatosObjetos.");
        return null;
    }
}
