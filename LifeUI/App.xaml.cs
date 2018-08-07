using System;
using System.Windows;
using Microsoft.FSharp.Core;

namespace FunctionsinWPF
{
    // F# interop extension so we can send actions into F#
    public static class ext {
        private static readonly Unit Unit = (Unit)Activator.CreateInstance(typeof(Unit), true);

        public static Func<T, Unit> ToFunc<T>(this Action<T> action)
        {
            return x => { action(x); return Unit; };
        }

        public static FSharpFunc<T, Unit> ToFSharpFunc<T>(this Action<T> action)
        {
            return FSharpFunc<T, Unit>.FromConverter(new Converter<T, Unit>(action.ToFunc()));
        }
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {        
        public static void functionalmain(Action<string> fnc)
        {            
            FunctionLib.Main.main(fnc.ToFSharpFunc());
        }
    }
}
