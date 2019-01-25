using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPop : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        RectTransform g = gameObject.GetComponent<RectTransform>();

        Vector2 min = g.anchorMin;
        Vector2 max = g.anchorMax;

        min.y += 0.005f;
        max.y += 0.005f;

        gameObject.GetComponent<RectTransform>().anchorMin = min;
        gameObject.GetComponent<RectTransform>().anchorMax = max;
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        if (gameObject.GetComponent<RectTransform>().anchorMin.y > 1)
        {
            GM.pop.qtyOfBalls--;
            Debug.Log("Balls left: " + GM.pop.qtyOfBalls +
                "\nObjects with tag found: " + GameObject.FindGameObjectsWithTag("ballPop").Length);

            if (GameObject.FindGameObjectsWithTag("ballPop").Length == 1)
            {
                GM.pop.done = true;
                Destroy(gameObject);
            }
            else
                GM.pop.done = false;
            Destroy(gameObject);
        }

    }
}
