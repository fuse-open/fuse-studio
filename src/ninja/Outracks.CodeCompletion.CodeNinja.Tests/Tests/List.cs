using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class List : Test
	{
		[Test]
		public void List00()
		{
			AssertCode(
@"namespace Unol
{
    public class List<T>
    {
        T[] data;
        int used;
        /*
        class Enumerator: IEnumerator<T>
        {
            List<T> list;
            int pos = -1;
            public Enumerator(List<T> list)
            {
                this.list = list;
            }
            public bool MoveNext()
            {
                pos++;
                return pos < list.Count;
            }
            public T Current { get { return list[pos]; } }
        }

        public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }
        */
        public List()
        {
            data =$(null) null;
            used = 0;
        }

        public T[] ToArray()
        {
            var t = new T[used];
            for (int i = 0; i < used; i++) t[i] = data[$(i, used)i];
            return t;
        }

        void ensureCapacity()
        {
            if (data == null)
            {
                data = new T[2];
            }
            else if ($(used)used + 1 == data.$(Length)Length)
            {
                var newData = new T[data.Length *$(data) 2];

                for (int i = 0; i < used; i++)
                {
                    newData[i] = data[i];
                }

                data = newData;
            }
        }

        public int Count
        {
            get { return used; }
        }

        public void Add(T item)
        {
            ensureCapacity();
            data[used++] = item;
        }

        public void Insert(int index, T item)
        {
            ensureCapacity();
            for (int i = used; i >= index; i--)
            {
                data[i + 1] = data[i];
            }
            data[index] = item;
            used++;
        }

        public void Remove(T item)
        {
            for (int i = 0; i < used; i++)
            {
                if (data[i] == item)
                {
                    RemoveAt(i);
                    return;
                }
            }
            throw new Exception(""List does not contain the given item"");
        }

        public void RemoveAt(int index)
        {
            for (int i = index; i < used - 1; i++)
            {
                data[i] = data[i + 1];
            }
            used = used - 1;
        }

        public void Clear()
        {
            used = 0;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < used; i++)
            {
                if (data[i] == item) return true;
            }
            return false;
        }

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

		public void Sort(Comparison<T> comparer)
        {
			Uno.$(Array)
        }

    }
}

"
			);
		}

		[Test]
		public void List01()
		{
			AssertCode(
@"

using Uno.Collections;
public class a : Uno.Application{}

class b
{
 	List<int> derp;
	b()
	{
		derp.$(Add,Sort)
	}
}

"
			);
		}

	}
}