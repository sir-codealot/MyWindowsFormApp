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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class mainWindow : Form
    {
        // ###############   Wichtige Variablen fuer die App   #######################
        static String AppVersion = "0.4a.NET";
        static String LastModified = "01.12.2017";
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
        private void MakeBtn_Click(object sender, EventArgs e)
        {
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
                    if (IsChecked)
                    {
                        Result = DialogResult.Yes;
                    }
                    else
                    {
                        Result = MessageBox.Show("Zieldatei existiert bereits. Überschreiben?", "Frage", MessageBoxButtons.YesNo, icon: MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    Result = DialogResult.Yes;
                }

                // Datei erstellen
                if (Result == DialogResult.Yes)
                {
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
            StreamWriter Stream = new StreamWriter(OutputFile);  // Stream-Objekt zum Speichern in die Zieldatei

            FileStream FILE = File.Create(InputFile);           // Programm-Objekt
            String NAMES = findstr("%_N_") InputFile;           // Dateinamen
            String PGMNAME = "";                                // Hauptprogramm-Name
            String L_NAME[] = findstr("%_N_L") InputFile;       // Array von benoetigten Unterprogrammen
            String UPNAME[] = findstr("%_N_UP") InputFile;      // Array Bohr-UPs
            String TOOLS[] = findstr("WZP1") InputFile;         // Array von benoetigten Werkzeugen
            String SPANNUNG = findstr("SPANNUNG") Inputfile;    // Welche Spannung
            String LASTRUN = findstr("GELAUFEN") InputFile;     // Zeile mit Datum wann Programm zuletzt abgearbeitet wurde
            String oriSearchX = ";(X0.=";                       // String fuer X-NP Kommentarsuche
            String oriSearchY = ";(Y0.=";                       // String fuer Y-NP Kommentarsuche
            String oriSearchZ = ";(Z0.=";                       // String fuer Z-NP Kommentarsuche
            String placeholder = " - Fraes-UP";
            String UPnotUsed = " - !!! Unerprogramm wird nicht verwendet !!!";

            // write-host "[DEBUG] Spannung-Laenge: "([String]$SPANNUNG.length)

            for (String i in NAMES) {
                if (i.Contains("_MPF")) {
                    PGMNAME = i;
                    break; }
            }

            // Sachen in Datei schreiben
            Stream.WriteLine("****  Programm  ****");
            Stream.WriteLine("MPF" +[String]PGMNAME.TrimStart("%_N_").Trim("MPF").Replace("_", ".MPF"));
            if (SPANNUNG.Length > 12) { 
                Stream.WriteLine(SPANNUNG.Trim(';').Trim(' ').Trim('(').Trim(')').Insert('.', ". "));
            } 
            else
            {
                $stream.WriteLine([String]$SPANNUNG[0].Trim(";").Trim(" ").Trim("(").Trim(")").Replace(".", ". ")) }
                $stream.WriteLine("********************")
                $stream.WriteLine()
            }    
            if ($LASTRUN) { $stream.WriteLine([String]$LASTRUN.TrimStart("; ")) }
            $stream.WriteLine()
            $stream.WriteLine("Benoetigte Werkzeuge:")
            $stream.WriteLine()
            

            if (findstr("UNIPORT") $inputFile) {
                $preT = ""
                $separator = "-"
                $oriSearchX = ";.(X0.="
                $oriSearchY = ";.(Y0.="
                $oriSearchZ = ";.(Z0.="
            }
            else if (findstr("PC-130") $inputFile) {
                $oriSearchX = ";(G5[0-9].X0.="
                $oriSearchY = ";(G5[0-9].Y0.="
                $oriSearchZ = ";(G5[0-9].Z0.="
            } else {
                $preT = "T"
                $separator = "- "
            }
            $originX = findstr($oriSearchX) $inputfile  # NP-Beschreibung fuer X finden
            $originY = findstr($oriSearchY) $inputfile  # NP-Beschreibung fuer Y finden
            $originZ = findstr($oriSearchZ) $inputfile  # NP-Beschreibung fuer Z finden

            forEach($tool in $tools)
            {
                if (!($tool.contains("_DELETE")) -and !($tool.contains("_STOP"))) {
                    $stream.WriteLine($preT +$tool.TrimStart("WZP1 ").Replace("(", "").Replace(")", "").Replace('"', "").Replace(";", $separator))
                }
            }

            $stream.WriteLine()
            $stream.WriteLine("")
            $stream.WriteLine("Unterprogramme:")
            $stream.WriteLine()

            // Auflisten der Fraes-UPs
            forEach($UP in $L_NAME)
            {
                if ($UP) {
                    $comment = ""
    

                    if ($RadioBTN1.checked -eq $True) {        #  Button 1 - DMC100
                        $FIRST_L = $UP.Trim("%_N_").Trim("_SPF")
                        $count = 1
                        // Kommentar fuer Fraes-UPs suchen
                        while ((!$comment.Contains(";(")) -or($comment.Contains("(AKS ")) - or($comment.Contains("VORSCHUB"))) {
                            $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$FIRST_L" $inputFile) -split ":")[0]), 10)-[convert]::ToInt32([int]$count, 10)]
                            // Write-Host "[DEBUG] Aktueller Kommentar DMC100: $comment"
                            if ($comment.endsWith("M6")) { 
                                $comment = $placeholder
                                break
                            } else {
                                $count += 1 }
                        }
                    } else if($RadioBTN2.checked -eq $True)
                    {   
                        // Button 2 - UniPort 6000
                        $FIRST_L = $UP.Replace("%_N_L", "R500=").TrimEnd("_SPF")
                        $count = 2


                        if (!(findstr $FIRST_L $inputFile)) {
                            $comment = $UPnotUsed
                            // Write - Host "[DEBUG] Kommentar in IF: $comment | $FIRST_L"
                        } else {
                            // Kommentar fuer Fraes-UPs suchen
                            while (!$comment.Contains("; (") - or($comment - match("A[0-9].")) - or($comment - match("A-[0-9].")) - or($comment - match("CYCLE800\(")) - or($comment - match("; \(AK "))) {
                                $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$FIRST_L" $inputFile) -split ":")[0]), 10)-[convert]::ToInt32([int]$count, 10)]
                                // write-host "[DEBUG] Unisign: Kommentar = $comment / UP $FIRST_L"
                                if ($comment.endsWith("M6") -or $comment.endsWith("M66")) {
                                    $comment = $placeholder
                                    // write-host "[DEBUG] Unisign: Kommentar = $comment"
                                    break
                                } else {
                                    $count += 1 }
                        }
                    }

                    if ($comment.Contains("M6")) {
                        $comment = $placeholder }
                    // Write-Host "[DEBUG] Fertiger Kommentar = $comment / $FIRST_L"
                    } else if($RadioBTN3.checked -eq $True)
                    {   
                        //  Button 3: Forest
                        $FIRST_L = $UP.Replace("%_N_L", "R500=").TrimEnd("_SPF")
                        $count = 1

                        if ((findstr $FIRST_L $inputFile)) {
                            // Kommentar fuer Fraes-UPs suchen
                            $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$FIRST_L" $inputFile) -split ":")[0]), 10)-[convert]::ToInt32([int]$count, 10)]
                            // Write-Host "[DEBUG] Forest: Kommentar = $comment"

                            while (!($comment.Contains("DEC("))) {
                                $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$FIRST_L" $inputFile) -split ":")[0]), 10)-[convert]::ToInt32([int]$count, 10)]

                                if (!$comment.Contains(";(")) {
                                $count += 1
                                } else {
                                    break
                                }
                            }
                        } else {
                            $comment = $UPnotUsed
                        }
                    } else {
                        // Ansonsten: Union
                        $FIRST_L = $UP.Replace("%_N_L","R500=").TrimEnd("_SPF")
                        $count = 2

                        if ((findstr $FIRST_L $inputFile)) {
                            // Kommentar fuer Fraes-UPs suchen
                            $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$FIRST_L" $inputFile) -split ":")[0]), 10)-[convert]::ToInt32([int]$count, 10)]
                            // Write-Host "[DEBUG] Union: Kommentar = $comment"

                            while (!$comment.Contains("ATC")) {
                                //Write-Host "[DEBUG] In while-Schleife (count = $count )"
                                $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$FIRST_L" $inputFile) -split ":")[0]), 10)-[convert]::ToInt32([int]$count, 10)]

                                if (!$comment.Contains(";(") -or $comment.Contains("ACHS")) {
                                    // Write-Host "[DEBUG] In if-Schleife (count = $count )"
                                    $count += 1
                                } else {
                                    break
                                }
                                // $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$FIRST_L" $inputFile) -split ":")[0]), 10)-[convert]::ToInt32([int]($count-2), 10)]
                            }
                        } else {
                            $comment = $UPnotUsed
                        }
                    }

                // Write-Host "[DEBUG] comment vor Ausgabe = $comment / UP = $FIRST_L / Satz: "((findstr /n "$FIRST_L" $inputFile) -split ":")[0]
            
            // UP-Nummer und Kommentar in Einrichteblatt schreiben, Satznummer und Klammern aus Kommentar entfernen
            if (!$comment) {
                $stream.WriteLine($UP.TrimStart("%_N_").TrimEnd("_SPF"))            
            } else if (($comment -eq $UPnotUsed) -or ($comment -eq $placeholder)) {
                $stream.WriteLine($UP.TrimStart("%_N_").TrimEnd("_SPF")+$comment)
            } else {
                $stream.WriteLine($UP.TrimStart("%_N_").TrimEnd("_SPF")+" - "+($comment -split ";")[1].trimStart(" (").Trim(")"))
            }
        }
    }

    $stream.WriteLine()  # Leerzeile zwischen Fraes- und Bohr-UPs

    // Auflisten der Bohr-UPs
    forEach ($UP in $UPNAME) {
        if ($UP) {
            # Kommentarzeile fuer Bearbeitung suchen, abhaengig von UP-Zeile
            $comment = ($FILE)[[convert]::ToInt32((((findstr /n "$UP" $inputFile) -split ":")[0]), 10)+[convert]::ToInt32(1, 10)]

            # Aktuelle Zeile schreiben nach Schema: [UP..] - [UP-Beschreibung]
            $stream.WriteLine($UP.TrimStart("%_N_").TrimEnd("_SPF")+" - "+$comment.TrimStart("; ").Replace("-","").TrimStart(" "))
        }
    }

    // Eingabe-Stream schließen und Dateibearbeitung abschliessen
    $stream.Close()
}
        }
    }
}
