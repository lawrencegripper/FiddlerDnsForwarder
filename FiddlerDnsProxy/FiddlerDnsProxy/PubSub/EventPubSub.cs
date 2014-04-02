using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.PlatformServices;

namespace FiddlerDnsProxy.Helper
{
    public class EventPubSub
    {
        private Dictionary<Type, object> _subjects
            = new Dictionary<Type, object>();


        public IDisposable Subscribe<T>(Action<T> observer)
        {
            lock (_subjects)
            {
                if (!_subjects.ContainsKey(typeof(T)))
                {
                    _subjects.Add(typeof(T), new Subject<T>());
                }
                return (_subjects[typeof(T)] as Subject<T>)
                    .Subscribe(observer);
            }
        }

        public void Publish<T>(T item)
        {
            lock (_subjects)
            {
                if (_subjects.ContainsKey(typeof(T)))
                {
                    (_subjects[typeof(T)] as Subject<T>)
                        .OnNext(item);
                }
            }
        }
    }
}
