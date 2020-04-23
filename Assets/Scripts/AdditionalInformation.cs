using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdditionalInformation : MonoBehaviour
{
    public Dropdown neighborhoodDropdown;
    public Dropdown nucleationDropdown;

    public GameObject setRadiusNeighborhoodScreen;
    public GameObject setRadiusNucleationScreen;
    public GameObject setPlainNucleationScreen;
    public GameObject setRandomNucleationScreen;

    private void Start()
    {
        neighborhoodDropdown.onValueChanged.AddListener(delegate { NeighborhoodDropdownValueChanged(neighborhoodDropdown);});
        nucleationDropdown.onValueChanged.AddListener(delegate { NucleationDropdownValueChanged(nucleationDropdown);});
    }


    void NucleationDropdownValueChanged(Dropdown change)
    {
        string value = nucleationDropdown.options[nucleationDropdown.value].text;
        switch (value)
        {
            case "Plain":
                {
                    setPlainNucleationScreen.SetActive(true);
                    break;  
                }
            case "Radius":
                {
                    setRadiusNucleationScreen.SetActive(true);
                    break;
                }
            case "Random":
                {
                    setRandomNucleationScreen.SetActive(true);
                    break;
                }
        }
        Debug.Log(value);
    }

    void NeighborhoodDropdownValueChanged(Dropdown change)
    {
        string value = neighborhoodDropdown.options[neighborhoodDropdown.value].text;

        switch (value)
        {
            case "Radius":
                {
                    setRadiusNeighborhoodScreen.SetActive(true);
                    break;
                }
        }
        Debug.Log(value);
    }

}
