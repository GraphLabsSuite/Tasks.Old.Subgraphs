using System;

namespace GraphLabs.Tasks.Template
{
    public partial class HelpDialog
    {
        public HelpDialog(String text)
        {
            InitializeComponent();

            Title = Strings.Strings_RU.helpButtonDisc;
            Info.Text = text;
        }
    }
}
