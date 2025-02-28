﻿using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MetroFramework.Forms;
using BreakingBudget.Services;
using BreakingBudget.Services.Lang;
using BreakingBudget.Services.SQL;
using System.Collections.Generic;

namespace BreakingBudget.Views
{
    public partial class AjouterTransaction : MetroForm
    {
        // Type de la transaction lors de la modification de celle-ci 
        int ajoutTransaction_typeTransac;

        // Déclaration du DialogueResult pour confirmer la modification de la transaction
        DialogResult ajoutTransactionSureModif;

        // Accesseurs 
        public string AjoutTransaction_Date
        {
            get
            {
                return this.calAjoutTransaction.Value.ToShortDateString();
            }
        }
        public string AjoutTransaction_Description
        {
            get
            {
                return this.txtDesc.Text;
            }
        }
        public string AjoutTransaction_Montant
        {
            get
            {
                return this.txtMontant.Text;
            }
        }
        public bool AjoutTransaction_Recette
        {
            get
            {
                return this.ckbRecette.Checked;
            }
        }
        public bool AjoutTransaction_Percu
        {
            get
            {
                return this.ckbPercu.Checked;
            }
        }
        public int AjoutTransaction_Type
        {
            get
            {
                return (int)this.cboType.SelectedValue;
            }
        }
        public DialogResult AjoutTransaction_confirmModif
        {
            get
            {
                return this.ajoutTransactionSureModif;
            }
        }

        public AjouterTransaction()
        {
            InitializeComponent();

            // inherit theme from settings
            this.metroStyleExtender.StyleManager = Program.settings.styleManager;
            this.StyleManager = this.metroStyleManager;
            this.StyleManager.Theme = Program.settings.styleManager.Theme;
            this.StyleManager.Style = Program.settings.styleManager.Style;

            // change the background of the "ListPersonneContainer" (will make a border-like)
            if (this.StyleManager.Theme == MetroFramework.MetroThemeStyle.Dark)
            {
                this.ListPersonneContainer.BackColor = System.Drawing.Color.FromArgb(55, 56, 57);
            }
            else
            {
                this.ListPersonneContainer.BackColor = System.Drawing.Color.FromArgb(0xCC, 0xCC, 0xCC);
            }

            this.IconBtnAjoutTransaction_AddType.Font = (new IconFonts()).GetFont(IconFonts.FONT_FAMILY.MaterialIcons, 17.0f);
            this.IconBtnAjoutTransaction_AddType.Text = Encoding.UTF8.GetString(new byte[] { 0xEE, 0x85, 0x87 });

            // Paramètre des composants
            this.btnEdit.Visible = false;
            panelAjoutTransac.Visible = true;
            this.Text = Program.settings.localize.Translate("Nouvelle transaction");
            this.Refresh();

            Program.settings.localize.ControlerTranslator(this);

            this.AjoutTransaction_Load();
        }

        public AjouterTransaction(
            DateTime dataTransac, string description, string montant, bool recette, bool percu, int type) : this()
        {
            // Paramètres des composants
            panelAjoutTransac.Visible = false;
            this.Text = Program.settings.localize.Translate("Modifier une transaction");
            this.Refresh();

            this.btnCancel.Visible = true;
            this.btnEdit.Visible = true;
            this.btnEdit.Enabled = true;

            this.btnSubmit.Enabled = false;
            this.btnSubmit.Visible = false;

            // Initialisation de la transaction à modifier
            // Récupération des info de la transaction du formulaire père
            this.calAjoutTransaction.Value = dataTransac;
            this.txtDesc.Text = description;
            this.txtMontant.Text = montant;
            this.ckbRecette.Checked = recette;
            this.ckbPercu.Checked = percu;
            this.ajoutTransaction_typeTransac = type;
        }

        ////////////// FONCTIONS OUTILS ///////////////////////////////////////////////////////////////////
        // Remplir listbox par liaison de donnée
        private void RemplirListboxNomPrenom()
        {
            OleDbConnection connec = DatabaseManager.GetConnection();

            // On récupère les données de la table Personne
            // puis on les stocks dans une table locale "Personne"
            string requete = @"SELECT *
                               FROM [Personne]";
            OleDbCommand cmd = new OleDbCommand(requete, connec);
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            DataTable tblPersonne = new DataTable("Personne");

            try
            {
                connec.Open();

                da.Fill(tblPersonne);

                // On créé une colonne NOM+PRENOM pour pouvoir  faire la liaison de donnée
                tblPersonne.Columns.Add("NomPrenom");
                foreach (DataRow dr in tblPersonne.Rows)
                    dr["NomPrenom"] = dr["nomPersonne"] + " " + dr["pnPersonne"];

                // On complète la listbox avec les "Noms Prénoms" des personnes
                // par liaison de donnée
                // avec la table locale créée
                listBoxAjoutTransaction_Personne.DataSource = tblPersonne;
                listBoxAjoutTransaction_Personne.DisplayMember = "NomPrenom";
                listBoxAjoutTransaction_Personne.ValueMember = "codePersonne";

                // On désélectionne tous les éléments
                listBoxAjoutTransaction_Personne.ClearSelected();
            }
            catch (OleDbException ex)
            {
                ErrorManager.HandleOleDBError(ex);
            }
            finally
            {
                connec.Close();
            }
        }

        // Remplir combobox par liaison de donnée
        private void RemplirCombobox(ComboBox cbo, string nomTable, string champAffiche, string champCache)
        {
            OleDbConnection connec = DatabaseManager.GetConnection();

            // On récupère les données de la table en paramètre
            // puis on les stocks dans une table locale "table"
            string requete = @"SELECT *
                               FROM " + nomTable;
            OleDbCommand cmd = new OleDbCommand(requete, connec);
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            DataTable table = new DataTable("nomTable");

            try
            {
                connec.Open();

                da.Fill(table);

                // On remplit le composant par liaison de donnée
                // avec la table locale créée
                cbo.DataSource = table;
                cbo.DisplayMember = champAffiche.ToString();
                cbo.ValueMember = champCache.ToString();
            }
            catch (OleDbException ex)
            {
                ErrorManager.HandleOleDBError(ex);
            }
            finally
            {
                connec.Close();
            }
        }

        // Vérifier la saisie d'un montant 
        public static void VerifSaisieMontant(KeyPressEventArgs e, MetroFramework.Controls.MetroTextBox txt)
        {
            // Si c'est un chiffre, on accepte
            if (char.IsDigit(e.KeyChar))
                e.Handled = false;
            // On accepte l'effacement
            else if (e.KeyChar == (char)Keys.Back)
                e.Handled = false;
            // On accepte le "-" uniquement un première position
            else if (e.KeyChar == '-')
            {
                if (txt.Text == string.Empty)
                    e.Handled = false;
                else if (txt.Text.Length > 0 && !txt.Text.Contains("-"))
                {
                    e.Handled = true;
                    string txtsave = txt.Text;
                    txt.Text = "-" + txtsave;
                    txt.SelectionStart = txt.Text.Length;
                }
                else
                    e.Handled = true;
            }
            // On autorise uniquement UNE SEULE virgule uniquement en milieu de chaine
            else if (e.KeyChar == ',' || e.KeyChar == '.')
            {
                if (txt.Text.Contains(',') || txt.Text.Contains('.') || txt.Text == string.Empty)
                    e.Handled = true;
            }
            else
                e.Handled = true;
        }

        // Obtenir le prochaine numéro de transaction disponible
        private int GetNextIdTransac()
        {
            OleDbConnection connec = DatabaseManager.GetConnection();

            int res = 1;
            try
            {
                connec.Open();
                string requete = @"SELECT MAX(codeTransaction) 
                                   FROM [Transaction]";
                OleDbCommand cmd = new OleDbCommand(requete, connec);
                res = (int)cmd.ExecuteScalar() + 1;
            }
            catch (InvalidCastException)
            {
                res = 1;
            }
            catch (OleDbException ex)
            {
                ErrorManager.HandleOleDBError(ex);
            }
            finally
            {
                connec.Close();
            }
            // Return 1 si l'index n'a pas été trouvé
            return res;
        }

        // Si le champ du montant est non renseigné ou qu'il contient juste un "-"
        // ou si aucune personne bénéficiaire n'est sélectionnée
        // Alors on désactive le bouton
        private void VerifConditionTransaction()
        {
            if (txtMontant.Text == string.Empty || txtMontant.Text == "-" || listBoxAjoutTransaction_Personne.SelectedItems.Count == 0)
            {
                btnSubmit.Enabled = false;
            }
            else
            {
                btnSubmit.Enabled = true;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void AjoutTransaction_Load()
        {
            // On récupère tous les types pour compléter la combobox "cboType"
            RemplirCombobox(cboType, "TypeTransaction", "libType", "codeType");

            // On préselectionne la bonne valeur de "type de transaction"
            // lors de la modification de celle-ci à partir du tableau de bord
            if (!panelAjoutTransac.Visible)
            {
                cboType.SelectedValue = ajoutTransaction_typeTransac;
            }
            // On complète la listbox avec les "Noms Prénoms" des personnes
            RemplirListboxNomPrenom();
        }

        private void txtMontantAjoutTransaction_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Contrôle de la saisie du montant lors de la 
            // pression d'une touche du clavier
            VerifSaisieMontant(e, txtMontant);
        }

        private void txtMontantAjoutTransaction_KeyUp(object sender, KeyEventArgs e)
        {
            // Changer le status du bouton recette si il y a un moins 
            // dans la textebox du montant
            if (txtMontant.Text != "")
            {
                if (txtMontant.Text.Substring(0, 1) == "-")
                    ckbRecette.Checked = false;
                else
                    ckbRecette.Checked = true;
            }
        }

        private void txtMontant_TextChanged(object sender, EventArgs e)
        {
            VerifConditionTransaction();

            // Vérification des entrées lors de la 
            // modification d'une transaction
            if (btnEdit.Visible)
            {
                if (txtMontant.Text == string.Empty || txtMontant.Text == "-")
                {
                    btnEdit.Enabled = false;
                }
                else
                {
                    btnEdit.Enabled = true;
                }
            }

        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClearAjoutTransaction_Click(object sender, EventArgs e)
        {
            // Remettre la fenêtre par défaut (vider tous les composants)
            ckbRecette.Checked = false;
            ckbPercu.Checked = false;
            txtDesc.Text = string.Empty;
            txtMontant.Text = string.Empty;
            calAjoutTransaction.Value = DateTime.Today;
            listBoxAjoutTransaction_Personne.SelectedItems.Clear();
            cboType.SelectedIndex = 0;
        }

        // Bouton ajouter une personne
        private void btnAddPers_Click(object sender, EventArgs e)
        {
            // Affichage du formulaire d'ajout d'une personne
            UserCreation frmAddPers = new UserCreation();
            frmAddPers.ShowDialog();
            // Si une personne a été ajoutée, alors
            // on rafraichi la liste des personnes
            if (frmAddPers.DialogResult == DialogResult.OK)
                RemplirListboxNomPrenom();
        }

        private void btnAddType_Click(object sender, EventArgs e)
        {
            FrmAjoutType frmAddType = new FrmAjoutType();
            frmAddType.ShowDialog();
            if (frmAddType.DialogResult == DialogResult.OK)
            {
                // On récupère tous les types pour compléter la combobox "cboType"
                RemplirCombobox(cboType, "TypeTransaction", "libType", "codeType");
            }
        }

        // Valider la transaction
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Récupération des informations du formulaire
            string dateTransaction = calAjoutTransaction.Value.ToShortDateString();
            string description = txtDesc.Text;
            decimal montant;
            int type = (int)cboType.SelectedValue;
            int idTransac = GetNextIdTransac(); // Récupérer le numéro de la prochaine transaction

            int codeRetour;

            // Tente de convertir le montant en un double. Sinon : affiche une erreur et stop le traitement.
            if (!LocalizationManager.ConvertFloatingTo<decimal>(txtMontant.Text, decimal.TryParse, out montant))
            {
                ErrorManager.ShowNotANumberError(this);
                return;
            }

            // Ouverture de la connection
            OleDbConnection connec = DatabaseManager.GetConnection();

            // Construction de la chaine de la requête
            string requeteTransac = @"INSERT INTO [Transaction]
                                        VALUES (?,?,?,?,?,?,?)";

            // Création de la requete INSERT de la transaction
            OleDbCommand cmdTransac = new OleDbCommand(requeteTransac, connec);
            cmdTransac.Parameters.AddWithValue("@codeTransaction", idTransac);
            cmdTransac.Parameters.AddWithValue("@dateTransaction", dateTransaction);

            // Si jamais la description n'est pas renseigne, on insert "NULL" dans la colonne description
            cmdTransac.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
            cmdTransac.Parameters.AddWithValue("@montant", montant);
            cmdTransac.Parameters.AddWithValue("@recetteON", ckbRecette.Checked);
            cmdTransac.Parameters.AddWithValue("@percuON", ckbPercu.Checked);
            cmdTransac.Parameters.AddWithValue("@type", type);

            try
            {
                // Execution de la requête
                connec.Open();
                codeRetour = cmdTransac.ExecuteNonQuery();

                // Création des requetes dans la table des bénéficiaires
                OleDbCommand cmdBenef = new OleDbCommand();
                cmdBenef.Connection = connec;
                string requeteBenef = @"INSERT INTO [Beneficiaires]([codeTransaction], [codePersonne])
                                            VALUES (?,?)";
                cmdBenef.CommandText = requeteBenef;
                string numPers = string.Empty;

                // Pour chaque personne sélectionnée dans la listbox,
                // ajouter la transaction avec le codePersonne correspondant
                foreach (DataRowView drw in listBoxAjoutTransaction_Personne.SelectedItems)
                {
                    cmdBenef.Parameters.Clear();
                    numPers = drw.Row[0].ToString();
                    cmdBenef.Parameters.AddWithValue("@codeTransaction", idTransac);
                    cmdBenef.Parameters.AddWithValue("@codePersonne", numPers);
                    cmdBenef.ExecuteNonQuery();
                }
                ErrorManager.EntriesSuccessfullyAdded(this);


                // if the amount is a expense
                if (montant < 0)
                {
                    ///////// Envoi d'un SMS si la somme dépasse la totalité des revenus + 10 %
                    // Recherche et calcul du revenu de la famille
                    string requeteSumRevenus = "SELECT SUM(montant) FROM [PosteRevenu]";
                    OleDbCommand cmdSumRevenus = new OleDbCommand(requeteSumRevenus, connec);

                    object _sumRevenus = cmdSumRevenus.ExecuteScalar();

                    // we put in decimal because of a bug from the VS compiler installed in rds's server
                    // (see https://github.com/dotnet/roslyn/issues/7148)
                    decimal sumRevenus = (_sumRevenus.GetType() != typeof(DBNull)) ? decimal.Parse(_sumRevenus.ToString()) : 0;

                    // put the sum Revenu to negative to compare the negative expense
                    decimal sommeLimite = (sumRevenus * -1) - 0.1M * sumRevenus;

                    List<string> numerosTel = new List<string>();

                    // Si le montant de la transaction dépasse cette somme max, alors on envoi 
                    // un sms a tous les numéros renseignés dans la base
                    if (montant < sommeLimite)
                    {
                        // On récupère tous les numéros de téléphone de la table Personne
                        string requeteAllNum = "SELECT telMobile FROM [Personne] WHERE telMobile IS NOT NULL";
                        OleDbCommand cmdAllNum = new OleDbCommand(requeteAllNum, connec);
                        OleDbDataReader drAllNum = cmdAllNum.ExecuteReader();
                        while (drAllNum.Read())
                        {
                            string num = drAllNum[0].ToString();
                            if (num[0] == '0')
                            {
                                num = num.Substring(1, num.Length - 1);
                                num = "+33" + num;
                            }
                            numerosTel.Add(num);
                        }

                        // if there are num found
                        if (numerosTel.Count > 0)
                        {
                            // Envoi des SMS
                            string message = string.Format(Program.settings.localize.Translate("sms_big_expense_msg_{0}"), montant);
                            SMSManager.SendSMS(this, numerosTel.ToArray(), message);
                            ErrorManager.SMSSuccessfullySent(this);
                        }
                    }
                }

                // Clear formulaire
                btnClearAjoutTransaction_Click(null, null);

                this.Close();
            }
            catch (OleDbException ex)
            {
                ErrorManager.HandleOleDBError(ex);
            }
            finally
            {
                connec.Close();
            }
        }

        private void btnModif_Click(object sender, EventArgs e)
        {
            ajoutTransactionSureModif = MessageBox.Show(
                Program.settings.localize.Translate("transaction_edit_confirmation_msg"),
                "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            this.Close();
        }

        // Bouton "tout sélectionner"
        private void ckbTtSelect_Click(object sender, EventArgs e)
        {
            if (ckbTtSelect.Checked)
            {
                for (int i = 0; i < listBoxAjoutTransaction_Personne.Items.Count; i++)
                    listBoxAjoutTransaction_Personne.SetSelected(i, true);
            }
            else
                listBoxAjoutTransaction_Personne.SelectedItems.Clear();
        }

        private void ckbPercuAjoutTransaction_Click(object sender, EventArgs e)
        {
            // On vérifie que le bouton recette soit coché
            // pour pouvoir cocher le bouton perçu
            if (ckbPercu.Checked)
            {
                if (!ckbRecette.Checked)
                {
                    ckbRecette.Checked = true;
                    ckbRecetteAjoutTransaction_Click(null, null);
                }
            }
        }

        private void ckbRecetteAjoutTransaction_Click(object sender, EventArgs e)
        {
            // Rajouter/enlever le "-" dans la zone de txt
            // de saisie du montant si il n'existe pas déjà
            // On decheck également le bouton "Perçu"
            // qui n'a aucun sens quand la transaction n'est 
            // pas une recette
            if (ckbRecette.Checked == false)
            {
                string txtsave = txtMontant.Text;
                txtMontant.Text = "-" + txtsave;
                ckbPercu.Checked = false;
            }
            else
            {
                if (txtMontant.Text.Contains("-"))
                {
                    txtMontant.Text = txtMontant.Text.Replace("-", "");
                }
            }
        }

        private void listBoxPersonneAjoutTransaction_SelectedIndexChanged(object sender, EventArgs e)
        {
            VerifConditionTransaction();
            // On décheck le bouton tout selectionner si on déselectionne un élement dans la listbox
            if (listBoxAjoutTransaction_Personne.SelectedItems.Count != listBoxAjoutTransaction_Personne.Items.Count && ckbTtSelect.Checked)
                ckbTtSelect.Checked = false;
            // On check le bouton tout selectionner si on coche tous les élements de la liste
            if (listBoxAjoutTransaction_Personne.SelectedItems.Count == listBoxAjoutTransaction_Personne.Items.Count)
                ckbTtSelect.Checked = true;
        }

        private void AjoutTransaction_Load(object sender, EventArgs e)
        {
            this.AjoutTransaction_Load();
        }
    }
}
