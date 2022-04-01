using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rcv
{
    //  Наследуем наш клас от SerialPort для более красивого кода
    public class _serialPort : SerialPort
    {
        public _serialPort()
            : base()
        {

            //  все папаметры вы должны указать в соответствии с вашим устройством
            //base.PortName = "COM4";//;
            base.BaudRate = 1000000;
            base.DataBits = 8;
            base.StopBits = StopBits.One;
            base.Parity = Parity.None;
            base.ReadTimeout = 100;
            base.ReadBufferSize = 2048;
            //base.NewLine = "\r";


            //base.DataReceived += SerialPort_DataReceived;
            //base.PinChanged += SerialPort_PinChanged;
        }
        //назначаем порт
        public bool NamePort(string portName)
        {
            try
            {
                base.PortName = portName;
                return true;
            }
            catch
            {
                serialText("Необходимо выбрать COM порт\r\n");
                return false;
            }
        }
        
        //  открываем порт передав туда имя
        public bool Open()
        {
            if (base.IsOpen)
            {
                base.Close();
            }
            try
            {
                base.Open();
                //serialText("\r\n");
                serialText("порт " + base.PortName + " открыт" + "\r\n");
                return true;
            }
            catch (Exception ex)
            {
                serialText("Не удалось подключиться к  "+ base.PortName +"\r\n");
                return false;
            }
        }
        //  закрываем порт передав туда имя
        public void Clos()
        {
            try
            {
                //base.PortName = portName; 
                base.Close();

                serialText("порт закрыт");
                serialText("\r\n");
            }
            catch (Exception ex)
            {
                serialText("Не удалось закрыть\r\n");
                base.Dispose();
                //return false;
            }
        }

        public void Send(char[] buf, int count)
        {
            if (base.IsOpen)
            {
                base.Write(buf, 0, count);
            }
            else
            {
                serialText("Port Not Opened");
                serialText("\r\n");
            }
        }

        private void serialText(string txt)//all text
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is Form1)
                {
                    if (txt == "Scroll") ((Form1)form).outTextBox.ScrollToCaret();
                    else if (((Form1)form).outTextBox.InvokeRequired) ((Form1)form).outTextBox.Invoke(new Action<string>((s) => ((Form1)form).outTextBox.AppendText(s)), txt);
                    else ((Form1)form).outTextBox.AppendText(txt);
                }
            }
        }

        //*********************************************************************
        //эта функция вызвется 
        //при удалении устройства
        //*********************************************************************
/*        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            if ((e.EventType == SerialPinChange.DsrChanged) || (e.EventType == SerialPinChange.Break))
            {
                SerialPort sp = (SerialPort)sender;
                try
                {
                    if (sp.IsOpen)
                        sp.Close();
                }
                catch
                {

                }
                finally
                {
                    sp.Dispose();
                }
            }
        }
*/

        //*********************************************************************
        //эта функция вызвется каждый раз, 
        //когда в порт что-то будет передано от вашего устройства
        //*********************************************************************
/*        private const int DataSize = 4096;    //число в байтах
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
                    switch(data[rx_ptr_in])
                    {
                        case 't'://STD ID
                            if(rx_length - rx_ptr_in < 5)
                                break;
                            dataBuffer[0] = 't';
                            rx_ptr_in++;
                            rx_ptr_out = 1;
                            for (; rx_ptr_in < rx_length; rx_ptr_in++)
                            {
                                dataBuffer[rx_ptr_out] = data[rx_ptr_in];
                                if ((dataBuffer[rx_ptr_out]) == '\r')
                                {
                                    if(rx_ptr_out < 26)
                                        CallBack.callbackEventHandler(dataBuffer, rx_ptr_out);
                                    //serialText(rx_ptr_out + "\r\n");
                                    break;
                                }
                                //serialText(dataBuffer[rx_ptr_out] + " ");
                                rx_ptr_out++;

                            }
                            break;

                        case 'T'://EXT ID
                            dataBuffer[0] = 'T';
                            rx_ptr_in++;
                            rx_ptr_out = 1;
                            for (; rx_ptr_in < rx_length; rx_ptr_in++)
                            {
                                dataBuffer[rx_ptr_out] = data[rx_ptr_in];
                                if ((dataBuffer[rx_ptr_out]) == '\r')
                                {
                                    if(rx_ptr_out < 31)
                                        CallBack.callbackEventHandler(dataBuffer, rx_ptr_out);
                                    //serialText(rx_ptr_out + "\r\n");
                                    break;
                                }
                                //serialText(dataBuffer[rx_ptr_out] + " ");
                                rx_ptr_out++;

                            }
                            break;

                        case '\r'://CR
                            break;
                        
                        case '\a'://BEL
                            break;

                        default:
                            break;
                    }
                }

            }
            catch { }
        }
//*/




        //*********************************************************************
        //
        //*********************************************************************
/*        
        private void sendDataOut(byte[] data, int length)//
        {
            CallBack.callbackEventHandler(data, length);
        }
*/

    }
}
