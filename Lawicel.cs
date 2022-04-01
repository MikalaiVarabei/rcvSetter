using System;

public class Lawicel
{
    public string sId = "";
    public string sDlc = "";
    public string sMsg = "";
    public int iPeriod = 0;



    public int tsimbolRx (char[] data, int rx_ptr_in)
    {
        rx_ptr_in++;
        //ID
        sId = data[rx_ptr_in++] + "" + data[rx_ptr_in++] + "" + data[rx_ptr_in++];
        //DLC
        sDlc = data[rx_ptr_in] + "";
        int iDlc = ((data[rx_ptr_in++] & 0x0F) * 2);
        if (iDlc > 16) iDlc = 16;
        //MSG
        for (int i = 0; i < iDlc; i += 2)
        {
            sMsg += data[rx_ptr_in++] + "" + data[rx_ptr_in++] + " ";
        }
        //Period
        iPeriod = ((AsciiToHex(data[rx_ptr_in++]) << 12) |
                         (AsciiToHex(data[rx_ptr_in++]) << 8) |
                         (AsciiToHex(data[rx_ptr_in++]) << 4) |
                         (AsciiToHex(data[rx_ptr_in]) << 0));//

        return rx_ptr_in;
    }

    public int TsimbolRx (char[] data, int rx_ptr_in)
    {
        rx_ptr_in++;
        //ID
        sId = data[rx_ptr_in++] + "" + data[rx_ptr_in++] + "" + data[rx_ptr_in++] + "" + data[rx_ptr_in++] + "" +
                data[rx_ptr_in++] + "" + data[rx_ptr_in++] + "" + data[rx_ptr_in++] + "" + data[rx_ptr_in++];
        //DLC
        sDlc = data[rx_ptr_in] + "";
        int iDlc = (data[rx_ptr_in++] & 0x0F) * 2;
        if (iDlc > 16) iDlc = 16;
        //MSG
        for (int i = 0; i < iDlc; i += 2)
        {
            sMsg += data[rx_ptr_in++] + "" + data[rx_ptr_in++] + " ";
        }
        //Period
        iPeriod = ((AsciiToHex(data[rx_ptr_in++]) << 12) |
                         (AsciiToHex(data[rx_ptr_in++]) << 8) |
                         (AsciiToHex(data[rx_ptr_in++]) << 4) |
                         (AsciiToHex(data[rx_ptr_in++]) << 0));//
        return rx_ptr_in;
    }

    public int AsciiToHex(int ascii)
    {
        if ((ascii >= '0') && (ascii <= '9')) return (ascii - '0');
        if ((ascii >= 'A') && (ascii <= 'F')) return (ascii - 'A' + 10);
        if ((ascii >= 'a') && (ascii <= 'f')) return (ascii - 'a' + 10);
        return (0xFF);
    }

    public Lawicel()
	{
	}
}
