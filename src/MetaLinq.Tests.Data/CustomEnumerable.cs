namespace MetaLinq.Tests;

public class CustomEnumerable<T> : IEnumerable<T> {
    protected readonly T[] data;

    public CustomEnumerable(T[] data) {
        this.data = data;
    }

    public struct Enumerator : IEnumerator<T> {
        readonly T[] data;
        int index;
        public Enumerator(T[] data) {
            this.data = data;
            index = -1;
        }
        public T Current => data[index];
        public bool MoveNext() {
            index++;
            return index < data.Length;
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
    public CustomCollection(T[] data) : base(data) {
    }

    public int Count => data.Length;

    bool ICollection<T>.IsReadOnly => throw new NotImplementedException();
    void ICollection<T>.Add(T item) => throw new NotImplementedException();
    void ICollection<T>.Clear() => throw new NotImplementedException();
    bool ICollection<T>.Contains(T item) => throw new NotImplementedException();
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
    bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
}