using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "WindowConfig", menuName = "WindowConfig", order = 0)]
public class WindowConfig : ScriptableObject
{
    //��¼��Ҫ��ȡ���ļ���
    private string[] windowRootArray = new string[] {"Game","Hall","Window" };
    public List<WindowData> windowDataList=new List<WindowData> ();
    public void GeneraWindowConfig()
    {
        //���Ԥ������û�����������û�оͲ���Ҫ��������
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
            Debug.Log("Ԥ�������û�з����ı䣬�����ɴ�������");
            return;
        }
        windowDataList.Clear();
        foreach (var item in windowRootArray)
        {
            //��ȡԤ�����ļ��ж�ȡ·��
            string floder = Application.dataPath + "/UIFrameWork/Resources/" + item;
            //��ȡ�ļ����µ�����Prefab�ļ�
            string[] filePathArray = Directory.GetFiles(floder, "*.prefab", SearchOption.AllDirectories);
            foreach (var path in filePathArray)
            {
                if (path.EndsWith(".meta"))
                {
                    continue;
                }
                //��ȡԤ��������
                string fileName = Path.GetFileNameWithoutExtension(path);
                //�����ļ���ȡ·��
                string filePath = item + "/" + fileName;
                //�����ļ�����·��
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
        Debug.LogError(name + "�����ļ��в�����,����Ԥ����λ�û��������ļ�");
        return "";
    }
}
[System.Serializable]
public class WindowData
{
    public string name;
    public string path;
}