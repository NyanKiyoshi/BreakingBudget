﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using MetroFramework.Controls;
using BreakingBudget.Repositories;
using BreakingBudget.Services.Lang;
using BreakingBudget.Services.SQL;

namespace BreakingBudget.Views.FrmMain
{
    partial class FrmMain
    {
        private void InitiliazePosteRevenu()
        {
            FillBeneficiaireList();
        }

        private void FillBeneficiaireList()
        {
            this.listBeneficiairesComboBox.Items.Clear();

            try {
                this.listBeneficiairesComboBox.Items.AddRange(PersonneRepository.List());
            } catch (OleDbException e)
            {
                ErrorManager.HandleOleDBError(e);
            }
        }

        private void ClearPosteRevenuForm()
        {
            this.txtBoxPosteRevenu.Text = string.Empty;
            this.txtBoxMontantRevenu.Text = string.Empty;
            this.txtTousLesXDuMoisRevenu.Text = string.Empty;

            // Reset the selected user
            this.listBeneficiairesComboBox.ResetText();
            this.listBeneficiairesComboBox.Refresh();
        }

        private void btnValiderRevenu_Click(object _s, EventArgs _ev)
        {

            OleDbConnection dbConn;
            OleDbTransaction dbTransaction;

            // Retrieve the budget's title + comments and remove any leading whitespacess
            string revenuPoste = this.txtBoxPosteRevenu.Text.Trim();
            decimal revenuMontant;
            int chaqueXDuMois;
            PersonneRepository.PersonneModel personne;

            // try to convert the user amount to a decimal and the day to a int,
            // if it fails, we trigger a "not a number" error and we stop proceeding
            if (!LocalizationManager.ConvertFloatingTo<decimal>(this.txtBoxMontantRevenu.Text, decimal.TryParse, out revenuMontant)
                ||  !this.isDayOfTheMonth(this.txtTousLesXDuMoisRevenu.Text, out chaqueXDuMois))
            {
                ErrorManager.ShowNotANumberError(this);
                return;
            }

            // - if: 
            //     - the poste is null/empty
            //     - or only has spaces:
            //     - or the beneficiary is not selected
            //   ---> show missing fields error and stop proceeding
            //
            // - otherwise: proceed and insert the data (if the poste does not exist yet)
            if (this.listBeneficiairesComboBox.SelectedItem == null
                || string.IsNullOrWhiteSpace(revenuPoste))
            {
                ErrorManager.ShowMissingFieldsError(this);
                return;
            }

            // retrieve the selected beneficiary
            personne = (PersonneRepository.PersonneModel)listBeneficiairesComboBox.SelectedItem;

            // check if the budgetTitle is unique in the database
            // if not unique: show an error saying that it already exists and stop proceeding
            try {
                if (!PosteRepository.IsUnique(revenuPoste))
                {
                    // show a duplicate value error and specify the field
                    ErrorManager.ShowDuplicateError(this,
                        Program.settings.localize.Translate(this.lblPosteRevenu.Name));
                    return;
                }
            } catch (OleDbException ex)
            {
                ErrorManager.HandleOleDBError(ex);
                return;
            }

            // otherwise: continue and insert the data
            dbConn = DatabaseManager.GetConnection();

            dbConn.Open();

            dbTransaction = dbConn.BeginTransaction();

            // Insert the data to the data base
            try
            {
                PosteRevenuRepository.Create(dbConn, dbTransaction,
                    revenuPoste,
                    personne,
                    revenuMontant,
                    chaqueXDuMois
                );

                Console.WriteLine("<- Commit");
                dbTransaction.Commit();

                ErrorManager.EntriesSuccessfullyAdded(this);
                ClearPosteRevenuForm();
            }
            catch (OleDbException e)
            {
                // cancel the changes
                dbTransaction.Rollback();

                // handle the error (log it and report it to the user)
                ErrorManager.HandleOleDBError(e);
            }
            catch (ArgumentException e)
            {
                ErrorManager.ShowOperationFailed(this, Program.settings.localize.Translate(e.Message));
            }
            finally
            {
                dbConn.Close();
            }
        }

        private void btnAjouterBeneficiaireRevenu_Click(object sender, EventArgs e)
        {
            if ((new UserCreation()).ShowDialog() == DialogResult.OK)
            {
                FillBeneficiaireList();
            }
        }
    }
}
