using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace Lab1
{
    public partial class Form1 : Form
    {
        DataSet myDataSet = new DataSet();
        BindingSource bindingSourceDGV1 = new BindingSource();
        BindingSource bindingSourceDGV2 = new BindingSource();
        DataRelation relation;
        SqlCommandBuilder cb = new SqlCommandBuilder();
        SqlCommand updateCmd;
        SqlCommand insertCmd;
        SqlCommand deleteCmd;
        SqlDataAdapter adapter = new SqlDataAdapter();
        string connectionString = "Data Source=DESKTOP-52IPY73; Initial Catalog=Gallery;" + "Integrated Security=true";
        
        
        public Form1()
        {
            InitializeComponent();
            disableButtons();
            disableTextBoxes();
        }

        //Clears all the textBoxes for an easier use
        private void enableTextBoxes()
        {
            idTB.Enabled = true;
            nameTB.Enabled = true;
            yearTB.Enabled = true;
            idMalerTB.Enabled = true;
            galleryIDTB.Enabled = true;
        }
        private void disableTextBoxes()
        {
            idTB.Enabled = false;
            nameTB.Enabled = false; 
            yearTB.Enabled = false;
            idMalerTB.Enabled = false;
            galleryIDTB.Enabled = false;
        }
        private void clearTextBoxes()
        {
            idTB.Clear();
            nameTB.Clear();
            yearTB.Clear();
            idMalerTB.Clear();
            galleryIDTB.Clear();
        }

        private void disableButtons()
        {
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;

        }

        //helps setting some boundaries for the User
        //If the User wants to add a child to the parent ( action that happens when clicking on a row in dataGridView1 ) then the parentID TextBox should be readOnly + the galleryID TextBox should be filled in by the User
        //If the User wants to update or delete a child ( action that happens when clicking on a row in dataGridView2 ) then the childID  TextBox should remain the same --> readOnly + the galleryID TextBox should remain the same
        private void toggleTextBoxAccessibility(string msg)
        {
            

            if (msg== "dataGridView1")
            {
                this.idMalerTB.ReadOnly = true;
                this.idTB.ReadOnly = false;
                galleryIDTB.ReadOnly = false;
            }
            else if( msg == "dataGridView2")
            {
                this.idMalerTB.ReadOnly = false;
                this.idTB.ReadOnly = true;
                galleryIDTB.ReadOnly = true;
            }
        }

        //when button clicked with the help of a dataSet load the infos from the database onto the datagridview
        private void connectButton_Click(object sender, EventArgs e)
        {
            //to not have any unexpected surprises disable the update, delete, insert button
            disableButtons();
           

            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {
                // create a SqlDataAdapter for the "Maler" table and use it to fill the dataSet
                SqlDataAdapter malerAdapter = new SqlDataAdapter();
                malerAdapter.SelectCommand = new SqlCommand("SELECT * FROM Maler", connection);

                //Fill the dataSet with a dataTable named "Maler"
                //Name given to differentiate the two tables that will be in the dataSet
                myDataSet.Clear();
                malerAdapter.Fill(myDataSet, "Maler");

                // Create a SqlDataAdapter for the "Bilder" table and use it to fill the dataSet
                SqlDataAdapter bilderAdapter = new SqlDataAdapter();
                bilderAdapter.SelectCommand = new SqlCommand("SELECT * FROM Bilder", connection);

                //Fill the dataSet with a dataTable named "Bilder"
                //Name given to differentiate the two tables that will be in the dataSet
                bilderAdapter.Fill(myDataSet, "Bilder");
                cb = new SqlCommandBuilder(bilderAdapter);

                //Start building a dataRelation between the two dataTables
                //Get the DataColumn objs from the two DataTables from myDataSet 
                DataColumn parentColumn = myDataSet.Tables["Maler"].Columns["IDMaler"];
                DataColumn childColumn = myDataSet.Tables["Bilder"].Columns["IDMaler"];

                //Build a DataRelation based on those matching columns
                relation = new DataRelation("FK_Bilder_Maler", parentColumn, childColumn);

                //try to add the DataRelation to the DataSet
                try
                {
                    myDataSet.Relations.Add(relation);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                //bind the parent dataGridView to the bindingSource
                if (myDataSet.Tables[0].Rows.Count != 0)
                {
                    
                    bindingSourceDGV1.DataSource = myDataSet;
                    bindingSourceDGV1.DataMember = "Maler";
                    dataGridView1.DataSource = bindingSourceDGV1;
                }

            }
        }
       
            
       
        //Show the child tables of the chosen row from the parent dataGridView only when clicking  
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            toggleTextBoxAccessibility("dataGridView1");
            
            clearTextBoxes();
            disableButtons();
            enableTextBoxes();
            //bind the child dataGridView to the bindingSource
            //the dataSource for the second bindingSource is the bindingSource of the parent dataTable
            bindingSourceDGV2.DataSource = bindingSourceDGV1;
            //Assign to the dataGridView of the child its bindingSource 
            dataGridView2.DataSource = bindingSourceDGV2;
            //The relation is global and was defined when pressing the connect button
            bindingSourceDGV2.DataMember = "FK_Bilder_Maler"; 
            
            string idMaler = dataGridView1.CurrentRow.Cells[0].Value.ToString();

            if (idMaler != null)
            {
               //once the painter (parent) is chosen and his id is valid the idMaler TextBox is filled in with his id and after is disabled to avoid any changing of the id
                this.idMalerTB.Text =idMaler;
                this.button4.Enabled = true;



 

            }
        }

        //Gets the values of the row and writes them in their assigned TextBoxes on Cell_Click on the dataGridView of the child Table 
        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            toggleTextBoxAccessibility("dataGridView2");

            clearTextBoxes();
            if (dataGridView1.SelectedRows.Count > 0)
            {
                
                string id = dataGridView2.CurrentRow.Cells[0].Value.ToString();
                string name = dataGridView2.CurrentRow.Cells[1].Value.ToString(); 
                string malerID = dataGridView2.CurrentRow.Cells[2].Value.ToString(); 
                string year = dataGridView2.CurrentRow.Cells[3].Value.ToString();
                string galleryID = dataGridView2.CurrentRow.Cells[4].Value.ToString();
                //If id chosen valid -->  fill all the textBoxes with the data from the selected row turned into strings 
                if (id != null)
                {
                    this.idTB.Text = id;
                    this.nameTB.Text = name; 
                    this.yearTB.Text = year;
                    this.idMalerTB.Text = malerID;
                    this.galleryIDTB.Text = galleryID;
                    
                   // this.idTB.Enabled = false; // to not set a different ID for our child element


                }
            }
            button2.Enabled = true;
            //update button was enabled
            button3.Enabled = true;
            //delete button was enabled
            button4.Enabled = false;
            //insert button was disabled


        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            
            SqlConnection connection = new SqlConnection(connectionString);
            //create an SqlCommand to update the database tables
            updateCmd = new SqlCommand("UPDATE Bilder SET NAME=@NAME, JAHR=@JAHR, IDMALER = @IDMALER, IDGALLERY=@GALLERYID  where IDBILD = @ID", connection);
            //set the UpdateCommand of the adapter to the created SqlCommand
            adapter.UpdateCommand = updateCmd; 
            //set the values to the parameters of the UpdateCommand to the input from the TextBoxes
            adapter.UpdateCommand.Parameters.Add("@NAME", SqlDbType.NChar).Value = nameTB.Text.ToString();
            adapter.UpdateCommand.Parameters.Add("@JAHR", SqlDbType.Int).Value = Convert.ToInt32(yearTB.Text);
            adapter.UpdateCommand.Parameters.Add("@IDMALER", SqlDbType.Int).Value = Convert.ToInt32(idMalerTB.Text);
            adapter.UpdateCommand.Parameters.Add("@ID", SqlDbType.Int).Value = Convert.ToInt32(idTB.Text);
            adapter.UpdateCommand.Parameters.Add("@GALLERYID", SqlDbType.Int).Value = Convert.ToInt32(galleryIDTB.Text);

            //Connect to the database and make the changes
            connection.Open();
            adapter.UpdateCommand.ExecuteNonQuery();
            connection.Close();

            clearTextBoxes();
            
            MessageBox.Show("Database updated!", "INFO");

        }

        private void deleteButton_Click(object sender, EventArgs e)
        {

            
            SqlConnection connection = new SqlConnection(connectionString);
            //create an SqlCommand to delete the database tables
            deleteCmd = new SqlCommand("DELETE FROM Bilder where IDBILD = @ID", connection);
            //set the DeleteCommand of the adapter to the created SqlCommand
            adapter.DeleteCommand = deleteCmd;

            //set the parameter of the adapters DeleteCommand to the ID of the element to be deleted
            adapter.DeleteCommand.Parameters.Add("@ID", SqlDbType.Int).Value = Convert.ToInt32(this.idTB.Text);
            
            //Connect to the database and make the changes
            connection.Open();
            adapter.DeleteCommand.ExecuteNonQuery();
            connection.Close();

            clearTextBoxes();
            MessageBox.Show("Database updated\n element deleted!", "INFO");

        }

        private void insertButton_Click(object sender, EventArgs e)
        {

            
            SqlConnection connection = new SqlConnection(connectionString);
            
            //create an SqlCommand to delete the database tables
            insertCmd = new SqlCommand("INSERT INTO Bilder VALUES(@ID, @NAME, @IDMALER, @JAHR, @GALLERYID)", connection);
            //set the DeleteCommand of the adapter to the created SqlCommand
            adapter.InsertCommand = insertCmd;
            
            //verify the ID TextBox input for it to be made only out of digits 
            string tString = idTB.Text;
          

            //verify ID TextBox input not to be already in the childTable ID column
            foreach (DataRow row in myDataSet.Tables[1].Rows)
                if ((int)row["idBild"] == Convert.ToInt32(idTB.Text))
                {
                    MessageBox.Show("!ID ALREADY IN DATABASE!\nPLEASE TRY AGAIN WITH A DIFFERENT VALUE!", "INFO");
                    clearTextBoxes();
                    return;
                }

            //set the values to the parameters of the InsertCommand to the input from the TextBoxes
            adapter.InsertCommand.Parameters.Add("@NAME", SqlDbType.NChar).Value = nameTB.Text.ToString();
            adapter.InsertCommand.Parameters.Add("@JAHR", SqlDbType.Int).Value = Convert.ToInt32(yearTB.Text);
            adapter.InsertCommand.Parameters.Add("@IDMALER", SqlDbType.Int).Value = Convert.ToInt32(idMalerTB.Text);
            adapter.InsertCommand.Parameters.Add("@ID", SqlDbType.Int).Value = Convert.ToInt32(idTB.Text);
            adapter.InsertCommand.Parameters.Add("@GALLERYID", SqlDbType.Int).Value = Convert.ToInt32(galleryIDTB.Text);
            
            //Connect to the database and make the changes
            connection.Open();
            adapter.InsertCommand.ExecuteNonQuery();
            connection.Close();

            clearTextBoxes();
            MessageBox.Show("Database updated\n element added!", "INFO");
            
        }

        private void updateTablesButton_Click(object sender, EventArgs e)
        {
            //to not have any unexpected surprises disable the update, delete, insert button
            disableButtons();

            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {
                // create a SqlDataAdapter for the "Maler" table and use it to fill the dataSet
                SqlDataAdapter malerAdapter = new SqlDataAdapter();
                malerAdapter.SelectCommand = new SqlCommand("SELECT * FROM Maler", connection);

                //Fill the dataSet with a dataTable named "Maler"
                //Name given to differentiate the two tables that will be in the dataSet
                myDataSet.Clear();
                malerAdapter.Fill(myDataSet, "Maler");

                // Create a SqlDataAdapter for the "Bilder" table and use it to fill the dataSet
                SqlDataAdapter bilderAdapter = new SqlDataAdapter();
                bilderAdapter.SelectCommand = new SqlCommand("SELECT * FROM Bilder", connection);

                //Fill the dataSet with a dataTable named "Bilder"
                //Name given to differentiate the two tables that will be in the dataSet
                bilderAdapter.Fill(myDataSet, "Bilder");
                cb = new SqlCommandBuilder(bilderAdapter);

                //Start building a dataRelation between the two dataTables
                //Get the DataColumn objs from the two DataTables from myDataSet 

                //bind the parent dataGridView to the bindingSource
                if (myDataSet.Tables[0].Rows.Count != 0)
                {

                    bindingSourceDGV1.DataSource = myDataSet;
                    bindingSourceDGV1.DataMember = "Maler";
                    dataGridView1.DataSource = bindingSourceDGV1;
                }

            }
        }


        //functions to check if the input in the textBoxes is valid
        //show a MessageBox Msg if not
        private void idTB_TextChanged(object sender, EventArgs e)
        {
            string tString = idTB.Text;
            if (tString.Trim() == "") return;

            for (int i = 0; i < tString.Length; i++)
            {
                if (!char.IsNumber(tString[i]))
                {
                    MessageBox.Show("!ONLY DIGITS ALLOWED!");
                    idTB.Clear();
                    return;
                }
            }
           
        }
        private void yearTB_TextChanged(object sender, EventArgs e)
        {
           string tString = yearTB.Text;
           if (tString.Trim() == "") return;


           for (int i = 0; i < tString.Length; i++)
           {
                if (!char.IsNumber(tString[i]))
                {
                    MessageBox.Show("!ONLY DIGITS ALLOWED!");
                    yearTB.Clear();
                    return;
                }
           }

            if (tString.Length > 4)
            {
                MessageBox.Show("!YEAR CANNOT CONTAIN MORE THAN 4 DIGITS!");
                yearTB.Clear();
            }

            else if (Convert.ToInt32(tString) > 2022)
            {
                MessageBox.Show("Please enter a valid year!");
                yearTB.Clear();
            }
        }

        private void nameTB_TextChanged(object sender, EventArgs e)
        {
            string tString = nameTB.Text;
            if (tString.Trim() == "") return;

            for (int i = 0; i < tString.Length; i++)
            {
                if (char.IsNumber(tString[i]))
                {
                    MessageBox.Show("!ONLY LETTERS ALLOWED!");
                    nameTB.Clear();
                    return;
                }
            }
        }

        private void galleryIDTB_TextChanged(object sender, EventArgs e)
        {
            string tString = idTB.Text;
            if (tString.Trim() == "") return;

            for (int i = 0; i < tString.Length; i++)
            {
                if (!char.IsNumber(tString[i]))
                {
                    MessageBox.Show("!ONLY DIGITS ALLOWED!");
                    idTB.Clear();
                    return;
                }
            }
        }
    }
}


