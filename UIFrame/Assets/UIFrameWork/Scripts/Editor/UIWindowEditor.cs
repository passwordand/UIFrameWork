using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Text.RegularExpressions;

public class UIWindowEditor : EditorWindow
{
    private string scriptContent;
    private string filePath;
    private Vector2 scroll=new Vector2 ();

    /// <summary>
    /// 生成展示WINDOW窗口
    /// </summary>
    /// <param name="content"></param>
    /// <param name="filePath"></param>
    /// <param name="insertDic">创建的ui的方法字典</param>
    public static void ShowWindow(string content,string filePath,Dictionary<string,string> insertDic=null)
    {
        //创建代码展示窗口
        UIWindowEditor window = (UIWindowEditor)GetWindowWithRect(typeof(UIWindowEditor), new Rect(100, 50, 800, 700), true, "Window生成界面");
        window.scriptContent = content;
        window.filePath = filePath;
        //新增代码的处理
        if(File.Exists(filePath)&&insertDic!=null)
        {
            //先获取原始的代码
            string originScript=File.ReadAllText(filePath);
            foreach (var item in insertDic)
            {
                //如果老代码没有就插入代码
                if(!originScript.Contains(item.Key))
                {
                    int index = window.GetInsertIndex(item.Key);
                    originScript=window.scriptContent=originScript.Insert(index,item.Value+"\t\t");
                }
            }
        }
        window.Show();
    }
    public void OnGUI()
    {
        //绘制ScroView
        scroll= EditorGUILayout.BeginScrollView(scroll,GUILayout.Height(600),GUILayout.Width(800));
        EditorGUILayout.TextArea(scriptContent);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space();

        //绘制脚本生成路径
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextArea("脚本生成路径：" + filePath);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        //绘制按钮
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("生成脚本",GUILayout.Height(30)))
        {
            //按钮事件
            ButtonClick();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ButtonClick()
    {
        if(File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        StreamWriter writer = File.CreateText(filePath);
        writer.Write(scriptContent);
        writer.Close();
        AssetDatabase.Refresh();
        if (EditorUtility.DisplayDialog("自动化生成工具", "生成脚本成功", "确定"))
        {
            Close();
        }
    }

    /// <summary>
    /// 获取插入代码的下标
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public int GetInsertIndex(string content)
    {
        //找到UI组件事件下面的第一个public位置进行插入
        //先声明正责表达式
        Regex regex = new Regex("UI组件事件");
        Match match=regex.Match(content);

        Regex regex1 = new Regex("public");
        MatchCollection matchCollection=regex1.Matches(content);
        //找到所有的public的索引,然后遍历去找到比match的索引大的那个
        for(int i=0;i<matchCollection.Count;i++)
        {
            if (matchCollection[i].Index>match.Index)
            {
                return matchCollection[i].Index;
            }
        }
        return -1;
    }
}
