using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace MachineDataReader
{
    public partial class Form1 : Form
    {
        DataTable Tabelle = new DataTable();
        System.IO.StreamWriter SW;

        public Form1()
        {
            InitializeComponent();
            Tabelle.Columns.Add("Channel", typeof(string));
            Tabelle.Columns.Add("Description", typeof(string));
            Tabelle.Columns.Add("Value", typeof(string));
            dataGridView1.DataSource = Tabelle;
            loadConf();
            timer1.Interval = Convert.ToInt32(T_Refreshtime.Text);
          }

        private void b_Refreshtime_Click(object sender, EventArgs e)
        {
            
        }

        private void Abfrage()
        {
            try
            {
                String Requeststring = t_url.Text + "?";
                foreach (DataRow i in Tabelle.Rows)
                {
                    Requeststring = Requeststring + "&request=" + i[0];
                }
                String Antwortstring = HTTPAbfrage(Requeststring);
                String[] Antwortarray = Antwortstring.Split(';');

                //Tabelle füllen;
                int j = 0;
                foreach (DataRow i in Tabelle.Rows)
                {
                    i["Value"] = Antwortarray[j];
                    j++;
                }

                //Save to file wenn angewählt
                if (cb_toFile.Checked == true)
                {
                    SW.WriteLine(Antwortstring.Replace('.', ','));
                }
            }
            catch (Exception e)
            {
                timer1.Stop();
                MessageBox.Show("Fehler bei der Abfrage!\nBitte stoppen, dann alle Kanäle kontrollieren und erneut auf Start klicken!\n", "Fehler bei der Abfrage");
                
            }

        }
                
        string HTTPAbfrage(string requeststring)
        {
            HttpWebRequest Anfrage = (HttpWebRequest)WebRequest.Create(requeststring);
            HttpWebResponse Antwort = (HttpWebResponse)Anfrage.GetResponse();
            System.IO.Stream Antwort_Stream = Antwort.GetResponseStream();
            System.IO.StreamReader Antwort_Streamreader = new System.IO.StreamReader(Antwort_Stream);
            String Ausgabe = Antwort_Streamreader.ReadToEnd();
            return Ausgabe;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Abfrage();
        }

        private void b_start_Click(object sender, EventArgs e)
        {
            //Refreshtime setzen
            timer1.Interval = Convert.ToInt32(T_Refreshtime.Text);
            //Dateispeicherung vorbereiten
            cb_toFile.Enabled = false;
            dataGridView1.Enabled = false;
            T_Refreshtime.Enabled = false;

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
            dataGridView1.Enabled = true;
            T_Refreshtime.Enabled = true;
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

        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            saveConf();
        }

        private void T_Refreshtime_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            dataGridView1.Width = this.Width - 40;
            dataGridView1.Height = this.Height - 113;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
