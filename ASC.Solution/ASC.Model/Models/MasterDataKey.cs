﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Model.BaseTypes;

namespace ASC.Model.Models
{
    public class MasterDataKey : BaseEntity
    {
        public MasterDataKey() { }
        public MasterDataKey(string key) 
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = key;
        }
        public bool IsActive { get; set; }
        public string Name { get; set; }
    }
}
