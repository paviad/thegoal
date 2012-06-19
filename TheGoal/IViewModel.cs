using System;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Collections.ObjectModel;
using ServiceModelEx;

namespace TheGoal
{
    public interface IViewModel
    {
        ObservableCollection<ObservableInt> Bowls { get; }

        int Cycles { get; set; }
        int Inventory { get; set; }
        double Profit { get; set; }
        int Progress { get; set; }
        double Throughput { get; set; }
        int VariableCount { get; set; }

        DelegateCommand<object> ResetCommand { get; set; }
        DelegateCommand<int> RollAllCommand { get; set; }
        DelegateCommand<int> RollCommand { get; set; }
        DelegateCommand<object> CancelCommand { get; set; }
    }
}
