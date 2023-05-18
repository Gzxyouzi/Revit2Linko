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

namespace revit2linko_WPF
{
    class upload_slab : IExternalEventHandler
    {
        public string BranchName { get; set; }
        public string Message_linko { get; set; }

        public void Execute(UIApplication app)
        {
            //界面交互的doc
            UIDocument uiDoc = app.ActiveUIDocument;
            //实际内容的doc
            Document doc = app.ActiveUIDocument.Document;
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

            List<Column> columns_linko = new List<Column>();
            List<Beam> beams_linko = new List<Beam>();
            List<Slab> slabs_linko = new List<Slab>();
            foreach (Floor floorInstance in floorElements)
            {
                //2 获取Slab的参数
                //...              
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;
                options.DetailLevel = ViewDetailLevel.Fine;

                GeometryElement geoElement = floorInstance.get_Geometry(options);
                List<ICurve> segments = new List<ICurve>();
                Polycurve polycurve = new Polycurve();
                double elevation_linko = 0;

                //
                var faces = HostObjectUtils.GetTopFaces(floorInstance);
                Face face = floorInstance.GetGeometryObjectFromReference(faces[0]) as Face;
                EdgeArrayArray edgeLoops = face.EdgeLoops;
                foreach (EdgeArray edgeLoop in edgeLoops)
                {
                    foreach (Edge edge in edgeLoop)
                    {
                        Autodesk.Revit.DB.Curve curve = edge.AsCurve();
                        double start_x = curve.GetEndPoint(0).X * 0.3048;
                        double start_y = curve.GetEndPoint(0).Y * 0.3048;
                        double start_z = curve.GetEndPoint(0).Z * 0.3048;
                        double end_x = curve.GetEndPoint(1).X * 0.3048;
                        double end_y = curve.GetEndPoint(1).Y * 0.3048;
                        double end_z = curve.GetEndPoint(1).Z * 0.3048; //看一下数据对不对
                        Objects.Geometry.Point start = new Objects.Geometry.Point(start_x, start_y, start_z);
                        Objects.Geometry.Point end = new Objects.Geometry.Point(end_x, end_y, end_z);
                        Objects.Geometry.Line curve_linko = new Objects.Geometry.Line(start, end);


                        segments.Add(curve_linko);
                        elevation_linko = Convert.ToDouble((start_z).ToString("F2"));//意思不大


                    }
                }
                polycurve.segments = segments;


                // 获取楼板厚度
                Parameter thicknessParam = floorInstance.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
                double thickness = thicknessParam.AsDouble() * 304.8; // 转换为mm

                // 获取楼板名称
                string floorName = floorInstance.Name;

                // 获取楼板族名称
                string familyName = floorInstance.FloorType.FamilyName;

                // 获取楼板类型名称
                string typeName = floorInstance.FloorType.Name;

                //...
                Linkit.BuiltElements.Style.SlabStyle slabStyle = new Linkit.BuiltElements.Style.SlabStyle();
                slabStyle.ArchThickness = 50;
                slabStyle.StructThickness = thickness - 50; //内置 单位mm
                slabStyle.Name = floorName;
                slabStyle.FamilyName = familyName;
                slabStyle.TypeName = typeName;

                var level = doc.GetElement(floorInstance.LevelId) as Level;
                string level_name = level.Name;
                double level_elevation = level.Elevation * 0.3048; //转换成m 
                Linkit.Organization.Level level_linko = new Linkit.Organization.Level(level_name, level_elevation);
                //3 创建Slab
                Slab slab = new Slab(elevation_linko, polycurve, slabStyle, level_linko);//尽量填充剩余的可选参数参数
                slabs_linko.Add(slab);
            }
          


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
            //var message_linko = "slab_all";

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

        }

        public string GetName()
        {
            return "upload_all";
        }
    }
}
