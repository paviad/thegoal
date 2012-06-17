using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace TheGoal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        ObservableCollection<ObservableInt> _Bowls = new ObservableCollection<ObservableInt>(new ObservableInt[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        public ObservableCollection<ObservableInt> Bowls
        {
            get
            {
                return _Bowls;
            }
        }
        Random rnd = new Random();

        int _Inventory;
        public int Inventory
        {
            get { return _Inventory; }
            set
            {
                if (_Inventory != value)
                {
                    _Inventory = value;
                    if (!NotificationInhibiter.IsNotificationInhibited)
                        RaisePropertyChanged(this, x => x.Inventory);
                }
            }
        }
        double _Throughput;
        public double Throughput
        {
            get { return _Throughput; }
            set
            {
                if (_Throughput != value)
                {
                    _Throughput = value;
                    if (!NotificationInhibiter.IsNotificationInhibited)
                        RaisePropertyChanged(this, x => x.Throughput);
                }
            }
        }
        int _Cycles;
        public int Cycles
        {
            get { return _Cycles; }
            set
            {
                if (_Cycles != value)
                {
                    _Cycles = value;
                    if (!NotificationInhibiter.IsNotificationInhibited)
                        RaisePropertyChanged(this, x => x.Cycles);
                }
            }
        }
        double _Profit;
        public double Profit
        {
            get { return _Profit; }
            set
            {
                if (_Profit != value)
                {
                    _Profit = value;
                    if (!NotificationInhibiter.IsNotificationInhibited)
                        RaisePropertyChanged(this, x => x.Profit);
                }
            }
        }
        int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set
            {
                if (_Progress != value)
                {
                    _Progress = value;
                    RaisePropertyChanged(this, x => x.Progress);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitCommands(this);
        }

        void InitCommands<T>(T instance)
        {
            var initMethods = typeof(T)
                .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Where(x => x.Name.StartsWith("Init") && x.Name.EndsWith("Command"))
                .ToDictionary(x => x.Name.Substring(4));

            foreach (var prop in typeof(T).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DelegateCommand<>)))
            {
                if (prop.GetValue(this, null) != null) continue;
                if (!initMethods.ContainsKey(prop.Name))
                    throw new Exception("Command '" + prop.Name + "' does not have an initialization method.");
                initMethods[prop.Name].Invoke(this, null);
                if (prop.GetValue(this, null) == null)
                    throw new Exception("Command '" + prop.Name + "' not properly initialized.");
            }
        }

        public DelegateCommand<int> RollCommand { get; set; }
        void InitRollCommand()
        {
            RollCommand = new DelegateCommand<int>(
                ExecuteRoll,
                x => true);
        }
        void ExecuteRoll(int parameter)
        {
            int n = Roll();
            if (parameter > 0)
            {
                n = Math.Min(Bowls[parameter - 1], n);
                Bowls[parameter - 1].Increment(-n);
            }
            Bowls[parameter].Increment(n);
            if (parameter == _Bowls.Count - 1)
            {
                Cycles++;
                Throughput = 1.0 * Bowls.Last().Value / Cycles;
                Profit = Throughput - Inventory / 10000.0;
            }
            Inventory = Bowls.Reverse().Skip(1).Sum(x => x.Value);
        }

        public DelegateCommand<int> RollAllCommand { get; set; }
        void InitRollAllCommand()
        {
            RollAllCommand = new DelegateCommand<int>(
                ExecuteRollAll,
                x => !NotificationInhibiter.IsNotificationInhibited);
        }
        void ExecuteRollAll(int count)
        {
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += (s, e) =>
            {
                int step = count / 100;
                for (int j = 0; j < count; j++)
                {
                    for (int i = 0; i < _Bowls.Count; i++)
                        ExecuteRoll(i);
                    if (j % step == 0)
                        bgw.ReportProgress((int)(1.0 * j / count * 100.0));
                }
            };
            bgw.RunWorkerCompleted += (s, e) =>
            {
                Progress = 0;
                InhibitNotificationEvents(false);
            };
            bgw.WorkerReportsProgress = true;
            bgw.ProgressChanged += (s, e) =>
            {
                Progress = e.ProgressPercentage;
            };
            InhibitNotificationEvents(true);
            bgw.RunWorkerAsync();
        }

        private void InhibitNotificationEvents(bool inhibit)
        {
            NotificationInhibiter.IsNotificationInhibited = inhibit;
            RollAllCommand.RaiseCanExecuteChanged();
            ResetCommand.RaiseCanExecuteChanged();
            if (!inhibit)
            {
                RaisePropertyChanged(this, x => x.Inventory);
                RaisePropertyChanged(this, x => x.Cycles);
                RaisePropertyChanged(this, x => x.Throughput);
                RaisePropertyChanged(this, x => x.Profit);
                foreach (var item in Bowls)
                {
                    item.Notify();
                }
            }
        }

        public DelegateCommand<object> ResetCommand { get; set; }
        void InitResetCommand()
        {
            ResetCommand = new DelegateCommand<object>(
                ExecuteReset,
                x => !NotificationInhibiter.IsNotificationInhibited);
        }
        void ExecuteReset(object parameter)
        {
            foreach (var item in Bowls)
                item.Value = 0;
            Cycles = 0;
            Inventory = 0;
            Profit = 0;
            Throughput = 0;
        }

        private int Roll()
        {
            return rnd.Next(1, 6);
        }

        protected void RaisePropertyChanged<T, R>(T sender, Expression<Func<T, R>> x)
        {
            var body = x.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("'x' should be a member expression");

            string propertyName = body.Member.Name;

            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(sender, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
