using UnityEngine;
using TMPro; // Para el texto de la UI

public class CoreManager : MonoBehaviour
{
    public int totalCores = 0;
    public TextMeshProUGUI coreTextUI;
    public GameObject chestPanel;

    private void Start()
    {
        // Cargamos los núcleos guardados (si existen)
        totalCores = PlayerPrefs.GetInt("PlayerCores", 0);
    }

    // Llama a esto cuando ganes un nivel
    public void AddCore()
    {
        totalCores++;
        PlayerPrefs.SetInt("PlayerCores", totalCores);
        PlayerPrefs.Save();
    }

    // Llama a esto desde el Interactable de la Caja
    public void ToggleChestUI()
    {
        bool isActive = chestPanel.activeSelf;
        chestPanel.SetActive(!isActive);

        if (!isActive)
        {
            coreTextUI.text = "Núcleos obtenidos: " + totalCores;
        }
    }
}