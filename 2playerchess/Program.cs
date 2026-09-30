using System;
using System.IO;
using System.Windows.Forms;
using ChessApp.Tests;
using ChessApp.UI;

namespace ChessApp;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test")
        {
            ChessEngineTests.RunAllTests();
            return;
        }

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_error.txt");
            File.WriteAllText(logPath, ex.ToString());
            MessageBox.Show($"Application Error:\n{ex.Message}\n\nDetails saved to: {logPath}", "Chess App Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}