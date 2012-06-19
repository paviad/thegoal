using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace TheGoal
{
    public static class EventToCommandBehavior
    {
        static Dictionary<UIElement, ICommand> dicMouseDown = new Dictionary<UIElement, ICommand>();

        public static ICommand GetMouseDownCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseDownCommandProperty);
        }

        public static void SetMouseDownCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseDownCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for MouseDownCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.RegisterAttached("MouseDownCommand", typeof(ICommand), typeof(EventToCommandBehavior), new UIPropertyMetadata(null, cb));

        static void cb(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uie = d as UIElement;
            if (uie == null) return;
            ICommand cmd = e.NewValue as ICommand;
            uie.MouseDown -= uie_MouseDown;
            if (cmd != null)
            {
                uie.MouseDown += new MouseButtonEventHandler(uie_MouseDown);
                dicMouseDown[uie] = cmd;
            }
            else
            {
                dicMouseDown.Remove(uie);
            }
        }

        static void uie_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UIElement uie = (UIElement)sender;
            dicMouseDown[uie].Execute(e);
        }
    }
}
