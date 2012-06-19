using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows;
using System.Windows.Input;
using ServiceModelEx;
using System.Transactions;

namespace TheGoal
{
    [Serializable]
    public class ObservableInt : INotifyPropertyChanged
    {
        private Transactional<int> _Value = new Transactional<int>();
        public int Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (_Value != value)
                {
                    _Value.Value = value;
                    if (!NotificationInhibiter.IsNotificationInhibited)
                        RaisePropertyChanged(this, x => x.Value);
                }
            }
        }
        Transactional<bool> _IsFinal = new Transactional<bool>();
        public bool IsFinal
        {
            get { return _IsFinal; }
            set
            {
                if (_IsFinal != value)
                {
                    _IsFinal.Value = value;
                    RaisePropertyChanged(this, x => x.IsFinal);
                }
            }
        }
        Transactional<int> _Minimum = new Transactional<int>();
        public int Minimum
        {
            get { return _Minimum; }
            set
            {
                if (_Minimum != value)
                {
                    _Minimum.Value = value;
                    RaisePropertyChanged(this, x => x.Minimum);
                }
            }
        }
        Transactional<int> _Maximum = new Transactional<int>();
        public int Maximum
        {
            get { return _Maximum; }
            set
            {
                if (_Maximum != value)
                {
                    _Maximum.Value = value;
                    RaisePropertyChanged(this, x => x.Maximum);
                }
            }
        }

        public ObservableInt(int value)
        {
            _Value.Value = value;
            _Minimum.Value = 1;
            _Maximum.Value = 6;
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

        [NonSerialized]
        private PropertyChangedEventHandler _myEvent;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    _myEvent += value;
                }
            }
            remove
            {
                lock (this)
                {
                    _myEvent -= value;
                }
            }
        }
        protected void RaisePropertyChanged<T, R>(T sender, Expression<Func<T, R>> x)
        {
            var body = x.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("'x' should be a member expression");

            string propertyName = body.Member.Name;

            PropertyChangedEventHandler handler = this._myEvent;

            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(sender, e);
            }
        }

        public static implicit operator int(ObservableInt oint) { return oint.Value; }
        public static implicit operator ObservableInt(int value) { return new ObservableInt(value); }

        public void Increment(int p)
        {
            Value += p;
        }
        public void Notify()
        {
            RaisePropertyChanged(this, x => x.Value);
            RaisePropertyChanged(this, x => x.IsFinal);
            RaisePropertyChanged(this, x => x.Minimum);
            RaisePropertyChanged(this, x => x.Maximum);
        }

        public DelegateCommand<object> ShowPropertiesCommand { get; set; }
        void InitShowPropertiesCommand()
        {
            ShowPropertiesCommand = new DelegateCommand<object>(
                ExecuteShowProperties,
                x => true);
        }
        void ExecuteShowProperties(object parameter)
        {
            MouseButtonEventArgs e = parameter as MouseButtonEventArgs;
            if (e == null || e.ClickCount == 2)
            {
                using (var scope = new TransactionScope())
                {
                    ElementProperties dlg = new ElementProperties();
                    dlg.DataContext = this;
                    if (dlg.ShowDialog().GetValueOrDefault())
                    {
                        scope.Complete();
                    }
                    else
                    {
                    }
                }
                Notify();
            }
        }

        public DelegateCommand<string> AdjustValueCommand { get; set; }
        void InitAdjustValueCommand()
        {
            AdjustValueCommand = new DelegateCommand<string>(
                ExecuteAdjustValue,
                x => true);
        }
        void ExecuteAdjustValue(string parameter)
        {
            switch (parameter)
            {
                case "MinDown":
                    Minimum--;
                    break;
                case "MinUp":
                    Minimum++;
                    break;
                case "MaxDown":
                    Maximum--;
                    break;
                case "MaxUp":
                    Maximum++;
                    break;
            }
        }
    }
}
