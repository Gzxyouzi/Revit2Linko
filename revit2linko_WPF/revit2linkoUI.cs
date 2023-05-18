//using Autodesk.Revit.Attributes;
//using Autodesk.Revit.UI;
//using Autodesk.Revit.UI.Selection;
//using Autodesk.Revit.ApplicationServices;
//using System.Windows.Media.Imaging;
//using System.Reflection;

//namespace revit2linko_UI
//{
//    [Transaction(TransactionMode.Manual)]
//    public class revit2linkoUI : IExternalApplication
//    {
//        public Result OnShutdown(UIControlledApplication application)
//        {
//            return Result.Succeeded;
//        }

//        public Result OnStartup(UIControlledApplication application)
//        {
//            //创建rhibbon tab
//            application.CreateRibbonTab("Linko_test");
//            //创建ribbon pannel
//            RibbonPanel rp = application.CreateRibbonPanel("revit2linko", "revit2linko");
//            string assemblyPath = @"D:\REPO\RevitAddIn_Test\RevirAddIn_Test1\revit2linko_WPF\bin\Debug\revit2linko_WPF.dll";
//            string classnamerevit2linko = "revit2linko_WPF.MainProgram";
//            PushButtonData pbd = new PushButtonData("innerNameRevit", "Revit2Linko", assemblyPath, classnamerevit2linko);
//            PushButton pushButton = rp.AddItem(pbd) as PushButton;
//            return Result.Succeeded;
//        }
//    }
//}
