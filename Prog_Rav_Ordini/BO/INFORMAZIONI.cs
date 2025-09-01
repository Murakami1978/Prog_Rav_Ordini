using DevExpress.Xpo;

namespace Prog_Rav_Ordini.BO
{
    public class INFORMAZIONI : XPCustomObject
    {
        public INFORMAZIONI(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        int fOID;
        [Key(true)]
        public int OID
        {
            get { return fOID; }
            set { SetPropertyValue<int>(nameof(OID), ref fOID, value); }
        }

        string fAGGIORNAMENTO_DATA;
        [DbType("VARCHAR(10) CHARACTER SET ISO8859_1")]
        public string AGGIORNAMENTO_DATA
        {
            get { return fAGGIORNAMENTO_DATA; }
            set { SetPropertyValue<string>(nameof(AGGIORNAMENTO_DATA), ref fAGGIORNAMENTO_DATA, value); }
        }
        string fCARTELLA_FILES;
        [DbType("VARCHAR(100) CHARACTER SET ISO8859_1")]
        public string CARTELLA_FILES
        {
            get { return fCARTELLA_FILES; }
            set { SetPropertyValue<string>(nameof(CARTELLA_FILES), ref fCARTELLA_FILES, value); }
        }
        string fCARTELLA_FILES_FTP;
        [DbType("VARCHAR(100) CHARACTER SET ISO8859_1")]
        public string CARTELLA_FILES_FTP
        {
            get { return fCARTELLA_FILES_FTP; }
            set { SetPropertyValue<string>(nameof(CARTELLA_FILES_FTP), ref fCARTELLA_FILES_FTP, value); }
        }
    }
}
