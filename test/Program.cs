using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceModelEx;
using System.Transactions;
using TheGoal;

namespace test
{
    [Serializable]
    class MyIntEx
    {
        public int zzz { get; set; }
    }

    [Serializable]
    class MyInt : Transactional<MyIntEx>
    {
        public MyInt()
            : base(new MyIntEx())
        {

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ObservableInt a = new ObservableInt(6);
            Console.WriteLine(a.Value);
            using (var scope = new TransactionScope())
            {
                a.Increment(7);
                //scope.Complete();
                Console.WriteLine(a.Value);
            }
            Console.WriteLine(a.Value);
        }
    }
}
