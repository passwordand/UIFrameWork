using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Text;

public class GeneratorWindowTool : Editor
{
    static Dictionary<string, string> methodDic = new Dictionary<string, string>();
    [MenuItem("GameObject/生成Window脚本", false, 0)]
    static void CreateFindComponentScripts()
    {
        GameObject obj = Selection.objects.First() as GameObject;//获取到当前选择的物体
        if (obj == null)
        {
            Debug.LogError("需要选择 GameObject");
            return;
        }
        //设置脚本生成路径
        if (!Directory.Exists(GeneratorConfig.WindowGeneratorPath))
        {
            Directory.CreateDirectory(GeneratorConfig.WindowGeneratorPath);
        }


        string csContent = CreateWindoCs(obj.name);
        Debug.Log("CsConent:\n" + csContent);

        string cspath = GeneratorConfig.WindowGeneratorPath + "/" + obj.name + ".cs";
        UIWindowEditor.ShowWindow(csContent,cspath,methodDic);
        //if (File.Exists(cspath))
        //{
        //    File.Delete(cspath);
        //}
        //StreamWriter writer = File.CreateText(cspath);
        //writer.Write(csContent);
        //writer.Close();
        //AssetDatabase.Refresh();

    }

    /// <summary>
    /// 生成Window脚本
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string CreateWindoCs(string name)
    {
        //读取json
        var objDatalist = JsonMgr.Instance.LoadData<List<EditorObjectData>>(GeneratorConfig.OBJDATALIST_KEY);

        StringBuilder sb=new StringBuilder();
        //添加引用
        sb.AppendLine("/*---------------------------------");
        sb.AppendLine(" *Title:UI表现层脚本自动化生成工具");
        sb.AppendLine(" *Date:" + System.DateTime.Now);
        sb.AppendLine(" *Description:UI 表现层，该层只负责界面的交互、表现相关的更新，不允许编写任何业务逻辑代码");
        sb.AppendLine(" *注意:以下文件是自动生成的，再次生成不会覆盖原有的代码，会在原有的代码上进行新增，可放心使用");
        sb.AppendLine("---------------------------------*/");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UIFrameWork;");
        //sb.AppendLine("using UIFrameWork;");
        sb.AppendLine();

        //生成类名
        sb.AppendLine($"\tpublic class {name}:WindowBase");
        sb.AppendLine("\t{");
        sb.AppendLine("\t");

            //生成字段
        sb.AppendLine($"\t\t public {name}UIComponent uiCompt=new {name}UIComponent();");

        //生成声明周期函数 Awake
        sb.AppendLine("\t");
        sb.AppendLine($"\t\t #region 声明周期函数");
        sb.AppendLine($"\t\t //调用机制与Mono Awake一致");
        sb.AppendLine("\t\t public override void OnAwake()");
        sb.AppendLine("\t\t {");
        sb.AppendLine($"\t\t\t uiCompt.InitComponent(this);");
        sb.AppendLine("\t\t\t base.OnAwake();");
        sb.AppendLine("\t\t }");
        //OnShow
        sb.AppendLine($"\t\t //物体显示时执行");
        sb.AppendLine("\t\t public override void OnShow()");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t\t base.OnShow();");
        sb.AppendLine("\t\t }");
        //OnHide
        sb.AppendLine($"\t\t //物体隐藏时执行");
        sb.AppendLine("\t\t public override void OnHide()");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t\t base.OnHide();");
        sb.AppendLine("\t\t }");

        //OnDestroy
        sb.AppendLine($"\t\t //物体销毁时执行");
        sb.AppendLine("\t\t public override void OnDestroy()");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t\t base.OnDestroy();");
        sb.AppendLine("\t\t }");

        sb.AppendLine($"\t\t #endregion");

        //API Function 
        sb.AppendLine($"\t\t #region API Function");
        sb.AppendLine($"\t\t    ");
        sb.AppendLine($"\t\t #endregion");
        //UI组件事件生成
        sb.AppendLine($"\t\t #region UI组件事件");
        foreach (var item in objDatalist)
        {
            string type = item.fileType;
            string methodName = "On" + item.fileName;
            string suffix = "";
            if (type.Contains("Button"))
            {
                suffix = "ButtonClick";
                CreateMethod(sb, ref methodDic, methodName + suffix);
            }
            else if (type.Contains("InputField"))
            {
                suffix = "InputChange";
                CreateMethod(sb, ref methodDic, methodName + suffix, "string text");
                suffix = "InputEnd";
                CreateMethod(sb, ref methodDic, methodName + suffix, "string text");
            }
            else if (type.Contains("Toggle"))
            {
                suffix = "ToggleChange";
                CreateMethod(sb, ref methodDic, methodName + suffix, "bool state,Toggle toggle");
            }
        }
        sb.AppendLine($"\t\t #endregion");

        sb.AppendLine("\t}");
        return sb.ToString();
    }

    public static void CreateMethod(StringBuilder sb,ref Dictionary<string,string> methodDic,string methodName,string param="")
    {
        //声明UI组件事件
        sb.AppendLine($"\t\t public void {methodName}({param})");
        sb.AppendLine("\t\t {");
        sb.AppendLine("\t\t");
        if (methodName == "OnCloseButtonClick")
        {
            sb.AppendLine("\t\t\tHideWindow();");
        }
        sb.AppendLine("\t\t }");

        //存储UI组件事件 提供给后续新增代码使用
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"\t\t public void {methodName}({param})");
        builder.AppendLine("\t\t {");
        builder.AppendLine("\t\t");
        builder.AppendLine("\t\t }");
        methodDic.Add(methodName, builder.ToString());
    }
}
