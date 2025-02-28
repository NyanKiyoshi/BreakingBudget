﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Data.OleDb;
using System.Text;
using System.Drawing.Text;
using System.Threading.Tasks;
using BreakingBudget.Repositories;
using BreakingBudget.Services.SQL;
using BreakingBudget.Services.Lang;
using MetroFramework;
using MetroFramework.Forms;
using MetroFramework.Controls;

namespace BreakingBudget.Views.FrmMain
{
    partial class FrmMain
    {
        private void InitializePostesFixes()
        {
            FillPostesComboBox();
            FillPeriodicitesComboBox();
            this.HelpPosteLabel.Font = this.IconFont;
            this.HelpPosteLabel.Text = Encoding.UTF8.GetString(ICON_HELP_MARK);
        }

        private void FillPostesComboBox()
        {
            // empty the ComboBox
            this.ComboxBoxListePostes.Items.Clear();

            // Reset the selection
            this.ComboxBoxListePostes.ResetText();
            this.ComboxBoxListePostes.Refresh();

            try
            {
                this.ComboxBoxListePostes.Items.AddRange(PosteRepository.ListAvailableToUse());
            }
            catch (OleDbException ex)
            {
                ErrorManager.HandleOleDBError(ex);
            }
        }

        private void FillPeriodicitesComboBox()
        {
            // empty the ComboBox
            this.ComboxBoxListePeriodicites.Items.Clear();

            try {
                // Translate and add every item
                foreach (PeriodiciteRepository.PeriodiciteModel e in
                    PeriodiciteRepository.List())
                {
                    e.libPer = Program.settings.localize.Translate(e.libPer);
                    this.ComboxBoxListePeriodicites.Items.Add(e);
                }
            } catch (OleDbException e)
            {
                ErrorManager.HandleOleDBError(e);
            }
        }

        private void ClearPosteFixeForm()
        {
            this.TxtBoxTousLesXMois.Text = string.Empty;
            this.TxtBoxMontantPosteFixe.Text = string.Empty;

            // Reset the selected poste
            this.ComboxBoxListePostes.ResetText();
            this.ComboxBoxListePostes.Refresh();

            // Reset the selected period
            this.ComboxBoxListePeriodicites.ResetText();
            this.ComboxBoxListePeriodicites.Refresh();
        }

        private void BtnValiderBudgetFixe_Click(object _s, EventArgs e)
        {
            MetroButton sender = (MetroButton)_s;
            PosteRepository.PosteModel SelectedPoste;
            PeriodiciteRepository.PeriodiciteModel SelectedPeriode;
            decimal montant;
            int TousLesXDuMois;

            // disable submit button
            sender.Enabled = false;

            // Check if every field was filled
            if (
                this.ComboxBoxListePeriodicites.SelectedItem == null
                || this.ComboxBoxListePostes.SelectedItem == null
                || this.TxtBoxTousLesXMois.Text.Length < 1
                || this.TxtBoxMontantPosteFixe.Text.Length < 1
                )
            {
                MetroMessageBox.Show(this,
                    Program.settings.localize.Translate("err_missing_fields_msg"),
                    Program.settings.localize.Translate("err_missing_fields_caption"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // try to convert the decimals and integers
            else if (!(LocalizationManager.ConvertFloatingTo<decimal>(TxtBoxMontantPosteFixe.Text, decimal.TryParse, out montant)
                  && int.TryParse(TxtBoxTousLesXMois.Text, out TousLesXDuMois)))
            {
                MetroMessageBox.Show(this,
                    Program.settings.localize.Translate("err_day_of_month_and_sum_not_number"),
                    Program.settings.localize.Translate("err_uh_oh_caption"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // retrieve the selected items
                SelectedPoste = (PosteRepository.PosteModel)this.ComboxBoxListePostes.SelectedItem;
                SelectedPeriode = (PeriodiciteRepository.PeriodiciteModel)this.ComboxBoxListePeriodicites.SelectedItem;

                OleDbCommand cmd = DatabaseManager.InsertInto("PostePeriodique",
                    DatabaseManager.GetConnection(),
                    new KeyValuePair<string, object>("codePoste", SelectedPoste.codePoste),
                    new KeyValuePair<string, object>("typePer", SelectedPeriode.codePer),
                    new KeyValuePair<string, object>("montant", montant),
                    new KeyValuePair<string, object>("jourDuMois", TousLesXDuMois)
                );
                
                try
                {
                    if (PosteRepository.IsAvailable(SelectedPoste.codePoste))
                    {
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();  // insert data

                        ErrorManager.EntriesSuccessfullyAdded(this);
                        ClearPosteFixeForm();
                    }
                    else
                    {
                        ErrorManager.ShowAlreadyUsedError(this, this.lblCmbPostes.Text);
                    }
                }
                catch (OleDbException ex)
                {
                    ErrorManager.HandleOleDBError(ex);
                }
                finally
                {
                    cmd.Connection.Close();
                }

                this.FillPostesComboBox();
            }

            // re-enable the submit button
            sender.Enabled = true;
        }

        private void btnGererPostes_Click(object sender, EventArgs e)
        {
            GererPostes InstanceGererPostes = new GererPostes();
            InstanceGererPostes.ShowDialog();

            // update the postes's ComboBox
            this.FillPostesComboBox();
        }

        private void TxtBoxTousLesXMois_KeyUp(object _s, KeyEventArgs e)
        {

            int val;
            MetroTextBox sender = (MetroTextBox)_s;
            if (!int.TryParse(sender.Text, out val))
            {
                errorProvider.SetError(lblDuMois, Program.settings.localize.Translate("err_not_a_valid_number"));
            }
            else
            {
                errorProvider.SetError(lblDuMois, (val > 0 && val < 29) ? null : Program.settings.localize.Translate("err_must_be_between_1_and_31"));
            }
        }
    }
}
