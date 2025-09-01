using System.Runtime.InteropServices;
using BOOL = System.Boolean;
using DWORD = System.UInt32;
using LPWSTR = System.String;
using NET_API_STATUS = System.UInt32;
using System.Windows.Forms;

namespace Prog_Rav_Ordini.Forms
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct USE_INFO_2
    {
        internal LPWSTR ui2_local;
        internal LPWSTR ui2_remote;
        internal LPWSTR ui2_password;
        internal DWORD ui2_status;
        internal DWORD ui2_asg_type;
        internal DWORD ui2_refcount;
        internal DWORD ui2_usecount;
        internal LPWSTR ui2_username;
        internal LPWSTR ui2_domainname;
    }


    static class Accedi_Rete
    {
        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern NET_API_STATUS NetUseAdd(LPWSTR UncServerName, DWORD Level, ref USE_INFO_2 Buf, out DWORD ParmError);

        public static void Accedi(string percorso, string utente, string password, string dominio)
        {
            USE_INFO_2 useInfo = new USE_INFO_2();
            useInfo.ui2_local = null;
            useInfo.ui2_remote = percorso;
            //useInfo.ui2_remote = "\\\\servername\\sharename";
            useInfo.ui2_password = password;
            useInfo.ui2_asg_type = 0;    //disk drive
            useInfo.ui2_usecount = 1;
            useInfo.ui2_username = utente;
            useInfo.ui2_domainname = dominio;

            uint paramErrorIndex;
            uint returnCode = NetUseAdd(null, 2, ref useInfo, out paramErrorIndex);
            /*
            if (returnCode != 0)
            {
                MessageBox.Show(returnCode.ToString());
            }*/
        }

    }
}
