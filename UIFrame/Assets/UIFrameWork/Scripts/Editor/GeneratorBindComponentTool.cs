using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System;
using System.Reflection;

public class GeneratorBindComponentTool : Editor
{
    public static List<EditorObjectData> objDataList;//查找对象的数据
    [MenuItem("GameObject/生成组件数据脚本(Shift+B) #B", false, 0)]
    static void CreateFindComponentScripts()
    {
        GameObject obj = Selection.objects.First() as GameObject;//获取到当前选择的物体
        if (obj == null)
        {
            Debug.LogError("需要选择 GameObject");
            return;
        }
        objDataList = new List<EditorObjectData>();
        //设置脚本生成路径
        if (!Directory.Exists(GeneratorConfig.BindComponentGeneratorPath))
        {
            Directory.CreateDirectory(GeneratorConfig.BindComponentGeneratorPath);
        }
        //解析窗口组件数据
        if (GeneratorConfig.ParseType == ParseType.Tag)
            ParseWindowDataByTag(obj.transform, obj.name);
        else
            PresWindowNodeData(obj.transform, obj.name);

        JsonMgr.Instance.SaveData(objDataList, GeneratorConfig.OBJDATALIST_KEY);

        string csContent = CreatCS(obj.name);
        Debug.Log("CsConent:\n" + csContent);

        string cspath = GeneratorConfig.BindComponentGeneratorPath + "/" + obj.name + "DataComponent.cs";
        UIWindowEditor.ShowWindow(csContent, cspath);
        EditorPrefs.SetString("GeneratorClassName", obj.name + "DataComponent");
    }

    /// <summary>
    /// 解析窗口数据
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="WinName"></param>
    public static void PresWindowNodeData(Transform trans, string WinName)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            GameObject obj = trans.GetChild(i).gameObject;
            string name = obj.name;
            if (name.Contains("[") && name.Contains("]"))
            {
                int index = name.IndexOf("]") + 1;
                string fileName = name.Substring(index, name.Length - index);//获取字段昵称
                string fileType = name.Substring(1, index - 2);//获取字段类型
                objDataList.Add(new EditorObjectData { fileName = fileName, fileType = fileType, insID = obj.GetInstanceID() });
            }
            PresWindowNodeData(trans.GetChild(i), WinName);
        }
    }
    public static void ParseWindowDataByTag(Transform trans, string WinName)
    {
        for(int i = 0; i < trans.childCount; i++)
        {
            GameObject obj = trans.GetChild(i).gameObject;
            string tagName = obj.tag;
            if (GeneratorConfig.TAGArr.Contains(tagName))
            {
                string fileName = obj.name;//获取字段名
                string fileType = tagName;//获取字段类型
                objDataList.Add(new EditorObjectData { fileName = fileName, fileType = fileType, insID = obj.GetInstanceID() });
            }
            ParseWindowDataByTag(trans.GetChild(i), WinName);
        }    
    }

    /// <summary>
    /// 自动化生成脚本
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string CreatCS(string name)
    {
        StringBuilder sb = new StringBuilder();
        string nameSpaceName = "UIFrameWork";
        //添加引用和注释
        sb.AppendLine("/*---------------------------------");
        sb.AppendLine(" *Title:UI自动化组件生成代码工具");
        sb.AppendLine(" *Date:" + System.DateTime.Now);
        sb.AppendLine(" *Description:变量需要以[Text]括号加组件类型的格式进行声明，然后右键窗口物体—— 一键生成UI数据组件查找脚本即可");
        sb.AppendLine(" *注意:以下文件是自动生成的，任何手动修改都会被下次生成覆盖,若手动修改后,尽量避免自动生成");
        sb.AppendLine("---------------------------------*/");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using UnityEngine;");

        sb.AppendLine();
        //生成命名空间
        if (!string.IsNullOrEmpty(nameSpaceName))
        {
            sb.AppendLine($"namespace {nameSpaceName}");
            sb.AppendLine("{");
        }
        sb.AppendLine($"\tpublic class {name + "DataComponent:MonoBehaviour"}");
        sb.AppendLine("\t{");
        //根据字段数据列表 声明字段
        foreach (var item in objDataList)
        {
            sb.AppendLine("\t\tpublic  " + item.fileType + "  " + item.fileName + item.fileType + ";\n");
        }
        //声明初始化组件接口
        sb.AppendLine("\t\tpublic  void InitComponent(WindowBase target)");
        sb.AppendLine("\t\t{");
        sb.AppendLine("\t");
        sb.AppendLine("\t");
        sb.AppendLine("\t\t     //组件事件绑定");
        //得到逻辑类 WindowBase => LoginWindow
        sb.AppendLine($"\t\t    {name} mWindow=({name})target;");

        //生成UI事件绑定代码
        foreach (var item in objDataList)
        {
            string type = item.fileType;
            string methodName = item.fileName;
            string suffix = "";
            if (type.Contains("Button"))
            {
                suffix = "Click";
                sb.AppendLine($"\t\t     target.AddButtonClickListener({methodName}{type},mWindow.On{methodName}Button{suffix});");
            }
            if (type.Contains("InputField"))
            {
                sb.AppendLine($"\t\t     target.AddInputFieldListener({methodName}{type},mWindow.On{methodName}InputChange,mWindow.On{methodName}InputEnd);");
            }
            if (type.Contains("Toggle"))
            {
                suffix = "Change";
                sb.AppendLine($"\t\t     target.AddToggleClickListener({methodName}{type},mWindow.On{methodName}Toggle{suffix});");
            }
        }
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        if (!string.IsNullOrEmpty(nameSpaceName))
        {
            sb.AppendLine("}");
        }
        return sb.ToString();
    }

    public static EditorObjectData GetEditorObjectData(int insid)
    {
        foreach (var item in objDataList)
        {
            if (item.insID == insid)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// 编译完成系统自动调用
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    public static void AddComponentToWindow()
    {
        //如果当前不是生成数据脚本的回调,就不处理
        string className= EditorPrefs.GetString("GeneratorClassName");
        if (string.IsNullOrEmpty(className))
        {
            return;
        }
        //1.通过反射的方式，从程序集中找到这个脚本，把它挂在到当前的物体上
        //获取所有的程序集
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //找到C#程序集
        var cSharpAssembly = assemblies.First(assembly => assembly.GetName().Name == "Assembly-CSharp");
        //获取类所在的程序集路径
        string relClassName = "UIFrameWork." + className;
        Type classType=cSharpAssembly.GetType(relClassName);
        if(classType == null)
        {
            return;
        }
        //获取要挂载的那个物体
        string windowObjName = className.Replace("DataComponent", "");//去掉classname中多余的部分 
        GameObject windowObj = GameObject.Find(windowObjName);
        if(windowObj == null)
        {
            windowObj = GameObject.Find("UIRoot/" + windowObjName);
            if(windowObj==null)
            {
                return;
            }
        }
        //先获取现窗口上有没有挂载该数据组件,如果没挂载再进行挂载
        Component compt = windowObj.GetComponent(classType);
        if(compt == null)
        {
            compt=windowObj.AddComponent(classType);
        }
        //2.通过反射的方式，遍历数据列表 找到对应的字段，赋值
        //获取对象数据列表
        var objDataList = JsonMgr.Instance.LoadData<List<EditorObjectData>>(GeneratorConfig.OBJDATALIST_KEY);
        //获取脚本所有字段
        FieldInfo[] fieldInfoList = classType.GetFields();

        foreach (var item in fieldInfoList)
        {
            foreach (var objData in objDataList)
            {
                if (item.Name == objData.fileName + objData.fileType)
                {
                    //根据Insid找到对应的对象
                    GameObject uiObject = EditorUtility.InstanceIDToObject(objData.insID) as GameObject;
                    //设置该字段所对应的对象
                    if (string.Equals(objData.fileType, "GameObject"))
                    {
                        item.SetValue(compt, uiObject);
                    }
                    else
                    {
                        item.SetValue(compt, uiObject.GetComponent(objData.fileType));
                    }
                    break;
                }
            }
        }
        EditorPrefs.DeleteKey("GeneratorClassName");
    }
}
