﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grain : MonoBehaviour
{
    public Vector2 position;
    public int energy;
    void Start()
    {
        float biasX = Random.Range(0f, 0.8f);
        float biasY = (-1)*Random.Range(0f, 0.8f);
        this.energy = 0; 
        position = new Vector2(position.x + biasX, position.y + biasY);
    }

    public void OnButtonClick()
    {
        gameObject.GetComponent<Image>().color = ColorHandler.GenerateColor();
    }
}
