using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleMenu : MonoBehaviour
{
    private const string dataFolder = "Data";

    [Header("File List")]
    public GameObject fileButtonPrefab;
    public RectTransform fileButtonList;

    [Header("Settings Input Fields")]
    public TMP_InputField textDt;
    public TMP_InputField textG;
    public TMP_Dropdown dropdownComputeMode;
    public TMP_Dropdown dropdownSimulationMode;
    public Toggle toggleColors;
    public Toggle toggleCollision;

    private void Start()
    {
        Cursor.visible = true;
    }
    public void StartSimulation(string initialCondsFilePath)
    {
        SimulationParameters.initConditionsFilename = initialCondsFilePath;
        SceneManager.LoadScene(1);
    }

    public void OpenFileSelector()
    {
        foreach (string filepath in Directory.GetFiles(dataFolder)) {
            // Determine basename of file
            string filename = filepath.Substring(dataFolder.Length + 1);

            // Instantiate a button
            GameObject fileButtonObj = Instantiate(fileButtonPrefab, fileButtonList);
            FileButton fileButton = fileButtonObj.GetComponent<FileButton>();

            // Set button properties
            fileButton.titleMenuScript = this;
            fileButton.filepath = filepath;
            fileButton.transform.Find("FileName").GetComponent<TextMeshProUGUI>().text = filename;
        }
    }

    public void SetSettingsFields()
    {
        textDt.text = SimulationParameters.dt.ToString();
        textG.text = SimulationParameters.G.ToString();
        dropdownComputeMode.value = SimulationParameters.computeModeIx;
        dropdownSimulationMode.value = SimulationParameters.exportToVideo ? 1 : 0;
        toggleColors.isOn = SimulationParameters.colorBodies;
        toggleCollision.isOn = SimulationParameters.enableCollision;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /************************************************************/
    /* SETTERS FOR SIMULATION PARAMETERS FROM THE SETTINGS MENU */
    /************************************************************/

    public void SetComputeMode(int index)
    {
        SimulationParameters.computeModeIx = index;
    }

    public void SetExportToVideo(int index)
    {
        SimulationParameters.exportToVideo = (index == 1);
    }

    public void SetG(string G)
    {
        if (G.Equals("")) return;
        SimulationParameters.G = float.Parse(G);
    }

    public void SetDt(string dt)
    {
        if (dt.Equals("")) return;
        SimulationParameters.dt = float.Parse(dt);
    }

    public void SetColorBodies(bool isEnabled)
    {
        SimulationParameters.colorBodies = isEnabled;
    }

    public void SetCollisions(bool isEnabled)
    {
        SimulationParameters.enableCollision = isEnabled;
    }

}
