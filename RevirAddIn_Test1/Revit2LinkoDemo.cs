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

namespace Linko.ConnectorRevit
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
            ICollection<Element> columnElements =  revitColumns.OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
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
            //2 获取Wall的参数
            List<Linkit.BuiltElements.Main.Wall> walls_linko = new List<Linkit.BuiltElements.Main.Wall>();
            double height = 0;

            Objects.Geometry.Line wallLine_Linko = new Objects.Geometry.Line();
            Linkit.BuiltElements.Style.WallStyle wallStyle = new Linkit.BuiltElements.Style.WallStyle();

            foreach (var revitWall in wallElements)
            {

                if (revitWall != null)
                {
                    Autodesk.Revit.DB.Wall revitWall_Instance = revitWall as Autodesk.Revit.DB.Wall;
                    height = revitWall_Instance.LookupParameter("无连接高度").AsDouble() * 0.3048;   //取parameter 中英文的问题
                    // 获取墙的线条
                    LocationCurve wallLocationCurve = revitWall_Instance.Location as LocationCurve;
                    Autodesk.Revit.DB.Curve wallCurve = wallLocationCurve.Curve;
                    Autodesk.Revit.DB.Line wallLine = wallCurve as Autodesk.Revit.DB.Line;
                    double start_x = wallLine.GetEndPoint(0).X * 0.3048;
                    double start_y = wallLine.GetEndPoint(0).Y * 0.3048;
                    double start_z = wallLine.GetEndPoint(0).Z * 0.3048;
                    double end_x = wallLine.GetEndPoint(1).X * 0.3048;
                    double end_y = wallLine.GetEndPoint(1).Y * 0.3048;
                    double end_z = wallLine.GetEndPoint(1).Z * 0.3048;
                    Objects.Geometry.Point start_linko = new Objects.Geometry.Point(start_x, start_y, start_z);
                    Objects.Geometry.Point end_linko = new Objects.Geometry.Point(end_x, end_y, end_z);
                    Interval domain = new Interval(1, 2);

                    wallLine_Linko = new Objects.Geometry.Line(start_linko,end_linko);

                    // 获取墙的名称
                    string wallName = revitWall_Instance.Name;

                    // 获取墙的族名称
                    string familyName = revitWall_Instance.WallType.FamilyName;
                    wallStyle.Name = wallName;
                    wallStyle.FamilyName = familyName;

                    //TaskDialog.Show("Wall Info", $"Wall: {revitWall_Instance.Id}\nWall Line: {wallLine}\nWall Name: {wallName}\nFamily Name: {familyName}");
                }               
            }
            //3 创建Column
            Linkit.BuiltElements.Main.Wall wall = new Linkit.BuiltElements.Main.Wall(wallLine_Linko, height, wallStyle);//尽量填充剩余的可选参数参数
            walls_linko.Add(wall);


            //2 获取Column的参数
            List<Column> columns_linko = new List<Column>();
            foreach (var revitColumn in columnElements)
            {
                
                double height_column = Convert.ToDouble((revitColumn.LookupParameter("长度").AsDouble() * 0.3048).ToString("F2")) ;
                double bottom_offset = revitColumn.LookupParameter("底部偏移").AsDouble() * 0.3048;

                Autodesk.Revit.DB.XYZ column_point = (revitColumn.Location as LocationPoint).Point;

                Objects.Geometry.Point origin = new Objects.Geometry.Point(column_point.X * 0.3048, column_point.Y * 0.3048, column_point.Z * 0.3048 + bottom_offset);

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
                           
                            profile.Width = Convert.ToDouble((widthParam.AsDouble() * 0.3048).ToString("F2")); // 转换为米
                            profile.Height = Convert.ToDouble((heightParam.AsDouble() * 0.3048).ToString("F2")); // 转换为米
                            //TaskDialog.Show("Column Section Info", $"Column: {revitColumnInstance.Id}\nWidth: {profile.Width} m\nHeight: {profile.Height} m");
                        }
                    }
                }

                FamilyInstance revitColumn_Instance = revitColumn as FamilyInstance;
                columnStyle.Name = revitColumn_Instance.Name;
                columnStyle.FamilyName = revitColumn_Instance.Symbol.Family.Name;

                columnStyle.Profile = profile;

                var level = doc.GetElement(revitColumn_Instance.LevelId) as Level;
                string level_name = level.Name;
                double level_elevation = Convert.ToDouble((level.Elevation * 0.3048).ToString("F2"));
                Linkit.Organization.Level level_linko = new Linkit.Organization.Level(level_name, level_elevation);
                //3 创建Column
                Column column = new Column(location, height_column, columnStyle, level_linko);//尽量填充剩余的可选参数参数
                columns_linko.Add(column);
            }


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
                Slab slab = new Slab(elevation_linko,polycurve, slabStyle, level_linko);//尽量填充剩余的可选参数参数
                slabs_linko.Add(slab);
            }
            //2 获取Beam的参数
            List<Beam> beams_linko = new List<Beam>();
            foreach (var revitBeam in beamElements)
            {               
                //...
                //...
                double height_beam = (revitBeam.Location as LocationCurve).Curve.Length;
                Autodesk.Revit.DB.Curve beam_curve = (revitBeam.Location as LocationCurve).Curve;
                
                double location_end_x = beam_curve.GetEndPoint(1).X * 0.3048;
                double location_end_y = beam_curve.GetEndPoint(1).Y * 0.3048;
                double location_end_z = beam_curve.GetEndPoint(1).Z * 0.3048;

                double location_start_x = beam_curve.GetEndPoint(0).X * 0.3048;
                double location_start_y = beam_curve.GetEndPoint(0).Y * 0.3048;
                double location_start_z = beam_curve.GetEndPoint(0).Z * 0.3048;
                
                Objects.Geometry.Point start = new Objects.Geometry.Point(location_start_x, location_start_y, location_start_z);
                Objects.Geometry.Point end = new Objects.Geometry.Point(location_end_x, location_end_y, location_end_z);
                Objects.Geometry.Line line = new Objects.Geometry.Line(start,end);
                //line.length = line.length * 0.3048;//转换成m

                Linkit.BuiltElements.Style.BeamStyle beamStyle = new Linkit.BuiltElements.Style.BeamStyle();
               
                var profile = new Linkit.Other.Profiles.RectangularProfile();
                FamilyInstance revitBeamInstance = revitBeam as FamilyInstance;

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

                FamilyInstance revitBeam_Instance = revitBeam as FamilyInstance;
                beamStyle.Name = revitBeam_Instance.Name;
                beamStyle.FamilyName = revitBeam_Instance.Symbol.Family.Name;

                beamStyle.Profile = profile;               

                //
                string level_name = revitBeam.LookupParameter("参照标高").AsValueString();
                var level_elevation = revitBeam.LookupParameter("参照标高高程").AsDouble() * 0.3048;
                level_elevation = Convert.ToDouble((level_elevation).ToString("F2")); //保留两位小数

                Linkit.Organization.Level level_linko = new Linkit.Organization.Level(level_name, level_elevation);


                //3 创建Beam
                Beam beam = new Beam(line, beamStyle, level_linko);//尽量填充剩余的可选参数参数
                beams_linko.Add(beam);
            }


            //4 返回
            //return columns
            //
            //1 读取当前文档中的各种构件
            //List<Column> columns_linko = columns;
            //2 创建Commit
            StructureGeneration commitData = new StructureGeneration(columns_linko,beams_linko,slabs_linko);
            // 这里用了StructureGeneration，但是这里面只有梁板柱，所以可能需要Linkit定义一种新的commitData，能包含你要上传的所有数据
            // 或者你创建多个commitData，有建筑的，结构的，机电的，分多次commit上传到多个不同的branch
            CommitMetaData commitMetaData = new CommitMetaData("version..", "dependentVersion...", Role.User);
            Linkit.Commits.Commit commit = new Linkit.Commits.Commit(commitData, commitMetaData);

            //3 上传
            var token = "5bfe6d6730f385a94752f292683608bdf45218d58a";
            var streamId = "594bb8dc86";
            var branchName = "test_revit2linko";
            var message_linko = "0423 test";

            Task createCommit = new Task(async ()=>
            await Core.LinkoUtils.CreateSpeckleCommit(commit, token, streamId, branchName,message_linko));
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
