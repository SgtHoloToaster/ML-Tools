using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TextProcessing
{
    public class CountVectorizerSettings
    {
        readonly static Regex _defaultWordPattern = new Regex(@"\b\w+(\'\w+)?\b", RegexOptions.Compiled);

        public (byte, byte) NgramRange { get; set; } = (1, 1);

        public IEnumerable<string> StopWords { get; set; }

        public int? MaxFeatures { get; set; }

        public Regex WordPattern { get; set; } = _defaultWordPattern;

        public HashSet<string> Vocabulary { get; set; }
    }
}
