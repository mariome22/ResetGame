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
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Base de Datos de Objetos")]
    [Tooltip("Arrastra aquí todos los assets ItemData del proyecto para poder reconstruir el inventario al cargar")]
    public List<ItemData> baseDatosObjetos = new List<ItemData>();

    private string saveFilePath;
    private SaveData pendingLoadData = null;

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
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Partida guardada borrada.");
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 1. Escena actual
        data.escenaGuardada = SceneManager.GetActiveScene().name;

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

        // 5. Estado Mundo 2
        data.livesMundo2 = PlayerPlatformerController.lives;
        data.totalCoinsMundo2 = PlayerPlatformerController.totalCoins;
        data.secretCoinsCollectedMundo2 = PlayerPlatformerController.secretCoinsCollected;
        data.lastCheckpointSceneMundo2 = PlayerPlatformerController.lastCheckpointScene;
        data.lastCheckpointPosXMundo2 = PlayerPlatformerController.lastCheckpointPos.x;
        data.lastCheckpointPosYMundo2 = PlayerPlatformerController.lastCheckpointPos.y;
        data.remainingTimeMundo2 = PlayerPlatformerController.remainingTime;
        data.checkpointTimeMundo2 = PlayerPlatformerController.checkpointTime;
        data.consecutiveCheckpointDeathsMundo2 = PlayerPlatformerController.consecutiveCheckpointDeaths;

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
        SceneManager.LoadScene(pendingLoadData.escenaGuardada);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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

        if (HUDPlatformerManager.Instance != null)
        {
            HUDPlatformerManager.Instance.UpdateHUD();
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
