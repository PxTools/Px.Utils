using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxUtils.Validation.SyntaxValidation
{
    public static class SyntaxValidationUtilityMethods
    {
        public static bool HasMoreThanOneParameter(string input, char startSymbol)
        {
            int startIndex = input.IndexOf(startSymbol);
            if (startIndex == -1)
            {
                return false;
            }
            else
            {
                string remainder = input[(startIndex + 1)..];
                int secondStartIndex = remainder.IndexOf(startSymbol);
                if (secondStartIndex != -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
