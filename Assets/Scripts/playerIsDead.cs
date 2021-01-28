using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerIsDead : MonoBehaviour
{
    public GameObject Player;

    // Death = false, Win = true.
    public bool deathOrWin;

    private bool hasCoroutineStarted;
    
    private MainGameManager myMainGameManagerScript;
    public GameObject sceneManager;

    void Start()
    {
        myMainGameManagerScript = sceneManager.GetComponent<MainGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player != null)
        {
            if (!Player.active && hasCoroutineStarted == false)
            {
                hasCoroutineStarted = true;
                StartCoroutine(DetermineWinOrDeath());
            }
        }
    }

    IEnumerator DetermineWinOrDeath()
    {
        
        if (deathOrWin == false)
        {
            yield return new WaitForSeconds(1f);
            myMainGameManagerScript.enablePausing = 2;
            gameObject.GetComponent<playerIsDead>().enabled = false;
        }
        else
        {
            yield return new WaitForSeconds(5f);
            myMainGameManagerScript.enablePausing = 3;
            gameObject.GetComponent<playerIsDead>().enabled = false;
        }
    }
}
