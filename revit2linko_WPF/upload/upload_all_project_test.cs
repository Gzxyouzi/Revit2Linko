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
using Speckle_Core_Api;
using Linkit.Organization;
using LWall = Linkit.BuiltElements.Main.Wall;
using LDoor = Linkit.BuiltElements.Other.Door;
using LWindow = Linkit.BuiltElements.Other.Window;
using Linkit.BuiltElements.Style;

namespace revit2linko_WPF
{
    class upload_all_project_TEST : IExternalEventHandler
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
            FilteredElementCollector revitDoors = new FilteredElementCollector(doc);
            FilteredElementCollector revitWindows = new FilteredElementCollector(doc);

            //过滤，墙元素
            //快速过滤（category)
            ICollection<Element> wallElements = revitWalls.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Autodesk.Revit.DB.Wall)).ToElements();
            ICollection<Element> columnElements = revitColumns.OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
            ICollection<Element> beamElements = revitBeams.OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
            ICollection<Element> floorElements = revitSlabs.OfClass(typeof(Autodesk.Revit.DB.Floor)).ToElements();
            ICollection<Element> doorElements = revitDoors.OfCategory(BuiltInCategory.OST_Doors).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();
            ICollection<Element> windowElements = revitWindows.OfCategory(BuiltInCategory.OST_Windows).OfClass(typeof(Autodesk.Revit.DB.FamilyInstance)).ToElements();

            //2 获取Wall的参数
            List<LWall> walls_linko = new List<LWall>();
            double height = 0;

            Objects.Geometry.Line wallLine_Linko = new Objects.Geometry.Line();
            Linkit.BuiltElements.Style.WallStyle wallStyle = new Linkit.BuiltElements.Style.WallStyle();

            foreach (var revitWall in wallElements)
            {
                if (revitWall != null)
                {
                    Autodesk.Revit.DB.Wall revitWall_Instance = revitWall as Autodesk.Revit.DB.Wall;
                    height = revitWall_Instance.LookupParameter("无连接高度").AsDouble() * 304.8;   //取parameter 中英文的问题
                    // 获取墙的线条
                    LocationCurve wallLocationCurve = revitWall_Instance.Location as LocationCurve;
                    Autodesk.Revit.DB.Curve wallCurve = wallLocationCurve.Curve;
                    Autodesk.Revit.DB.Line wallLine = wallCurve as Autodesk.Revit.DB.Line;
                    double start_x = wallLine.GetEndPoint(0).X * 304.8;
                    double start_y = wallLine.GetEndPoint(0).Y * 304.8;
                    double start_z = wallLine.GetEndPoint(0).Z * 304.8;
                    double end_x = wallLine.GetEndPoint(1).X * 304.8;
                    double end_y = wallLine.GetEndPoint(1).Y * 304.8;
                    double end_z = wallLine.GetEndPoint(1).Z * 304.8;
                    Objects.Geometry.Point start_linko = new Objects.Geometry.Point(start_x, start_y, start_z, "mm");
                    Objects.Geometry.Point end_linko = new Objects.Geometry.Point(end_x, end_y, end_z, "mm");
                    
                    wallLine_Linko = new Objects.Geometry.Line(start_linko, end_linko,"mm");

                    // 获取墙的名称
                    string wallName = revitWall_Instance.Name;

                    // 获取墙的族名称
                    string familyName = revitWall_Instance.WallType.FamilyName;
                    wallStyle.Name = wallName;
                    wallStyle.FamilyName = familyName;

                    //2 创建Wall
                    Linkit.BuiltElements.Main.Wall wall = new Linkit.BuiltElements.Main.Wall(wallLine_Linko, height, wallStyle);//尽量填充剩余的可选参数参数
                    walls_linko.Add(wall);
                    idMapping[revitWall_Instance] = wall;

                    //TaskDialog.Show("Wall Info", $"Wall: {revitWall_Instance.Id}\nWall Line: {wallLine}\nWall Name: {wallName}\nFamily Name: {familyName}");
                }
            }
            
            //3 获取Column的参数
            List<Column> columns_linko = new List<Column>();
            
            //4 获取Slab的参数
            List<Slab> slabs_linko = new List<Slab>();
           
            //5 获取Beam的参数
            List<Beam> beams_linko = new List<Beam>();

            //7 获取Door的参数

            List<LDoor> doors_linko = new List<LDoor>();
            foreach (var revitDoor in doorElements)
            {
                if (revitDoor != null)
                {
                   
                    double x = ((LocationPoint)revitDoor.Location).Point.X * 304.8;
                    //double start_x = 0;
                    double y = ((LocationPoint)revitDoor.Location).Point.Y * 304.8;
                    double z = ((LocationPoint)revitDoor.Location).Point.Z * 304.8;

                    Objects.Geometry.Point origin = new Objects.Geometry.Point(x, y, z, "mm");

                    Objects.Geometry.Vector vector_x = new Objects.Geometry.Vector(1000, 0, 0, "mm");
                    Objects.Geometry.Vector vector_y = new Objects.Geometry.Vector(0, 1000, 0, "mm");
                    Objects.Geometry.Vector vector_z = new Objects.Geometry.Vector(0, 0, 1000, "mm");

                    Objects.Geometry.Plane location = new Objects.Geometry.Plane(origin, vector_z, vector_x, vector_y, "mm");

                    FamilyInstance door = revitDoor as Autodesk.Revit.DB.FamilyInstance;                    

                    DoorStyle doorStyle = new DoorStyle();
                    doorStyle.FamilyName = door.Symbol.Family.Name;
                    doorStyle.TypeName = door.Symbol.Name;
                    LWall host = idMapping[(Autodesk.Revit.DB.Wall)door.Host];
                    LDoor lDoor = new LDoor(location, doorStyle,host);
                    doors_linko.Add(lDoor);
                }
            }
            
            
            //创建Door

            //8 获取Windows的参数
            List<LWindow> windows_linko = new List<LWindow>();
            //创建Windows



            List<Module> modules_linko = new List<Module>();
            List<Room> rooms_linko = new List<Room>();
            //1 读取当前文档中的各种构件
            //List<Column> columns_linko = columns;
            //2 创建Commit
            // Modules = modules;
            //Rooms = rooms;
            //Columns = columns;
            //Beams = beams;
            //Slabs = slabs;
            //Walls = walls;
            //Doors = doors;
            //Windows = windows;

            ProjectCommitData projectCommitData = new ProjectCommitData(modules_linko,rooms_linko,columns_linko,beams_linko,slabs_linko,walls_linko,doors_linko,windows_linko);
            //StructureGeneration commitData = new StructureGeneration(columns_linko, beams_linko, slabs_linko);
            // 这里用了StructureGeneration，但是这里面只有梁板柱，所以可能需要Linkit定义一种新的commitData，能包含你要上传的所有数据
            // 或者你创建多个commitData，有建筑的，结构的，机电的，分多次commit上传到多个不同的branch
            CommitMetaData commitMetaData = new CommitMetaData("version..", "dependentVersion...", Role.User);
            Linkit.Commits.Commit commit = new Linkit.Commits.Commit(projectCommitData, commitMetaData);

            //3 上传
            var token = "5bfe6d6730f385a94752f292683608bdf45218d58a";
            var streamId = "594bb8dc86";
            //var branchName = BranchName;
            //var message_linko = Message_linko;
            var branchName = "test_yjk";
            var message_linko = "wall";

            Task createCommit = new Task(async () =>
            await Core.LinkoUtils.CreateSpeckleCommit(commit, token, streamId, branchName, message_linko));
            createCommit.Start();

            //4 结果处理
            //TaskDialog.Show("提示框", "已成功上传整个模型构件，请至YJK中下载查看");
            //if (success)
            //{
            //    //弹个窗，成功了
            //}
            //else
            //{
            //    //弹个窗，失败了
            //}

        }
        readonly Dictionary<Autodesk.Revit.DB.Wall, LWall> idMapping = new Dictionary<Autodesk.Revit.DB.Wall, LWall>();

        public string GetName()
        {
            return "upload_all";
        }
    }
}
