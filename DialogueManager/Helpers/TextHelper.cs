/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

namespace DialogueManager
{
    static class TextHelper
    {
        public static string GetNumberText(int number)
        {
            switch (number)
            {
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "three";
                case 4:
                    return "four";
                case 5:
                    return "five";
                case 6:
                    return "six";
                case 7:
                    return "seven";
                case 8:
                    return "eight";
                case 9:
                    return "nine";
                case 10:
                    return "ten";
                case 11:
                    return "eleven";
                default:
                    return number.ToString();
            }
        }

        public static string GetRuleNumberText(int index)
        {
            switch (index)
            {
                case 0:
                    return "Rule one";
                case 1:
                    return "Rule two";
                case 2:
                    return "Rule three";
                case 3:
                    return "Rule four";
                case 4:
                    return "Rule five";
                case 5:
                    return "Rule six";
                case 6:
                    return "Rule seven";
                case 7:
                    return "Rule eight";
                case 8:
                    return "Rule nine";
                case 9:
                    return "Rule ten";
                default:
                    return "Rule eleven";
            }
        }
    }
}
