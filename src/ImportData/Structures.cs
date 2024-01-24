using System;

namespace ImportData
{
    public enum ExceptionType
    {
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

            public ExceptionType exceptionType;
        }
    }
}
