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
        private IEnumerable<Lazy<IAssistant, IAssistantMetadata>> assistants = null;

        public ExerciseShell()
        {
            if (Application.Current != null)
                Application.Current.MainWindow = new MainWindow();
        }

        public override void Initialize()
        {
            if (Application.Current != null)
            {
                assistants.Any(a => a.Value == null);
            }
            else
            {
                assistants.Where(a => a.Metadata.MainProcessOnly == false).Any(a => a.Value == null);
                new Algorithm.Algorithm(false, null).ServiceMain(System.Environment.GetCommandLineArgs());
            }
        }
    }
}
