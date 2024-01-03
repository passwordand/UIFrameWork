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
        UIModule.Instance.PreLoadWindow<HallWindow>();
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    UIModule.Instance.PopUpWindow<LoginWindow>();
        //}
        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    UIModule.Instance.HideWindow<TestWindow>();
        //}
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    UIModule.Instance.PopUpWindow<TempWindow1>();
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    UIModule.Instance.HideWindow<TempWindow1>();
        //}
    }
}
