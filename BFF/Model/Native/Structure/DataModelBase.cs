using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BFF.Model.Native.Structure
{
    public abstract class DataModelBase
    {
        protected string _primaryKeyField;
        protected List<string> _props = new List<string>();

        public DataModelBase()
        {
            PropertyInfo pkProp = this.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).Length > 0).FirstOrDefault();
            if (pkProp != null)
            {
                _primaryKeyField = pkProp.Name;
            }

            foreach (PropertyInfo prop in this.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(DataFieldAttribute), false).Length > 0))
            {
                _props.Add(prop.Name);
            }
        }

        public virtual string TableName { get { return this.GetType().Name; } }

        public virtual string InsertStatement
        {
            get
            {
                return string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})",
                    this.TableName,
                    GetDelimitedSafeFieldList(", "),
                    GetDelimitedSafeParamList(", "));
            }
        }

        public virtual string UpdateStatement
        {
            get
            {
                return string.Format("UPDATE [{0}] SET {1} WHERE [{2}] = @{2}",
                    this.TableName,
                    GetDelimitedSafeSetList(", "),
                    _primaryKeyField);
            }
        }

        public virtual string DeleteStatement
        {
            get
            {
                return string.Format("DELETE [{0}] WHERE [{1}] = @{1}",
                    this.TableName,
                    _primaryKeyField);
            }
        }

        public virtual string SelectStatement
        {
            get
            {
                return string.Format("SELECT [{0}], {1} FROM [{2}]",
                    _primaryKeyField,
                    GetDelimitedSafeFieldList(", "),
                    this.TableName);
            }
        }

        protected string GetDelimitedSafeParamList(string delimiter)
        {
            return string.Join(delimiter, _props.Select(k => string.Format("@{0}", k)));
        }

        protected string GetDelimitedSafeFieldList(string delimiter)
        {
            return string.Join(delimiter, _props.Select(k => string.Format("[{0}]", k)));
        }

        protected string GetDelimitedSafeSetList(string delimiter)
        {
            return string.Join(delimiter, _props.Select(k => string.Format("[{0}] = @{0}", k)));
        }
    }
}
