using System;

namespace ImportData
{
    public enum ExceptionType
    {
        Info,
        Debug,
        Warn,
        Error
    }

    public class Structures
    {
        public struct ExceptionsStruct
        {
            public uint RowNumber;
            public string ErrorType;
            public string Message;
            public ExceptionType ExceptionType;
        }
    }
}
