using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.Dto
{
    public interface IDtoEntity
    {
        public uint RowNumber { get; set; }
        public int Id { get; set; }
    }
}
