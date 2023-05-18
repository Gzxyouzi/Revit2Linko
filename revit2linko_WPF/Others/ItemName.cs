using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ItemName : Attribute
    {
        public string Name { get; set; }
        public ItemName(string name)
        {
            Name = name;
        }
    }
}
