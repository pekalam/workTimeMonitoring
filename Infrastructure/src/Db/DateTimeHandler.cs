using System;
using System.Data;
using Dapper;

namespace Infrastructure.Db
{
    internal class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }

        public override DateTime Parse(object value)
        {
            return ((DateTime) value).ToUniversalTime();
        }
    }
}