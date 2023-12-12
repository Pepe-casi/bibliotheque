using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    public partial class Frm_Livre : Form
    {
        private static string dbCommand = "";
        private static BindingSource bindingSrc;
        private static string dbPath = Application.StartupPath + "\\" + "bibliotheque.db;";
        private static string conString = "Data Source=" + dbPath + "Version=3;New=False;Compress=True;";

        private static SQLiteConnection connection = new SQLiteConnection(conString);
        private static SQLiteCommand command = new SQLiteCommand("", connection);
        private static string sql;

        private static Boolean nouveau = false;

        public Frm_Livre()
        {
            InitializeComponent();
        }

        private void ViderChamps()
        {
            txt_ISBN.Text = string.Empty;
            txt_Titre.Text = string.Empty;
            txt_Annee.Text = string.Empty;
            txt_Genre.Text = string.Empty;

            cb_Auteur.Text = string.Empty;

            dataGridView1.ClearSelection();
        }

        private void AddCmdParameters()
        {
            command.Parameters.Clear();
            command.CommandText = sql;

            command.Parameters.AddWithValue("ISBN", txt_ISBN.Text.Trim());
            command.Parameters.AddWithValue("Title", txt_Titre.Text.Trim());
            command.Parameters.AddWithValue("Genre", txt_Genre.Text.Trim());
            command.Parameters.AddWithValue("PublicationYear", txt_Annee.Text.Trim());
            command.Parameters.AddWithValue("AuthorId", cb_Auteur.SelectedValue);
        }

        private void ChargerDonneesAuthor(SQLiteCommand cmd = null)
        {
            try
            {
                sql = "SELECT * FROM Authors ORDER BY FirstName ASC;";
                if (cmd != null)
                {
                    command = cmd;
                }
                else
                {
                    command.CommandText = sql;
                }

                SQLiteDataAdapter da = new SQLiteDataAdapter(command);
                DataSet ds = new DataSet();
                da.Fill(ds, "Authors");

                bindingSrc = new BindingSource();
                bindingSrc.DataSource = ds.Tables["Authors"];

                cb_Auteur.DisplayMember = "FirstName";
                cb_Auteur.ValueMember = "AuthorId";

                cb_Auteur.DataSource = bindingSrc;
            }
            catch (Exception ex)
            {

                MessageBox.Show("Erreur de chargement : " + ex.Message.ToString(), "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChargerDonneesLivre(SQLiteCommand cmd = null)
        {
            try
            {
                sql = "SELECT ISBN, Title, Genre, PublicationYear, FirstName, b.AuthorId " +
                    "FROM Books b INNER JOIN Authors a ON a.AuthorId = b.AuthorId ORDER BY ISBN ASC;";
                if(cmd != null)
                {
                    command = cmd;
                }
                else
                {
                    command.CommandText = sql;
                }

                SQLiteDataAdapter da = new SQLiteDataAdapter(command);
                DataSet ds = new DataSet();
                da.Fill(ds, "Books");

                bindingSrc = new BindingSource();
                bindingSrc.DataSource = ds.Tables["Books"];

                txt_ISBN.DataBindings.Clear();
                txt_Titre.DataBindings.Clear();
                txt_Genre.DataBindings.Clear();
                txt_Annee.DataBindings.Clear();
                cb_Auteur.DataBindings.Clear();

                //Databinding
                txt_ISBN.DataBindings.Add("Text", bindingSrc, "ISBN");
                txt_Titre.DataBindings.Add("Text", bindingSrc, "Title");
                txt_Genre.DataBindings.Add("Text", bindingSrc, "Genre");
                txt_Annee.DataBindings.Add("Text", bindingSrc, "PublicationYear");
                cb_Auteur.DataBindings.Add("Text", bindingSrc, "FirstName");

                dataGridView1.DataSource = bindingSrc;

                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
            catch (Exception ex)
            {

                MessageBox.Show("Erreur d'affichage : " + ex.Message.ToString(), "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BloquerDebloquer(Boolean etat)
        {
            txt_ISBN.ReadOnly = etat;
            txt_Titre.ReadOnly = etat;
            txt_Genre.ReadOnly = etat;
            cb_Auteur.Enabled = !etat;

            btnAnnuler.Visible = !etat;
            btnEnregistrer.Visible = !etat;
            btnModifier.Visible = etat;
            btnNouveau.Visible = etat;
            btnSupprimer.Enabled = etat;

            dataGridView1.Enabled = etat;
        }

        private void btnNouveau_Click(object sender, EventArgs e)
        {
            nouveau = true;
            ViderChamps();
            BloquerDebloquer(false);
        }

        private void btnModifier_Click(object sender, EventArgs e)
        {
            nouveau = false;
            BloquerDebloquer(false);
            txt_ISBN.ReadOnly = !nouveau;
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(txt_ISBN.Text.Trim()) 
                || String.IsNullOrEmpty(txt_Titre.Text.Trim()) || 
                String.IsNullOrEmpty(txt_Genre.Text.Trim()) || 
                String.IsNullOrEmpty(txt_Annee.Text.Trim()))
            {
                MessageBox.Show("Renseignez les champs obligatoires svp", "Bibliothèque", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            openConnection();

            try
            {
                if (nouveau)
                {
                    dbCommand = "INSERT";
                    sql = "INSERT INTO Books(ISBN, Title, AuthorId, Genre, PublicationYear) " +
                        "VALUES(@ISBN, @Title, @AuthorId, @Genre, @PublicationYear)";
                }
                else
                {
                    dbCommand = "UPDATE";
                    sql = "UPDATE Books SET Title = @Title, Genre = @Genre, PublicationYear = @PublicationYear, " +
                        "AuthorId = @AuthorId WHERE ISBN = @ISBN";
                }

                AddCmdParameters();

                int executeResult = command.ExecuteNonQuery();
                if(executeResult == -1)
                {
                    MessageBox.Show("Livre non enregistré ", "Bibliothèque", 
                        MessageBoxButtons.OK, MessageBoxIcon.Stop); 
                }
                else
                {
                    MessageBox.Show(nouveau ? "Enregistrement réussi avec succès." : "Modification réussie avec succès", "Bibliothèque", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ViderChamps();
                    BloquerDebloquer(true);
                    ChargerDonneesLivre();
                }
            } 
            catch(Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message.ToString(), "Bibliothèque", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbCommand = "";
                closeConnection();
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            nouveau = false;
            ViderChamps();
            BloquerDebloquer(true);
            ChargerDonneesLivre();
        }

        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            if(txt_ISBN.Text.Trim() == "" || String.IsNullOrEmpty(txt_ISBN.Text.Trim())) { 
                MessageBox.Show("Sélectionnez un élément dans la liste svp", "Bibliothèque (Suppression)", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return; 
            }

            openConnection();

            try
            {
                if(MessageBox.Show("ID : " + txt_ISBN.Text.Trim() + " -- Voulez-vous vraiment supprimer cet élément ?", "Bibliothèque (Suppression)",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                dbCommand = "DELETE";
                sql = "DELETE FROM Books WHERE ISBN = @ISBN";

                command.Parameters.Clear();
                command.CommandText = sql;
                command.Parameters.AddWithValue("ISBN", txt_ISBN.Text.Trim());

                int executeResult = command.ExecuteNonQuery();
                if(executeResult == 1)
                {
                    MessageBox.Show("Suppression effectuée avec succès", "Bibliothèque (Suppression)",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ChargerDonneesLivre();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message.ToString(), "Message d'erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dbCommand = "";
                closeConnection();
            }
        }

        private void Frm_Livre_Load(object sender, EventArgs e)
        {
            BloquerDebloquer(true);
            openConnection();
            ChargerDonneesLivre();
            ChargerDonneesAuthor();
            closeConnection();
        }

        private void closeConnection()
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
                // MessageBox.Show("La connexion est: " + connection.State.ToString());
            }
        }

        private void openConnection()
        {
            if(connection.State == ConnectionState.Closed)
            {
                connection.Open();
                // MessageBox.Show("La connexion est: " + connection.State.ToString());
            }
        }
    }
}
