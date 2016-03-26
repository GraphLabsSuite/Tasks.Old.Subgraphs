using System;

namespace GraphLabs.Tasks.Template
{
    public partial class HelpDialog
    {
        public HelpDialog(String text)
        {
            InitializeComponent();

            Title = Strings.Strings_RU.buttonHelp;
            Info.Text = text;
        }
    }
}
