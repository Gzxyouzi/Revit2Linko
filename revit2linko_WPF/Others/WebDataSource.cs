using Core;
using Core.Models;
using Core.Models.Services;
using Linkit.Commits;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Models;
using Speckle.Core.Models.Extensions;
using Speckle.Core.Transports;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Models;
using Speckle_Core_Api;

namespace Web
{
    public class WebDataSource : IDataSource
    {
        public string Url { get; } = "https://linko.archi-alpha.com";
        private SourceStatus _sourceStatus = SourceStatus.Offline;
        public SourceStatus SourceStatus => _sourceStatus;
        public Action<string, Exception> OnErrorAction { get; set; } = null;
        public Action<ConcurrentDictionary<string, int>> OnProgressAction { get; set; } = null;
        public Action<int> OnTotalChildrenCountKnown { get; set; } = null;


        public async Task<Base> GetDataAsync(DataUri dataUri)
        {
            if (SourceStatus == SourceStatus.Offline)
            {
                throw new Exception("无法连接到服务器，请登录或检查网络连接。");
            }

            if (dataUri is WebDataUri uri)
            {
                return await GetDataAsync(uri.StreamId, uri.BranchName);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        private async Task<Base> GetDataAsync(string streamId, string branchName)
        {
            ServerTransport transport = new ServerTransport(_account, streamId);
            Branch branch = await BranchGet(streamId, branchName);
            string objectId = GetLatestObjectId(branch);
            Base rootObj = await Speckle_Core_Api.Operations.Receive(
                objectId,
                remoteTransport: transport,
                onErrorAction: OnErrorAction,
                onProgressAction: OnProgressAction,
                onTotalChildrenCountKnown: OnTotalChildrenCountKnown,
                disposeTransports: true);
            return GetCommitData(rootObj);
        }


        private Account _account;
        public Account Account => _account;
        private User _user;
        public User User => _user;


        private Client _client;
        public void Init(string token)
        {
            _account = new Account { token = token, serverInfo = new ServerInfo { url = Url } };
            if (_account != null && _account.isOnline)  
                _sourceStatus = SourceStatus.Online;
            _client = new Client(_account);
        }

        public async Task<User> ActiveUserGet()
        {
            return await _client.ActiveUserGet();
        }




        public async Task<Branch> BranchGet(string streamId, string branchName, int commitsLimit = 30)
        {
            return await _client.BranchGet(streamId, branchName, commitsLimit);
        }
        public string GetLatestObjectId(Branch branch) => branch.commits.items[0].referencedObject;

        //因为有些情况会有GH的包装，为了拿到内部的数据，需要做一些判断
        public static Base GetCommitData(Base rootObj)
        {
            if (rootObj is Linkit.Commits.Commit) return rootObj;

            List<Base> datas = rootObj.Flatten().ToList();
            Linkit.Commits.Commit commit = datas.Find(c => c is Linkit.Commits.Commit) as Linkit.Commits.Commit;

            return commit;
        }

        public async Task<List<Stream>> StreamsGet(int streamLimit = 30)
        {
            return await _client.StreamsGet(streamLimit);
        }

        public async Task<List<Branch>> StreamGetBranches(string streamId, int branchesLimit = 50, int commitsLimit = 30)
        {
            return await _client.StreamGetBranches(streamId, branchesLimit, commitsLimit);
        }
    }
}
