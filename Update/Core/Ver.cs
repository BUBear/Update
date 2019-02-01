using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Update.Core
{
    public struct Ver
    {
        private int _Major;
        private int _Minor;
        private int _Build;
        private int _Revision;

        public Ver(int Major, int Minor, int Build, int Revision)
        {
            this._Major = Major;
            this._Minor = Minor;
            this._Build = Build;
            this._Revision = Revision;
        }

        public Ver(string ver)
        {
            var verSplit = ver.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            this._Major = Convert.ToInt32(verSplit[0]);
            this._Minor = Convert.ToInt32(verSplit[1]);
            this._Build = Convert.ToInt32(verSplit[2]);
            this._Revision = verSplit.Length < 3 ? Convert.ToInt32(verSplit[3]) : -1;
        }

        public static int VersionCompare(Ver ver, Ver ver2)
        {
            int result = 0;
       
            if (ver.Revision != -1 || ver2.Revision != -1)
            {
                if (ver.Revision != ver2.Revision)
                    result = ver.Revision < ver2.Revision ? 1 : -1;
            }
            if (ver.Build != ver2.Build)
                result = ver.Build < ver2.Build ? 1 : -1;
            if (ver.Minor != ver2.Minor)
                result = ver.Minor < ver2.Minor ? 1 : -1;
            if (ver.Major != ver2.Major)
                result = ver.Major < ver2.Major ? 1 : -1;

            return result;
        }

        public static int VersionCompare(string version,string version2)
        {
            int result = 0;
            Ver ver = new Ver(version);
            Ver ver2 = new Ver(version2);
            if (ver.Revision != -1 || ver2.Revision != -1)
            {
                if (ver.Revision != ver2.Revision)
                    result = ver.Revision < ver2.Revision ? 1 : -1;
            }
            if (ver.Build != ver2.Build)
                result = ver.Build < ver2.Build ? 1 : -1;
            if (ver.Minor != ver2.Minor)
                result = ver.Minor < ver2.Minor ? 1 : -1;
            if (ver.Major != ver2.Major)
                result = ver.Major < ver2.Major ? 1 : -1;

            return result;
        }

        public override string ToString()
        {
            if(_Revision == -1)
                return _Major + "." + _Minor + "." + _Build;
            else
                return _Major + "." + _Minor + "." + _Build + "." + _Revision;
        }

        public int Major
        {
            get { return _Major; }
        }

        public int Minor
        {
            get { return _Minor; }
        }

        public int Build
        {
            get { return _Build; }
        }

        public int Revision
        {
            get { return _Revision; }
        }
    }
}
