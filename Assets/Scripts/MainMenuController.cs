using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public InputField width;
    public InputField height;

    public Dropdown neighborhood;
    public Dropdown nucleation;
    public Dropdown boundary;


    public void OnStartClick()
    {
        if (string.IsNullOrEmpty(width.text)||string.IsNullOrEmpty(height.text) || int.Parse(width.text) < 1 || int.Parse(height.text) < 1)
        {
            Debug.Log("Some values are incorect!");
            return;
        }

        if (neighborhood.options[neighborhood.value].text.Equals("None"))
        {
            return;
        }

        if (nucleation.options[nucleation.value].text.Equals("None"))
        {
            return;
        }

        if (boundary.options[boundary.value].text.Equals("None"))
        {
            return;
        }

        PlayerPrefs.SetInt("width", int.Parse(width.text));
        PlayerPrefs.SetInt("height", int.Parse(height.text));
        PlayerPrefs.SetString("neighborhoodMethod", neighborhood.options[neighborhood.value].text);
        PlayerPrefs.SetString("nucleationMethod", nucleation.options[nucleation.value].text);
        PlayerPrefs.SetString("boundaryMethod", boundary.options[boundary.value].text);

        SceneManager.LoadScene("Visualisation");
    }
}
