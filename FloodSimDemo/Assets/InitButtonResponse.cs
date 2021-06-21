using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;
using UnityEngine.UIElements;

public class InitButtonResponse : MonoBehaviour
{
    public GameObject InitPanel;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void ButtonOnClickEvent()
    {
        if(InitPanel.active == false)
        {
            InitPanel.SetActive(true);
        }
        else
        {
            InitPanel.SetActive(false);
        }
    }
}
