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
    static class ColourHelper
    {
        public const string TitleColour = "#FF3588A5";

        // Button Colours
        public const string StatementColour = "#FFA0D6FA";
        public const string StatementSelectedColour = "#FF89B7D6";
        public const string QuestionColour = "#FFF1E4B3";
        public const string QuestionSelectedColour = "#FFC9BF95"; // "#FFABA27F";
        public const string ActionColour = "#FFF5C238";
        public const string ActionSelectedColour = "#FFB38D29";
        public const string ConditionColour = "#FF8EEB95";
        public const string ConditionSelectedColour = "#FF67AB6D";
        public const string HiddenColour = "#FFDDDDDD";
        public const string HiddenSelectedColour = "#FFBBBBBB";

        public static string GetBtnInverseColour(string colour)
        {
            switch (colour)
            {
                case StatementColour:
                    return StatementSelectedColour;
                case StatementSelectedColour:
                    return StatementColour;
                case QuestionColour:
                    return QuestionSelectedColour;
                case QuestionSelectedColour:
                    return QuestionColour;
                case ActionColour:
                    return ActionSelectedColour;
                case ActionSelectedColour:
                    return ActionColour;
                case ConditionColour:
                    return ConditionSelectedColour;
                case ConditionSelectedColour:
                    return ConditionColour;
                case HiddenColour:
                    return HiddenSelectedColour;
                case HiddenSelectedColour:
                    return HiddenColour;
            }
            return colour;
        }

        public static string GetCategoryColour(string category)
        {
            switch (category)
            {
                case "Action":
                    return ActionColour;
                case "Ruleset Question":
                    return QuestionColour;
                case "Ruleset":
                case "Standard":
                    return StatementColour;
                case "Condition":
                    return ConditionColour;
                case "Trigger":
                case "TimeTrigger":
                    return ConditionColour;
                default:
                    return QuestionColour;
            }
        }
    }
}
