using System.Runtime.CompilerServices;

namespace MetaLinq.Internal {
    public static class AggregateHelper {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Zero<T>() {
            if(typeof(T) == typeof(int)) {
                return (T)(object)0;
            }
            if(typeof(T) == typeof(int?)) {
                return (T)(object)0;
            }
            if(typeof(T) == typeof(long)) {
                return (T)(object)0L;
            }
            if(typeof(T) == typeof(long?)) {
                return (T)(object)0L;
            }
            if(typeof(T) == typeof(float)) {
                return (T)(object)0F;
            }
            if(typeof(T) == typeof(float?)) {
                return (T)(object)0F;
            }
            if(typeof(T) == typeof(double)) {
                return (T)(object)0.0;
            }
            if(typeof(T) == typeof(double?)) {
                return (T)(object)0.0;
            }
            if(typeof(T) == typeof(decimal)) {
                return (T)(object)default(decimal);
            }
            if(typeof(T) == typeof(decimal?)) {
                return (T)(object)default(decimal);
            }
            throw new InvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sum<T>(T value1, T value2) {
            if(typeof(T) == typeof(int)) {
                return (T)(object)((int)(object)value1! + (int)(object)value2!);
            }
            if(typeof(T) == typeof(int?)) {
                if(value2 == null)
                    return value1;
                return (T)(object)((int?)(object)value1! + (int?)(object)value2!);
            }

            if(typeof(T) == typeof(long)) {
                return (T)(object)((long)(object)value1! + (long)(object)value2!);
            }
            if(typeof(T) == typeof(long?)) {
                if(value2 == null)
                    return value1;
                return (T)(object)((long?)(object)value1! + (long?)(object)value2!);
            }

            if(typeof(T) == typeof(float)) {
                return (T)(object)((float)(object)value1! + (float)(object)value2!);
            }
            if(typeof(T) == typeof(float?)) {
                if(value2 == null)
                    return value1;
                return (T)(object)((float?)(object)value1! + (float?)(object)value2!);
            }

            if(typeof(T) == typeof(double)) {
                return (T)(object)((double)(object)value1! + (double)(object)value2!);
            }
            if(typeof(T) == typeof(double?)) {
                if(value2 == null)
                    return value1;
                return (T)(object)((double?)(object)value1! + (double?)(object)value2!);
            }

            if(typeof(T) == typeof(decimal)) {
                return (T)(object)((decimal)(object)value1! + (decimal)(object)value2!);
            }
            if(typeof(T) == typeof(decimal?)) {
                if(value2 == null)
                    return value1;
                return (T)(object)((decimal?)(object)value1! + (decimal?)(object)value2!);
            }

            throw new InvalidOperationException();
        }
    }
}
