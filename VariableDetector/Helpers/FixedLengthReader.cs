using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VariableDetector.Helpers
{
    class FixedLengthReader
    {
        private string stream;

        public FixedLengthReader(string stream)
        {
            this.stream = stream;
        }

        public void read<T>(T data)
        {
            foreach (PropertyInfo fi in typeof(T).GetProperties())
            {
                foreach (object attr in fi.GetCustomAttributes())
                {
                    if (attr is LayoutAttribute)
                    {
                        LayoutAttribute la = (LayoutAttribute)attr;

                        if(la.index + la.length <= stream.Length)
                        { 
                            string sub = stream.Substring(la.index, la.length);

                            if (fi.PropertyType.Equals(typeof(int)))
                            {
                                if(sub.Trim().Length > 0)
                                    fi.SetValue(data, Convert.ToInt32(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(char)))
                            {
                                fi.SetValue(data, Convert.ToChar(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(bool)))
                            {
                                fi.SetValue(data, Convert.ToBoolean(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(string)))
                            {
                                // --- If string was written using UTF8 ---
                                fi.SetValue(data, sub.Trim());

                                // --- ALTERNATIVE: Chars were written to file ---
                                //char[] tmp = new char[la.length - 1];
                                //for (int i = 0; i < la.length; i++)
                                //{
                                //    tmp[i] = BitConverter.ToChar(buffer, i * sizeof(char));
                                //}
                                //fi.SetValue(data, new string(tmp));
                            }
                            else if (fi.PropertyType.Equals(typeof(double)))
                            {
                                fi.SetValue(data, Convert.ToDouble(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(decimal)))
                            {
                                fi.SetValue(data, Convert.ToDecimal(sub)/(decimal)la.divider);
                            }
                            else if (fi.PropertyType.Equals(typeof(short)))
                            {
                                fi.SetValue(data, Convert.ToInt16(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(long)))
                            {
                                fi.SetValue(data, Convert.ToInt64(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(float)))
                            {
                                if(sub.Trim().Length > 0)
                                    fi.SetValue(data, Convert.ToSingle(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(ushort)))
                            {
                                fi.SetValue(data, Convert.ToUInt16(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(uint)))
                            {
                                fi.SetValue(data, Convert.ToUInt32(sub));
                            }
                            else if (fi.PropertyType.Equals(typeof(ulong)))
                            {
                                fi.SetValue(data, Convert.ToUInt64(sub));
                            }
                        }
                    }
                }
            }
        }
    }
    }
