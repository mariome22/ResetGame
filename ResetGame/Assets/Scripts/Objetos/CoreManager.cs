using UnityEngine;
using TMPro;

public class CoreManager : MonoBehaviour
{
    public int totalCores = 0;
    public TextMeshProUGUI coreTextUI;
    public GameObject chestPanel;

    private void Start()
    {
        //Cargamos los núcleos guardados (si existen)
        totalCores = PlayerPrefs.GetInt("PlayerCores", 0);
    }

    //Cuando se gana un nivel
    public void AddCore()
    {
        totalCores++;
        PlayerPrefs.SetInt("PlayerCores", totalCores);
        PlayerPrefs.Save();
    }

    //Abre el cofre de nucleos del hub
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