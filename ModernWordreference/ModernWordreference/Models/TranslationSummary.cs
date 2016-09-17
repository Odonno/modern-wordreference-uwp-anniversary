using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Models
{
    public class TranslationSummary
    {
        public DateTime SearchedDate { get; set; }
        public string Filename { get; set; }
        public bool Removed { get; set; }
    }
}
