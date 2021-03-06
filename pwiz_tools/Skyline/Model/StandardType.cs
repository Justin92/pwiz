﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using pwiz.Skyline.Properties;

namespace pwiz.Skyline.Model
{
    public class StandardType
    {
        public static readonly StandardType IRT = new StandardType("iRT", // Not L10N
            () => Resources.PeptideDocNode_GetStandardTypeDisplayName_iRT);
        public static readonly StandardType QC = new StandardType("QC", // Not L10N
            () => Resources.PeptideDocNode_GetStandardTypeDisplayName_QC);
        public static readonly StandardType GLOBAL_STANDARD = new StandardType("Normalization", // Not L10N
            ()=>Resources.PeptideDocNode_GetStandardTypeDisplayName_Normalization);
        public static readonly StandardType SURROGATE_STANDARD = new StandardType("Surrogate Standard", // Not L10N
            ()=>Resources.StandardType_SURROGATE_STANDARD_Surrogate_Standard);
        private readonly Func<String> _getTitleFunc;
        private StandardType(String name, Func<String> getTitleFunc)
        {
            Name = name;
            _getTitleFunc = getTitleFunc;
        }

        public String Name { get; private set; }
        public String Title { get { return _getTitleFunc(); } }
        public override String ToString()
        {
            if (CultureInfo.InvariantCulture.Equals(CultureInfo.CurrentCulture))
            {
                // When exporting the "Invariant" report for external tools,
                // we always use the persisted Name.
                return Name;
            }
            return Title;
        }

        public static IList<StandardType> ListStandardTypes()
        {
            return new[] {IRT, QC, GLOBAL_STANDARD, SURROGATE_STANDARD};
        }

        public static StandardType FromName(String name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (name == "Global Standard") // Not L10N
            {
                // "Global Standard" was the name that was used briefly during Skyline 3.6 development
                // It was changed back to "Normalization" for backward compatibility.
                return GLOBAL_STANDARD;
            }
            return ListStandardTypes().FirstOrDefault(standardType => standardType.Name == name);
        }
    }
}
