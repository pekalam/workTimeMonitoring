using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WindowUI.Profile
{
    public static class LinqExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            var coll = new ObservableCollection<T>();

            foreach (var item in source)
            {
                coll.Add(item);   
            }

            return coll;
        }
    }
}