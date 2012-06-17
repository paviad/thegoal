using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace TheGoal
{
    public class ObservableInt : INotifyPropertyChanged
    {
        private int _Value;
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
                    _Value = value;
                    if (!NotificationInhibiter.IsNotificationInhibited)
                        RaisePropertyChanged(this, x => x.Value);
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public ObservableInt(int value)
        {
            this._Value = value;
        }

        public static implicit operator ObservableInt(int value) { return new ObservableInt(value); }
        public static implicit operator int(ObservableInt oint) { return oint.Value; }

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

        internal void Increment(int p)
        {
            Value += p;
        }

        internal void Notify()
        {
            RaisePropertyChanged(this, x => x.Value);
        }
    }
}
