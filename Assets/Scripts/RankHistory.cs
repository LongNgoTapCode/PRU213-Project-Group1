using UnityEngine;

public class RankHistory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   void Start()
{
    LocalScoreStorage.SaveRun(100, 50);
    LocalScoreStorage.SaveRun(200, 40);
    LocalScoreStorage.SaveRun(150, 60);
}


    // Update is called once per frame
    void Update()
    {
        
    }
}
