using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Score : MonoBehaviour
{
    private Text scoreText;
    void Start()
    {
        scoreText = GetComponent<Text>();
    }

    void Update()
    {
        GameObject tower = GameObject.Find("Tower");
        if(tower != null)
        {
            if(tower.GetComponent<Tower>() != null)
            {
                scoreText.text = tower.GetComponent<Tower>().getScore().ToString();
            }
        }
    }
}
