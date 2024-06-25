using NUnit.Framework;

namespace Outracks.CodeNinja.Tests.Tests
{
    public class Comments : Test
	{
		[Test]
		public void Comments00()
		{
			AssertCode(
@"class Foo
{
	void Bar()
	{
		Uno.$(Array)
		// hello
	}
}

"
			);
		}

		[Test]
		public void Comments01()
		{
			AssertCode(
@"

namespace Unol
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
            data = null;
            used = 0;
        }

        public T[] ToArray()
        {
            var t = new T[used];
            for (int i = 0; i < used; i++) t[i] = data[i];
            return t;
        }

        void ensureCapacity()
        {
            if (data == null)
            {
                data = new T[2];
            }
            else if (used + 1 == data.Length)
            {
                var newData = new T[data.Length * 2];

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

		public void Sort(Comparison<T> comparer)
        {
			Uno.$(Array)
            //Array.(data, compaer, 0, used);
        }

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }



    }
}
"
			);
		}

	}
}