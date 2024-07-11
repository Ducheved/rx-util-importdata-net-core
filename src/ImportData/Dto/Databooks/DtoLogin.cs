﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ImportData.Dto.Databooks
{
    public class DtoLogin : IDtoEntity
    {
        public uint RowNumber { get; set; }
        public bool? NeedChangePassword { get; set; }
        public string LoginName { get; set; }
        public string TypeAuthentication { get; set; }
        public string Status { get; set; }
    }
}
