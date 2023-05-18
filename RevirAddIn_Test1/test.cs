using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using SpeckleCore;
using Speckle.Core;
using Linkit;
using Linkit.BuiltElements.Main;
using Linkit.Commits;
using Linkit.Commits.CommitData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Objects;
using Objects.Geometry;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Objects.Primitive;

namespace Linko.Connector
{
    [Transaction(TransactionMode.Manual)]
    public class RevitLinkoSample : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //List<FamilyInstance> revitColumns = new List<FamilyInstance>();
            //List<FamilyInstance> revitSlabs = new List<FamilyInstance>();
            //List<FamilyInstance> revitBeams = new List<FamilyInstance>();
            //1 用过滤器获取所有Column
            // ...
            //界面交互的doc
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            //实际内容的doc
            Document doc = commandData.Application.ActiveUIDocument.Document;
            //创建收集器
            FilteredElementCollector revitWalls = new FilteredElementCollector(doc);
            FilteredElementCollector revitColumns = new FilteredElementCollector(doc);
            FilteredElementCollector revitBeams = new FilteredElementCollector(doc);
            FilteredElementCollector revitSlabs = new FilteredElementCollector(doc);


            //过滤，墙元素
            //快速过滤（category)
            ICollection<Element> wallElements = revitWalls.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Autodesk.Revit.DB.Wall)).ToElements();
            ICollection<Element> columnElements = revitColumns.OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
            ICollection<Element> beamElements = revitBeams.OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
            ICollection<Element> floorElements = revitSlabs.OfClass(typeof(Autodesk.Revit.DB.Floor)).ToElements();
            //通用过滤
            //ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            //ElementClassFilter elementClassFilter = new ElementClassFilter(typeof(Wall));
            //collector.WherePasses(elementClassFilter).WherePasses(elementClassFilter);          

            ////高亮显示所有墙
            //var sel = uiDoc.Selection.GetElementIds();
            //foreach (var item in revitWalls)
            //{
            //    var height0 = item.LookupParameter("无连接高度").AsDouble() * 0.3048;
            //    TaskDialog.Show("查看结果", height0.ToString());
            //    sel.Add(item.Id);
            //}
            ////
            //uiDoc.Selection.SetElementIds(sel);

            ////高亮显示所有柱
            //var sel_column = uiDoc.Selection.GetElementIds();
            //foreach (var item in revitColumns)
            //{
            //    double height0 = item.LookupParameter("长度").AsDouble() * 0.3048;
            //    TaskDialog.Show("查看结果", height0.ToString());
            //    sel_column.Add(item.Id);
            //}
            ////
            //uiDoc.Selection.SetElementIds(sel_column);
            // ...




           
            //2 获取Column的参数
            List<Column> columns_linko = new List<Column>();
            foreach (var revitColumn in columnElements)
            {

                double height_column = revitColumn.LookupParameter("长度").AsDouble() * 0.3048;

                Autodesk.Revit.DB.XYZ column_point = (revitColumn.Location as LocationPoint).Point;

                Objects.Geometry.Point origin = new Objects.Geometry.Point(column_point.X, column_point.Y, column_point.Z);

                Objects.Geometry.Vector vector_x = new Objects.Geometry.Vector(1, 0, 0);
                Objects.Geometry.Vector vector_y = new Objects.Geometry.Vector(0, 1, 0);
                Objects.Geometry.Vector vector_z = new Objects.Geometry.Vector(0, 0, 1);

                Objects.Geometry.Plane location = new Objects.Geometry.Plane(origin, vector_z, vector_x, vector_y);
                //
                Linkit.BuiltElements.Style.ColumnStyle columnStyle = new Linkit.BuiltElements.Style.ColumnStyle();
                var profile = new Linkit.Other.Profiles.RectangularProfile();
                FamilyInstance revitColumnInstance = revitColumn as FamilyInstance;

                if (revitColumnInstance != null)
                {
                    FamilySymbol symbol = revitColumnInstance.Symbol;
                    if (symbol != null)
                    {
                        Parameter widthParam = symbol.LookupParameter("b");
                        Parameter heightParam = symbol.LookupParameter("h");

                        if (widthParam != null && heightParam != null)
                        {
                            profile.Width = widthParam.AsDouble() * 0.3048; // 转换为米
                            profile.Height = heightParam.AsDouble() * 0.3048; // 转换为米

                            TaskDialog.Show("Column Section Info", $"Column: {revitColumnInstance.Id}\nWidth: {profile.Width} m\nHeight: {profile.Height} m");
                        }
                    }
                }

                FamilyInstance revitColumn_Instance = revitColumn as FamilyInstance;
                columnStyle.Name = revitColumn_Instance.Name;
                columnStyle.FamilyName = revitColumn_Instance.Symbol.Family.Name;

                columnStyle.Profile = profile;
                //3 创建Column
                Column column = new Column(location, height_column, columnStyle);//尽量填充剩余的可选参数参数
                columns_linko.Add(column);
            }
            //2 获取Beam的参数
            List<Beam> beams_linko = new List<Beam>();
            foreach (var revitBeam in beamElements)
            {

                //...
                //...
                double height_beam = (revitBeam.Location as LocationCurve).Curve.Length;
                Autodesk.Revit.DB.Curve beam_curve = (revitBeam.Location as LocationCurve).Curve;

                double location_end_x = beam_curve.GetEndPoint(1).X;
                double location_end_y = beam_curve.GetEndPoint(1).Y;
                double location_end_z = beam_curve.GetEndPoint(1).Z;

                double location_start_x = beam_curve.GetEndPoint(0).X;
                double location_start_y = beam_curve.GetEndPoint(0).Y;
                double location_start_z = beam_curve.GetEndPoint(0).Z;

                Objects.Geometry.Point start = new Objects.Geometry.Point(location_start_x, location_start_y, location_start_z);
                Objects.Geometry.Point end = new Objects.Geometry.Point(location_end_x, location_end_y, location_end_z);
                Objects.Geometry.Line line = new Objects.Geometry.Line(start, end);

                Linkit.BuiltElements.Style.BeamStyle beamStyle = new Linkit.BuiltElements.Style.BeamStyle();

                //内置
                FamilyInstance revitBeam_Instance = revitBeam as FamilyInstance;
                beamStyle.Name = revitBeam_Instance.Name;
                beamStyle.FamilyName = revitBeam_Instance.Symbol.Family.Name;

                string level_name = revitBeam.LookupParameter("参照标高").AsValueString();

                //3 创建Beam
                Beam beam = new Beam(line, beamStyle);//尽量填充剩余的可选参数参数
                beams_linko.Add(beam);
            }


            //1 读取当前文档中的各种构件
            //List<Column> columns_linko = columns;
            //2 创建Commit
            StructureGeneration commitData = new StructureGeneration(columns_linko,beams_linko);

            // 这里用了StructureGeneration，但是这里面只有梁板柱，所以可能需要Linkit定义一种新的commitData，能包含你要上传的所有数据
            // 或者你创建多个commitData，有建筑的，结构的，机电的，分多次commit上传到多个不同的branch
            CommitMetaData commitMetaData = new CommitMetaData("version..", "dependentVersion...", Role.User);
            Linkit.Commits.Commit commit = new Linkit.Commits.Commit(commitData, commitMetaData);

            //3 上传
            var token = "5bfe6d6730f385a94752f292683608bdf45218d58a";
            var streamId = "594bb8dc86";
            var branchName = "test_revit2linko";
            var message_linko = "0421 test";

            Task createCommit = new Task(async () =>
            await Core.LinkoUtils.CreateSpeckleCommit(commit, token, streamId, branchName, message_linko));
            createCommit.Start();





            ////4 结果处理
            //if (success)
            //{
            //    //弹个窗，成功了
            //}
            //else
            //{
            //    //弹个窗，失败了
            //}         
            return Result.Succeeded;
        }



    }
}
