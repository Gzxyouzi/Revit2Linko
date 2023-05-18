using Speckle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Services
{
    /// <summary>
    /// 数据源相关，例如查询数据库是否在线，获取数据列表和项目数据结构，下载数据
    /// </summary>
    public interface IDataSource
    {
        //数据源可访问
        SourceStatus SourceStatus { get; }

        Action<string, Exception> OnErrorAction { get; set; }
        Action<ConcurrentDictionary<string, int>> OnProgressAction { get; set; }
        Action<int> OnTotalChildrenCountKnown { get; set; }
        void Init(string token);
        Task<Base> GetDataAsync(DataUri dataUri);
    }
}
