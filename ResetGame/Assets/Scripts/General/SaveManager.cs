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
    public static SaveManager Instance { get; private set; }

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

    public void RegisterDestroyedObject(string id)
    {
        if (!destroyedObjects.Contains(id))
        {
            destroyedObjects.Add(id);
        }
    }

    public bool IsObjectDestroyed(string id)
    {
        return destroyedObjects.Contains(id);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
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

        // Guardar lista de objetos destruidos de Mundo 1
        data.objetosDestruidosMundo1 = new List<string>(destroyedObjects);

        // Guardar lista de diálogos reproducidos de la partida
        data.dialogosReproducidos = new List<string>(dialogosReproducidos);

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
                    data.collectedCoinsMundo2 = new List<string>(PlayerPlatformerController.collectedCoinsActive);

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
        // Si venimos del Hub y entramos al Mundo 2 (caminando por el portal, sin cargar partida desde el menú),
        // restauramos el estado del punto de control y las estadísticas del Mundo 2 guardadas en disco
        if (pendingLoadData == null && previousSceneName == "01_Hub" && (scene.name == "2_Level1" || scene.name == "2_Level2") && HasSaveGame())
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData diskData = JsonUtility.FromJson<SaveData>(json);
                if (diskData != null)
                {
                    // Restauramos las estadísticas en PlayerPlatformerController
                    PlayerPlatformerController.lives = diskData.livesMundo2;
                    PlayerPlatformerController.totalCoins = diskData.totalCoinsMundo2;
                    PlayerPlatformerController.secretCoinsCollected = diskData.secretCoinsCollectedMundo2;
                    PlayerPlatformerController.consecutiveCheckpointDeaths = diskData.consecutiveCheckpointDeathsMundo2;
                    PlayerPlatformerController.remainingTime = diskData.remainingTimeMundo2;
                    PlayerPlatformerController.checkpointTime = diskData.checkpointTimeMundo2;

                    PlayerPlatformerController.collectedCoinsActive = new System.Collections.Generic.HashSet<string>(diskData.collectedCoinsMundo2);
                    PlayerPlatformerController.collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>(diskData.collectedCoinsMundo2);

                    PlayerPlatformerController.lastCheckpointScene = diskData.lastCheckpointSceneMundo2;
                    PlayerPlatformerController.lastCheckpointPos = new Vector2(diskData.lastCheckpointPosXMundo2, diskData.lastCheckpointPosYMundo2);

                    // Si el checkpoint guardado coincide con esta escena, posicionamos al jugador en el checkpoint
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

        // Si entramos al Hub o Mundo 1 normalmente (sin cargar partida) o al cargar,
        // nos aseguramos de limpiar los checkpoints del Mundo 2 en memoria
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

        // Si entramos a una escena de Mundo 1 o Hub normalmente (caminando, sin cargar partida desde el menú)
        // y coincide con la última escena donde se guardó, restauramos la posición del jugador
        if (pendingLoadData == null && HasSaveGame() && (scene.name == "01_Hub" || scene.name == "1_Level1" || scene.name == "1_Level2"))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData diskData = JsonUtility.FromJson<SaveData>(json);
                if (diskData != null && diskData.tienePosicionGuardadaMundo1 && diskData.escenaGuardada == scene.name)
                {
                    GameObject playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                    {
                        playerObj.transform.position = new Vector3(diskData.playerPosXMundo1, diskData.playerPosYMundo1, playerObj.transform.position.z);
                        Debug.Log("Posición del jugador en Mundo 1 restaurada al transicionar: " + scene.name);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error al restaurar posición desde archivo al transicionar: " + e.Message);
            }
        }

        previousSceneName = scene.name;

        if (pendingLoadData == null) return;

        // Aplicamos el progreso de núcleos a PlayerPrefs
        PlayerPrefs.SetInt("PlayerCores", pendingLoadData.playerCores);
        PlayerPrefs.Save();

        // Aplicar datos de Mundo 2 (Estáticos, se aplican inmediatamente)
        PlayerPlatformerController.lives = pendingLoadData.livesMundo2;
        PlayerPlatformerController.totalCoins = pendingLoadData.totalCoinsMundo2;
        PlayerPlatformerController.secretCoinsCollected = pendingLoadData.secretCoinsCollectedMundo2;
        PlayerPlatformerController.lastCheckpointScene = pendingLoadData.lastCheckpointSceneMundo2;
        PlayerPlatformerController.lastCheckpointPos = new Vector2(pendingLoadData.lastCheckpointPosXMundo2, pendingLoadData.lastCheckpointPosYMundo2);
        PlayerPlatformerController.remainingTime = pendingLoadData.remainingTimeMundo2;
        PlayerPlatformerController.checkpointTime = pendingLoadData.checkpointTimeMundo2;
        PlayerPlatformerController.consecutiveCheckpointDeaths = pendingLoadData.consecutiveCheckpointDeathsMundo2;
        PlayerPlatformerController.collectedCoinsActive = new System.Collections.Generic.HashSet<string>(pendingLoadData.collectedCoinsMundo2);
        PlayerPlatformerController.collectedCoinsAtCheckpoint = new System.Collections.Generic.HashSet<string>(pendingLoadData.collectedCoinsMundo2);

        if (HUDPlatformerManager.Instance != null)
        {
            HUDPlatformerManager.Instance.UpdateHUD();
        }

        // Cargar lista de objetos destruidos en Mundo 1
        destroyedObjects = new List<string>(pendingLoadData.objetosDestruidosMundo1);

        // Cargar lista de diálogos reproducidos
        if (pendingLoadData.dialogosReproducidos != null)
        {
            dialogosReproducidos = new List<string>(pendingLoadData.dialogosReproducidos);
        }
        else
        {
            dialogosReproducidos.Clear();
        }

        // Aplicar datos de Mundo 1 (Instancia de inventario y personaje si existen en la escena cargada)
        if (InventarioManager.Instance != null)
        {
            InventarioManager.Instance.objetosGuardados.Clear();
            for (int i = 0; i < pendingLoadData.objetosNombres.Count; i++)
            {
                ItemData item = BuscarItemPorNombre(pendingLoadData.objetosNombres[i]);
                if (item != null)
                {
                    InventarioManager.Instance.objetosGuardados.Add(new InventarioSlot(item, pendingLoadData.objetosCantidades[i]));
                }
            }

            InventarioManager.Instance.coleccionablesGuardados.Clear();
            for (int i = 0; i < pendingLoadData.coleccionablesNombres.Count; i++)
            {
                ItemData item = BuscarItemPorNombre(pendingLoadData.coleccionablesNombres[i]);
                if (item != null)
                {
                    InventarioManager.Instance.coleccionablesGuardados.Add(new InventarioSlot(item, pendingLoadData.coleccionablesCantidades[i]));
                }
            }

            InventarioManager.Instance.ActualizarUI();
            InventarioManager.Instance.ActualizarMenuPausa();
        }

        // Restaurar estado de salud y armas del jugador de Mundo 1
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            // Asignar munición y estado del arma mediante reflexión o métodos seguros
            var propArma = pc.GetType().GetField("armaDesbloqueada", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propArma != null) propArma.SetValue(pc, pendingLoadData.armaDesbloqueadaMundo1);

            var propUsando = pc.GetType().GetField("usandoArmaADistancia", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propUsando != null) propUsando.SetValue(pc, pendingLoadData.armaDesbloqueadaMundo1);

            pc.balasActualesCargador = pendingLoadData.balasActualesCargadorMundo1;
            pc.ActualizarHUDArma();
        }

        PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
        if (ph != null)
        {
            var propVida = ph.GetType().GetField("vidaActual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (propVida != null) propVida.SetValue(ph, pendingLoadData.vidaActualMundo1);
            ph.SendMessage("ActualizarHUD", SendMessageOptions.DontRequireReceiver);
        }

        // Restaurar posición del jugador si existe posición guardada y es una escena de Mundo 1 o Hub
        if (pendingLoadData.tienePosicionGuardadaMundo1 && (scene.name == "01_Hub" || scene.name == "1_Level1" || scene.name == "1_Level2"))
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerObj.transform.position = new Vector3(pendingLoadData.playerPosXMundo1, pendingLoadData.playerPosYMundo1, playerObj.transform.position.z);
            }
        }

        Debug.Log("Datos de partida aplicados correctamente a la escena: " + scene.name);
        pendingLoadData = null; // Limpiar después de aplicar
    }

    private ItemData BuscarItemPorNombre(string nombre)
    {
        if (baseDatosObjetos == null) return null;
        foreach (var item in baseDatosObjetos)
        {
            if (item != null && item.nombreObjeto == nombre)
            {
                return item;
            }
        }
        return null;
    }
}
