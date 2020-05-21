using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class ObservableExtension
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            ObservableCollection<T> collection = new ObservableCollection<T>();

            foreach (T item in source)
            {
                collection.Add(item);
            }

            return collection;
        }
    }
}
