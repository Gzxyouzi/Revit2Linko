using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Transports;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class LinkoUtils
    {
        public enum SourceApp
        {
            Unknown,
            Pkpm,
            PkpmPc,
            Revit,
            Rhino,
            Grasshopper,
        }
        /// <summary>
        /// 用于上传数据，并创建一个新的Commit
        /// </summary>
        /// <param name="data">需要上传的数据</param>
        /// <param name="token">账户的Token，需要具有相应写入权限</param>
        /// <param name="streamId"></param>
        /// <param name="branchName"></param>
        /// <param name="message">Commit附带的消息</param>
        /// <param name="sourceApp">从什么来源发送？</param>
        /// <returns>一个结果，True表示发送成功</returns>
        public static async Task<bool> CreateSpeckleCommit(Base data, string token, string streamId, string branchName, string message = "", SourceApp sourceApp = SourceApp.Unknown)
        {
            string commitId;
            try
            {
                var account = new Account
                {
                    token = token,
                    serverInfo = new ServerInfo
                    {
                        url = Constants.LINKO_ARCHI_ALPHA
                    }
                };

                var client = new Client(account);
                //var stream = client.StreamGet(streamId).Result;


                var transport = new ServerTransport(account, streamId);

                var objectId = await Operations.Send(
                  data,
                  new List<ITransport> { transport },
                  //useDefaultCache,
                  //onProgressAction,
                  //onErrorAction,
                  disposeTransports: true);


                commitId = await client.CommitCreate(
                  new CommitCreateInput
                  {
                      streamId = streamId,
                      branchName = branchName,
                      objectId = objectId,
                      message = message,
                      sourceApplication = sourceApp.ToString(),
                      totalChildrenCount = (int)data.GetTotalChildrenCount(),
                  });
            }
            catch (Exception)
            {
                return false;
            }

            return !string.IsNullOrEmpty(commitId);
        }
        public static class Constants
        {
            public const string LINKO_ARCHI_ALPHA = "https://linko.archi-alpha.com";

        }
    }
}