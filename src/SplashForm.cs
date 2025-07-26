using System;
using System.Windows.Forms;

namespace SenDev.XafSame;

public partial class SplashForm : Form
{
    public SplashForm(Action initializationAction)
    {
        InitializeComponent();
        Logger = new TextBoxLogger(logTextBox);
        InitializationAction = initializationAction;
    }


    private Exception? InitializationException { get; set; }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        try
        {
            Application.DoEvents();
            InitializationAction.Invoke();
        }
        catch (Exception ex)
        {
            Logger.LogInfo(ex.Message);
            InitializationException = ex;
            showExceptionDetailsButton.Visible = true;
            exitButton.Visible = true;
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void showExceptionDetailsButton_Click(object sender, EventArgs e)
    {
        if (InitializationException == null)
        {
            MessageBox.Show("No exception occurred during initialization.");
            return;
        }
        ThreadExceptionDialog dialog = new ThreadExceptionDialog(InitializationException);
        dialog.ShowDialog(this);
    }

    private void exitButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    internal ILogger Logger { get; }
    private Action InitializationAction { get; }



}

class TextBoxLogger : ILogger
    {
    private readonly TextBox logTextBox;
    public TextBoxLogger(TextBox logTextBox)
    {
        this.logTextBox = logTextBox;
    }
    public void LogError(Exception ex)
    {
        AppendLog($"Error: {ex.ToString()}");
    }
    public void LogInfo(string message)
    {
        AppendLog(message);
    }
    private void AppendLog(string message)
    {
        logTextBox.AppendText($"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}");
        logTextBox.SelectionStart = logTextBox.Text.Length;
        logTextBox.ScrollToCaret();
    }
}
