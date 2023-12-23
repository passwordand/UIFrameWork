using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private void Awake()
    {
        UIModule.Instance.Initialize();
    }
    private void Start()
    {
        LoginWindow loginWindow = UIModule.Instance.PopUpWindow<LoginWindow>();
        loginWindow.Test();
        LoginWindow login2=UIModule.Instance.GetWindow<LoginWindow>();
        login2.Test2();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            UIModule.Instance.HideWindow<LoginWindow>();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            UIModule.Instance.DestroyWindow<LoginWindow>();
        }
    }
}
