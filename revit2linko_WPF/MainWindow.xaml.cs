using Autodesk.Revit.UI;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace revit2linko_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //注册外部事件
        upload_all uploadCommand = null;
        ExternalEvent uploadEvent = null;

        upload_beam uploadbeamCommand = null;
        ExternalEvent uploadbeamEvent = null;

        upload_column uploadcolumnCommand = null;
        ExternalEvent uploadcolumnEvent = null;

        upload_slab uploadslabCommand = null;
        ExternalEvent uploadslabEvent = null;

        upload_beam_select uploadbeamselectoneCommand = null;
        ExternalEvent uploadbeamselectoneEvent = null;

        Create_limian create_LimianCommand = null;
        ExternalEvent create_LimianEvent = null;

        upload_all_project upload_All_ProjectCommand = null;
        ExternalEvent upload_All_ProjectEvent = null;

        public MainWindow()
        {
            InitializeComponent();

            //初始化
            uploadCommand = new upload_all();
            uploadEvent = ExternalEvent.Create(uploadCommand);

            uploadbeamCommand = new upload_beam();
            uploadbeamEvent = ExternalEvent.Create(uploadbeamCommand);

            uploadcolumnCommand = new upload_column();
            uploadcolumnEvent = ExternalEvent.Create(uploadcolumnCommand);

            uploadslabCommand = new upload_slab();
            uploadslabEvent = ExternalEvent.Create(uploadslabCommand);

            uploadbeamselectoneCommand = new upload_beam_select();
            uploadbeamselectoneEvent = ExternalEvent.Create(uploadbeamselectoneCommand);

            create_LimianCommand = new Create_limian();
            create_LimianEvent = ExternalEvent.Create(create_LimianCommand);

            upload_All_ProjectCommand = new upload_all_project();
            upload_All_ProjectEvent = ExternalEvent.Create(upload_All_ProjectCommand);
            //
            List<string> dicItem = new List<string>
            {
                "整个结构模型",
                "梁_beam",
                "柱_column",
                "板_slab",
                "梁_beam_selectone",
                "BIM审图模型"
            };

            Model_select_comboBox.ItemsSource = dicItem;

            Model_select_comboBox.SelectedIndex = 0;

        }

        private void upload_Click(object sender, RoutedEventArgs e)
        {
            //执行命令
            //属性传值
            if (Model_select_comboBox.SelectedIndex == 0)
            {
                uploadCommand.BranchName = branchName_text.Text;
                uploadCommand.Message_linko = Message_text.Text;
                uploadEvent.Raise();
            }
            else if (Model_select_comboBox.SelectedIndex == 1)//beam
            {
                uploadbeamCommand.BranchName = branchName_text.Text;
                uploadbeamCommand.Message_linko = Message_text.Text;
                uploadbeamEvent.Raise();
            }
            else if (Model_select_comboBox.SelectedIndex == 2)//column
            {
                uploadcolumnCommand.BranchName = branchName_text.Text;
                uploadcolumnCommand.Message_linko = Message_text.Text;
                uploadcolumnEvent.Raise();
            }
            else if (Model_select_comboBox.SelectedIndex == 3)//slab
            {
                uploadslabCommand.BranchName = branchName_text.Text;
                uploadslabCommand.Message_linko = Message_text.Text;
                uploadslabEvent.Raise();
            }
            else if (Model_select_comboBox.SelectedIndex == 4)//slab
            {
                uploadbeamselectoneCommand.BranchName = branchName_text.Text;
                uploadbeamselectoneCommand.Message_linko = Message_text.Text;
                uploadbeamselectoneEvent.Raise();
            }
            else if (Model_select_comboBox.SelectedIndex == 5)//test_wall/door/window
            {

                upload_All_ProjectCommand.BranchName = branchName_text.Text;
                upload_All_ProjectCommand.Message_linko = Message_text.Text;
                upload_All_ProjectEvent.Raise();
            }
        }

        private async void limian_Click(object sender, RoutedEventArgs e)
        {
            var account = new Account();
            account.token = "5bfe6d6730f385a94752f292683608bdf45218d58a";
            account.serverInfo = new ServerInfo
            {
                url = "https://linko.archi-alpha.com/"
            };
            Client client = new Client(account);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var stream = client.StreamGet("594bb8dc86").Result;
            string streamId_ = "594bb8dc86";
            string branchName_ = "facade";
            Branch branch =await client.BranchGet(streamId_, branchName_, 1);
            string objectId = branch.commits.items[0].referencedObject; // take last commit          
                                                                        //string objectId = "768e1ade28841df42ca4f977ddc506f8";


            ServerTransport transport = new ServerTransport(account, streamId_);


            Base receivedata = await Speckle_Core_Api.Operations.Receive(
              objectId,
              remoteTransport: transport,
              //onErrorAction: onErrorAction,
              //onProgressAction: onProgressAction,
              //onTotalChildrenCountKnown: onTotalChildrenCountKnown,
              disposeTransports: true
            );

            create_LimianCommand.Data = receivedata;
            create_LimianEvent.Raise();

        }
    }
}
