/*
 * 
 * 
 * 
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class mainWindow : Form
    {
        // ###############   Wichtige Variablen fuer die App   #######################
        static String AppVersion = "0.7a.NET";
        static String LastModified = "06.12.2017";
        static String Author = "Henry Wünsche <henry.wuensche@mail.de>";
        static String LICENSE = "GPLv2";
        static String AboutMSG = "Programm zum Erstellen von Einrichteblättern\nfür umfangreiche CNC-Programme.\n\nVersion: " + AppVersion +
                                 "\nAktualisiert am: " + LastModified + "\n\nAuthor: " + Author +
                                 "\n\nLizenz: " + LICENSE;

        public bool IsChecked = false;
        private String WorkingDir = "C:\\%USER%\\Documents";
        private String LastDir = "";
        private static String DefaultFileName = "Einrichteblatt.txt";

        // Kapseln des Ueber-Strings - VS17 will das so
        public static string MsgString { get => AboutMSG; set => AboutMSG = value; }

        // ################   Fenster-Logik zusammenreimen   ###########################
        public mainWindow()
        {
            InitializeComponent();
        }

        // Quell-Datei waehlen
        private void SourceBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openSourceFileDialog = new OpenFileDialog();
            openSourceFileDialog.InitialDirectory = WorkingDir;
            openSourceFileDialog.Filter = "NC-Dateien (*.nc; *.arc; *.mpf; *.spf) | *.nc; *.arc; *.mpf; *.spf |Alle Dateien| *.*";
            openSourceFileDialog.ShowDialog();

            try
            {
                WorkingDir = Path.GetDirectoryName(openSourceFileDialog.FileName);
                DestTextBox.Text = WorkingDir + "\\" + DefaultFileName;
                LastDir = openSourceFileDialog.FileName;
            } catch (ArgumentException) {
                // Mache nix   
            }

            SourceTextBox.Text = openSourceFileDialog.FileName;
            IsChecked = false;
        }

        // Ziel-Datei waehlen
        private void DestBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog openDestFileDialog = new SaveFileDialog();
            openDestFileDialog.InitialDirectory = WorkingDir;
            openDestFileDialog.Filter = "Textdateien (*.txt)| *txt|Alle Dateien| *.*";
            openDestFileDialog.FileName = "Einrichteblatt.txt";
            openDestFileDialog.DefaultExt = ".txt";
            openDestFileDialog.ShowDialog();

            DestTextBox.Text = openDestFileDialog.FileName;
            IsChecked = true;
        }

        // Bei Klick Info-Text anzeigen
        private void AboutBtn_Click(object sender, EventArgs e) => MessageBox.Show(MsgString, caption: "Info", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Question);

        // Datei erstellen
        private void MakeBtn_Click(object sender, EventArgs e) {
            String InputFile = SourceTextBox.Text;
            String OutputFile = DestTextBox.Text;
            DialogResult Result = DialogResult.No;

            // Kontrollieren, ob Eingaben alle korrekt sind und ob die Ausgabedatei evtl. bereits existiert
            if ((InputFile == "") || (OutputFile == "") || (Directory.Exists(Path.GetDirectoryName(InputFile)) == false)) {
                MessageBox.Show("Fehler! Falsche Eingabe oder Quelldatei existiert nicht!\nInputFile: " + InputFile + "\nOutpuFile: " + OutputFile + "\nDirExists = " + Directory.Exists(Path.GetDirectoryName(InputFile)), "", MessageBoxButtons.OK, icon: MessageBoxIcon.Warning);
            }
            else
            {
                if (Directory.Exists(Path.GetDirectoryName(OutputFile))) {
                    // Abfragen, ob bereits gefragt wurde ob Datei ueberschrieben werden darf
                    if (IsChecked) {
                        Result = DialogResult.Yes;
                    } else {
                        if (File.Exists(OutputFile)) {
                            Result = MessageBox.Show("Zieldatei existiert bereits. Überschreiben?", "Frage", MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning);
                        } else {
                            IsChecked = true;
                        }
                    }
                } else {
                    Result = DialogResult.Yes;
                }

                // Datei erstellen
                if (Result == DialogResult.Yes) {
                    MakeBtn.Text = "Erstelle ...";      // Button-Text aendern um die Abarbeitung zu signalisieren
                    makeSheet(InputFile, OutputFile);                        // Datei erstellen
                    MakeBtn.Text = "Erstellen";         // Button-Text wieder zuruecksetzen

                    // Fragen, ob die Datei direkt geoeffnet werden soll
                    Result = MessageBox.Show("Einrichteblatt wurde erstellt. Jetzt öffnen?", "Fertig", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (Result == DialogResult.Yes) {
                        Process.Start("notepad.exe", DestTextBox.Text);
                    }

                    IsChecked = true;
                }
            }
        }

        // Bei Klick App beenden
        private void QuitBtn_Click(object sender, EventArgs e) => Application.Exit();


        /* ################################################################################
         * Herzstueck des Programms: Hier wird die ausgewaehlte Datei auseinandergenommen
         * und das Einrichteblatt erstellt.
         * ################################################################################
         */
        void makeSheet(String InputFile, String OutputFile)
        {
            StreamWriter Stream = new StreamWriter(OutputFile);         // Stream-Objekt zum Speichern in die Zieldatei

            String PGMNAME = SearchForString("_MPF", InputFile);        // Hauptprogramm-Name
            String SPANNUNG = SearchForString("SPANNUNG", InputFile);   // Welche Spannung
            String LASTRUN = SearchForString("GELAUFEN", InputFile);    // Zeile mit Datum wann Programm zuletzt abgearbeitet wurde */
            String originX = "", originY = "", originZ = "";            // Fertige NP-Variablen
            String oriSearchX = ";(X0 =";           // String fuer X-NP Kommentarsuche
            String oriSearchY = ";(Y0 =";           // String fuer Y-NP Kommentarsuche
            String oriSearchZ = ";(Z0 =";           // String fuer Z-NP Kommentarsuche
            String preT = "T";                      // Vorangestelltes "T" in der WZ-Liste, maschinenabhaengig
            String separator = "- ";                // Bindestrich fuer WZ-Liste, maschinenabhaengig
            String Comment = "";                    // Variable fuer UP-Beschreibung
            String placeholder = " - UP FRAESEN";   // Platzhalter-String
            int Index = 0;                          // Index-Variable fuer gezieltes Suchen in Datei
            Boolean DMC100 = RadioBtn1.Checked,     // Fuer bessere Lesbarkeit innerhalb der Funktionen
                    UNIPORT = RadioBtn2.Checked,
                    FOREST = RadioBtn3.Checked,
                    UNION = RadioBtn4.Checked;

            // Sachen in Datei schreiben
            Stream.WriteLine("****  Programm  ****");
            Stream.WriteLine("MPF" + PGMNAME.Replace("%_N_", "").Replace("MPF", "").Replace("_", ".MPF"));
            Stream.WriteLine(SPANNUNG.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')').Replace(".", ". "));
            Stream.WriteLine("********************"+Environment.NewLine);

            /*          if (SPANNUNG.Length > 12) { 
                            Stream.WriteLine(SPANNUNG.Trim(';').Trim(' ').Trim('(').Trim(')').Insert('.', ". "));
                        } 
                        else
                        {
                            Stream.WriteLine(SPANNUNG[0].Trim(";").Trim(" ").Trim("(").Trim(")").Insert(".", ". ")); }
                            Stream.WriteLine("********************");
                            Stream.WriteLine();
                        }    */

            // Datum, falls gefunden eintragen
            if (LASTRUN.Length > 1)
                Stream.WriteLine(LASTRUN.TrimStart(';').TrimStart(' ')+Environment.NewLine);

            // Wz-Liste Nullpunktbeschreibungen parsen
            if (UNIPORT) {
                preT = "";
                separator = "-";
                oriSearchX = "; (X0 =";
                oriSearchY = "; (Y0 =";
                oriSearchZ = "; (Z0 =";
            }
            else if (UNION) {
                int NPnumber = 4;
                while (NPnumber < 8) {
                    if (SearchForString(";(G5" + NPnumber, InputFile) != "")
                        break;
                    NPnumber++;
                }
                oriSearchX = ";(G5" + NPnumber + " X0 =";
                oriSearchY = ";(G5" + NPnumber + " Y0 =";
                oriSearchZ = ";(G5" + NPnumber + " Z0 =";
            }
            else {
                preT = "T";
                separator = "- ";
            }
            
            Stream.WriteLine("Benoetigte Werkzeuge:");
            foreach (String i in File.ReadLines(InputFile)) {
                if (i.Contains("WZP1") && !i.Contains("_DELETE") && !i.Contains("_STOP"))
                    Stream.WriteLine(preT + i.Replace("WZP1","").TrimStart(' ').Replace("(","").Replace(")","").Replace("\"","").Replace(";",separator));
            }

            originX = SearchForString(oriSearchX, InputFile);  // NP-Beschreibung fuer X finden
            originY = SearchForString(oriSearchY, InputFile);  //NP-Beschreibung fuer Y finden
            originZ = SearchForString(oriSearchZ, InputFile);  // NP-Beschreibung fuer Z finden
            // MessageBox.Show(originX + "\n" + originY + "\n" + originZ, "DEBUG");    // Zum Debugging
            
            if (originX != "" || originZ != "" || originZ != "") {
                Stream.WriteLine(Environment.NewLine + "Nullpunkt:");
                Stream.WriteLine(originX.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')'));
                Stream.WriteLine(originY.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')'));
                Stream.WriteLine(originZ.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')'));
            }

            Stream.WriteLine(Environment.NewLine + "Unterprogramme:" + Environment.NewLine);
            
            // Fraes-UPs parsen
            Index = 0;
            foreach (String i in File.ReadLines(InputFile))
            {
                if (i.Contains("%_N_L") && i.Contains("_SPF")) {
                    // MessageBox.Show("Zeile: " + Index + "\nText: " + i, "DEBUG");
                    Comment = getMillComment(InputFile, i, placeholder, DMC100, UNIPORT, FOREST, UNION);
                    Stream.WriteLine(i.Replace("%_N_", "").Replace("_SPF", "") + Comment);
                }
                Index++;
            }

            // Bohr-UPs
            Index = 0;
            foreach (String i in File.ReadLines(InputFile))
            {
                if (i.Contains("%_N_UP") && i.Contains("_SPF")) {
                    Comment = File.ReadLines(InputFile).Skip(Index+2).Take(1).First().Replace(";", "").Replace("--", "").TrimStart();
                    Stream.WriteLine(i.Replace("%_N_", "").Replace("_SPF", "") + " - " + Comment);
                }
                Index++;
            }

            Stream.Close();
        }

        public String SearchForString(String SearchString, String SearchFile) {
            // MessageBox.Show("String: " + SearchString + "\nDatei: " + SearchFile, "DEBUG");  // Zum Debuggen
            foreach (String i in File.ReadLines(SearchFile)) {
                if (i.Contains(SearchString)){
                    // MessageBox.Show(i, "DEBUG"); // Zum Debuggin
                    return i;
                }
            }
            return "";
        }

        public String getMillComment(String InFile, String UP, String plh, Boolean DMC100, Boolean UNIPORT, Boolean FOREST, Boolean UNION){
            String FIRST_L = "";
            String comm = "";
            String[] com;
            String UPnotUsed = " - !!! Unterprogramm wird nicht verwendet !!!";
            int Idx = 0;
            Boolean found = false;

            if (DMC100) {
                FIRST_L = UP.Replace("%_N_", "").Replace("_SPF", "");
                foreach (String i in File.ReadLines(InFile)){
                    if (i.Contains(FIRST_L)) {
                        found = true;
                        break;
                    } else
                        Idx++;
                }

                if (!found)
                    return UPnotUsed;

                while ((!comm.Contains(";(")) || (comm.Contains("(AKS ")) || (comm.Contains("VORSCHUB"))) {
                    Idx--;
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();
                    if (comm.EndsWith("M6")) { 
                        return plh;
                    }
                }
                com = comm.Split(';');
                return " - " + com[1].Replace("(", "").Replace(")", "");

            } else if (UNIPORT) {
                FIRST_L = UP.Replace("%_N_L", "R500=").Replace("_SPF", "");

                foreach (String i in File.ReadLines(InFile)) {
                    if (i.Contains(FIRST_L)){
                        found = true;
                        break;
                    } else {
                        Idx++;
                    }
                }

                if (!found)
                    return UPnotUsed;

                while (!comm.Contains("; (") || (comm.Contains("; (A") && comm.Contains("C") && comm.Contains(".")) || (comm.Contains("CYCLE800(")) || (comm.Contains("; (AK "))) {
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();
                    if (comm.EndsWith("M6") || comm.EndsWith("M66")) {
                        return plh;
                    } else {
                        Idx--;
                    }
                }

                com = comm.Split(';');
                return " - " + com[1].TrimStart().Replace("(", "").Replace(")", "");

            } else if (FOREST) {
                FIRST_L = UP.Replace("%_N_L", "R500=").Replace("_SPF", "");

                foreach (String i in File.ReadLines(InFile)) {
                    if (i.Contains(FIRST_L)) {
                        found = true;
                        break;
                    } else {
                        Idx++;
                    }
                }

                if (!found)
                    return UPnotUsed;

                while (!(comm.Contains("DEC("))) {
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();

                    if (comm.Contains(";("))
                        break;

                    if (comm.Contains("M6"))
                        return plh;
                    Idx--;
                }
                com = comm.Split(';');
                MessageBox.Show(com[1].TrimStart().Replace("(", "").Replace(")", ""), "DEBUG");
                return " - " + com[1].TrimStart().Replace("(", "").Replace(")", "");
            } else {
                FIRST_L = UP.Replace("%_N_L", "R500=").Replace("_SPF", "");

                foreach (String i in File.ReadLines(InFile)) {
                    if (i.Contains(FIRST_L)) {
                        found = true;
                        break;
                    } else {
                        Idx++;
                    }
                }

                if (!found)
                    return UPnotUsed;

                while (!comm.Contains("ATC")) {
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();

                    if (comm.Contains(";(") && !comm.Contains("ACHS"))
                        break;

                    if (comm.Contains("M6"))
                        return plh;
                    Idx--;
                }

                com = comm.Split(';');
                return " - " + com[1].TrimStart().Replace("(", "").Replace(")", "");
            }
        }
    }
}
