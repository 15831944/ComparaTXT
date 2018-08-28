using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Collections.Generic;

namespace ComparaTXT
{
	public partial class Form1 : Form
	{
		string[] arquivoSistema, arquivoRede;
		static string arquivoSistemaName, arquivoRedeName;
		
		private PrintDocument document = new PrintDocument();

		public Dictionary<int, string> dicio = new Dictionary<int, string>()
		{
			{ 0, "sigla" }, { 1, "parte" }, { 2, "codigo" }, { 4, "posição" },
			{ 5, "quantidade" }, { 6, "largura" }, { 7, "comprimento" },
			{ 8, "chanfro" }, { 9, "quant. furos" }, { 10, "recortes" }, { 11, "dobra" },
			{ 12, "abertura de aba" }, { 13, "furos esp." }, { 14, "quant. diametros" },
			{ 15, "trusq. min." }, { 16, "solda" }
		};

		public Form1()
		{
			InitializeComponent();
		}
		
		private static void run_cmd()
		{
			File.Delete(@"C:\Temp\caminho.txt");

			if (File.Exists(@"M:\Projetos\PROGRAMAS\Ler CAM\LerCAM.exe"))
			{
				Process cmd = new Process();
				cmd.StartInfo.FileName = @"M:\Projetos\PROGRAMAS\Ler CAM\LerCAM.exe";
				cmd.StartInfo.RedirectStandardInput = true;
				cmd.StartInfo.RedirectStandardOutput = false;
				cmd.StartInfo.CreateNoWindow = false;
				cmd.StartInfo.UseShellExecute = false;
				cmd.Start();
	
				cmd.StandardInput.Flush();
				cmd.StandardInput.Close();
				cmd.WaitForExit();
			}
			else
			{
				MessageBox.Show("Não foi possível abrir o programa \"Ler CAM\", verifique na pasta!");
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			run_cmd();

			arquivoSistemaName = File.ReadAllText(@"C:\Temp\caminho.txt");
			
			textBox2.Text = arquivoSistemaName;
			
			try
			{
				arquivoSistema = File.ReadAllLines(arquivoSistemaName);

				string path = Path.GetDirectoryName(arquivoSistemaName);
				
				DirectoryInfo files = new DirectoryInfo(path);
				FileInfo[] arquivos = files.GetFiles();
				Array.Sort(arquivos, (FileInfo a, FileInfo b) => DateTime.Compare(a.CreationTime, b.CreationTime));
				Array.Reverse(arquivos);
				
				var query = from a in arquivos
				            where a.Name.Contains("OPER")
				            select a;

				arquivoRedeName = query.First().ToString();
			}
			catch
			{
				MessageBox.Show("O LerCAM foi interrompido ou deu erro na execução!\n" + "Verifique e tente novamente.");
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (textBox2.Text != string.Empty)
			{
				listBox1.Items.Clear();
				
				AnalisaTXT();
			}
			else
				MessageBox.Show("Por favor, clique em 'Arquivo Sistema'!");
		}
        
		private void AnalisaTXT()
		{
			try
			{
				string path = Path.GetDirectoryName(arquivoSistemaName);
				path += (char)92;
				arquivoRede = File.ReadAllLines(path + arquivoRedeName);
			}
			catch (Exception)
			{
				MessageBox.Show("Não foi possível ler o arquivo \"OPER\". Favor verificar!");
			}

			string position = string.Empty;

			int a = 0;
			foreach (string linhaArquivoRede in arquivoRede)
			{
				string[] arrayLinhaArquivoRede = linhaArquivoRede.Split(';');

				foreach (string linhaArquivoSistema in arquivoSistema)
				{
					string[] arrayLinhaArquivoSistema = linhaArquivoSistema.Split(';');

					if (arrayLinhaArquivoSistema[4] == arrayLinhaArquivoRede[4])
					{
						int i = 0;

						foreach (string indexArrayLinhaArquivoSistema in arrayLinhaArquivoSistema)
						{
							string indexArrayLinhaArquivoRede = arrayLinhaArquivoRede[i];
							position = arrayLinhaArquivoRede[4];
							
							string indexArrayLinhaSistema = string.Empty;

							if (indexArrayLinhaArquivoRede == "0")
								indexArrayLinhaArquivoRede = "";
							
							if (indexArrayLinhaArquivoSistema == "0")
								indexArrayLinhaSistema = "";
							else
								indexArrayLinhaSistema = indexArrayLinhaArquivoSistema;

							if (i == 15)
								indexArrayLinhaArquivoRede = indexArrayLinhaArquivoRede.Replace(".0", "");

							if (indexArrayLinhaSistema != indexArrayLinhaArquivoRede) //(indexArrayLinhaArquivoSistema != indexArrayLinhaArquivoRede)
							{
								switch (i)
								{
									case 4:
									case 6:
									case 7:
									case 8:
									case 9:
									case 10:
									case 11:
									case 12:
									case 13:
									case 14:
									case 15:
									case 16:
										if (!(listBox1.Items.Contains("Verificar " + dicio[i] + " da posição -> " + position)))
											listBox1.Items.Add("Verificar " + dicio[i] + " da posição -> " + position);
										break;
								}
							}
							i += 1;
						}
					}
				}
				a += 1;
			}
			
			Resultado();
		}
        
		private void Resultado()
		{
			if (listBox1.Items.Count == 0 && arquivoRede != null)
				listBox1.Items.Add("Não há diferenças");
		}

		private void Button1Click(object sender, EventArgs e)
		{
			PrintPreviewDialog ppd = new PrintPreviewDialog();
			ppd.Document = document;
			ppd.Document.DocumentName = "Log";
			document.PrintPage += document_PrintPage;
			ppd.ShowDialog();
		}
		
		private void document_PrintPage(object sender, PrintPageEventArgs e)
		{
			e.Graphics.PageUnit = GraphicsUnit.Millimeter;
			const int leading = 2;
			const int leftMargin = 10;
			const int topMargin = 5;
	
			// a few simple formatting options..
			StringFormat FmtRight = new StringFormat() { Alignment = StringAlignment.Near };
			StringFormat FmtLeft = new StringFormat() { Alignment = StringAlignment.Near };
			StringFormat FmtCenter = new StringFormat() { Alignment = StringAlignment.Near };
	
			StringFormat fmt = FmtRight;
	
			using (Font font = new Font("Arial Narrow", 10f))
			{
				SizeF sz = e.Graphics.MeasureString("_|", Font);
				float h = sz.Height + leading;
	
				for (int i = 0; i < listBox1.Items.Count; i++)
					e.Graphics.DrawString(listBox1.Items[i].ToString(), font, Brushes.Black, 
						leftMargin, topMargin + h * i, fmt);
			}
		}

		void Button5Click(object sender, EventArgs e)
		{
			string caminho = File.ReadAllText(@"C:\Temp\caminho.txt");
			string path = Path.GetDirectoryName(caminho);
			string data = (DateTime.Now.ToString()).Replace("/", "-").Replace(":", "-");
			string inicio = Path.GetFileNameWithoutExtension(arquivoSistemaName);
			string arquivo = (path + (char)92 + inicio + "_LogCompara_" + data + ".txt");

			string[] logs = new string[listBox1.Items.Count];

			for (int i = 0; i < listBox1.Items.Count; i++)
			{
				logs[i] = listBox1.GetItemText(listBox1.Items[i]);
			}

			File.WriteAllLines(arquivo, logs);
		}

		void Button6Click(object sender, EventArgs e)
		{
			try
			{
				string caminho = File.ReadAllText(@"C:\Temp\caminho.txt");
				string path = Path.GetDirectoryName(caminho);

				DirectoryInfo files = new DirectoryInfo(path);
				FileInfo[] arquivos = files.GetFiles();
				Array.Sort(arquivos, (FileInfo a, FileInfo b) => DateTime.Compare(a.CreationTime, b.CreationTime));
				Array.Reverse(arquivos);

				var query = from a in arquivos
				            where a.Name.Contains("_LOG_")
				            select a;

				Process.Start("notepad.exe",path + (char)92 + query.First());
			}
			catch
			{
				MessageBox.Show("O LogCompara não foi encontrado!\n" + "Verifique e tente novamente.");
			}
		}
	}
}