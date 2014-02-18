using System;
using System.Collections.Generic;
using System.Text;

using umbraco;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.interfaces;

namespace FALMTabularFolderBrowser
{
    class FALMTabularFolderBrowserUtility
    {
        // get property name from a field descriptor in format "PropertyName[widht]"
        static internal string GetPropertyName(string fieldDesc)
        {
            int startIdx = fieldDesc.IndexOf('[');
            int endIdx = fieldDesc.IndexOf(']');

            if (startIdx < 0 || startIdx > endIdx)
            {
                return fieldDesc;
            }
            else
            {
                return fieldDesc.Substring(0, startIdx);
            }
        }

        // get field widht from a field descriptor in format "PropertyName[widht]"
        static internal string GetFieldWidth(string fieldDesc)
        {
            int startIdx = fieldDesc.IndexOf('[');
            int endIdx = fieldDesc.IndexOf(']');

            if (startIdx < 0 || startIdx > endIdx)
            {
                return "";
            }
            else
            {
                return fieldDesc.Substring(startIdx + 1, endIdx - startIdx - 1);
            }
        }

        static internal string GetPropertyText(string fieldDesc)
        {
            string name = "";
            string desc = "";

            name = GetPropertyName(fieldDesc);
            desc = Application.SqlHelper.ExecuteScalar<string>("SELECT DISTINCT cmsPropertyType.Name FROM cmsPropertyType WHERE cmsPropertyType.Alias  = '" + name + "'");

            if (string.IsNullOrEmpty(desc))
            {
                desc = ui.Text(name);
            }
            return desc;
        }

    }
}
