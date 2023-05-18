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
using Autodesk.Revit.UI.Selection;
using TuoLuoUtils;

namespace revit2linko_WPF
{
    class upload_beam_select : IExternalEventHandler
    {
        public string BranchName { get; set; }
        public string Message_linko { get; set; }

        public void Execute(UIApplication app)
        {
            //界面交互的doc
            UIDocument uiDoc = app.ActiveUIDocument;
            //实际内容的doc
            Document doc = app.ActiveUIDocument.Document;

            Selection selection = uiDoc.Selection;
            //单选，只选Floor
            Element elem = uiDoc.PickElementByElementType<Element>();
            

            ////创建收集器
            //FilteredElementCollector revitWalls = new FilteredElementCollector(doc);
            //FilteredElementCollector revitColumns = new FilteredElementCollector(doc);
            //FilteredElementCollector revitBeams = new FilteredElementCollector(doc);
            //FilteredElementCollector revitSlabs = new FilteredElementCollector(doc);

            ////过滤，墙元素
            ////快速过滤（category)
            //ICollection<Element> wallElements = revitWalls.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Autodesk.Revit.DB.Wall)).ToElements();
            //ICollection<Element> columnElements = revitColumns.OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
            //ICollection<Element> beamElements = revitBeams.OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
            //ICollection<Element> floorElements = revitSlabs.OfClass(typeof(Autodesk.Revit.DB.Floor)).ToElements();
            
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

            //2 获取Beam的参数
            List<Column> columns_linko = new List<Column>();
            List<Slab> slabs_linko = new List<Slab>();
            List<Beam> beams_linko = new List<Beam>();
            //
            double height_beam = (elem.Location as LocationCurve).Curve.Length;
            Autodesk.Revit.DB.Curve beam_curve = (elem.Location as LocationCurve).Curve;

            double location_end_x = beam_curve.GetEndPoint(1).X * 0.3048;
            double location_end_y = beam_curve.GetEndPoint(1).Y * 0.3048;
            double location_end_z = beam_curve.GetEndPoint(1).Z * 0.3048;

            double location_start_x = beam_curve.GetEndPoint(0).X * 0.3048;
            double location_start_y = beam_curve.GetEndPoint(0).Y * 0.3048;
            double location_start_z = beam_curve.GetEndPoint(0).Z * 0.3048;

            Objects.Geometry.Point start = new Objects.Geometry.Point(location_start_x, location_start_y, location_start_z);
            Objects.Geometry.Point end = new Objects.Geometry.Point(location_end_x, location_end_y, location_end_z);
            Objects.Geometry.Line line = new Objects.Geometry.Line(start, end);
            //line.length = line.length * 0.3048;//转换成m

            Linkit.BuiltElements.Style.BeamStyle beamStyle = new Linkit.BuiltElements.Style.BeamStyle();

            var profile = new Linkit.Other.Profiles.RectangularProfile();
            FamilyInstance revitBeamInstance = elem as FamilyInstance;

            if (revitBeamInstance != null)
            {
                FamilySymbol symbol = revitBeamInstance.Symbol;
                if (symbol != null)
                {
                    Parameter widthParam = symbol.LookupParameter("b");
                    Parameter heightParam = symbol.LookupParameter("h");

                    if (widthParam != null && heightParam != null)
                    {
                        profile.Width = Convert.ToDouble((widthParam.AsDouble() * 0.3048).ToString("F2")); // 转换为米,保留两位小数                             
                        profile.Height = Convert.ToDouble((heightParam.AsDouble() * 0.3048).ToString("F2")); // 转换为米,保留两位小数                           
                                                                                                             //TaskDialog.Show("Column Section Info", $"Column: {revitColumnInstance.Id}\nWidth: {profile.Width} m\nHeight: {profile.Height} m");
                    }
                }
            }

            FamilyInstance revitBeam_Instance = elem as FamilyInstance;
            beamStyle.Name = revitBeam_Instance.Name;
            beamStyle.FamilyName = revitBeam_Instance.Symbol.Family.Name;

            beamStyle.Profile = profile;

            //
            string level_name = elem.LookupParameter("参照标高").AsValueString();
            //int length = level_name.Length;
            //string str = (Convert.ToDouble(level_name[length - 1]) - 1).ToString();
            //level_name = level_name.Remove(-1, 1);
            //level_name = level_name.Insert(-1, str);//beam都下一层 0423 适用PKPM需求

            var level_elevation = elem.LookupParameter("参照标高高程").AsDouble() * 0.3048;
            level_elevation = Convert.ToDouble((level_elevation).ToString("F2")); //保留两位小数

            Linkit.Organization.Level level_linko = new Linkit.Organization.Level(level_name, level_elevation);


            //3 创建Beam
            Beam beam = new Beam(line, beamStyle, level_linko);//尽量填充剩余的可选参数参数
            beams_linko.Add(beam);


            //4 返回
            //return columns
            //
            //1 读取当前文档中的各种构件
            //List<Column> columns_linko = columns;
            //2 创建Commit
            StructureGeneration commitData = new StructureGeneration(columns_linko, beams_linko, slabs_linko);
            // 这里用了StructureGeneration，但是这里面只有梁板柱，所以可能需要Linkit定义一种新的commitData，能包含你要上传的所有数据
            // 或者你创建多个commitData，有建筑的，结构的，机电的，分多次commit上传到多个不同的branch
            CommitMetaData commitMetaData = new CommitMetaData("version..", "dependentVersion...", Role.User);
            Linkit.Commits.Commit commit = new Linkit.Commits.Commit(commitData, commitMetaData);

            //3 上传
            var token = "5bfe6d6730f385a94752f292683608bdf45218d58a";
            var streamId = "594bb8dc86";
            var branchName = BranchName;
            var message_linko = Message_linko;
            //var branchName = "test_yjk";
            //var message_linko = "beam_selectone";

            Task createCommit = new Task(async () =>
            await Core.LinkoUtils.CreateSpeckleCommit(commit, token, streamId, branchName, message_linko));
            createCommit.Start();

            ////4 结果处理
            //TaskDialog.Show("提示框", "已成功上传您所选择的构件，请至YJK中下载查看");
            //if (success)
            //{
            //    //弹个窗，成功了
            //}
            //else
            //{
            //    //弹个窗，失败了
            //}         

        }

        public string GetName()
        {
            return "upload_all";
        }
    }
}
