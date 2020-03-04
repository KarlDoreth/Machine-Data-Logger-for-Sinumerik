using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NDde.Client;
namespace Maschinendatenlogger_HMI
{
    public partial class Form1 : Form
    {
        DataTable Tabelle = new DataTable();
        System.IO.StreamWriter SW;
        DdeClient KarlsClient;
        int Preset = 0;
        public Form1()
        {
            InitializeComponent();
          }

    
        private void Abfrage()
        {
            try
            {
                String Requeststring = t_url.Text + "?";
                foreach (DataRow i in Tabelle.Rows)
                {
                    String Kanal = i[0].ToString();
                    i["Value"] = KarlsClient.Request(Kanal, 1000).TrimEnd('\0');                 
                }
                
                //Save to file wenn angewählt
                String Antwortstring = "";
                
                if (cb_toFile.Checked == true)
                {
                    foreach (DataRow i in Tabelle.Rows)
                    {
                        Antwortstring = Antwortstring + i["Value"] + ";"; 
                    }
                    Antwortstring = Antwortstring.TrimEnd(';');
                    SW.WriteLine(Antwortstring.Replace('.', ',').Replace('\r', ' ').Replace('\n', ' '));
                }
            }
            catch (Exception e)
            {
                timer1.Stop();
                MessageBox.Show("Fehler bei der Abfrage!\nBitte stoppen, dann alle Kanäle kontrollieren und erneut auf Start klicken!\n", "Fehler bei der Abfrage");

            }

        }
                 
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            if (checkBox1.Checked == true)
                {
                 //Getriggerte Abfrage
                String Test = KarlsClient.Request("/Channel/State/acProg", 1000).TrimEnd('\0');
                if (Test == "2")
                {
                    label2.Text = "Status: Trigger running";
                    Abfrage();
                }
                else
                {
                    label2.Text = "Status: Trigger waiting";
                }
                 
                }
            else
                {
                    //Ungetriggerte Abfrage
                    Abfrage();
                }
        }

        private void b_start_Click(object sender, EventArgs e)
        {
            
            //Refreshtime setzen
            timer1.Interval = Convert.ToInt32(T_Refreshtime.Text);
            //Dateispeicherung vorbereiten
            cb_toFile.Enabled = false;
            checkBox1.Enabled = false;
            dataGridView1.Enabled = false;
            T_Refreshtime.Enabled = false;
            label2.Text = "Status: Running";
            try
            {
                //KarlsClient = new DdeClient("myapp", "mytopic");
                KarlsClient = new DdeClient("ncdde", "machineswitch");
                KarlsClient.Connect();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (cb_toFile.Checked == true)
            {
                
                String Dateiname =  "Werte vom " +
                                    System.DateTime.Now.ToString().Replace(':','.') +
                                    ".csv";


                SW = new System.IO.StreamWriter(Dateiname);
                String Dateiheader = "";
                foreach (DataRow i in Tabelle.Rows)
                {
                    Dateiheader = Dateiheader + i[0] + ";";
                }
                SW.WriteLine(Dateiheader);
            }
             
            timer1.Start();
        }

        private void b_stop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            
            if (cb_toFile.Checked == true)
            {
                SW.Close();
            }
            cb_toFile.Enabled = true;
            checkBox1.Enabled = true;
            dataGridView1.Enabled = true;
            T_Refreshtime.Enabled = true;
            label2.Text = "Status: Stopped";
        }

        private void saveConf()
        {
            System.IO.StreamWriter SWConf = new System.IO.StreamWriter("config.txt");
            foreach (DataRow i in Tabelle.Rows)
            {
                String Zeilenstream ="";
                for (int j = 0; j < Tabelle.Columns.Count; j++)
                {
                    Zeilenstream = Zeilenstream + i[j].ToString() + ";";
                }
                SWConf.WriteLine(Zeilenstream.TrimEnd(';'));
            }
            SWConf.Close();
        }

        private void loadConf()
        {
            try
            {
                System.IO.StreamReader SRConf = new System.IO.StreamReader("config.txt");

                while (SRConf.EndOfStream == false)
                {
                    String Zeilenstream = SRConf.ReadLine();
                    Tabelle.Rows.Add(Zeilenstream.Split(';'));
                }
                SRConf.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Eine Konfiguration wurde nicht gefunden. \r Die Konfigurationsdatei wird neu erstellt!");
            }
        }

        private void loadPreset()
        {
            Tabelle.Clear();
            Preset++;
            string presetdateiname = "Preset" + Preset + ".txt";
            try
            {
                System.IO.StreamReader SRConf = new System.IO.StreamReader(presetdateiname);

                while (SRConf.EndOfStream == false)
                {
                    String Zeilenstream = SRConf.ReadLine();
                    Tabelle.Rows.Add(Zeilenstream.Split(';'));
                }
                SRConf.Close();
                label2.Text = "Status: Loaded " + presetdateiname;
            }
            catch (Exception)
            {
                label2.Text = "Status: " + presetdateiname + " not existing. Repress Button for Preset 1";
                Preset = 0;
            }
        }

        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            saveConf();
        }

        private void T_Refreshtime_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Tabelle einrichten
            Tabelle.Columns.Add("Channel", typeof(string));
            Tabelle.Columns.Add("Description", typeof(string));
            Tabelle.Columns.Add("Value", typeof(string));
            dataGridView1.DataSource = Tabelle;
            loadConf();
            timer1.Interval = Convert.ToInt32(T_Refreshtime.Text);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void b_close_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            String Test = e.KeyCode.ToString();

            if (e.Shift == true)
            {
                if (e.KeyCode.ToString() == "F1")
                {
                    b_start.PerformClick();
                    Activate();

                }
                if (e.KeyCode.ToString() == "F2")
                {
                    b_stop.PerformClick();
                }
                if (e.KeyCode.ToString() == "F3")
                {
                    if (cb_toFile.Checked)
                    {
                        cb_toFile.Checked = false;
                    }
                    else
                    {
                        cb_toFile.Checked = true;
                    }
                }
                if (e.KeyCode.ToString() == "F4")
                {
                    b_close.PerformClick();
                }
                if (e.KeyCode.ToString() == "F5")
                {
                    if (checkBox1.Checked)
                    {
                        checkBox1.Checked = false;
                    }
                    else
                    {
                        checkBox1.Checked = true;
                    }
                }
                if (e.KeyCode.ToString() == "F6")
                {
                    b_preset.PerformClick();
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        public Color Highlight { get; set; }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void b_preset_Click(object sender, EventArgs e)
        {
            loadPreset();
        }


    }
}
