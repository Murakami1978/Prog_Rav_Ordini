using DevExpress.Xpo;

namespace Prog_Rav_Ordini.BO
{
    [Indices(new string[] { "LAMIERA_CODICE" })]
    public class CRUSCOTTO : XPCustomObject
    {
        public CRUSCOTTO(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        int fOID;
        [Key(true)]
        public int OID
        {
            get { return fOID; }
            set { SetPropertyValue<int>(nameof(OID), ref fOID, value); }
        }

        [Association(@"Cruscotto_Inventari")]
        public XPCollection<INVENTARI> inventari { get { return GetCollection<INVENTARI>(nameof(inventari)); } }


        string fLAMIERA_CODICE;
        [DbType("VARCHAR(10) CHARACTER SET ISO8859_1")]
        public string LAMIERA_CODICE
        {
            get { return fLAMIERA_CODICE; }
            set { SetPropertyValue<string>(nameof(LAMIERA_CODICE), ref fLAMIERA_CODICE, value); }
        }
        string fDESCRIZIONE;
        [DbType("VARCHAR(100) CHARACTER SET ISO8859_1")]
        public string DESCRIZIONE
        {
            get { return fDESCRIZIONE; }
            set { SetPropertyValue<string>(nameof(DESCRIZIONE), ref fDESCRIZIONE, value); }
        }
        int fCASSETTI_NUMERO;
        public int CASSETTI_NUMERO
        {
            get { return fCASSETTI_NUMERO; }
            set { SetPropertyValue<int>(nameof(CASSETTI_NUMERO), ref fCASSETTI_NUMERO, value); }
        }
        int fKG_MAX_CASSETTO;
        public int KG_MAX_CASSETTO
        {
            get { return fKG_MAX_CASSETTO; }
            set { SetPropertyValue<int>(nameof(KG_MAX_CASSETTO), ref fKG_MAX_CASSETTO, value); }
        }
        int fFG_MAX_CASSETTO;
        public int FG_MAX_CASSETTO
        {
            get { return fFG_MAX_CASSETTO; }
            set { SetPropertyValue<int>(nameof(FG_MAX_CASSETTO), ref fFG_MAX_CASSETTO, value); }
        }
        // Campo Calcolato
        [NonPersistent]
        public int Kg_Max_Totali
        {
            get { return CASSETTI_NUMERO * KG_MAX_CASSETTO; }
        }
        int fSCORTA_MIN_NUM_CASSETTI;
        public int SCORTA_MIN_NUM_CASSETTI
        {
            get { return fSCORTA_MIN_NUM_CASSETTI; }
            set { SetPropertyValue<int>(nameof(SCORTA_MIN_NUM_CASSETTI), ref fSCORTA_MIN_NUM_CASSETTI, value); }
        }
        // Campo Calcolato
        [NonPersistent]
        public int Scorta_Min_Kg_Totali
        {
            get { return SCORTA_MIN_NUM_CASSETTI * KG_MAX_CASSETTO; }
        }
        [NonPersistent]
        public int Scorta_Min_Fg_Totali
        {
            get { return SCORTA_MIN_NUM_CASSETTI * FG_MAX_CASSETTO; }
        }
        int fFG_GIACENTI;
        public int FG_GIACENTI
        {
            get { return fFG_GIACENTI; }
            set { SetPropertyValue<int>(nameof(FG_GIACENTI), ref fFG_GIACENTI, value); }
        }
        int fKG_GIACENTI;
        public int KG_GIACENTI
        {
            get { return fKG_GIACENTI; }
            set { SetPropertyValue<int>(nameof(KG_GIACENTI), ref fKG_GIACENTI, value); }
        }
        int fNUM_CASSETTI_VUOTI;
        public int NUM_CASSETTI_VUOTI
        {
            get { return fNUM_CASSETTI_VUOTI; }
            set { SetPropertyValue<int>(nameof(NUM_CASSETTI_VUOTI), ref fNUM_CASSETTI_VUOTI, value); }
        }
        // Campo Calcolato
        [NonPersistent]
        public int Num_Cassetti_Pieni
        {
            get { return CASSETTI_NUMERO - NUM_CASSETTI_VUOTI; }
        }

    }
}
