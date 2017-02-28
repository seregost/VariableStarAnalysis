using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector.Helpers
{
    [AttributeUsage(AttributeTargets.Field)]
    class LayoutAttribute : Attribute
    {
        private int _index;
        private int _length;
        private int _divider;

        public int index
        {
            get { return _index; }
        }

        public int length
        {
            get { return _length; }
        }

        public int divider
        {
            get { return _divider; }
        }

        public LayoutAttribute(int index, int length)
        {
            this._index = index;
            this._length = length;
        }

        public LayoutAttribute(int index, int length, int divider)
        {
            this._index = index;
            this._length = length;
            this._divider = divider;
        }

    }
}
