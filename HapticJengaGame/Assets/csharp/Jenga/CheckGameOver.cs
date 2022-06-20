using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGameOver : MonoBehaviour
{
    public int numOfObjectCollided;
    public int boundNum = 6;
    void Start()
    {
        numOfObjectCollided = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Block>() != null)
        {
            numOfObjectCollided += 1;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<Block>() != null)
        {
            numOfObjectCollided -= 1;
        }
    }

    public void ResetNumOfObjectCollided()
    {
        numOfObjectCollided = 0;
    }

    public bool IsGameOver()
    {
        if (numOfObjectCollided > boundNum)
        {
            return true;
        }
        return false;
    }
}