using Account;
using Base.Boot;
using Exercise.View;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace Exercise
{

    public class ExerciseShell : Shell
    {

        [ImportMany(typeof(IAssistant))]
        private IEnumerable<Lazy<IAssistant, IAssistantMetadata>> assistants;

        public ExerciseShell()
        {
        }

        public override void Initialize()
        {
            if (Application.Current != null)
            {
                assistants.Any(a => a.Value == null);
                Window window = new MainWindow();
                new AccountWindow().ShowDialog();
            }
            else
            {
                assistants.Where(a => a.Metadata.MainProcessOnly == false).Any(a => a.Value == null);
                new Algorithm.Algorithm(false, null).ServiceMain(System.Environment.GetCommandLineArgs());
            }
        }
    }
}
