using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class mainWindow : Form
    {
        // ###############   Wichtige Variablen fuer die App   #######################
        static private Assembly assembly = Assembly.GetExecutingAssembly();
        static private FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

        static private string gitURL = "https://git.io/vblQ0";
        static private string dotNetVer = "4.5.2";
        static private string AboutMSG = fvi.Comments + 
                                 "\n\nVersion: " + fvi.FileVersion +
                                 "\n.NET Target-Version: " + dotNetVer +
                                 "\nAktualisiert am: " + new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToShortDateString() +
                                 "\n\nAuthor: " + fvi.CompanyName +
                                 "\n\nLizenz: " + fvi.LegalCopyright +
                                 "\nQuellcode: " + gitURL;

        private bool IsChecked = false;
        private string WorkingDir = "C:\\%USER%\\Documents";
        private static String DefaultFileName = "Einrichteblatt.txt";

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
            
            if (openSourceFileDialog.ShowDialog() == DialogResult.OK) {
                DestTextBox.Text = Path.GetDirectoryName(openSourceFileDialog.FileName) + "\\" + DefaultFileName;
                SourceTextBox.Text = openSourceFileDialog.FileName;
            }
            
            IsChecked = false;
        }

        // Ziel-Datei waehlen
        private void DestBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog openDestFileDialog = new SaveFileDialog();
            openDestFileDialog.InitialDirectory = WorkingDir;
            openDestFileDialog.Filter = "Textdateien (*.txt)| *txt|Alle Dateien| *.*";
            openDestFileDialog.FileName = DefaultFileName;
            openDestFileDialog.DefaultExt = ".txt";
            
            if (openDestFileDialog.ShowDialog() == DialogResult.OK) {
                DestTextBox.Text = openDestFileDialog.FileName;
                IsChecked = true;
            }
        }

        // Falls die Datei im Ausgabe-Pfad vorhanden ist wird der "Oeffnen"-Button Aktiviert
        private void DestTextBox_TextChanged(object sender, EventArgs e) {
            if (File.Exists(@DestTextBox.Text))
                OpenBtn.Enabled = true;
            else
                OpenBtn.Enabled = false;
        }

        // Wenn Zieldatei-Pfad gueltig ist wird die Datei in Notepad geöffnet
        private void OpenBtn_Click(object sender, EventArgs e) {
            // Nochmalige Abfrage, ob Zieldatei immernoch vorhanden ist.
            // Falls nicht wird eine Fehlermeldung ausgegeben und der
            // "Oeffnen"-Button deaktiviert
            if (File.Exists(DestTextBox.Text))
                Process.Start("notepad.exe", DestTextBox.Text);
            else {
                MessageBox.Show("Datei nicht gefunden", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                OpenBtn.Enabled = false;
            }
        }

        // Bei Klick Info-Text anzeigen
        private void AboutBtn_Click(object sender, EventArgs e) => MessageBox.Show(AboutMSG, caption: "Info", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Question);

        // Datei erstellen
        private void MakeBtn_Click(object sender, EventArgs e) {
            string InputFile = SourceTextBox.Text;
            string OutputFile = DestTextBox.Text;
            DialogResult Result = DialogResult.No;

            // Kontrollieren, ob Eingaben alle korrekt sind und ob die Ausgabedatei evtl. bereits existiert
            if (String.IsNullOrWhiteSpace(InputFile) || String.IsNullOrEmpty(OutputFile))
                MessageBox.Show("Fehler! Nicht alle benötigten Eingaben wurden vorgenommen!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (!File.Exists(Path.GetFullPath(@InputFile)) || !Directory.Exists(Path.GetDirectoryName(@InputFile)))
                MessageBox.Show("Fehler! Falsche Eingabe oder Quelldatei existiert nicht!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (InputFile == OutputFile)
                MessageBox.Show("Fehler\nQuell- und Ziel-Datei sind identisch!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else {
                if (Directory.Exists(Path.GetDirectoryName(@OutputFile)))
                    // Abfragen, ob bereits gefragt wurde ob Datei ueberschrieben werden darf
                    if (IsChecked)
                        Result = DialogResult.Yes;
                    else
                        if (File.Exists(@OutputFile))
                        Result = MessageBox.Show("Zieldatei existiert bereits. Überschreiben?", "Frage", MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning);
                    else {
                        IsChecked = true;
                        Result = DialogResult.Yes;
                    }
                else
                    Result = DialogResult.Yes;

                // MessageBox.Show("IsChecked =" + IsChecked + "\nResult = " + Result, "DEBUG");
                // Datei erstellen
                if (Result == DialogResult.Yes) {
                    makeSheet(@InputFile, @OutputFile);                        // Datei erstellen

                    // Nach Erstellen der Zusammenfassung pruefen ob Datei vorhanden ist und gegebenenfalls
                    // den "Oeffnen"-Button aktivieren
                    if (File.Exists(@DestTextBox.Text)) {
                        // Fragen, ob die Datei direkt geoeffnet werden soll
                        Result = MessageBox.Show("Einrichteblatt wurde erstellt. Jetzt öffnen?", "Fertig", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (Result == DialogResult.Yes)
                            Process.Start("notepad.exe", DestTextBox.Text);

                        OpenBtn.Enabled = true;
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

            string PGMNAME = SearchForString("_MPF", InputFile);        // Hauptprogramm-Name
            string AUFTRAG = SearchForString("AUFTRAG", InputFile);     // Zeile mit Bezeichnung/Auftrag finden
            string SPANNUNG = SearchForString("SPANNUNG", InputFile);   // Welche Spannung
            string LASTRUN = SearchForString("GELAUFEN", InputFile);    // Zeile mit Datum wann Programm zuletzt abgearbeitet wurde */
            string originX = "", originY = "", originZ = "";            // Fertige NP-Variablen
            string oriSearchX = ";(X0 =";           // String fuer X-NP Kommentarsuche
            string oriSearchY = ";(Y0 =";           // String fuer Y-NP Kommentarsuche
            string oriSearchZ = ";(Z0 =";           // String fuer Z-NP Kommentarsuche
            string preT = "T";                      // Vorangestelltes "T" in der WZ-Liste, maschinenabhaengig
            string separator = "- ";                // Bindestrich fuer WZ-Liste, maschinenabhaengig
            string Comment = "";                    // Variable fuer UP-Beschreibung
            string placeholder = " - UP FRAESEN";   // Platzhalter-String
            int Index = 0;                          // Index-Variable fuer gezieltes Suchen in Datei
            bool DMC100 = RadioBtn1.Checked,        // Fuer bessere Lesbarkeit innerhalb der Funktionen
                 UNIPORT = RadioBtn2.Checked,
                 FOREST = RadioBtn3.Checked,
                 UNION = RadioBtn4.Checked;

            // Sachen in Datei schreiben
            Stream.WriteLine("-----------------------------------------------");
            Stream.WriteLine("PROGRAMM: MPF" + PGMNAME.Replace("%_N_", "").Replace("MPF", "").Replace("_", ".MPF"));
            if (AUFTRAG != "")
                Stream.WriteLine(AUFTRAG.TrimStart(';').TrimStart().TrimStart('(').TrimEnd(')'));
            foreach (string i in File.ReadLines(InputFile))
                if (i.ToUpper().Contains("(ZEICHNUNG"))
                    Stream.WriteLine(i.TrimStart(';').TrimStart().TrimStart('(').TrimEnd(')'));
            Stream.WriteLine(SPANNUNG.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')').Replace(".", ". "));
            Stream.WriteLine("-----------------------------------------------"+Environment.NewLine);
            
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
            
            Stream.WriteLine("Benoetigte Werkzeuge:" + Environment.NewLine + "---------------------");
            foreach (string i in File.ReadLines(InputFile)) {
                if (i.Contains("WZP1") && !i.Contains("_DELETE") && !i.Contains("_STOP"))
                    Stream.WriteLine(preT + i.Replace("WZP1","").TrimStart(' ').Replace("(","").Replace(")","").Replace("\"","").Replace(";",separator));
            }

            originX = SearchForString(oriSearchX, InputFile);  // NP-Beschreibung fuer X finden
            originY = SearchForString(oriSearchY, InputFile);  //NP-Beschreibung fuer Y finden
            originZ = SearchForString(oriSearchZ, InputFile);  // NP-Beschreibung fuer Z finden
            
            if (originX != "" || originZ != "" || originZ != "") {
                Stream.WriteLine(Environment.NewLine + "Nullpunkt:" + Environment.NewLine + "----------");
                Stream.WriteLine(originX.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')'));
                Stream.WriteLine(originY.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')'));
                Stream.WriteLine(originZ.TrimStart(';').TrimStart(' ').TrimStart('(').TrimEnd(')'));
            }

            Stream.WriteLine(Environment.NewLine + "Unterprogramme:" + Environment.NewLine + "---------------");
            
            // Fraes-UPs parsen
            Index = 0;
            foreach (string i in File.ReadLines(InputFile))
            {
                if (i.Contains("%_N_L") && i.Contains("_SPF")) {
                    Comment = getMillComment(InputFile, i, placeholder, DMC100, UNIPORT, FOREST, UNION);
                    Stream.WriteLine(i.Replace("%_N_", "").Replace("_SPF", "") + Comment);
                }
                Index++;
            }

            // Falls Frae-UPs gefunden werden wird die Variable "Comment" beschrieben.
            // Ist das der Fall wird eine zusaetzliche Leerzeile zwischen Fraes- und Bohr-UPs
            // eingefuegt, um die Lesbarkeit zu verbessern
            if (Comment != "")
                Stream.WriteLine();

            // Bohr-UPs parsen
            Index = 0;
            foreach (string i in File.ReadLines(InputFile))
            {
                if (i.Contains("%_N_UP") && i.Contains("_SPF")) {
                    Comment = File.ReadLines(InputFile).Skip(Index+2).Take(1).First().Replace(";", "").Replace("--", "").TrimStart();
                    Stream.WriteLine(i.Replace("%_N_", "").Replace("_SPF", "") + " - " + Comment);
                }
                Index++;
            }

            Stream.Close();
        }

        public string SearchForString(string SearchString, string SearchFile) {
            foreach (string i in File.ReadLines(SearchFile)) {
                if (i.ToUpper().Contains(SearchString)){
                    return i;
                }
            }
            return "";
        }

        //
        // Kommentar zu Fraes-UPs zusammenbasteln
        // Hier wird auf die maschinenspezifischen Programm-Eigenschaften Ruecksicht genommen
        //
        public String getMillComment(string InFile, string UP, string plh, bool DMC100, bool UNIPORT, bool FOREST, bool UNION){
            string comm = "";
            string[] com;
            string UPnotUsed = " - !!! Unterprogramm wird nicht verwendet !!!";
            int Idx = 0;
            bool found = false;

            if (DMC100) {
                // Fraes-UP suchen
                checkUPs("L", InFile, ref UP, ref Idx, ref found);
                
                if (!found)
                    return UPnotUsed;

                while ((!comm.Contains(";(")) || (comm.Contains("(AKS ")) || (comm.Contains("VORSCHUB"))) {
                    Idx--;
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();
                    if (comm.EndsWith("M6"))
                        return plh;
                }
                com = comm.Split(';');
                return " - " + com[1].Replace("(", "").Replace(")", "");

            } else if (UNIPORT) {
                // Fraes-UP suchen
                checkUPs("R500=", InFile, ref UP, ref Idx, ref found);

                if (!found)
                    return UPnotUsed;

                while (!comm.Contains("; (") || (comm.Contains("; (A") && comm.Contains("C") && comm.Contains(".")) || (comm.Contains("CYCLE800(")) || (comm.Contains("; (AK "))) {
                    Idx--;
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();
                    if (comm.EndsWith("M6") || comm.EndsWith("M66"))
                        return plh;
                }
                com = comm.Split(';');
                return " - " + com[1].TrimStart().Replace("(", "").Replace(")", "");

            } else if (FOREST) {
                // Fraes-UP suchen
                checkUPs("R500=", InFile, ref UP, ref Idx, ref found);

                if (!found)
                    return UPnotUsed;

                while (!comm.Contains(";(") && !comm.Contains("DEC(")) {
                    Idx--;
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();
                    if (comm.Contains("M6"))
                        return plh;
                }
                com = comm.Split(';');
                return " - " + com[1].TrimStart().Replace("(", "").Replace(")", "");

            } else {
                // Fraes-UP suchen
                checkUPs("R500=", InFile,ref UP, ref Idx, ref found);

                if (!found) return UPnotUsed;

                while (!comm.Contains(";(") || comm.Contains("ACHS")) {
                    Idx--;
                    comm = File.ReadLines(InFile).Skip(Idx).Take(1).First();
                    if (comm.Contains("M6"))
                        return plh;
                }
                com = comm.Split(';');
                return " - " + com[1].TrimStart().Replace("(", "").Replace(")", "");
            }
        }
        
        //
        // Abarbeiten der Fraes-UP Suche
        // "Idx" und "found" sind als Referenz angegeben (sehr praktisch!)
        //
        private void checkUPs(string repStrg, string searchFile, ref string UP, ref int Idx, ref bool found) {
            // Bastelt den Such-String zusammen
            string FIRST_L = UP.Replace("%_N_L", repStrg).Replace("_SPF", "");

            foreach (string i in File.ReadLines(searchFile))
                if (i.Contains(FIRST_L) && !i.Contains("%_N_")) {
                    found = true;
                    break;
                } else
                    Idx++;
        }

    }
}
