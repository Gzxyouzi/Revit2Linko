using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Web.Models
{
    public class WebDataUri : DataUri
    {
        [ItemName("仓库")]
        public string StreamId { get; set; }
        [ItemName("分支")]
        public string BranchName { get;set; }

        public WebDataUri(IDictionary<string,string> dict) 
        { 
            StreamId = dict["仓库"];
            BranchName = dict["分支"];
        }

        public WebDataUri(string streamId, string branchName)
        {
            StreamId = streamId;
            BranchName = branchName;
        }
    }
}
