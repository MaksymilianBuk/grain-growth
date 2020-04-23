using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubscreenController : MonoBehaviour
{
    public InputField mainInput;
    public InputField secondInput;

    public void OnClick1Input(string type)
    {
        if (string.IsNullOrEmpty(mainInput.text) || int.Parse(mainInput.text) < 1)
        {
            Debug.Log("Some values are incorect!");
            return;
        }
        if (type.Equals("radiusNeighbor"))
        {
            PlayerPrefs.SetFloat("radiusNeighborhood", float.Parse(mainInput.text));
        }
        if (type.Equals("randomNucleation"))
        {
            PlayerPrefs.SetInt("randomUnits", int.Parse(mainInput.text));
        }

        gameObject.SetActive(false);
        
    }

    public void OnClick2Inputs(string type)
    {
        if (string.IsNullOrEmpty(mainInput.text) || string.IsNullOrEmpty(secondInput.text)||int.Parse(mainInput.text)<1||int.Parse(secondInput.text)<1)
        {
            Debug.Log("Some values are incorect!");
            return;
        }
        if (type.Equals("radiusNucleation"))
        {
            PlayerPrefs.SetFloat("radiusNucleation", float.Parse(mainInput.text));
            PlayerPrefs.SetInt("amountNucleation", int.Parse(secondInput.text));
        }
        if (type.Equals("plainNucleation"))
        {
            PlayerPrefs.SetInt("widthUnits", int.Parse(mainInput.text));
            PlayerPrefs.SetInt("heightUnits", int.Parse(secondInput.text));
        }
        gameObject.SetActive(false);
    }
}
