using UnityEngine;

public class FileButton : MonoBehaviour
{
    public TitleMenu titleMenuScript;
    public string filepath;

    public void OnClick()
    {
        titleMenuScript.StartSimulation(filepath);
    }
}
