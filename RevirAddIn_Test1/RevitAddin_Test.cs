using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevirAddIn_Test1
{
    [Transaction(TransactionMode.Manual)]
    public class RevitAddin_Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //界面交互的doc
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            //实际内容的doc
            Document doc = commandData.Application.ActiveUIDocument.Document;
            //创建收集器
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            FilteredElementCollector collectorTwo = new FilteredElementCollector(doc);
            //过滤，墙元素
            //快速过滤（category)
            collector.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Wall));
            //通用过滤
            //ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            //ElementClassFilter elementClassFilter = new ElementClassFilter(typeof(Wall));
            //collector.WherePasses(elementClassFilter).WherePasses(elementClassFilter);

            //某种墙组类型下族实例的获取
            // foreach获取
            List<Element> elementList = new List<Element>();
            foreach (var item in collector)
            {
                if (item.Name == "CL_W1")
                {
                    elementList.Add(item);
                }
            }
            //转为list处理
            List<Element> elementlistTwo = collector.ToList<Element>();
            //linq表达式
            var wallElement = from element in collector
                              where element.Name == "CL_W1"
                              select element;
            Element wallInstance = wallElement.LastOrDefault<Element>();
          

            //某族实例的获取（确定只有一个实例）
            //list获取
            Element wallInstanceTwo = elementList[0];
            //IEnumberable获取
            Element wallInstanceThree = wallElement.FirstOrDefault<Element>();
            //lambda表达式的一种写法
            Element ele = collectorTwo.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Wall)).FirstOrDefault<Element>(y => y.Name == "CL_W1");

            //有多个实例，但是只想获取其中一个，可以使用ElementID,或者根据一些特征
            Element wallInstanceFour = doc.GetElement(new ElementId(243274));

            //类型判断与转换(用别的类型的method)
            foreach(var item in elementList)
            {
                if(item is Wall)
                {
                    Wall wall = item as Wall;
                    Wall wallTwo = (Wall)item;
                }
            }

            //高亮显示实例
            var sel = uiDoc.Selection.GetElementIds();

            foreach(var item in collector)
            {
                //TaskDialog.Show("查看结果", item.Name);
                sel.Add(item.Id);
            }
            //
            return Result.Succeeded;
            

            throw new NotImplementedException();
        }
    }
}
