using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

namespace gov.ncats.ginas.excel.tools.Utils
{
    class UIUtils
    {
        public static void ShowMessageToUser(string message)
        {
            MessageBox.Show(message);
        }

        public static bool GetUserYesNo(string message)
        {
            DialogResult result= MessageBox.Show(message, "Select 'Yes' or 'No' ", 
                MessageBoxButtons.YesNoCancel);
            return (result == DialogResult.Yes);
        }

        public static DialogYesNoCancel GetUserYesNoCancel(string message)
        {
            DialogResult result = MessageBox.Show(message, "Select 'Yes', 'No' or 'Cancel' ",
                MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes) return DialogYesNoCancel.Yes;
            if (result == DialogResult.No) return DialogYesNoCancel.No;
            return DialogYesNoCancel.Cancel;
        }
    }
}
