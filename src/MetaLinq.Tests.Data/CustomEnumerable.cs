namespace MetaLinq.Tests;

public class CustomEnumerable<T> : IEnumerable<T> {
    protected readonly IList<T> data;

    public CustomEnumerable(IList<T> data) {
        this.data = data;
    }

    public struct Enumerator : IEnumerator<T> {
        readonly IList<T> data;
        int index;
        public Enumerator(IList<T> data) {
            this.data = data;
            index = -1;
        }
        public T Current => data[index];
        public bool MoveNext() {
            index++;
            return index < data.Count;
        }
        public void Dispose() { }

        object IEnumerator.Current => throw new NotImplementedException();
        void IEnumerator.Reset() => throw new NotImplementedException();
    }
    public Enumerator GetEnumerator() => new Enumerator(data);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}

public class CustomCollection<T> : CustomEnumerable<T>, ICollection<T> {
    public CustomCollection(IList<T> data) : base(data) {
    }

    public int Count => data.Count;

    bool ICollection<T>.IsReadOnly => throw new NotImplementedException();
    void ICollection<T>.Add(T item) => throw new NotImplementedException();
    void ICollection<T>.Clear() => throw new NotImplementedException();
    bool ICollection<T>.Contains(T item) => throw new NotImplementedException();
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
    bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
}