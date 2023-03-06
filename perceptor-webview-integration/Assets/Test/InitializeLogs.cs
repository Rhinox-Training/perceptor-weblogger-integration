using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rhinox.Perceptor;
using Rhinox.Utilities;

public class InitializeLogs : MonoBehaviour
{
    int frames;
    void Awake() {
        var logtarget = new WebsiteLogTarget();

        //LoggerDefaults.Instance.FindSetting();
        PLog.AppendLogTargetToDefault(logtarget);
        
        PLog.AppendLogTarget<UtilityLogger>(logtarget);
        PLog.CreateIfNotExists();


    }
    // Start is called before the first frame update
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        Test test;  
        frames++;
        if (frames % 200 == 0)
        {
            int i = Random.Range(-1,6) ;
            switch (i)
            {
                case 0:
                    PLog.Info("infoddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
                    break;
                case 1:
                    PLog.Trace("traceddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
                    break;
                case 2:

                    PLog.Debug("debug");
                    PLog.Fatal("object Test = null");
                    break;
                case 3:
                    PLog.Warn("warningdddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
                    break;
                case 4:
                    PLog.Fatal("Fatalddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
                    break;
                case 5:
                    PLog.Error("errordddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
                    break;
                default:
                    break;
            }

        
     
       
     
    
        }
    }
}


class Test {
    public void testvoid() { }
}