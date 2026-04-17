using DevExpress.Xpo;
using Prog_Rav_Ordini.BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prog_Rav_Ordini.Forms
{
    public partial class Form_Impostazioni : Form
    {
        UnitOfWork uow;
        bool Salva = false;

        public Form_Impostazioni(UnitOfWork uowX)
        {
            InitializeComponent();
            uow = uowX;
            XPCollection<CRUSCOTTO> col = new XPCollection<CRUSCOTTO>(uow);
            col.Sorting = new SortingCollection(new SortProperty("LAMIERA_CODICE", DevExpress.Xpo.DB.SortingDirection.Ascending));
            gridControl1.DataSource = col;
        }

        private void Form_Impostazioni_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Salva)
                uow.CommitChanges();
            else
                uow.ReloadChangedObjects();
        }

        private void simpleButton_Salva_Click(object sender, EventArgs e)
        {
            Salva = true;
            Close();
        }

        private void simpleButton_Annulla_Click(object sender, EventArgs e)
        {
            if (uow.GetObjectsToSave().Count > 0 || uow.GetObjectsToDelete().Count > 0)
                if (MessageBox.Show("Ci sono modifiche non salvate. Sicuro di voler uscire ed annullare le modifiche?", "Attenzione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;

            Salva = false;
            Close();
        }

        private void simpleButton_Aggiungi_Click(object sender, EventArgs e)
        {
            gridView1.AddNewRow();
            //CRUSCOTTO cr = new CRUSCOTTO(uow);
            //cr.Save();
            //gridControl1.RefreshDataSource();
        }

        private void simpleButton_Elimina_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Sicuro di voler eliminare la riga selezionata?", "Attenzione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            //gridView1.DeleteRow(gridView1.FocusedRowHandle);
            CRUSCOTTO cr = (CRUSCOTTO)gridView1.GetRow(gridView1.FocusedRowHandle);
            if (cr != null)
                cr.Delete();
            gridControl1.RefreshDataSource();
        }
    }
}
