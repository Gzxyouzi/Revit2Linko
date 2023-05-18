using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Linkit;
using SpeckleCore.Data;
using Speckle.Core.Api;
using Speckle.Core.Models;
using Speckle.Core.Credentials;
using Speckle.Core.Transports;
using CommunityToolkit.Mvvm.Input;




namespace Revit_Linko
{
    //[Transaction(TransactionMode.Manual)]
    //public class FilterWall : IExternalCommand
    //{
    //    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    //    {
    //        //界面交互的doc
    //        UIDocument uiDoc = commandData.Application.ActiveUIDocument;
    //        //实际内容的doc
    //        Document doc = commandData.Application.ActiveUIDocument.Document;
    //        //创建收集器
    //        FilteredElementCollector collector = new FilteredElementCollector(doc);
    //        FilteredElementCollector collectorTwo = new FilteredElementCollector(doc);
    //        //过滤，墙元素
    //        //快速过滤（category)
    //        collector.OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Wall));
    //        //通用过滤
    //        //ElementCategoryFilter elementCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
    //        //ElementClassFilter elementClassFilter = new ElementClassFilter(typeof(Wall));
    //        //collector.WherePasses(elementClassFilter).WherePasses(elementClassFilter);          

    //        //高亮显示所有墙
    //        var sel = uiDoc.Selection.GetElementIds();

    //        foreach (var item in collector)
    //        {
    //            var height0 = item.LookupParameter("无连接高度").AsDouble() * 0.3048;
    //            TaskDialog.Show("查看结果", height0.ToString());
    //            sel.Add(item.Id);
    //        }
    //        //
    //        uiDoc.Selection.SetElementIds(sel);


    //        //某种墙组类型下族实例的获取
    //        // foreach获取墙参数
    //        List<Element> elementList = new List<Element>();
    //        List<double> height = new List<double>();
    //        foreach (var item in collector)
    //        {
    //            height.Add(item.LookupParameter("无连接高度").AsDouble() * 0.3048);
    //        }

    //        return Result.Succeeded;


    //        public static async Task RevitLinko(List<Base> b)
    //        {
    //            var B = RevirAddIn_Test1.StaticBase.Data;
    //            updateLinko(ref B);

    //            var data = new Linkit.Commits.Commit();
    //            var StructureGeneration = new Linkit.Commits.CommitData.StructureGeneration();
    //            StructureGeneration.Columns = B.Where(c => c is Linkit.BuiltElements.Main.Column).Cast<Linkit.BuiltElements.Main.Column>().ToList();
    //            StructureGeneration.Beams = B.Where(c => c is Linkit.BuiltElements.Main.Beam).Cast<Linkit.BuiltElements.Main.Beam>().ToList();
    //            StructureGeneration.Slabs = B.Where(c => c is Linkit.BuiltElements.Main.Slab).Cast<Linkit.BuiltElements.Main.Slab>().ToList();
    //            //var CommitMetaData = new Linkit.Commits.CommitMetaData();
    //            //CommitMetaData.Version = "20230331";
    //            //CommitMetaData.Role = Linkit.Role.User;
    //            data.CommitData = StructureGeneration;
    //            //data.CommitMetaData = CommitMetaData;
    //            //string url_send = @"https://linko.archi-alpha.com/streams/594bb8dc86/branches/structure/complete";
    //            //var commitID = Helpers.Send(url_send, data).Result;
    //            var streamId = "594bb8dc86";
    //            var branchName = "test_revit2linko";
    //            var account = new Account();
    //            account.token = "5bfe6d6730f385a94752f292683608bdf45218d58a";
    //            account.serverInfo = new ServerInfo
    //            {
    //                url = "https://linko.archi-alpha.com/"
    //            };
    //            var client = new Client(account);
    //            var transport = new ServerTransport(account, streamId);
    //            var objectId = await Operations.Send(
    //                data,
    //                new List<ITransport> { transport }, disposeTransports: true);
    //            var commitId = await client.CommitCreate(
    //                new CommitCreateInput
    //                {
    //                    streamId = streamId,
    //                    branchName = branchName,
    //                    objectId = objectId,
    //                    message = "20230331",
    //                //sourceApplication = "PKPM",
    //                totalChildrenCount = Convert.ToInt32(data.totalChildrenCount),
    //                });
    //        }

    //        private static void updateLinko(ref List<Base> CData)
    //        {
    //            //#region 更新柱 
    //            //var columns = CData.Where(b => b is Linkit.BuiltElements.Main.Column).Cast<Linkit.BuiltElements.Main.Column>().ToList();
    //            //foreach (var temp_col in columns)
    //            //{
    //            //    var rp = temp_col.ColumnStyle.Profile as Linkit.Other.Profiles.RectangularProfile;
    //            //    rp.Width = b;
    //            //    rp.Height = h;
    //            //}
    //            //#endregion


    //            //#region 更新梁
    //            //var beams = CData.Where(b => b is Linkit.BuiltElements.Main.Beam).Cast<Linkit.BuiltElements.Main.Beam>().ToList(); 
    //            //foreach (var temp_beam in beams)
    //            //{
    //            //    var rp = temp_beam.BeamStyle.Profile as Linkit.Other.Profiles.RectangularProfile;
    //            //    rp.Width = b;
    //            //    rp.Height = h;
    //            //}
    //            //#endregion

    //            #region 更新墙
    //            var walls = CData.Where(b => b is Linkit.BuiltElements.Main.Wall).Cast<Linkit.BuiltElements.Main.Wall>().ToList();
    //            foreach (var temp_wall in walls)
    //            {
    //                //var line = temp_wall.Line;
    //                //var height = temp_wall.Height;
    //                //var level = temp_wall.Level;

    //                //var thickness = temp_wall.WallStyle.Thickness;
    //                //var name = temp_wall.WallStyle.Name;
    //                //var familyname = temp_wall.WallStyle.FamilyName;

    //                temp_wall.Line = ;

    //                var height = temp_wall.Height;
    //                height = 10;
    //                var level = temp_wall.Level;

    //                var thickness = temp_wall.WallStyle.Thickness;
    //                var name = temp_wall.WallStyle.Name;
    //                var familyname = temp_wall.WallStyle.FamilyName;

    //            }

    //            #endregion

    //        }

    //    }

    //    //public string Name => "上传Linko";
    //    //public IAsyncRelayCommand<List<Base>> Command => _command;
    //    //private AsyncRelayCommand<List<Base>> _command = new AsyncRelayCommand<List<Base>>((b) => RevitLinko(b));

        








    //}
}
