using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace revit2linko_WPF
{
    [Transaction(TransactionMode.Manual)]
    public class MainProgram : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MainWindow mainWindow = new MainWindow();
            //非模态（两个窗口可以动）
            mainWindow.Show();
            return Result.Succeeded;
        }
    }
    //public class MainProgram : IExternalApplication
    //{
    //    public Result OnStartup(UIControlledApplication application)
    //    {
    //        //添加一个新的Ribbon面板
    //        RibbonPanel ribbonPanel = application.CreateRibbonPanel("Linko");

    //        //在新的Ribbon面板上增加一个按钮
    //        //点击这个按钮，调用前面IExternalCommand中的示例，即删除选择对象的命令
    //        //下面语句中@后面字符串为我前面的示例RevitCommand : IExternalCommand中生成的dll包的位置
    //        PushButton pushButton = ribbonPanel.AddItem(new PushButtonData("RevitCommand",
    //            "删除",
    //            @"D:\REPO\RevitAddIn_Test\RevirAddIn_Test1\revit2linko_WPF\bin\Debug\revit2linko_WPF.dll",
    //            "revit2linko_WPF.RevitCommand")) as PushButton;
    //        return Result.Succeeded;
    //    }

    //    public Result OnShutdown(UIControlledApplication application)
    //    {
    //        //关闭时不需要特别的操作，直接返回即可
    //        return Result.Succeeded;
    //    }

    //    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    //    {
    //        MainWindow mainWindow = new MainWindow();
    //        //非模态（两个窗口可以动）
    //        mainWindow.Show();
    //        return Result.Succeeded;
    //    }
    //}
}
