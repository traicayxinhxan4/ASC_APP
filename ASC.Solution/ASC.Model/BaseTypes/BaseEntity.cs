using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Model.BaseTypes
{
    public class BaseEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string ? CreateBy { get; set; }
        public string ? UpdateBy { get; set; }
        public BaseEntity()
        {

        }
    }
}
