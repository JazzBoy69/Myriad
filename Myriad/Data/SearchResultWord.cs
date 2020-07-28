using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feliciana.Library;

namespace Myriad.Data
{
    internal class SearchResultWord
    {
        internal static string SelectString = "select keywords.keyid, RTrim(keywords.leadingsymbols), RTrim(keywords.text), keywords.iscapitalized, RTrim(keywords.trailingsymbols), keywords.ismaintext from keywords ";

        private bool used = false;
        private bool highlighted = false;
        private string substituteText = "";
        private bool substituted = false;
        bool erased = false;
        readonly bool isMainText = true;
        readonly int id;
        int length = 1;

        internal bool IsMainText
        {
            get { return isMainText; }
        }

        internal bool Used
        {
            get { return used; }
            set { used = value; }
        }

        public bool Highlight { get { return highlighted; } set { highlighted = value; } }

        public bool Erased { get { return erased; } set { erased = value; } }

        public int Length { get { return length; } set { length = value; } }

        public string SubstituteText
        {
            get
            {
                return substituteText;
            }
            set
            {
                substituteText = value;
                substituted = true;
            }
        }

        public bool Substituted => substituted;

        public int ID { get { return id; } }
    }
}
