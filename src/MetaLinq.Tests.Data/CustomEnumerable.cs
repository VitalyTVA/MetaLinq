namespace MetaLinq.Tests;

public class CustomEnumerable<T> : IEnumerable<T> {
    readonly T[] data;

    public int Count => data.Length;

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
