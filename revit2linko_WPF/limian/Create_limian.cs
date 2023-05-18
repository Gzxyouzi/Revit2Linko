using Speckle.Core.Credentials;
using Speckle.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Speckle.Core.Transports;
using Speckle.Core.Models;
using Objects;
using Objects.Converter.Revit;

using Autodesk.Revit.DB;

using Autodesk.Revit.Attributes;
using Objects.Geometry;
using Objects.Converter.Revit;
using Autodesk.Revit.DB.Architecture;
using Speckle_Core_Api;
using SLine = Objects.Geometry.Line;
using SpeckleCore;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Controls;
using Linkit.Organization;
using Objects.BuiltElements;
using Linkit.BuiltElements.Main;
using System.Windows.Media;
using Autodesk.Revit.DB.Structure;
using Linkit.Other.Profiles;
using LWall = Linkit.BuiltElements.Main.Wall;
using Speckle.netDxf;

namespace revit2linko_WPF
{
    
    class Create_limian : IExternalEventHandler
    {
        public Base Data { get; set; }
        public void Execute(UIApplication app)
        {                  
            var result = Data;
            Base FacadeData = (((Data["@Data"] as Base)["@{0}"] as List<object>)[0] as Base) as Linkit.Commits.Commit;

            var facadedata = ((FacadeData as Linkit.Commits.Commit).CommitData) as Linkit.Commits.CommitData.FacadeGeneration;
            var Handrails = facadedata.Handrails;
            var Slabs = facadedata.Slabs;
            var Walls = facadedata.Walls;
            var Windows = facadedata.Windows;
            var Wallsweeps = facadedata.WallSweeps;

            //界面交互的doc
            UIDocument uiDoc = app.ActiveUIDocument;
            //实际内容的doc
            Document doc = app.ActiveUIDocument.Document;

            FilteredElementCollector collector_level = new FilteredElementCollector(doc);
            List<Element> elems = collector_level.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().
                ToElements().
                ToList();

            //          
            //扶手
            var Handrails_count = Handrails.Count;
            for (int a = 0; a < Handrails_count; a++)
            {
                string familyName = Handrails[a].RailStyle.FamilyName;
                string typeName = Handrails[a].RailStyle.TypeName;
                bool isContain = familyName.Contains("系统族：");

                if (isContain == true)
                {
                    familyName = familyName.Replace("系统族：", "");

                }
                //Findtype
                RailingType railingType = FindHandRailType(doc, familyName, typeName);

                //create path
                List<SLine> slines = Handrails[a].Path.segments.Cast<SLine>().ToList();
                CurveLoop path = new CurveLoop();
                foreach (SLine sLine in slines)
                {
                    XYZ pointaa = new XYZ(sLine.start.x / 0.3048 /1000, sLine.start.y / 0.3048/1000, sLine.start.z / 0.3048 / 1000);
                    XYZ pointab = new XYZ(sLine.end.x / 0.3048 / 1000, sLine.end.y / 0.3048/ 1000, sLine.end.z / 0.3048 / 1000);
                    Autodesk.Revit.DB.Line linea = Autodesk.Revit.DB.Line.CreateBound(pointaa, pointab);
                    path.Append(linea);
                }
                double height_ = slines[0].start.z;
                double level_int = Math.Floor(height_ / 3500);
                int index = (int)level_int;
                //Level
                
                Autodesk.Revit.DB.Level level_railing = elems[index] as Autodesk.Revit.DB.Level;
                //foreach (var elem in elems)//now just use one to try
                //{
                //    level_railing = elem as Autodesk.Revit.DB.Level;
                //}
                //Autodesk.Revit.DB.Level level_ = CreateOrFindLevelAndSetElevation(doc,Handrails[a].Level);  handrail传过来没有level信息
                //Create Railing(need_doc_path_type_level)
                Transaction trans_railing = new Transaction(doc, "创建扶手");
                trans_railing.Start();
                Railing railing = Railing.Create(doc, path, railingType.Id, level_railing.Id);
                trans_railing.Commit();
            }
            //墙

            //获得 常规 - 200mm 类型的墙
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            Element ele = collector.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(WallType)).FirstOrDefault(x => x.Name == "常规 - 200mm");
            //WallType wallType = ele as WallType;
            var Walls_count = Walls.Count;
            for (int a = 0; a < Walls_count; a++)
            {
                string familyName = Walls[a].WallStyle.FamilyName;
                string typeName = Walls[a].WallStyle.TypeName;
                WallType wallType = FindWallType(doc, familyName, typeName);
                //Autodesk.Revit.DB.Level level = null;
                //获取标高
                //Transaction trans_level = new Transaction(doc, "创建level");
                //trans_level.Start();
                //Autodesk.Revit.DB.Level level = CreateOrFindLevelAndSetElevation(doc, Walls[a].Level);  //create 标高会卡住 还没明白
                //trans_level.Commit();

                
                //


                //创建线
                SLine line = Walls[a].Line as SLine;
                XYZ start = new XYZ(line.start.x / 304.8, line.start.y / 304.8, (line.start.z) / 304.8);
                XYZ end = new XYZ(line.end.x / 304.8, line.end.y / 304.8, (line.end.z) / 304.8);
                Autodesk.Revit.DB.Line geomline = Autodesk.Revit.DB.Line.CreateBound(start, end);

                double height_ = line.start.z;
                double level_int = Math.Floor(height_ / 3500);
                int index = (int)level_int;
                //Level(墙的level有问题 往上移了一层)

                Autodesk.Revit.DB.Level level = elems[index] as Autodesk.Revit.DB.Level;
                //墙的高度
                double height_wall = Walls[a].Height / 304.8;
                double offset = 0;
                //创建墙（事务的使用，修改doc 相关的内容放进去就可，若里面的代码出现错误，也会报错）
                Transaction trans = new Transaction(doc, "创建墙");
                trans.Start();

                Autodesk.Revit.DB.Wall wall = Autodesk.Revit.DB.Wall.Create(doc, geomline, wallType.Id, level.Id, height_wall, offset, false, false);
                idMapping[Walls[a].id] = wall.Id;
                trans.Commit();
            }

            //
            //slab(主要阳台板)        //
            var Slabs_count = Slabs.Count;
            for (int a = 0; a < Handrails_count; a++)
            {
                int structThickness = Convert.ToInt32(Slabs[a].SlabStyle.StructThickness);

                string typeName = Slabs[a].SlabStyle.TypeName;
                string familyName = Slabs[a].SlabStyle.FamilyName;
                //string typeName = $"常规 - {structThickness}mm";
                bool isContain = familyName.Contains("系统族：");

                if (isContain == true)
                {
                    familyName = familyName.Replace("系统族：", "");

                }
                FloorType floorType = FindFloorType(doc, familyName, typeName);
                FloorType defaultFloorType = FindFloorType(doc, DEFAULT_FLOOR_FAMILY, DEFAULT_FLOOR_TYPE); //TODO:寻找默认类型？
                //CompoundStructure compoundStructure = defaultFloorType.GetCompoundStructure();
                //int layerIndex = compoundStructure.GetFirstCoreLayerIndex();
                //CompoundStructureLayer layer = compoundStructure.GetLayers()[layerIndex];
                //layer.Width = MillimetersToFeet(structThickness);
                //compoundStructure.SetLayer(layerIndex, layer);
                //Transaction trans_type = new Transaction(doc, "创建type");
                //trans_type.Start();
                //FloorType floorType = defaultFloorType.Duplicate(typeName) as FloorType;
                //trans_type.Commit();
                //floorType.SetCompoundStructure(compoundStructure);



                //TODO: Polycurve无法处理

                List<Objects.Geometry.Line> slines = Slabs[a].Outline.segments.Cast<Objects.Geometry.Line>().ToList();


                CurveArray profile = new CurveArray();

                foreach (Objects.Geometry.Line sLine in slines)
                {
                    XYZ pointaa = new XYZ(sLine.start.x / 0.3048 / 1000, sLine.start.y / 0.3048 / 1000, sLine.start.z / 0.3048 / 1000);
                    XYZ pointab = new XYZ(sLine.end.x / 0.3048 / 1000, sLine.end.y / 0.3048 / 1000, sLine.end.z / 0.3048 / 1000);
                    Autodesk.Revit.DB.Line linea = Autodesk.Revit.DB.Line.CreateBound(pointaa, pointab);
                    profile.Append(linea);
                }

                Transaction trans = new Transaction(doc, "创建level");
                trans.Start();
                Autodesk.Revit.DB.Level level = CreateOrFindLevelAndSetElevation(doc, Slabs[a].Level);
                trans.Commit();
                bool structual = false;//TODO:楼板是否承重？
                XYZ normal = XYZ.BasisZ;
                Transaction trans_floor = new Transaction(doc, "创建floor");
                trans_floor.Start();
                Autodesk.Revit.DB.Floor floor = doc.Create.NewFloor(profile, floorType, level, structual, normal);
                trans_floor.Commit();
            }









            //墙饰条
            var wallSweeps_count = Wallsweeps.Count;
            for (int a = 0; a < wallSweeps_count; a++)
            {
                string familyName = Wallsweeps[a].WallSweepStyle.FamilyName;
                bool isContain = familyName.Contains("系统族：");

                if (isContain == true)
                {
                    familyName = familyName.Replace("系统族：", "");
                }
                string typeName = Wallsweeps[a].WallSweepStyle.TypeName;
                Autodesk.Revit.DB.Level level = null;
                //ElementType wallSweepType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Cornices).WhereElementIsElementType().Cast().First();
                ElementType wallSweepType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Cornices).WhereElementIsElementType().FirstOrDefault(x => x.Name == typeName) as ElementType;

                WallSweepInfo wallSweepInfo = new WallSweepInfo(WallSweepType.Sweep, false);
                wallSweepInfo.WallSide = WallSide.Exterior;
                wallSweepInfo.Distance = 10;
                wallSweepInfo.WallOffset = 0;
                var Lwall = Wallsweeps[a].Host;
                //Element host = doc.GetElement(wallId);
                string wallId = Lwall.id ?? Lwall.GetId();
                Autodesk.Revit.DB.Wall hostwall = (Autodesk.Revit.DB.Wall)doc.GetElement(idMapping[wallId]);
                //Autodesk.Revit.DB.Wall wall = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.Wall)).First() as Autodesk.Revit.DB.Wall;
                Transaction trans = new Transaction(doc, "创建WallSweep");
                trans.Start();
                WallSweep ws = WallSweep.Create(hostwall, wallSweepType.Id, wallSweepInfo);
                trans.Commit();
            }
            //
            //窗
            var windows_count = Windows.Count;
            int width, height;
            for (int a = 0; a < windows_count; a++)
            {
                
                
                if (Windows[a].WindowStyle.Profile is RectangularProfile rectangular)
                {
                    width = Convert.ToInt32(rectangular.Width);
                    height = Convert.ToInt32(rectangular.Height);
                }
                else
                {
                    throw new NotSupportedException("不支持非矩形窗");
                }
                string typeName = Windows[a].WindowStyle.TypeName;
                string familyName = Windows[a].WindowStyle.FamilyName;

                FamilySymbol windowType = FindWindowType(doc, familyName, typeName);
                //windowType.Activate();
                if (windowType is null)
                {
                    FamilySymbol defaultWindowType = FindWindowType(doc, DEFAULT_WINDOW_FAMILY, DEFAULT_WINDOW_TYPE);
                    //windowType = defaultWindowType.Duplicate(typeName) as FamilySymbol ?? throw new Exception($"创建新类型失败: {typeName}");
                    windowType = defaultWindowType;
                }
                //TODO: 有bug，无法设置长宽
                //if (!windowType.IsActive)
                //{
                //    Transaction trans_windowtype = new Transaction(doc, "改变参数Windowtype");
                //    trans_windowtype.Start();
                //    windowType.get_Parameter(BuiltInParameter.WINDOW_WIDTH)?.Set(width / 304.8);

                //    windowType.get_Parameter(BuiltInParameter.WINDOW_HEIGHT)?.Set(height / 304.8);
                //    trans_windowtype.Commit();

                //}


                try//lose host
                {
                    Linkit.BuiltElements.Main.Wall hostData = Windows[a].Host;
                    if (hostData is null)
                    {

                    }
                    else
                    {
                        string wallId = hostData.id ?? hostData.GetId();

                        XYZ location = new XYZ(Windows[a].LocationPoint.x / 304.8, Windows[a].LocationPoint.y / 304.8, Windows[a].LocationPoint.z / 304.8);

                        Element host = doc.GetElement(idMapping[wallId]);

                        double height_ = Windows[a].LocationPoint.z;
                        double level_int = Math.Floor(height_ / 3500);
                        int index = (int)level_int;
                        //Level

                        Autodesk.Revit.DB.Level level = elems[index] as Autodesk.Revit.DB.Level;
                        //Transaction trans_level = new Transaction(doc, "创建level");
                        //trans_level.Start();
                        //Autodesk.Revit.DB.Level level = CreateOrFindLevelAndSetElevation(doc, hostData.Level);
                        //trans_level.Commit();
                        StructuralType structuralType = StructuralType.NonStructural;
                        Transaction trans_window_ = new Transaction(doc, "创建Window");
                        trans_window_.Start();
                        if (!windowType.IsActive)
                        {
                            windowType.Activate();
                            doc.Regenerate();
                        }
                        FamilyInstance window = doc.Create.NewFamilyInstance(location, windowType, host, level, structuralType);
                        trans_window_.Commit();

                    }
                    

                }
                catch
                {

                }
                

            }

        }

        //    private double MillimetersToFeet(double millimetersValue) => UnitUtils.Convert(millimetersValue, UnitTypeId.Millimeters, UnitTypeId.Feet);
        private const string DEFAULT_FLOOR_FAMILY = "楼板";
        private const string DEFAULT_FLOOR_TYPE = "常规 - 150mm";
        private const string DEFAULT_WINDOW_FAMILY = "固定";
        private const string DEFAULT_WINDOW_TYPE = "1000 x 1200mm";
        readonly Dictionary<string, ElementId> idMapping = new Dictionary<string, ElementId>();
        public string GetName()
        {
            return "create_facade";
        }

        private T FindElementType<T>(Document doc,string familyName, string typeName, BuiltInCategory? builtInCategory = null) where T : ElementType
        {
            
            FilteredElementCollector col = new FilteredElementCollector(doc);
            IEnumerable<T> elementTypes = builtInCategory is BuiltInCategory category
                ? col.OfClass(typeof(T)).OfCategory(category).ToElements().Cast<T>()
                : col.OfClass(typeof(T)).ToElements().Cast<T>();
            foreach (T elementType in elementTypes)
            {
                if (elementType.FamilyName == familyName && elementType.Name == typeName)
                {
                    return elementType;
                }
            }
            return null;
        }
        private RailingType FindHandRailType(Document doc, string familyName, string typeName) => FindElementType<RailingType>(doc,familyName, typeName);
        private WallType FindWallType(Document doc, string familyName, string typeName) => FindElementType<WallType>(doc,familyName, typeName);
        //private WallSweepType FindWallSweepType(Document doc, string familyName, string typeName) => FindElementType<WallSweepType>(doc, familyName, typeName);
        private FamilySymbol FindWindowType(Document doc, string familyName, string typeName) => FindElementType<FamilySymbol>(doc, familyName, typeName, BuiltInCategory.OST_Windows);
        private FloorType FindFloorType(Document doc, string familyName, string typeName) => FindElementType<FloorType>(doc,familyName, typeName);
        
        public Autodesk.Revit.DB.Level CreateOrFindLevelAndSetElevation(Document Doc, Linkit.Organization.Level Level)
        {
            Document doc = Doc;

            double elevation_feet = MillimetersToFeet(Level.Elevation);

            Autodesk.Revit.DB.Level level_revit = FindLevelType(doc,Level.Name);
            if (level_revit != null)
            {
                level_revit.Elevation = elevation_feet;
                return level_revit;
            }

            level_revit = Autodesk.Revit.DB.Level.Create(doc, elevation_feet);
            level_revit.Name = Level.Name;
            return level_revit;
        }


        private double MillimetersToFeet(double millimetersValue) => UnitUtils.Convert(millimetersValue, UnitTypeId.Millimeters, UnitTypeId.Feet);

        private double FeetToMillimeters(double feetValue) => UnitUtils.Convert(feetValue, UnitTypeId.Feet, UnitTypeId.Millimeters);

        public Autodesk.Revit.DB.Level FindLevelType(Document Doc,string LevelName)
        {
            FilteredElementCollector Col = new FilteredElementCollector(Doc);
            IEnumerable<Autodesk.Revit.DB.Level> levelList = Col.OfClass(typeof(Autodesk.Revit.DB.Level)).ToElements().Cast<Autodesk.Revit.DB.Level>();
            foreach (Autodesk.Revit.DB.Level level in levelList)
            {
                if (level.Name == LevelName)
                {
                    return level;
                }
            }
            return null;
        }
    }
}
