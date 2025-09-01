using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using DevExpress.Data.Filtering;
using DevExpress.Utils.Drawing;
using DevExpress.Xpo;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using Prog_Rav_Ordini.BO;
using UtilityLuca;

namespace Prog_Rav_Ordini.Forms
{
    public partial class Principale : DevExpress.XtraEditors.XtraForm
    {
        CriteriaOperator crit_inventari = null;
        Avanzamento av = null;

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public Principale()
        {
            InitializeComponent();
            Utility_Generiche.Carica_ImpostazioniFinestra(this);
            Inizializza_EConnetti_Database();
            gridView_Inventari.OptionsBehavior.Editable = false;
            gridView_Inventari.OptionsBehavior.ReadOnly = true;
            
            INFORMAZIONI inf = unitOfWork1.FindObject<INFORMAZIONI>(null);
            if (inf != null)
            {
                label_AggiornamentoData.Text = Utility_Generiche.DataDaAmericanaAdItaliana(inf.AGGIORNAMENTO_DATA);
                Globale.Cartella_Files = inf.CARTELLA_FILES;
            }

            Aggiorna_Criteria_Inventari();

            // Ordino la griglia
            gridView_Inventari.BeginSort();
            try
            {
                gridView_Inventari.ClearSorting();
                //fai(item.Campo.NOME_CAMPO);
                GridColumn col = gridView_Inventari.Columns.ColumnByFieldName("LAMIERA_CODICE");
                if (col != null) col.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            }
            finally { gridView_Inventari.EndSort(); }

            dateEdit_Inventari.DateTime = DateTime.Now;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Aggiorna_Criteria_Inventari()
        {
            crit_inventari = CriteriaOperator.Parse("DATA=?", Utility_Generiche.DataDaItalianaAdAmericana(dateEdit_Inventari.DateTime.ToShortDateString()));
            xpCollection_Inventari.Criteria = crit_inventari;
            xpCollection_Inventari.Reload();
            gridView_Inventari.FocusedRowHandle = 0;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Inizializza_EConnetti_Database()
        {
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(Globale.ConnectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            string errore = "";
            if (!Aggiorna_Tutti_PersistentObject(ref errore)) { MessageBox.Show(errore, "Errore", MessageBoxButtons.OK); }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static bool Aggiorna_Tutti_PersistentObject(ref string errore)
        {
            try
            {
                UnitOfWork Sessione = new UnitOfWork();
                Sessione.UpdateSchema(typeof(CRUSCOTTO));
                Sessione.UpdateSchema(typeof(INVENTARI));
                Sessione.UpdateSchema(typeof(INFORMAZIONI));
                return true;
            }
            catch (Exception ex) { errore = ex.Message; return false; }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void checkEdit_AbilitaModifiche_CheckedChanged(object sender, EventArgs e)
        {
            gridView_Inventari.OptionsBehavior.Editable = checkEdit_AbilitaModifiche.Checked;
            gridView_Inventari.OptionsBehavior.ReadOnly = !checkEdit_AbilitaModifiche.Checked;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void simpleButton_AggiornaDati_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            av = new Avanzamento();
            av.Show();
            Aggiorna_Dati_Inventari();
            Aggiorna_Criteria_Inventari();
            av.Close();
            //Aggiorna_Dati();
            Cursor.Current = Cursors.Default;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Aggiorna_Dati_Inventari()
        {
            // Ricavo l'ultima data importata dentro INVENTARI
            string ultima_data = "";
            object ultima_data_obj = unitOfWork1.Evaluate<INVENTARI>(CriteriaOperator.Parse("Max(DATA)"), null);
            if (ultima_data_obj != null) ultima_data = ultima_data_obj.ToString();

            // Scanno i files ed esamino solo quelli con data superiore a quella trovata
            // li inserisco dentro INVENTARI
            IEnumerable<string> elenco_files = Directory.EnumerateFiles(Globale.Cartella_Files);
            av.progressBar1.Maximum = elenco_files.Count<string>();
            av.progressBar1.Value = 0;
            foreach (string nome_file in elenco_files)
            {
                av.label_avanzamento.Text = "Importo da file \"" + Path.GetFileName(nome_file) + "\""; av.label_avanzamento.Refresh(); Application.DoEvents();
                string data = Utility_Generiche.DataDaItalianaAdAmericana(EstraiData_Da_NomeFile(nome_file));
                if (string.Compare(data, ultima_data) > 0)
                    Inserisci_Dentro_Inventari(nome_file, data);
                av.progressBar1.Value++; av.progressBar1.Refresh(); Application.DoEvents();
            }
            unitOfWork1.CommitTransaction();

            // Calcolo i DELTA ...  partendo da ultima_data in avanti.

        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Inserisci_Dentro_Inventari(string nome_file, string data)
        {
            if (nome_file == "") return;
            // Azzero le somme (nel caso siano già presenti i dati)
            XPCollection<INVENTARI> coll = new XPCollection<INVENTARI>(CriteriaOperator.Parse("DATA=?", data));
            foreach (INVENTARI item in coll)
            {
                item.KG_GIACENTI = 0;
                item.FG_GIACENTI = 0;
                item.NUM_CASSETTI_VUOTI = 0;
                item.Save();
            }

            // Riempio "tabella" con i dati presi dal file
            DataTable tabella = ConvertCSVtoDataTable(nome_file);
            if (tabella == null) return;

            // Scansiono le righe della tabella
            DataRow[] Righe_Trovate = tabella.Select();
            foreach (DataRow riga in Righe_Trovate)
            {
                // Cerco il "codice lamiera" dentro al database
                string cod_lam = riga.ItemArray[CX.Lamiera_Codice].ToString();
                INVENTARI dato = null;
                dato = unitOfWork1.FindObject<INVENTARI>(CriteriaOperator.Parse("DATA=? AND LAMIERA_CODICE=?", data, cod_lam));
                if (dato == null)
                {
                    dato = new INVENTARI(unitOfWork1);
                    dato.DATA = data;
                    dato.LAMIERA_CODICE = cod_lam;
                    dato.Cruscotto = unitOfWork1.FindObject<CRUSCOTTO>(CriteriaOperator.Parse("LAMIERA_CODICE=?", cod_lam));
                }

                int Kg = Int32.Parse(riga.ItemArray[CX.Kg].ToString());
                dato.KG_GIACENTI += Kg;
                int Fg = Int32.Parse(riga.ItemArray[CX.Fg].ToString());
                dato.FG_GIACENTI += Fg;
                if (Fg == 0) dato.NUM_CASSETTI_VUOTI += 1;
                dato.Save();
                unitOfWork1.CommitChanges();
            }

            INVENTARI giorno_prima = null;
            object prima_data_obj = unitOfWork1.Evaluate<INVENTARI>(CriteriaOperator.Parse("Min(DATA)"), null);
            if (prima_data_obj == null) return;
            string prima_data = prima_data_obj.ToString();

            // Calcolo i DELTA
            coll = new XPCollection<INVENTARI>(CriteriaOperator.Parse("DATA=?", data));
            foreach (INVENTARI item in coll)
            {
                // Cerco il giorno subito prima
                while ((giorno_prima == null) && (string.Compare(data, prima_data) > 0))
                {
                    string data1 = Utility_Generiche.DataDaItalianaAdAmericana(DateTime.Parse(data).AddDays(-1).ToShortDateString());
                    giorno_prima = unitOfWork1.FindObject<INVENTARI>(CriteriaOperator.Parse("DATA=? AND LAMIERA_CODICE=?", data1, item.LAMIERA_CODICE));
                }
                if (giorno_prima != null)
                {
                    item.DELTA_KG = giorno_prima.KG_GIACENTI - item.KG_GIACENTI; 
                    item.DELTA_FG = giorno_prima.FG_GIACENTI - item.FG_GIACENTI;
                }
                else
                {
                    item.DELTA_KG = 0;
                    item.DELTA_FG = 0;
                }
                giorno_prima = null;
                item.Save();
            }
            unitOfWork1.CommitTransaction();
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /*private void Aggiorna_Dati()
        {
            // Metto a zero tutte le somme
            Resetta_Dati();

            // Trovo il file più recente dentro la cartella "Globale.Cartella_Files"
            if (!Directory.Exists(Globale.Cartella_Files)) { MessageBox.Show("Cartella \"" + Globale.Cartella_Files + "\" inesistente", "Errore", MessageBoxButtons.OK); return; }
            string nome_file = new DirectoryInfo(Globale.Cartella_Files).GetFiles().OrderByDescending(o => o.Name).FirstOrDefault().FullName;
            if (nome_file == "") return;

            // Riempio "tabella" con i dati presi dal file
            DataTable tabella = ConvertCSVtoDataTable(nome_file);
            if (tabella == null) return;

            // Scansiono le righe della tabella
            DataRow[] Righe_Trovate = tabella.Select();
            foreach (DataRow riga in Righe_Trovate)
            {
                // Cerco il "codice lamiera" dentro al database
                string cod_lam = riga.ItemArray[CX.Lamiera_Codice].ToString();
                CRUSCOTTO dato = unitOfWork1.FindObject<CRUSCOTTO>(CriteriaOperator.Parse("LAMIERA_CODICE=?", cod_lam));
                if (dato != null)
                {
                    int Kg = Int32.Parse(riga.ItemArray[CX.Kg].ToString());
                    dato.KG_GIACENTI += Kg;
                    int Fg = Int32.Parse(riga.ItemArray[CX.Fg].ToString());
                    dato.FG_GIACENTI += Fg;
                    if (Fg == 0) dato.NUM_CASSETTI_VUOTI += 1;
                    dato.Save();
                }
            }

            // Aggiorno Data Aggiornamento
            string data = EstraiData_Da_NomeFile(nome_file);
            label_AggiornamentoData.Text = data;

            INFORMAZIONI inf = unitOfWork1.FindObject<INFORMAZIONI>(null);
            if (inf == null) inf = new INFORMAZIONI(unitOfWork1);
            if (inf != null)
            {
                inf.AGGIORNAMENTO_DATA = Utility_Generiche.DataDaItalianaAdAmericana(data);
                inf.Save();
            }

            unitOfWork1.CommitTransaction();
        }*/
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(';');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(';');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        /*private void Resetta_Dati()
        {
            foreach (CRUSCOTTO item in xpCollection_Cruscotto)
            {
                item.FG_GIACENTI = 0;
                item.KG_GIACENTI = 0;
                item.NUM_CASSETTI_VUOTI = 0;
                item.Save();
            }
            unitOfWork1.CommitTransaction();
        }*/
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string EstraiData_Da_NomeFile(string nome_file)
        {
            // 20020238INV20181017.csv
            string s = nome_file;

            if (s.Length < 4) return "";
            // tolgo l'estensione
            s = s.Substring(0, s.Length - 4);

            if (s.Length < 11) return "";
            string tipo = s.Substring(s.Length - 11, 3);
            if (tipo.ToUpper() != "INV") return "";

            if (s.Length < 8) return "";

            s = s.Substring(s.Length - 8, 8);
            string data = s.Substring(s.Length - 2, 2) + "/" + s.Substring(s.Length - 4, 2) + "/" + s.Substring(s.Length - 8, 4);
            return data;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private string Cerca_NomeFile_DaData(string data)
        {
            IEnumerable<string> elenco_files = Directory.EnumerateFiles(Globale.Cartella_Files);
            foreach (string nome_file in elenco_files)
            {
                string data_trovata = Utility_Generiche.DataDaItalianaAdAmericana(EstraiData_Da_NomeFile(nome_file));
                if (data_trovata == data) return nome_file;
            }
            return "";
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utility_Generiche.Salva_ImpostazioniFinestra(this);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void gridView_Cruscotto_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            /*GridView gv = (sender as GridView);
            int IndiceRiga_DataSource = gv.GetDataSourceRowIndex(e.RowHandle);
            CRUSCOTTO riga = (CRUSCOTTO)xpCollection_Cruscotto[IndiceRiga_DataSource];

            if (
                ((e.Column.FieldName == "Num_Cassetti_Pieni") && (riga.Num_Cassetti_Pieni < riga.SCORTA_MIN_NUM_CASSETTI)) || 
                ((e.Column.FieldName == "KG_GIACENTI") && (riga.KG_GIACENTI < riga.Scorta_Min_Kg_Totali))
               )
            {
                //e.Appearance.ForeColor = Color.Red;
                //e.Appearance.Font = new Font(e.Appearance.Font.Name, e.Appearance.Font.Size, FontStyle.Bold);
                e.Appearance.BackColor = Color.Red;
                e.Appearance.ForeColor = Color.White;
                e.DefaultDraw();
                e.Handled = true;
            }*/
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void dateEdit_Inventari_DateTimeChanged(object sender, EventArgs e)
        {
            Aggiorna_Criteria_Inventari();
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void gridView_Inventari_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            GridView gv = (sender as GridView);
            int IndiceRiga_DataSource = gv.GetDataSourceRowIndex(e.RowHandle);
            INVENTARI riga = (INVENTARI)xpCollection_Inventari[IndiceRiga_DataSource];

            if (
                ((e.Column.FieldName == "Num_Cassetti_Pieni") && (riga.Num_Cassetti_Pieni < riga.Cruscotto.SCORTA_MIN_NUM_CASSETTI)) ||
                ((e.Column.FieldName == "KG_GIACENTI") && (riga.KG_GIACENTI < riga.Cruscotto.Scorta_Min_Kg_Totali))
               )
            {
                if (gv.FocusedRowHandle == e.RowHandle)
                {
                    if (gv.FocusedColumn == e.Column) e.Appearance.BackColor = Color.DarkRed; else e.Appearance.BackColor = Color.OrangeRed; 
                }
                else e.Appearance.BackColor = Color.Red;

                e.Appearance.ForeColor = Color.White;

                e.DefaultDraw();
                e.Handled = true;
            }
            if (
                ((e.Column.FieldName == "DELTA_KG") && (riga.DELTA_KG <= 0)) ||
                ((e.Column.FieldName == "DELTA_FG") && (riga.DELTA_FG <= 0))
               )
            {
                e.DisplayText = "";
                e.DefaultDraw();
                e.Handled = true;
            }
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void simpleButton_EliminaGiornata_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Elimina_Giornata(dateEdit_Inventari.DateTime);
            Aggiorna_Criteria_Inventari();
            Cursor.Current = Cursors.Default;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Elimina_Giornata(DateTime data)
        {
            CriteriaOperator crit = CriteriaOperator.Parse("DATA=?", Utility_Generiche.DataDaItalianaAdAmericana(data.ToShortDateString()));
            XPCollection<INVENTARI> coll = new XPCollection<INVENTARI>(unitOfWork1, crit);
            unitOfWork1.Delete(coll);
            unitOfWork1.CommitTransaction();
            unitOfWork1.PurgeDeletedObjects();
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void simpleButton_ImportaQuestoGiorno_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string data = Utility_Generiche.DataDaItalianaAdAmericana(dateEdit_Inventari.DateTime.ToShortDateString());
            string nome_file = Cerca_NomeFile_DaData(data);
            Inserisci_Dentro_Inventari(nome_file, data);
            Aggiorna_Criteria_Inventari();
            Cursor.Current = Cursors.Default;
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void simpleButton_Left_Click(object sender, EventArgs e)
        {
            dateEdit_Inventari.DateTime = dateEdit_Inventari.DateTime.AddDays(-1);
            Aggiorna_Criteria_Inventari();
        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void simpleButton_Right_Click(object sender, EventArgs e)
        {
            dateEdit_Inventari.DateTime = dateEdit_Inventari.DateTime.AddDays(1);
            Aggiorna_Criteria_Inventari();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string user = "metalFTP01";
            string psw = "$ruota2018";


            //var networkPath = @"\\192.168.176.230\home";
            var networkPath = @"//192.168.176.230/home";
            //var networkPath = @"//server/share";

            var credentials = new NetworkCredential(user, psw);

            using (new NetworkConnection(networkPath, credentials))
            {
                var fileList = Directory.GetFiles(networkPath);
                foreach (var file in fileList)
                {
                    Console.WriteLine("{0}", Path.GetFileName(file));
                }
            }

        }
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    }
}
