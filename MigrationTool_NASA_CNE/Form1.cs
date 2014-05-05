using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MigrationTool_NASA_CNE.Entities;
using System.Globalization;

namespace MigrationTool_NASA_CNE
{
    public partial class Form1 : Form
    {
        private static string _estado = "A";
        private static string _sqlFile = "";
        private static string _currentFile = "";

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Recorre un directorio especificado
        /// </summary>
        /// <param name="directory">Directorio</param>
        static void WalkDirectoryTree(System.IO.DirectoryInfo directory)
        {
            System.IO.FileInfo[] files = null;

            try
            {
                files = directory.GetFiles("*.opo");
            }
            catch (UnauthorizedAccessException e)
            {
                throw e;
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                throw e;
            }

            if (files != null)
            {
                int idOpo = 0;
                foreach (System.IO.FileInfo fi in files)
                {
                    idOpo++;
                    _currentFile = fi.Name;

                    if (fi.Length == 0)
                    {
                        continue;
                    }

                    //Formato Nuevo
                    if (_currentFile.Contains(".opo"))
                    {
                        ParseFile(fi.FullName, fi.Name, idOpo);
                    }

                    //Formato Viejo
                    if (_currentFile.Contains(".OPO"))
                    {
                        //ParseFile(fi.FullName, fi.Name, idOpo);
                    }
                }
            }
        }

        /// <summary>
        /// Lee un Archivo del Directorio Especificado
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        static void ParseFile(string fileFullName, string fileName, int idOpo)
        {
            StreamReader objReader = new StreamReader(fileFullName);
            string fileContent = "";

            fileContent = objReader.ReadToEnd();
            objReader.Close();

            string[] linesCSV = fileContent.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            OPO opo = stringToOpo(linesCSV, fileName, idOpo);
            string opoSQL = opoToSQL(opo);
            writeFileSQL(opoSQL);
        }

        static OPO stringToOpo(string[] fileLines, string nroOPO, int idOpo)
        {
            _currentFile = nroOPO;
            int cantidadTarjetas = 0;
            string[] campos;
            IList<Linea> lineas = new List<Linea>();
            int contadorLineas = 0;
            int pasoAnterior = 0;
            foreach (string line in fileLines)
            {
                if (line.Contains("|") && contadorLineas > 0) //Es una linea de la OPO
                {
                    campos = line.Split('|');
                    
                    Linea linea = new Linea();
                    linea.NroLinea = contadorLineas;
                    linea.LetraPaso = (!campos[5].Equals("") ? char.Parse(campos[5]) : char.Parse(" "));
                    linea.Dispositivo = campos[6];
                    linea.Operacion = campos[8];
                    linea.NroPaso = (campos[4].Equals("") ? pasoAnterior : int.Parse(campos[4]));
                    pasoAnterior = linea.NroPaso;
                    linea.DebeColocarTarjeta = (campos[7].Equals("1") ? true : false);
                    linea.DebeRetirarTarjeta = (campos[9].Equals("1") ? true : false);
                    linea.Comentario = campos[5];
                    linea.IdOpo = idOpo;
                    lineas.Add(linea);

                    if (campos[7].Equals("1") || campos[9].Equals("1")) { cantidadTarjetas++; }
                }

                contadorLineas++;
            }

            //Nro de OPO
            string bsi = nroOPO.Substring(0, 4);
            string nro = nroOPO.Substring(5, 3);
            string letra = nroOPO.Substring(4, 1);
            string titulo = fileLines[0].Replace("'","\"");
            string fecha = (fileLines[fileLines.Length - 1]).Trim();
            OPO opo = new OPO()
            {
                Id = idOpo,
                Titulo = titulo,
                Codigo = nroOPO.ToLower().Replace(".opo", ""),
                Estado = _estado,
                FechaCreacion = DateTime.Parse(fecha, new CultureInfo("es-ES")),
                Lineas = lineas,
                Bsi = bsi,
                Letra = letra,
                Numero = nro,
                CantidadLineas = contadorLineas - 2, //Le resta la cabecera y la fecha
                Eliminado = false,
                CantidadTarjetas = cantidadTarjetas
            };
            
            return opo;
        }

        static string opoToSQL(OPO opo)
        {
            string sqlQuery = "";
            string titulo = "";
            string fecha = "";
            string codigo = "";
            string cantTarjetas = "";
            string cantLineas = "";
            string nro = "";
            string bsi = "";
            string letra = "";
            string estado = "";

            titulo = opo.Titulo;
            //fecha = opo.FechaCreacion.ToShortDateString();
            fecha = opo.FechaCreacion.Year.ToString("0000") + "-" + opo.FechaCreacion.Month.ToString("00") + "-" + opo.FechaCreacion.Day.ToString("00") + " 00:00:00.000"; 
            codigo = opo.Codigo;
            cantTarjetas = opo.CantidadTarjetas.ToString();
            cantLineas = opo.Lineas.Count.ToString();
            nro = opo.Numero;
            bsi = opo.Bsi;
            letra = opo.Letra;
            estado = opo.Estado;

            sqlQuery += "INSERT INTO [dbo].[Opo] ([Titulo],[FechaCreacion],[Codigo],[CantidadTarjetas],[CantidadLineas],[Eliminado],[Estado],[Letra],[Bsi],[Numero])" +
                            "VALUES ('" + titulo + "', '" + fecha + "', '" + codigo + "', " + cantTarjetas + ", " + cantLineas + " ,0 ,'" + estado + "','" + letra + "', '" + bsi + "','" + nro + "') ";
                            //"SELECT CAST(SCOPE_IDENTITY() as int);\r\n";

            int contadorLineas = 0;
            foreach (Linea linea in opo.Lineas)
            {
                contadorLineas++;
                sqlQuery += "INSERT INTO Lineas (Id, NroLinea, NroPaso, LetraPaso, Comentario, Dispositivo, Operacion, DebeRetirarTarjeta, DebeColocarTarjeta, IdOpo) " +
                            " VALUES ( " + contadorLineas.ToString() + "," + linea.NroLinea.ToString() + "," + linea.NroPaso.ToString() + ",'" + linea.LetraPaso + "','" + linea.Comentario + "','" + linea.Dispositivo + "'," +
                            linea.DebeColocarTarjeta.ToString() + "," + linea.DebeRetirarTarjeta.ToString() + "," + linea.IdOpo.ToString() + ");\r\n";
            }

            return sqlQuery;
        }

        static void writeFileSQL(string sql)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(_sqlFile, true);
            sw.WriteLine(sql);
            sw.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbEstadoOPO.Items.Add("A");
            cbEstadoOPO.Items.Add("G");
            cbEstadoOPO.Items.Add("P");
            cbEstadoOPO.SelectedIndex = 0;
            cbEstadoOPO.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private static string getCurrentFile()
        {
            return _currentFile;
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            _estado = cbEstadoOPO.Text;
            _sqlFile = Application.StartupPath + "\\SQL\\Opos_" + _estado + ".sql";
            string pathOpos = Application.StartupPath + "\\OPOS_" + _estado;
            try
            {
                if (System.IO.File.Exists(_sqlFile))
                {
                    File.Delete(_sqlFile);
                }
                System.IO.DirectoryInfo diOPOs = new System.IO.DirectoryInfo(pathOpos);
                WalkDirectoryTree(diOPOs);

                MessageBox.Show("Exportación realizada con éxito. Archivo: " + _sqlFile,Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Information,MessageBoxDefaultButton.Button1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excepcion del sistema: " + ex.Message + ". Archivo : " + getCurrentFile());
            }
        }
    }
}
