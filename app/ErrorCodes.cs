using System;
using System.Collections.Generic;

namespace dcw
{
    class ErrorCodes
    {
        public enum Codes
        {
            NO_ERR = 0,
            NOT_ENOUGH_ARGS = 1,
        }

        public static Dictionary<Codes, string> Strings = new Dictionary<Codes, string>()
        {
            { Codes.NO_ERR,            "Exited with no error code." },
            { Codes.NOT_ENOUGH_ARGS,   "Error: Not enough arguments provided." }
        };
    }
}