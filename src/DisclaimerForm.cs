using System;
using System.Windows.Forms;

namespace SenDev.XafSame
{
    public partial class DisclaimerForm : Form
    {
        public DisclaimerForm()
        {
            InitializeComponent();
            disclaimerTextBox.Rtf = @"{\rtf1\ansi
\b Warning:\b0  This editor is a \b prerelease version\b0  and may not be fully stable. It modifies project files.\line
Please ensure \b all project files are backed up\b0  before proceeding.
}""";
        }
    }
}