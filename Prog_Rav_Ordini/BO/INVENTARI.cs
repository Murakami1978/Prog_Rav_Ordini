using DevExpress.Xpo;

namespace Prog_Rav_Ordini.BO
{
    [Indices(new string[] { "DATA;LAMIERA_CODICE" })]
    public class INVENTARI : XPCustomObject
    {
        public INVENTARI(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        int fOID;
        [Key(true)]
        public int OID
        {
            get { return fOID; }
            set { SetPropertyValue<int>(nameof(OID), ref fOID, value); }
        }

        CRUSCOTTO fCruscotto;
        [Association(@"Cruscotto_Inventari")]
        public CRUSCOTTO Cruscotto
        {
            get { return fCruscotto; }
            set { SetPropertyValue(nameof(Cruscotto), ref fCruscotto, value); }
        }

        string fDATA;
        [DbType("VARCHAR(10) CHARACTER SET ISO8859_1")]
        public string DATA
        {
            get { return fDATA; }
            set { SetPropertyValue<string>(nameof(DATA), ref fDATA, value); }
        }
        string fLAMIERA_CODICE;
        [DbType("VARCHAR(10) CHARACTER SET ISO8859_1")]
        public string LAMIERA_CODICE
        {
            get { return fLAMIERA_CODICE; }
            set { SetPropertyValue<string>(nameof(LAMIERA_CODICE), ref fLAMIERA_CODICE, value); }
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

        [NonPersistent] public int Num_Cassetti_Pieni_Calc
        {
            get
            {
                if (DATA.CompareTo(Globale.Data_Cambio_Calcolo) < 0) return Num_Cassetti_Pieni_Old;
                else return NUMERO_CASSETTI_PIENI;
            }
        }

        [NonPersistent] public int Num_Cassetti_Vuoti_Calc
        {
            get
            {
                if (DATA.CompareTo(Globale.Data_Cambio_Calcolo) < 0) return NUM_CASSETTI_VUOTI;
                else return Numero_Cassetti_Vuoti;
            }
        }


        // NUOVI
        int fNUMERO_CASSETTI_PIENI;
        public int NUMERO_CASSETTI_PIENI
        {
            get { return fNUMERO_CASSETTI_PIENI; }
            set { SetPropertyValue<int>(nameof(NUMERO_CASSETTI_PIENI), ref fNUMERO_CASSETTI_PIENI, value); }
        }
        [NonPersistent] public int Numero_Cassetti_Vuoti
        {
            get
            {
                if (fCruscotto != null) return fCruscotto.CASSETTI_NUMERO - NUMERO_CASSETTI_PIENI;
                else return -1;
            }
        }

        // VECCHI
        int fNUM_CASSETTI_VUOTI;
        public int NUM_CASSETTI_VUOTI
        {
            get { return fNUM_CASSETTI_VUOTI; }
            set { SetPropertyValue<int>(nameof(NUM_CASSETTI_VUOTI), ref fNUM_CASSETTI_VUOTI, value); }
        }
        [NonPersistent] public int Num_Cassetti_Pieni_Old
        {
            get
            {
                if (fCruscotto != null) return fCruscotto.CASSETTI_NUMERO - NUM_CASSETTI_VUOTI;
                else return -1;
            }
        }

        int fDELTA_FG;
        public int DELTA_FG
        {
            get { return fDELTA_FG; }
            set { SetPropertyValue<int>(nameof(DELTA_FG), ref fDELTA_FG, value); }
        }
        int fDELTA_KG;
        public int DELTA_KG
        {
            get { return fDELTA_KG; }
            set { SetPropertyValue<int>(nameof(DELTA_KG), ref fDELTA_KG, value); }
        }
    }
}
