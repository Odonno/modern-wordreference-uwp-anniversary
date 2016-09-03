using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Services
{
    public interface IReactiveService
    {
        Subject<Models.TranslationResult> NewTranslationDone { get; }
        Subject<Models.Dictionary> SelectDictionaryDone { get; }
        Subject<Unit> ShowNewTranslationControlDone { get; }
    }

    public class ReactiveService : IReactiveService
    {
        public Subject<Models.TranslationResult> NewTranslationDone { get; } = new Subject<Models.TranslationResult>();
        public Subject<Models.Dictionary> SelectDictionaryDone { get; } = new Subject<Models.Dictionary>();
        public Subject<Unit> ShowNewTranslationControlDone { get; } = new Subject<Unit>();
    }
}
