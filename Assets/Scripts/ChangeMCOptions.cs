using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeMCOptions : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject canvas;
    
    public InputField ktParameter;
    public InputField iterationNumber;
    public InputField radius;
    
    public Dropdown neighborhood;
    public Dropdown boundary;

    public void OnChangeClick()
    {
        if (!(string.IsNullOrEmpty(iterationNumber.text) || int.Parse(iterationNumber.text) < 1))
        {
            PlayerPrefs.SetInt("iterationMax", int.Parse(iterationNumber.text));
        }

        if (!(string.IsNullOrEmpty(ktParameter.text) || float.Parse(ktParameter.text) < 0.1 || float.Parse(ktParameter.text) > 6))
        {
            PlayerPrefs.SetFloat("kt", float.Parse(ktParameter.text));
        }

        if (!(neighborhood.options[neighborhood.value].text.Equals("None")))
        {
            PlayerPrefs.SetString("neighborhoodMethod", neighborhood.options[neighborhood.value].text);
        }

        if (!(boundary.options[boundary.value].text.Equals("None")))
        {
            if (boundary.options[boundary.value].text.Equals("Radius") && (!(string.IsNullOrEmpty(radius.text))) && float.Parse(radius.text) > 0)
            {
                PlayerPrefs.SetString("boundaryMethod", boundary.options[boundary.value].text);
                PlayerPrefs.SetFloat("radiusNeighborhood", float.Parse(radius.text));
            }
            else
            {
                PlayerPrefs.SetString("boundaryMethod", boundary.options[boundary.value].text);
            }   
        }

        canvas.GetComponent<VisualisationController>().ReloadPrefs();
        gameObject.SetActive(false);
    }

}
