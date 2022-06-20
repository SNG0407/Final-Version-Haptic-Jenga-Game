using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    private Text scoreResultText; 
    void Start()
    {
        scoreResultText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject tower = GameObject.Find("Tower");
        if (tower != null)
        {
            if (tower.GetComponent<Tower>() != null)
            {
                scoreResultText.text = "Score : " + tower.GetComponent<Tower>().getScore().ToString();
            }
        }
    }
}
