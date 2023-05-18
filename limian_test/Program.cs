using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Speckle;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using Objects.Geometry;


using System.Reflection;


namespace limian_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var account = new Account();
            account.token = "698bda5636b333489d8fc7a1c3dbe645b01fe4c16b";
            account.serverInfo = new ServerInfo
            {
                url = "https://speckle.xyz.com/"
            };



            Client client = new Client(account);

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var stream = client.StreamGet("9ecd956028").Result;

            string streamId = "9ecd956028";
            string branchName = "elevation";
            Branch branch = client.BranchGet(streamId, branchName, 1).Result;
            string objectId = branch.commits.items[0].referencedObject; // take last commit

            ServerTransport transport = new ServerTransport(account, streamId);





            //接收
            Base receivedata = Operations.Receive(
              objectId,
              remoteTransport: transport,
              //onErrorAction: onErrorAction,
              //onProgressAction: onProgressAction,
              //onTotalChildrenCountKnown: onTotalChildrenCountKnown,
              disposeTransports: true
            ).Result;

            Base receiveData = ((receivedata["@Data"] as Base)["@{0}"] as List<object>)[0] as Base;

            
        }
    }
}
