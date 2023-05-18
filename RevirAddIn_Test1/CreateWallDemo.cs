using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace RevirAddIn_Test1
{
    [Transaction(TransactionMode.Manual)]
    class CreateWallDemo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //获得当前文档
            Document doc = commandData.Application.ActiveUIDocument.Document;

            //获得 CW 102-50-100p 类型的墙
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            Element ele = collector.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(WallType)).FirstOrDefault(x => x.Name == "常规 - 200mm");
            WallType wallType = ele as WallType;

            //获取标高
            //链式编程
            Level level = new FilteredElementCollector(doc).OfClass(typeof(Level)).FirstOrDefault(x => x.Name == "标高 1") as Level;

            //创建线
            XYZ start = new XYZ(0, 0, 0);
            XYZ end = new XYZ(10, 0, 0);
            Line geomline = Line.CreateBound(start, end);

            //墙的高度
            double height = 15 / 0.3048;
            double offset = 0;

            //创建墙（事务的使用，修改doc 相关的内容放进去就可，若里面的代码出现错误，也会报错）
            Transaction trans = new Transaction(doc, "创建墙");
            trans.Start();

            Wall wall = Wall.Create(doc, geomline, wallType.Id, level.Id, height,offset, false, false);

            trans.Commit();
        

            return Result.Succeeded;
            throw new NotImplementedException();
        }
    }
}
