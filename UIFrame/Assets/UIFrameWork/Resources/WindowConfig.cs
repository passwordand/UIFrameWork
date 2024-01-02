using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "WindowConfig", menuName = "WindowConfig", order = 0)]
public class WindowConfig : ScriptableObject
{
    //记录需要读取的文件夹
    private string[] windowRootArray = new string[] {"Game","Hall","Window" };
    public List<WindowData> windowDataList=new List<WindowData> ();
    public void GeneraWindowConfig()
    {
        //检测预制体有没有新增，如果没有就不需要生成配置
        int count = 0;
        foreach (var item in windowRootArray)
        {
            string[] filePathArr = Directory.GetFiles(Application.dataPath + "/UIFrameWork/Resources/" + item, "*.prefab", SearchOption.AllDirectories);
            foreach (var path in filePathArr)
            {
                if (path.EndsWith(".meta"))
                {
                    continue;
                }
                count += 1;
            }
        }
        if (count == windowDataList.Count)
        {
            Debug.Log("预制体个数没有发生改变，不生成窗口配置");
            return;
        }
        windowDataList.Clear();
        foreach (var item in windowRootArray)
        {
            //获取预制体文件夹读取路径
            string floder = Application.dataPath + "/UIFrameWork/Resources/" + item;
            //获取文件夹下的所有Prefab文件
            string[] filePathArray = Directory.GetFiles(floder, "*.prefab", SearchOption.AllDirectories);
            foreach (var path in filePathArray)
            {
                if (path.EndsWith(".meta"))
                {
                    continue;
                }
                //获取预制体名字
                string fileName = Path.GetFileNameWithoutExtension(path);
                //计算文件读取路径
                string filePath = item + "/" + fileName;
                //填入文件名和路径
                WindowData data = new WindowData { name = fileName, path = filePath };
                windowDataList.Add(data);
            }
        }
    }
    public string GetWindowPath(string name)
    {
        foreach (var item in windowDataList)
        {
            if(string.Equals(item.name,name))
            {
                return item.path;
            }
        }
        Debug.LogError(name + "配置文件中不存在,请检查预制体位置或者配置文件");
        return "";
    }
}
[System.Serializable]
public class WindowData
{
    public string name;
    public string path;
}