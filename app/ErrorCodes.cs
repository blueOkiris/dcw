using System;
using System.Collections.Generic;

namespace dcw
{
    class ErrorCodes
    {
        public enum Codes
        {
            NO_ERR =                 0,
            NOT_ENOUGH_ARGS =       -1,
            SOURCE_NOT_FOUND =      -2,
            NOT_ONE_MODULE =        -3,
            FILE_IO_ERR =           -4,
            MISSING_END_PARENTH =   -5
        }

        public static Dictionary<Codes, string> Strings = new Dictionary<Codes, string>()
        {
            { Codes.NO_ERR,                 "Exited with no error code." },
            { Codes.NOT_ENOUGH_ARGS,        "Error: Not enough arguments provided." },
            { Codes.SOURCE_NOT_FOUND,       "Error: Provided source file not found." },
            { Codes.NOT_ONE_MODULE,         "Error: Not exactly one module in file." },
            { Codes.FILE_IO_ERR,            "Error: Reading file." },
            { Codes.MISSING_END_PARENTH,    "Error: Missing right parenthesis in definition." }
        };
    }
}