using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace rcv
{
    public partial class Form1 : Form
    {
        string path = @"rcv.ini";//файл инициализации
        private string portName = "";
        static _serialPort comport = new _serialPort();
        static Lawicel lawicel = new Lawicel();

        DataTable canTable = new DataTable();
        DataTable swrTable = new DataTable();
        DataTable accTable = new DataTable();
        DataTable illTable = new DataTable();
        
        public Form1()
        {
            InitializeComponent();
            initDataTable();
            initTable();

            //CallBack.callbackEventHandler = new CallBack.callbackEvent(this.dataToTable);
            //CallBack.callbackEventHandler = new CallBack.callbackEvent(this.getData);
            comport.DataReceived += SerialPort_DataReceived;

            dgvReceive.CellValueChanged +=
            new DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            dgvReceive.DataError += new DataGridViewDataErrorEventHandler(dgvReceive_DataError);
            dgvSwrReceive.DataError += new DataGridViewDataErrorEventHandler(dgvSwrReceive_DataError);
            dgvAccReceive.DataError += new DataGridViewDataErrorEventHandler(dgvAccReceive_DataError);
            dgvIllReceive.DataError += new DataGridViewDataErrorEventHandler(dgvIllReceive_DataError);
            

            initFile();
            //
        }
        public void dgvReceive_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            //MessageBox.Show("Ошибка ввода!");
            anError.ThrowException = false;
        }
        public void dgvSwrReceive_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            //MessageBox.Show("Ошибка ввода!");
            anError.ThrowException = false;
        }
        public void dgvAccReceive_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            //MessageBox.Show("Ошибка ввода!");
            anError.ThrowException = false;
        }
        public void dgvIllReceive_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            //MessageBox.Show("Ошибка ввода!");
            anError.ThrowException = false;
        }

        //**********************************************************************
        //кнопка подключения-отключения к COMPORT
        //**********************************************************************
        private bool openClkFlg = false;
        private bool refreshDgv = false;
        //открываем COM порт
        private void open_Click(object sender, EventArgs e)
        {
            if (openClkFlg == false)
            {
                portName = comPortList.Text;
                // comport.NamePort(portName);
                if (comport.NamePort(portName) == true)//если имя порта сущесвует
                {
                    openClkFlg = comport.Open();//если порт открыли
                    if (openClkFlg)
                    {
                        comPortList.Enabled = false;//деактивируем выподающее меню
                        canSpeedList.Enabled = true;//активируем выподающее меню
                        tabControl1.SelectedIndex = 0;
                        this.openBtt.Image = global::rcv.Properties.Resources.cBtt;//меняем картинку на кнопке
                        refreshDgv = false;
                    }
                }
            }
            else
            {
                comport.Clos();
                comPortList.Enabled = true;//активируем выподающее меню
                canSpeedList.Enabled = false;//деактивируем выподающее меню
                this.openBtt.Image = global::rcv.Properties.Resources.oBtt;//меняем картинку на кнопке
                openClkFlg = false;
            }
        }
        //**********************************************************************
        //кнопка отключения от COMPORT (скрыта)
        //**********************************************************************
        private void close_Click(object sender, EventArgs e)
        {
            comport.Clos();
            comPortList.Enabled = true;
            openClkFlg = false;
        }
        //**********************************************************************
        //выбор COM PORT
        //**********************************************************************
        private void comPortList_DropDown(object sender, EventArgs e)
        {
            //составляем список доступных портов
            string[] ports = SerialPort.GetPortNames();
            comPortList.Items.Clear();
            //заполняем список доступных портов
            foreach (string port in ports)
            {
                comPortList.Items.Add(port);
            }
        }
        //**********************************************************************
        //заполнение списка скорости CAN BUS
        //**********************************************************************
        private void canSpeedList_DropDown(object sender, EventArgs e)
        {
            //составляем список
            string[] canSpeed = new string[13];
            canSpeed[0] = "10 Kbit/s";
            canSpeed[1] = "20 Kbit/s";
            canSpeed[2] = "50 Kbit/s";
            canSpeed[3] = "100 Kbit/s";
            canSpeed[4] = "125 Kbit/s";
            canSpeed[5] = "250 Kbit/s";
            canSpeed[6] = "500 Kbit/s";
            canSpeed[7] = "800 Kbit/s";
            canSpeed[8] = "1 Mbit/s";
            canSpeed[9] = "33.333 Kbit/s";
            canSpeed[10] = "47.619 Kbit/s";
            canSpeed[11] = "83.333 Kbit/s";
            canSpeed[12] = "95.238 Kbit/s";
            //очищаем
            canSpeedList.Items.Clear();
            // Display each port name to the console.
            foreach (string spd in canSpeed)
            {
                canSpeedList.Items.Add(spd);
            }

            canCmd[0] = 'C';//отключаем CAN
            comport.Send(canCmd, 1);
            label1.Text = canCmd[0].ToString();
        }
        //**********************************************************************
        //выбор скорости CAN BUS из списка
        //**********************************************************************
        char[] canCmd = new char[20];
        private void canSpeedList_DropDownClosed(object sender, EventArgs e)
        {
            canCmd[0] = 'O';
            comport.Send(canCmd, 1);
            label1.Text = canSpeedList.SelectedIndex.ToString();
            ActiveControl = openBtt;
        }
        private void canSpeedList_SelectedValueChanged(object sender, EventArgs e)
        {
            canCmd[0] = 'O';
            comport.Send(canCmd, 1);
            label1.Text = canSpeedList.SelectedIndex.ToString();
        }
        //**********************************************************************
        //наименование магнитолы
        //**********************************************************************
        private void brandsList_DropDown(object sender, EventArgs e)
        {
            //составляем список
            string[] brand = new string[6];
            brand[0] = "Alpine";
            brand[1] = "Clarion";
            brand[2] = "JVC";
            brand[3] = "Kenwood";
            brand[4] = "Pioneer";
            brand[5] = "User";
            //очищаем
            brandsList.Items.Clear();
            // Display each port name to the console.
            foreach (string brd in brand)
            {
                brandsList.Items.Add(brd);
            }
        }
        //**********************************************************************
        //чек боксы на tabHome
        //**********************************************************************
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((dgvReceive.Columns[e.ColumnIndex].Name == "swr") && e.RowIndex != -1)
            {
                int rowIndex = dgvReceive.CurrentRow.Index;
                string rowIdName = (string)dgvReceive["id", e.RowIndex].Value;
                outTextBox.Text = rowIndex.ToString() + "  " + rowIdName + "\r\n";
                for (int n = 0; n < canTable.Rows.Count; n++)
                {
                    if ((string)canTable.Rows[n]["id"] == rowIdName) canTable.Rows[n]["swr"] = true;//устанавливаем чекбокс
                    else canTable.Rows[n]["swr"] = false;                                   //сбрасываем остальные чекбоксы
                }
                tabSwr.Text = " SWR ID=" + rowIdName + " ";
            }
            else if ((dgvReceive.Columns[e.ColumnIndex].Name == "acc") && e.RowIndex != -1)
            {
                int rowIndex = dgvReceive.CurrentRow.Index;
                string rowIdName = (string)dgvReceive["id", e.RowIndex].Value;
                outTextBox.Text = rowIndex.ToString() + "  " + rowIdName + "\r\n";
                for (int n = 0; n < canTable.Rows.Count; n++)
                {
                    if ((string)canTable.Rows[n]["id"] == rowIdName) canTable.Rows[n]["acc"] = true;
                    else canTable.Rows[n]["acc"] = false;
                }
                tabAcc.Text = " ACC ID=" + rowIdName + " ";
            }
            else if ((dgvReceive.Columns[e.ColumnIndex].Name == "ill") && e.RowIndex != -1)
            {
                int rowIndex = dgvReceive.CurrentRow.Index;
                string rowIdName = (string)dgvReceive["id", e.RowIndex].Value;
                outTextBox.Text = rowIndex.ToString() + "  " + rowIdName + "\r\n";
                for (int n = 0; n < canTable.Rows.Count; n++)
                {
                    if ((string)canTable.Rows[n]["id"] == rowIdName) canTable.Rows[n]["ill"] = true;
                    else canTable.Rows[n]["ill"] = false;
                }
                tabIll.Text = " ILL ID=" + rowIdName + " ";
            }
            //
        }
        //**********************************************************************

        //**********************************************************************
        //инициализация таблицы
        //**********************************************************************
        private void initTable()
        {
            dgvReceive.RowHeadersVisible = false;
            dgvReceive.ClearSelection();
            dgvSwrReceive.ClearSelection();
            //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            //dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            //dataGridView1.Columns["Название_колонки"].Visible = false;
            //dataGridView1.Columns[индекс_колонки].Visible = false;
            //dataGridView1.SelectedCells[0].Selected = false;
           
            //текст подсказок
            DataGridViewColumn textID = dgvReceive.Columns["id"];
            DataGridViewColumn textEXT = dgvReceive.Columns["ext"];
            DataGridViewColumn textRTR = dgvReceive.Columns["rtr"];
            DataGridViewColumn textDLC = dgvReceive.Columns["dlc"];
            DataGridViewColumn textMSG = dgvReceive.Columns["msg"];
            DataGridViewColumn textPER = dgvReceive.Columns["period"];
            DataGridViewColumn textCNT = dgvReceive.Columns["count"];
            DataGridViewColumn textSWR = dgvReceive.Columns["swr"];
            DataGridViewColumn textACC = dgvReceive.Columns["acc"];
            DataGridViewColumn textILL = dgvReceive.Columns["ill"];
            textID.ToolTipText = " Идентификатор сообщения";
            textEXT.ToolTipText = " Тип идентификатора.\r 0-Стандартный.\r 1-Расширенный.";
            textRTR.ToolTipText = " Сообщения RTR";
            textDLC.ToolTipText = " Количесво байт в сообщении";
            textMSG.ToolTipText = " Сообщения CAN шины";
            textPER.ToolTipText = " Период следования сообщений, мс.";
            textCNT.ToolTipText = " Счетчик сообщений";
            textSWR.ToolTipText = "  Отметьте сообщения кнопок \rдистанционного управления на руле";
            textACC.ToolTipText = "  Отметьте сообщения для \rуправления выходом Аксессуары";
            textILL.ToolTipText = "  Отметьте сообщения для \rуправления выходом Подсветка";

            //
        }
        //**********************************************************************

        //**********************************************************************
        //DataTable
        //**********************************************************************
        private void initDataTable()
        {
            //столбцы RESEIVE
            canTable.Columns.Add("id", typeof(string));
            //canTable.Columns["id"].Unique = true;   //запрет повторяющихся
            //canTable.Columns.Add("ext", typeof(int));
            canTable.Columns.Add("dlc", typeof(string));
            canTable.Columns.Add("msg", typeof(string));
            canTable.Columns.Add("periodBuff", typeof(int));
            canTable.Columns.Add("period", typeof(string));
            canTable.Columns.Add("count", typeof(int));
            canTable.Columns.Add("swr", typeof(bool));
            canTable.Columns.Add("acc", typeof(bool));
            canTable.Columns.Add("ill", typeof(bool));
            //Столбцы SWR
            swrTable.Columns.Add("swrId", typeof(string));
            swrTable.Columns.Add("swrDlc", typeof(string));
            swrTable.Columns.Add("swrMsg", typeof(string));
            swrTable.Columns.Add("swrPeriod", typeof(string));
            swrTable.Columns.Add("swrCount", typeof(int));
            //Столбцы ACC
            accTable.Columns.Add("accId", typeof(string));
            accTable.Columns.Add("accDlc", typeof(string));
            accTable.Columns.Add("accMsg", typeof(string));
            accTable.Columns.Add("accPeriod", typeof(string));
            accTable.Columns.Add("accCount", typeof(int));
            //Столбцы ILL
            illTable.Columns.Add("illId", typeof(string));
            illTable.Columns.Add("illDlc", typeof(string));
            illTable.Columns.Add("illMsg", typeof(string));
            illTable.Columns.Add("illPeriod", typeof(string));
            illTable.Columns.Add("illCount", typeof(int));
            
            //Receive
            dgvReceive.AutoGenerateColumns = false;  //автогенерация таблицы
            dgvReceive.DataSource = canTable;        //источник данных Receive
            //Swr
            dgvSwrReceive.AutoGenerateColumns = false;  //автогенерация таблицы
            dgvSwrReceive.DataSource = swrTable;        //SWR
            swrTable.Rows.Add();
            //Acc
            dgvAccReceive.AutoGenerateColumns = false;  //автогенерация таблицы
            dgvAccReceive.DataSource = accTable;        //Acc
            accTable.Rows.Add();            
            //Ill
            dgvIllReceive.AutoGenerateColumns = false;  //автогенерация таблицы
            dgvIllReceive.DataSource = illTable;        //Ill
            illTable.Rows.Add();

            //DGV RECEIVE
            //dataGridView1.Columns["sort"].DataPropertyName = "ext";//для нормальной сортировки по id
            dgvReceive.Columns["id"].DataPropertyName = "id";
            //dataGridView1.Columns["ext"].DataPropertyName = "ext";
            dgvReceive.Columns["dlc"].DataPropertyName = "dlc";
            dgvReceive.Columns["msg"].DataPropertyName = "msg";
            dgvReceive.Columns["period"].DataPropertyName = "period";
            dgvReceive.Columns["count"].DataPropertyName = "count";
            dgvReceive.Columns["swr"].DataPropertyName = "swr";
            dgvReceive.Columns["acc"].DataPropertyName = "acc";
            dgvReceive.Columns["ill"].DataPropertyName = "ill";
            //DGV SWR
            dgvSwrReceive.Columns["swrId"].DataPropertyName = "swrId";
            dgvSwrReceive.Columns["swrDlc"].DataPropertyName = "swrDlc";
            dgvSwrReceive.Columns["swrMsg"].DataPropertyName = "swrMsg";
            dgvSwrReceive.Columns["swrPeriod"].DataPropertyName = "swrPeriod";
            dgvSwrReceive.Columns["swrCount"].DataPropertyName = "swrCount";
            //DGV ACC
            dgvAccReceive.Columns["accId"].DataPropertyName = "accId";
            dgvAccReceive.Columns["accDlc"].DataPropertyName = "accDlc";
            dgvAccReceive.Columns["accMsg"].DataPropertyName = "accMsg";
            dgvAccReceive.Columns["accPeriod"].DataPropertyName = "accPeriod";
            dgvAccReceive.Columns["accCount"].DataPropertyName = "accCount";
            //DGV ILL
            dgvIllReceive.Columns["illId"].DataPropertyName = "illId";
            dgvIllReceive.Columns["illDlc"].DataPropertyName = "illDlc";
            dgvIllReceive.Columns["illMsg"].DataPropertyName = "illMsg";
            dgvIllReceive.Columns["illPeriod"].DataPropertyName = "illPeriod";
            dgvIllReceive.Columns["illCount"].DataPropertyName = "illCount";
        }
        //**********************************************************************

        //**********************************************************************
        // загрузка начальных параметров из ini фаила
        //**********************************************************************
        private void initFile()
        {
            try
            {
                // Open the stream and read it back.
                using (StreamReader sr = File.OpenText(path))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        comPortList.Text = s;
                    }
                }
            }
            catch (Exception ex)
            {
                outTextBox.Text = "Не найден фаил rcv.ini\r\n";
            }
        }



        //**********************************************************************

        //*********************************************************************
        //эта функция вызвется каждый раз, 
        //когда в порт что-то будет передано от вашего устройства
        //*********************************************************************
        private const int DataSize = 2080;    //число в байтах
        private int rx_ptr_in = 0;
        private int rx_ptr_out = 0;
        private int rx_length = 0;
        private char[] data = new char[DataSize];
        private char[] dataBuffer = new char[DataSize];
        //  
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = (SerialPort)sender;
            try
            {
                //string message = port.ReadLine();
                string message = port.ReadExisting();       //читаем данные из com порта
                //serialText(message + " ");

                data = message.ToCharArray();
                rx_length = data.Length;                    //длина массива
                //serialText(rx_length + " ");
                //CallBack.callbackEventHandler(data, rx_length);

                for (rx_ptr_in = 0; rx_ptr_in < rx_length; rx_ptr_in++)
                {
                    switch (data[rx_ptr_in])
                    {
                        case 't'://STD ID
                            dataToTable();
                            break;

                        case 'T'://EXT ID
                            dataToTable();
                            break;

                        case '\r'://CR
                            break;

                        case '\a'://BEL
                            break;

                        default:
                            break;
                    }
                }
                //*/
            }
            catch { }
        }        
        //********************************************************************** 
        //Добавление данных в таблицу
        //********************************************************************** 
        //string sId = "";
        //string sDlc = "";
        //string sMsg = "";
        //int iPeriod = 0;
        private string sPeriod = "";
        private int iCount = 1;
        private bool updateFlg = true;
        private void dataToTable()
        {
                lawicel.sMsg = "";
                //оработка данных COM порта
                if (data[rx_ptr_in] == 't')  //STD ID
                {
                    rx_ptr_in = lawicel.tsimbolRx(data, rx_ptr_in);
                }
                else if (data[rx_ptr_in] == 'T')  //EXT ID
                {
                    rx_ptr_in = lawicel.TsimbolRx(data, rx_ptr_in);
                }

                int n;
                for (n = 0; n < canTable.Rows.Count; n++)//ищем совпадение по ID и DLC
                {
                    if ((string)canTable.Rows[n]["id"] == lawicel.sId)//если нашли по ID
                    {
                        if ((string)canTable.Rows[n]["dlc"] == lawicel.sDlc)//если нашли по DLC - обновляем
                        {
                            canTable.Rows[n]["msg"] = lawicel.sMsg;//обновляем сообщения
                            if (lawicel.iPeriod < (int)canTable.Rows[n]["periodBuff"])//расчитываем период
                            {
                                sPeriod = (60000 + lawicel.iPeriod - (int)canTable.Rows[n]["periodBuff"]).ToString("D");
                            }
                            else
                            {
                                sPeriod = (lawicel.iPeriod - (int)canTable.Rows[n]["periodBuff"]).ToString("D");
                            }
                            //sPeriod = iPeriod.ToString("D");
                            canTable.Rows[n]["periodBuff"] = lawicel.iPeriod;//обновляем период
                            canTable.Rows[n]["period"] = sPeriod;
                            iCount = (int)canTable.Rows[n]["count"];//обновляем счетчик сообщений
                            canTable.Rows[n]["count"] = (iCount + 1);
                            //обновляем вкладку SWR Receive
                            if ((bool)canTable.Rows[n]["swr"] == true)
                            {
                                swrTable.Rows[0]["swrId"] = lawicel.sId;
                                swrTable.Rows[0]["swrDlc"] = lawicel.sDlc;
                                swrTable.Rows[0]["swrMsg"] = lawicel.sMsg;
                                swrTable.Rows[0]["swrPeriod"] = sPeriod;
                                swrTable.Rows[0]["swrCount"] = (int)canTable.Rows[n]["count"];
                            }
                            //обновляем вкладку ACC Receive
                            if ((bool)canTable.Rows[n]["acc"] == true)
                            {
                                accTable.Rows[0]["accId"] = lawicel.sId;
                                accTable.Rows[0]["accDlc"] = lawicel.sDlc;
                                accTable.Rows[0]["accMsg"] = lawicel.sMsg;
                                accTable.Rows[0]["accPeriod"] = sPeriod;
                                accTable.Rows[0]["accCount"] = (int)canTable.Rows[n]["count"];
                            }
                            //обновляем вкладку ILL Receive
                            if ((bool)canTable.Rows[n]["ill"] == true)
                            {
                                illTable.Rows[0]["illId"] = lawicel.sId;
                                illTable.Rows[0]["illDlc"] = lawicel.sDlc;
                                illTable.Rows[0]["illMsg"] = lawicel.sMsg;
                                illTable.Rows[0]["illPeriod"] = sPeriod;
                                illTable.Rows[0]["illCount"] = (int)canTable.Rows[n]["count"];
                            }
                            break;
                        }
                    }
                }
                if (n == canTable.Rows.Count)//заносим новые данные
                {
                    canTable.Rows.Add(lawicel.sId, lawicel.sDlc, lawicel.sMsg, lawicel.iPeriod, sPeriod, iCount, false, false, false);
                }

        }

        //**********************************************************************
        //для отладки
        //**********************************************************************
        private void sendButton_Click(object sender, EventArgs e)
        {
            string text = this.tb_send.Text; //@"t10080123456789ABCDEF4D67\r";
            Regex regex = new Regex("");
            string[] arrText = regex.Split(text);
            byte[] data = new byte[arrText.Length];
            Encoding ascii = Encoding.ASCII;
            data = ascii.GetBytes(text);

            char[] charData = new char[arrText.Length];
            for (int i = 0; i < (arrText.Length-2); i++ )
            {
                charData[i] = (char)data[i];
            }

                //dataToTable(charData, 1);
            /*
            if ((comport.asyncReceive(dspBuffer)) == 't')
            {
                dataDisplay(dspBuffer);
            }
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string sID = "";
            byte[] bdata = new byte[30];
            bdata[0] = 0x31;
            bdata[1] = 0x41;
            bdata[2] = 0x41;
            
            char[] data = new char[3] {(char)49, (char)0x41, (char)0x41};
            ASCIIEncoding ascii = new ASCIIEncoding();
            
            //sID = ascii.GetString(bdata);

            sID = data[0] + "" + data[1] + "" + data[2];
            //byte[] bdata = new byte[3] {49,49,49};

            canTable.Rows.Add(sID, 8, "01 23 45 67 89 AB CD EF", 100, 1, true, false, true);
            canTable.Rows.Add(0x100.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 1000, 1, true, true, false);
            canTable.Rows.Add(0x000.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 100, 1, false, true, false);
            canTable.Rows.Add(0x1F4.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 100, 1, false, true, false);
            canTable.Rows.Add(0x100000FD.ToString("X8"), 8, "01 23 45 67 89 AB CD EF", 1800, 1, true, true, true);
            canTable.Rows.Add(0x105.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 1200, 1, true, true, true);
            canTable.Rows.Add(0x1FF.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 1300, 1, true, true, true);
            canTable.Rows.Add(0x1AD.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 1800, 1, true, true, true);
            canTable.Rows.Add(0x00000000.ToString("X8"), 8, "01 23 45 67 89 AB CD EF", 1800, 1, true, true, true);
            canTable.Rows.Add(0x10D.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 2000, 1, true, true, true);
            canTable.Rows.Add(0x1CA.ToString("X3"), 8, "01 23 45 67 89 AB CD EF", 2500, 1, true, true, true);
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //dataGridView1.Sort(dataGridView1.Columns["sort"], ListSortDirection.Ascending);//

        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            outTextBox.Text = " " + e.TabPageIndex;
        }


    }
}
