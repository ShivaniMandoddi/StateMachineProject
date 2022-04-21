using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameController
{


    private static GameController instance;
    public static GameController Instance { 
        
        get 
        { 
            if(instance==null)
            {
                instance =new  GameController();
                instance.CheckPoint.AddRange(GameObject.FindGameObjectsWithTag("Checkpoint"));
            }
            return instance; } 
     }
    private List<GameObject> checkPoint = new List<GameObject>();
    public List<GameObject> CheckPoint { get { return (checkPoint); } }

     
}

