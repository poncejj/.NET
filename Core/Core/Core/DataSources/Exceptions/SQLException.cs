using Core.Exceptions;

namespace Core.DataSources.Exceptions {

    public class SQLException : ResultException {

        public SQLException(SQLErrorEnum reason) {
            Key = "SQLException";
            Reason = reason.ToString();
        }

        public enum SQLErrorEnum {
            Connection = 1,
            ExecutionSQL,
            ExecutionSQLAsync
        }
    }
}