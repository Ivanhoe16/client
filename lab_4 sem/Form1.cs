using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using Tesseract;
using System.Net.Sockets;

namespace lab_4_sem
{
    public partial class Form1 : Form
    {
        private string picturePath = "";
        private string host = "127.0.0.1";
        private int port = 8085;

        public Form1()
        {
            InitializeComponent();
            getTable();
        }

        private void loadTable()
        {
            while (DBTable.RowCount > 0)
                DBTable.Rows.RemoveAt(0);
            getTable();
        }
        //post sampler
        private void sendData(string name, string phone)
        {
            error.Text = "";
            try
            {
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(host), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(point);
                string message = name + ";" + phone + ";";
                byte[] data = Encoding.Unicode.GetBytes(message);
                socket.Send(data);
                socket.Close();
            }catch (Exception e)
            {
                error.Text = "Ошибка подключения";
            }
        }
        private void getTable()
        {
            error.Text = "";
            try
            {
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(host), port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(point);
                byte[] data = Encoding.Unicode.GetBytes("get");
                socket.Send(data);
                data = new byte[256];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (socket.Available > 0);
                string a = builder.ToString();
                string[] phones = builder.ToString().Split(';');
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                for (int i = 0; i < phones.Length - 1; i += 2)
                    DBTable.Rows.Add(phones[i], phones[i + 1]);
            }catch (Exception e)
            {
                error.Text = "Ошибка подключения";
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
                loadTable();
        }
        //добавить в БД
        private void addButton_Click(object sender, EventArgs e)
        {
            sendData(nameTextBox.Text, phoneTextBox.Text);
            nameTextBox.Text = phoneTextBox.Text = "";
            loadTable();
        }
        //загрузить в эксель
        private void excelButton_Click(object sender, EventArgs e)
        {
            Excel.Application application = new Excel.Application();
            Excel.Workbook workbook;
            Excel.Worksheet worksheet;
            object misValue = System.Reflection.Missing.Value;
            workbook = application.Workbooks.Add(misValue);
            worksheet = workbook.ActiveSheet;
            worksheet.Name = "2022";
            worksheet.Cells[1, 1] = "Фамилия Имя Отчество";
            worksheet.Cells[1, 2] = "Номер телефона";
            for (int i = 0; i < DBTable.RowCount; ++i)
            {
                worksheet.Cells[i + 2, 1] = DBTable.Rows[i].Cells[0].Value;
                worksheet.Cells[i + 2, 2] = DBTable.Rows[i].Cells[1].Value;
            }
            workbook.SaveAs("адресная книга.xls", Excel.XlFileFormat.xlWorkbookNormal,misValue,misValue,misValue,misValue,Excel.XlSaveAsAccessMode.xlExclusive,misValue,misValue,misValue,misValue,misValue);
            workbook.Close(true, misValue, misValue);
            application.Quit();
        }
        //загрузка изображения
        private void loadPictureButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (open.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Image = new Bitmap(open.FileName);
                picturePath = open.FileName;
            }
        }
        //преобразование изображения в текст
        private void readButton_Click(object sender, EventArgs e)
        {
            var engine = new TesseractEngine("C:\\Users\\user\\Desktop\\универ\\мага\\lab_4 sem\\lab_4 sem\\bin\\Debug\\tessdata", "rus+eng", EngineMode.Default);
            var picture = Pix.LoadFromFile(picturePath);
            var result = engine.Process(picture);
            readedTextBox.Text = result.GetText();
        }
    }
}
