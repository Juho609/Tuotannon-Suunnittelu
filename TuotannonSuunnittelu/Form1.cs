using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;

namespace TuotannonSuunnittelu
{
    public partial class Form1 : Form
    {
        private SqlConnection sqlConnection;
        private string user = "demouser";
        private string password = "Demo2017";
        private string connectionString;

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectionString = ("user id=" + user + ";" +
                                       "password=" + password +"; server=vbrekry.database.windows.net;" +
                                       "database=ennakko_tammi2017; " +
                                       "connection timeout=30");

            // Loads data into the 'ennakko_tammi2017DataSet.resepti'.
            this.reseptiTableAdapter.Fill(this.ennakko_tammi2017DataSet.resepti);
            // Loads data into the 'ennakko_tammi2017DataSet.myynti'.
            this.myyntiTableAdapter.Fill(this.ennakko_tammi2017DataSet.myynti);

            RefreshProductPageMainArea();
            RefreshWarehousePageMainArea();
            RefreshPlanningPageMainArea();
        }
        
        private void refreshProductPageButton_Click(object sender, EventArgs e)
        {
            RefreshProductPageMainArea();
        }
        
        private void refreshWarehouseButton_Click(object sender, EventArgs e)
        {
            RefreshWarehousePageMainArea();
        }

        private void refreshPlanningPageButton_Click(object sender, EventArgs e)
        {
            RefreshPlanningPageMainArea();
        }
        
        private void buttonSearchItem_Click(object sender, EventArgs e)
        {
            searchItemFromGrid(dataGridViewProduct1, textBoxSearchItem.Text, comboBoxSearch.SelectedIndex);
        }

        private void buttonSearchItem1_Click(object sender, EventArgs e)
        {
            searchItemFromGrid(dataGridViewPlanning1, textBoxSearchItem1.Text, comboBoxSearch1.SelectedIndex);
        }

        private void dataGridViewProduct1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dataGrid = sender as DataGridView;
            if (dataGrid == null) { return; }

            if (dataGrid.CurrentRow.Selected)
            {
                RefreshProductPageTopRightTextBox();
                RefreshProductPageBottomRightListView();
            }            
        }

        private void buttonCalculateNeeds_Click(object sender, EventArgs e)
        {
            RefreshPlanningPageTopRightListView();
            RefreshPlanningPageBottomRightListView();
        }

        /// <summary>
        /// Refreshes product page DataGridView. View includes product info (ID, name, balance, unit).
        /// </summary>
        private void RefreshProductPageMainArea()
        {
            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                dataGridViewProduct1.DataSource = null;
                dataGridViewProduct1.Rows.Clear();

                SqlCommand command = new SqlCommand(
                  "SELECT tuote.id, tuote.nimi, saldo.eraId, saldo.saldo, tuote.yksikko " +
                  "FROM tuote " +
                  "INNER JOIN saldo " +
                  "ON tuote.id=saldo.tuoteId " +
                  "ORDER BY tuote.id;",
                  sqlConnection);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        
                        row.CreateCells(dataGridViewProduct1,
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetInt32(2),
                            ((float)(double)reader[3]),
                            reader.GetString(4));
                        
                        dataGridViewProduct1.Rows.Add(row);
                    }                 
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Refreshes product page top right ListView. View includes recipe info (ID, name, balance, unit).
        /// </summary>
        private void RefreshProductPageTopRightTextBox()
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridViewProduct1.SelectedRows;
            DataGridViewRow selectedRow = null;

            if (selectedRows.Count != 1) { return; }
            selectedRow = selectedRows[0];

            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                listViewProduct1.Items.Clear();

                SqlCommand command = new SqlCommand(
                    "SELECT resepti.resepti, resepti.item, resepti.maara, tuote.yksikko, tuote.nimi " +
                    "FROM resepti " +
                    "INNER JOIN tuote " +
                    "ON resepti.item=tuote.id " +
                    "WHERE resepti.resepti=" + (int)selectedRow.Cells[0].Value +
                    ";", sqlConnection);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ListViewItem listItem = new ListViewItem(reader.GetInt32(0).ToString());
                        listItem.SubItems.Add(reader.GetString(4));
                        listItem.SubItems.Add(((float)(double)reader[2]).ToString());
                        listItem.SubItems.Add(reader.GetString(3));

                        listViewProduct1.Items.Add(listItem);
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Refreshes product page bottom right ListView. View includes product balance info (batch, balance).
        /// </summary>
        private void RefreshProductPageBottomRightListView()
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridViewProduct1.SelectedRows;
            DataGridViewRow selectedRow = null;

            if (selectedRows.Count != 1) { return; }
            selectedRow = selectedRows[0];

            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                listViewProduct2.Items.Clear();

                SqlCommand command = new SqlCommand(
                  "SELECT eraId, saldo FROM saldo WHERE tuoteId=" + selectedRow.Cells[0].Value + ";",
                  sqlConnection);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ListViewItem listItem = new ListViewItem(reader.GetInt32(0).ToString());
                        listItem.SubItems.Add(((float)(double)reader[1]).ToString());

                        listViewProduct2.Items.Add(listItem);
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Refreshes warehouse page ListView. View includes warehouse info ( ID, warehouse, batch, balance).
        /// </summary>
        private void RefreshWarehousePageMainArea()
        {
            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                //listViewWarehouse1.Items.Clear();
                dataGridViewWarehouse1.DataSource = null;
                dataGridViewWarehouse1.Rows.Clear();

                SqlCommand command = new SqlCommand(
                  "SELECT varasto.id, varasto.nimi, saldo.eraId, saldo.saldo, tuote.yksikko FROM varasto " +
                  "INNER JOIN saldo " +
                  "ON varasto.id=saldo.varastoId " +
                  "INNER JOIN tuote " + 
                  "ON saldo.tuoteId=tuote.id " +
                  "ORDER BY saldo.eraId;",
                  sqlConnection);

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        DataGridViewRow row = new DataGridViewRow();

                        row.CreateCells(dataGridViewProduct1,
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetInt32(2),
                            ((float)(double)reader[3]),
                            reader.GetString(4));

                        dataGridViewWarehouse1.Rows.Add(row);
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                reader.Close();
            }
        }           

        /// <summary>
        /// Refreshes planning page GridView. Grid includes basic product info (product ID, product name).
        /// </summary>
        private void RefreshPlanningPageMainArea()
        {
            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                dataGridViewPlanning1.DataSource = null;
                dataGridViewPlanning1.Rows.Clear();

                SqlCommand command = new SqlCommand(
                  "SELECT tuote.id, tuote.nimi FROM tuote INNER JOIN resepti ON tuote.id=resepti.resepti;",
                  sqlConnection);

                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        DataGridViewRow row = new DataGridViewRow();

                        row.CreateCells(dataGridViewPlanning1,
                            reader.GetInt32(0),
                            reader.GetString(1));

                        dataGridViewPlanning1.Rows.Add(row);
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Refreshes planning page ListView1. View includes product creation requirements (product name, amount, unit, batch).
        /// </summary>
        private void RefreshPlanningPageTopRightListView()
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridViewPlanning1.SelectedRows;
            DataGridViewRow selectedRow = null;

            if (selectedRows.Count != 1 || textBoxNeededAmount.Text == "") { return; }
            selectedRow = selectedRows[0];

            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                listViewPlanning1.Items.Clear();

                SqlCommand command = new SqlCommand(
                    "SELECT resepti.resepti, resepti.item, resepti.maara, tuote.yksikko, tuote.nimi, saldo.eraId " +
                    "FROM resepti " +
                    "INNER JOIN tuote " +
                    "ON resepti.item=tuote.id " +
                    "INNER JOIN saldo " +
                    "ON tuote.id=saldo.tuoteId " +
                    "WHERE resepti.resepti=" + (int)selectedRow.Cells[0].Value +
                    ";", sqlConnection);

                Debug.WriteLine((int)selectedRow.Cells[0].Value);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ListViewItem listItem = new ListViewItem(reader.GetString(4));
                        listItem.SubItems.Add(((float)(double)reader[2] * int.Parse(textBoxNeededAmount.Text)).ToString());
                        listItem.SubItems.Add(reader.GetString(3));
                        listItem.SubItems.Add(reader.GetInt32(5).ToString());

                        listViewPlanning1.Items.Add(listItem);
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Refreshes planning page ListView2. View includes product salehistory (amount, unit, date).
        /// </summary>
        private void RefreshPlanningPageBottomRightListView()
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridViewPlanning1.SelectedRows;
            DataGridViewRow selectedRow = null;

            int salesMade = 0;
            int salesAmount = 0;

            if (selectedRows.Count != 1) { return; }
            selectedRow = selectedRows[0];

            using (sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                listViewPlanning2.Items.Clear();

                SqlCommand command = new SqlCommand(
                  "SELECT myynti.id, myynti.asiakas, myynti.tuote, myynti.pvm, myynti.maara, tuote.nimi, tuote.yksikko " + 
                  "FROM myynti " + 
                  "INNER JOIN tuote ON myynti.tuote=tuote.id " + 
                  "WHERE tuote=" + selectedRow.Cells[0].Value + 
                  " ORDER BY myynti.pvm ASC;",
                  sqlConnection);

                SqlDataReader reader = command.ExecuteReader();
                
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ListViewItem listItem = new ListViewItem(reader.GetInt32(4).ToString());
                        listItem.SubItems.Add(reader.GetString(6));
                        listItem.SubItems.Add(reader.GetDateTime(3).ToShortDateString());

                        salesAmount += reader.GetInt32(4);
                        salesMade++;

                        listViewPlanning2.Items.Add(listItem);
                    }
                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                reader.Close();
            }

            if (salesMade > 0)
            {
                textBoxCalculatedAmount.Text = (salesAmount / salesMade).ToString("0");
            }          
        }
                                     
        private void searchItemFromGrid(DataGridView data, string searchValue, int columnIndex = -1)
        {
            try
            {
                if (columnIndex == -1) { return; }

                int rowIndex = -1;
                foreach (DataGridViewRow row in data.Rows)
                {
                    if (row.Cells[columnIndex].Value.ToString().Contains(searchValue))
                    {
                        rowIndex = row.Index;
                        break;
                    }
                }
                if (rowIndex > -1)
                {
                    data.ClearSelection();
                    data.Rows[rowIndex].Selected = true;
                    data.FirstDisplayedScrollingRowIndex = rowIndex;
                    data.Focus();
                }
            }
            catch 
            {
                Debug.WriteLine("Item not found");
            }           
        }                   
    }
}
